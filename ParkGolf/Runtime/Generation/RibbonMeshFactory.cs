using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ParkGolf
{
    /// <summary>
    /// 스플라인 + 폭 → 평평한 리본 메시. 기본 SplineExtrude는 원형 단면(튜브)만 되므로 직접 생성한다.
    /// 모든 ISpline(로컬 좌표)을 localToTarget(Matrix4x4)로 변환해 처리하므로
    /// SplineContainer / HoleDefinition.centerline 양쪽에서 재사용된다.
    /// 사용 API는 전부 공식 문서로 confirmed: SplineUtility.Evaluate / CalculateLength, Mesh.Set*.
    /// </summary>
    public static class RibbonMeshFactory
    {
        public static Mesh Build(ISpline spline, Matrix4x4 localToTarget, float width, float spacing = 1f, bool faceUp = true)
        {
            float4x4 m = ToFloat4x4(localToTarget);

            // CalculateLength<T>(this T spline, float4x4 transform) → float
            float length = SplineUtility.CalculateLength(spline, m);
            int segments = Mathf.Max(2, Mathf.RoundToInt(length / Mathf.Max(0.01f, spacing)));
            float halfW = width * 0.5f;

            var verts = new List<Vector3>((segments + 1) * 2);
            var uvs   = new List<Vector2>((segments + 1) * 2);
            var tris  = new List<int>(segments * 6);

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                // Evaluate<T>(this T spline, float t, out float3 pos, out float3 tan, out float3 up) → bool (로컬)
                SplineUtility.Evaluate(spline, t, out float3 lp, out float3 lt, out float3 lu);

                float3 pos = math.transform(m, lp);            // 점 변환(이동 포함)
                float3 tan = math.rotate(m, lt);               // 방향 변환(이동 제외)
                float3 up  = math.rotate(m, lu);
                if (math.lengthsq(tan) < 1e-8f) tan = new float3(0, 0, 1);
                if (math.lengthsq(up)  < 1e-8f) up  = new float3(0, 1, 0);
                tan = math.normalize(tan);
                up  = math.normalize(up);

                float3 right = math.normalize(math.cross(tan, up)); // 부호는 winding에 따라 뒤집힐 수 있음 → 아래서 자동 보정
                float3 l = pos - right * halfW;
                float3 r = pos + right * halfW;

                verts.Add((Vector3)l);
                verts.Add((Vector3)r);
                float v = length * t;
                uvs.Add(new Vector2(0f, v));
                uvs.Add(new Vector2(1f, v));
            }

            for (int i = 0; i < segments; i++)
            {
                int b = i * 2;                                  // 인접 좌/우 4정점
                tris.Add(b);     tris.Add(b + 2); tris.Add(b + 1);
                tris.Add(b + 1); tris.Add(b + 2); tris.Add(b + 3);
            }

            var mesh = new Mesh { name = "ParkGolfRibbon" };
            if (verts.Count > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            if (faceUp) FlipIfFacingDown(mesh);
            return mesh;
        }

        // cross 부호/winding에 따라 면이 아래를 볼 수 있음 → 평균 노멀이 위를 보도록 자동 보정.
        static void FlipIfFacingDown(Mesh mesh)
        {
            var normals = mesh.normals;
            if (normals == null || normals.Length == 0) return;

            float dot = 0f;
            for (int i = 0; i < normals.Length; i++) dot += Vector3.Dot(normals[i], Vector3.up);
            if (dot >= 0f) return; // 이미 위를 봄

            var tris = mesh.triangles;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int tmp = tris[i + 1]; tris[i + 1] = tris[i + 2]; tris[i + 2] = tmp;
            }
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        static float4x4 ToFloat4x4(Matrix4x4 m)
        {
            return new float4x4(
                (float4)m.GetColumn(0),
                (float4)m.GetColumn(1),
                (float4)m.GetColumn(2),
                (float4)m.GetColumn(3));
        }
    }
}
