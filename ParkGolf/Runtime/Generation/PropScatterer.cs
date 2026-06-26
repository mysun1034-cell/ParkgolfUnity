using UnityEngine;

namespace ParkGolf
{
    /// <summary>
    /// 나무·벤치 등 프롭을 규칙 기반(시드 고정)으로 산포한다. 페어웨이 근처는 피한다.
    /// 결정적(seed 고정)이라 같은 코스는 항상 같은 배치 → 재현 가능.
    /// 수백 개 정적 프롭의 런타임 렌더 최적화는 Graphics.RenderMeshInstanced 참고(README).
    /// </summary>
    public class PropScatterer : MonoBehaviour
    {
        public GameObject[] treePrefabs;
        public int seed = 12345;
        [Min(0)] public int count = 40;
        public Vector2 areaSize = new Vector2(200f, 200f); // 중심 기준 X×Z(m)
        [Min(0f)] public float fairwayClearRadius = 5f;
        [Tooltip("페어웨이 중심선 샘플 등 — 이 점들 근처는 비운다.")]
        public Transform[] avoidPoints;

        public void Scatter()
        {
            if (treePrefabs == null || treePrefabs.Length == 0)
            {
                Debug.LogWarning("[ParkGolf] treePrefabs가 비어 있습니다(예: Quaternius CC0 나무).");
                return;
            }

            var rng = new System.Random(seed);
            int placed = 0, guard = 0, guardMax = Mathf.Max(1, count) * 30;
            float r2 = fairwayClearRadius * fairwayClearRadius;

            while (placed < count && guard++ < guardMax)
            {
                float x = (float)(rng.NextDouble() - 0.5) * areaSize.x;
                float z = (float)(rng.NextDouble() - 0.5) * areaSize.y;
                var p = new Vector3(x, 0f, z);
                if (TooClose(p, r2)) continue;

                var prefab = treePrefabs[rng.Next(treePrefabs.Length)];
                var go = Instantiate(prefab, transform);
                go.transform.localPosition = p;
                go.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                float s = 0.85f + (float)rng.NextDouble() * 0.4f;
                go.transform.localScale *= s;
                placed++;
            }

            Debug.Log($"[ParkGolf] 프롭 {placed}/{count}개 산포(seed={seed}).");
        }

        bool TooClose(Vector3 p, float r2)
        {
            if (avoidPoints == null) return false;
            foreach (var a in avoidPoints)
                if (a != null && (a.position - p).sqrMagnitude < r2) return true;
            return false;
        }
    }
}
