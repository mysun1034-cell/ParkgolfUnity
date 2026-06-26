using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ParkGolf.EditorTools
{
    /// <summary>
    /// (B) 실측 자동수입 — CourseDTO JSON → CourseDefinition + HoleDefinition 에셋 생성.
    /// centerlinePoints(트레이싱 점)를 Catmull-Rom 풍 핸들로 BezierKnot 스플라인으로 변환한다.
    /// </summary>
    public static class CourseJsonImporter
    {
        [MenuItem("ParkGolf/Import Course from JSON…")]
        public static void ImportMenu()
        {
            string path = EditorUtility.OpenFilePanel("코스 JSON 선택", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path)) return;

            const string folder = "Assets/ParkGolf/Courses";
            EnsureFolder(folder);
            var def = ImportFile(path, folder);
            if (def != null) { Selection.activeObject = def; EditorGUIUtility.PingObject(def); }
        }

        public static CourseDefinition ImportFile(string jsonPath, string assetFolder)
        {
            string json = File.ReadAllText(jsonPath);
            CourseDTO dto = JsonUtility.FromJson<CourseDTO>(json);   // 최상위 객체(CourseDTO) → JsonUtility OK
            if (dto == null) { Debug.LogError("[ParkGolf] JSON 파싱 실패: " + jsonPath); return null; }

            var course = ScriptableObject.CreateInstance<CourseDefinition>();
            course.courseName = string.IsNullOrEmpty(dto.name) ? Path.GetFileNameWithoutExtension(jsonPath) : dto.name;

            string safe = MakeSafe(course.courseName);
            string coursePath = AssetDatabase.GenerateUniqueAssetPath($"{assetFolder}/{safe}.asset");
            AssetDatabase.CreateAsset(course, coursePath);

            var holes = new List<HoleDefinition>();
            if (dto.holes != null)
            {
                foreach (var hd in dto.holes)
                {
                    var hole = ScriptableObject.CreateInstance<HoleDefinition>();
                    hole.name = $"{safe}_Hole{hd.holeNumber}";
                    hole.holeNumber = hd.holeNumber;
                    hole.par = hd.par;
                    hole.distanceMeters = hd.distanceMeters;
                    hole.fairwayWidth = Mathf.Max(ParkGolfStandards.MinFairwayWidth, hd.fairwayWidth);
                    hole.hazard = ParseHazard(hd.hazard);
                    hole.teePosition = hd.tee != null ? hd.tee.ToVector3() : Vector3.zero;
                    hole.cupPosition = hd.cup != null ? hd.cup.ToVector3() : Vector3.zero;
                    hole.centerline = BuildSpline(hd.centerlinePoints);

                    AssetDatabase.AddObjectToAsset(hole, course);   // 코스 에셋의 서브에셋으로 정리
                    holes.Add(hole);
                }
            }

            course.holes = holes.ToArray();
            EditorUtility.SetDirty(course);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ParkGolf] '{course.courseName}' 임포트 완료 — {holes.Count}홀, 기준타수 {course.TotalPar()}.");
            return course;
        }

        // 점 목록 → 스플라인. 각 점의 탄젠트를 이웃 점 방향으로 자동 산정(부드러운 곡선).
        static Spline BuildSpline(Vec3[] pts)
        {
            var spline = new Spline();
            if (pts == null || pts.Length == 0) return spline;

            var ps = new float3[pts.Length];
            for (int i = 0; i < pts.Length; i++) ps[i] = (float3)pts[i].ToVector3();

            for (int i = 0; i < ps.Length; i++)
            {
                float3 prev = ps[Mathf.Max(0, i - 1)];
                float3 next = ps[Mathf.Min(ps.Length - 1, i + 1)];
                float3 d = next - prev;
                float len = math.length(d);
                float3 dir = len > 1e-5f ? d / len : new float3(0, 0, 1);
                float handle = 0.25f * math.distance(prev, next);   // Catmull-Rom 풍 핸들 길이

                var k = new BezierKnot(ps[i]);
                k.TangentIn = -dir * handle;
                k.TangentOut = dir * handle;
                k.Rotation = quaternion.identity;   // BezierKnot(float3)가 이미 identity로 초기화 — 명시적 안전망(중복이나 무해)
                spline.Add(k);
            }
            return spline;
        }

        static HoleHazard ParseHazard(string s)
        {
            switch (s)
            {
                case "Sand": return HoleHazard.Sand;
                case "Water": return HoleHazard.Water;
                default: return HoleHazard.None;
            }
        }

        static string MakeSafe(string s)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            return s;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = "Assets";
            foreach (var part in folder.Substring("Assets/".Length).Split('/'))
            {
                string next = parent + "/" + part;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(parent, part);
                parent = next;
            }
        }
    }
}
