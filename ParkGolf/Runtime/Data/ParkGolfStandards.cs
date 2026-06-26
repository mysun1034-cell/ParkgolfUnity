using System.Collections.Generic;

namespace ParkGolf
{
    /// <summary>
    /// 파크골프 코스 규준 상수 + 검증 헬퍼.
    /// 각 값의 신뢰도를 주석에 명시한다(법정/공인 vs 운영 휴리스틱 구분). 출처는 설계문서 참조.
    /// high=출처 일치 / medium=출처 편차 일부 / low=출처 상충(휴리스틱).
    /// </summary>
    public static class ParkGolfStandards
    {
        // 9홀 기준타수 33 (high): 파3×4 + 파4×4 + 파5×1
        public const int StandardHolesPerCourse = 9;
        public const int StandardTotalPar = 33;

        // 홀 거리 상한 (high, m). 하한은 시설 재량(휴리스틱)이라 강제하지 않음.
        public const float MaxDistancePar3 = 60f;
        public const float MaxDistancePar4 = 100f;
        public const float MaxDistancePar5 = 150f;

        public const float MinFairwayWidth = 2f;    // 페어웨이 폭 최소 (medium)
        public const float MinGreenDiameter = 5f;    // 그린 지름 ≥5m (medium)

        public const float CupInnerDiameter = 0.20f; // 홀컵 내경 (high)
        public const float CupOuterDiameter = 0.21f; // 홀컵 외경 (high)

        public const float BallDiameter = 0.06f;     // 공 지름 60mm (high)
        public const float BallMassKg = 0.090f;      // 공 무게 ~90g (medium, 80~100g 편차 — 공인규정 원문으로 확정 권장)

        public const float MaxClubLength = 0.86f;    // 클럽 길이 ≤86cm (high)
        public const float MaxClubMassKg = 0.6f;     // 클럽 무게 ≤600g (medium, 550g 출처도 존재)

        public static float MaxDistanceForPar(int par)
        {
            switch (par)
            {
                case 3: return MaxDistancePar3;
                case 4: return MaxDistancePar4;
                case 5: return MaxDistancePar5;
                default: return MaxDistancePar5;
            }
        }

        /// <summary>규준 위반 경고 목록(빈 목록=통과). 상한·폭 등 확인된 값만 검사하고 하한은 검사 안 함.</summary>
        public static List<string> Validate(HoleDefinition h)
        {
            var w = new List<string>();
            if (h.par < 3 || h.par > 5)
                w.Add($"홀 {h.holeNumber}: 파{h.par}는 파크골프 표준(3~5) 밖.");
            if (h.distanceMeters > MaxDistanceForPar(h.par))
                w.Add($"홀 {h.holeNumber}: 거리 {h.distanceMeters}m가 파{h.par} 상한 {MaxDistanceForPar(h.par)}m 초과.");
            if (h.fairwayWidth < MinFairwayWidth)
                w.Add($"홀 {h.holeNumber}: 폭 {h.fairwayWidth}m가 최소 {MinFairwayWidth}m 미만.");
            return w;
        }
    }
}
