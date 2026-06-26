using UnityEngine;
using UnityEngine.Splines;

namespace ParkGolf
{
    public enum HoleHazard { None, Sand, Water }

    /// <summary>
    /// 한 홀의 정의 = 데이터. 디자이너가 인스펙터로 작성하거나 JSON 임포터가 생성한다.
    /// 형상은 centerline(Spline)으로, 위치는 tee/cup 좌표로 가진다(코스 로컬 좌표 기준).
    /// </summary>
    [CreateAssetMenu(fileName = "Hole", menuName = "ParkGolf/Hole Definition")]
    public class HoleDefinition : ScriptableObject
    {
        [Header("규준 파라미터")]
        [Range(1, 9)] public int holeNumber = 1;

        [Tooltip("파크골프 9홀 표준: 파3×4 + 파4×4 + 파5×1 = 33타")]
        public int par = 3;

        [Tooltip("규준 상한 — 파3 ≤60 / 파4 ≤100 / 파5 ≤150 (m). 하한은 시설 재량(휴리스틱).")]
        public float distanceMeters = 50f;

        [Tooltip("페어웨이 폭(m). 규준 최소 2m.")]
        public float fairwayWidth = 4f;

        public HoleHazard hazard = HoleHazard.None;

        [Header("형상 데이터")]
        [Tooltip("홀 중심선. Spline은 [Serializable]이라 ScriptableObject에 직접 저장된다.")]
        public Spline centerline = new Spline();

        [Tooltip("티 위치(코스 로컬).")] public Vector3 teePosition;
        [Tooltip("컵 위치(코스 로컬).")] public Vector3 cupPosition;
    }
}
