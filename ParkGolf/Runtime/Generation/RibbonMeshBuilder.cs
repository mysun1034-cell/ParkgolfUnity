using UnityEngine;
using UnityEngine.Splines;

namespace ParkGolf
{
    public enum SurfaceType { Fairway, Green, Tee, CartPath }

    /// <summary>
    /// 씬에서 SplineContainer를 직접 편집하며 페어웨이/그린/티/카트길 메시를 생성하는 작성용 컴포넌트.
    /// (코스 일괄 생성은 CourseBuilder가 HoleDefinition.centerline로 처리.)
    /// 인스펙터의 "Build Mesh" 버튼(에디터)으로 즉시 갱신.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RibbonMeshBuilder : MonoBehaviour
    {
        public SplineContainer spline;
        public SurfaceType surface = SurfaceType.Fairway;
        [Min(0.1f)] public float width = 4f;
        [Min(0.1f)] public float spacing = 1f;

        public Mesh Build()
        {
            if (spline == null || spline.Spline == null)
            {
                Debug.LogWarning("[ParkGolf] SplineContainer가 설정되지 않았습니다.");
                return null;
            }

            // 스플라인 로컬 → 이 오브젝트 로컬 좌표로 변환해 메시를 만든다(이 GameObject의 transform 이중적용 방지).
            Matrix4x4 localToTarget = transform.worldToLocalMatrix * spline.transform.localToWorldMatrix;
            Mesh mesh = RibbonMeshFactory.Build(spline.Spline, localToTarget, width, spacing);

            GetComponent<MeshFilter>().sharedMesh = mesh;
            var mc = GetComponent<MeshCollider>();
            if (mc != null) mc.sharedMesh = mesh;
            return mesh;
        }
    }
}
