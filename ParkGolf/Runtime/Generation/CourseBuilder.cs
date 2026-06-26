using UnityEngine;

namespace ParkGolf
{
    /// <summary>
    /// CourseDefinition(데이터)을 받아 코스 전체를 씬에 생성한다 — "코스 = 데이터 입력" 파이프라인의 실행기.
    /// 홀마다: 페어웨이 리본 메시 + 티/컵/깃대 프리팹 배치. 규준 위반은 경고 로그.
    /// </summary>
    public class CourseBuilder : MonoBehaviour
    {
        [Header("데이터")]
        public CourseDefinition course;

        [Header("프리팹 (Kenney/Quaternius CC0 권장)")]
        public GameObject cupPrefab;
        public GameObject flagPrefab;
        public GameObject teePrefab;

        [Header("머티리얼")]
        public Material fairwayMaterial;

        [Header("생성 옵션")]
        [Min(0.1f)] public float fairwaySpacing = 1f;

        public void BuildCourse()
        {
            if (course == null) { Debug.LogWarning("[ParkGolf] CourseDefinition이 설정되지 않았습니다."); return; }

            // 이전 생성물 정리
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroySafe(transform.GetChild(i).gameObject);

            if (course.holes != null)
            {
                foreach (var h in course.holes)
                {
                    if (h == null) continue;
                    foreach (var warn in ParkGolfStandards.Validate(h))
                        Debug.LogWarning("[ParkGolf 규준] " + warn);
                    BuildHole(h);
                }
            }

            int holeCount = course.holes != null ? course.holes.Length : 0;
            Debug.Log($"[ParkGolf] '{course.courseName}' 생성 완료 — {holeCount}홀, 기준타수 {course.TotalPar()}.");
        }

        void BuildHole(HoleDefinition h)
        {
            var holeRoot = new GameObject($"Hole_{h.holeNumber}_Par{h.par}");
            holeRoot.transform.SetParent(transform, false); // 로컬 항등 → centerline(코스 로컬)을 그대로 사용

            // 페어웨이 메시 (centerline은 코스 로컬 좌표, holeRoot도 로컬 항등이므로 변환 = 항등)
            if (h.centerline != null && h.centerline.Count >= 2)
            {
                var fw = new GameObject("Fairway");
                fw.transform.SetParent(holeRoot.transform, false);
                var mf = fw.AddComponent<MeshFilter>();
                var mr = fw.AddComponent<MeshRenderer>();
                if (fairwayMaterial != null) mr.sharedMaterial = fairwayMaterial;

                Mesh mesh = RibbonMeshFactory.Build(h.centerline, Matrix4x4.identity, h.fairwayWidth, fairwaySpacing);
                mf.sharedMesh = mesh;
                fw.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            PlaceMarker(teePrefab, h.teePosition, holeRoot.transform, "Tee");
            PlaceMarker(cupPrefab, h.cupPosition, holeRoot.transform, "Cup");
            PlaceMarker(flagPrefab, h.cupPosition, holeRoot.transform, "Flag");
        }

        void PlaceMarker(GameObject prefab, Vector3 localPos, Transform parent, string label)
        {
            if (prefab == null) return;
            var go = Instantiate(prefab, parent);
            go.name = label;
            go.transform.localPosition = localPos;
        }

        static void DestroySafe(GameObject go)
        {
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }
}
