using UnityEngine;

namespace ParkGolf
{
    /// <summary>코스 = 홀 정의 묶음 + (선택) 실측 지형. 디자이너 작성 또는 JSON 임포터 생성.</summary>
    [CreateAssetMenu(fileName = "Course", menuName = "ParkGolf/Course Definition")]
    public class CourseDefinition : ScriptableObject
    {
        public string courseName = "새 코스";

        [Tooltip("보통 9홀(파크골프 표준 단위).")]
        public HoleDefinition[] holes;

        [Header("실측 지형(선택)")]
        [Tooltip("DEM에서 만든 16-bit RAW 하이트맵 경로. DEM 임포터로 별도 처리도 가능.")]
        public string heightmapRawPath;

        [Tooltip("지형 크기(m): x=폭, y=최대표고, z=길이. DEM의 (max-min) 표고를 y에.")]
        public Vector3 terrainSize = new Vector3(200f, 30f, 200f);

        /// <summary>코스 전체 기준타수(파크골프 9홀 표준 = 33).</summary>
        public int TotalPar()
        {
            int sum = 0;
            if (holes != null)
                foreach (var h in holes) if (h != null) sum += h.par;
            return sum;
        }
    }
}
