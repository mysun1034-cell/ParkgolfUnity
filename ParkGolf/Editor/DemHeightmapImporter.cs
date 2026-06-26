using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ParkGolf.EditorTools
{
    /// <summary>
    /// (C) DEM 16-bit RAW → Unity Terrain 자동 임포트.
    /// 검증된 2대 함정을 자동 처리:
    ///   1) 바이트오더 — little/big 둘 다 디코드 후 '거칠기'가 작은 쪽을 자동 선택(엉뚱한 엔디안은 극단적 노이즈).
    ///   2) 상하반전  — GDAL(top-left) vs Unity(bottom-left) → 세로 자동 flip.
    /// 사용 API 전부 confirmed: TerrainData.heightmapResolution/size/SetHeights, Terrain.CreateTerrainGameObject.
    /// </summary>
    public static class DemHeightmapImporter
    {
        [MenuItem("ParkGolf/Import DEM Heightmap (.raw)…")]
        public static void ImportMenu()
        {
            string path = EditorUtility.OpenFilePanel("16-bit RAW 하이트맵 선택", Application.dataPath, "");
            if (string.IsNullOrEmpty(path)) return;

            int res = GuessSquareResolution(new FileInfo(path).Length);
            if (res <= 0)
            {
                Debug.LogError("[ParkGolf] 파일 크기가 16-bit 정사각(res×res×2 바이트)과 맞지 않습니다.");
                return;
            }

            // 기본값 — 실제론 gdalinfo -mm 의 (폭=잘라낸 m, 높이=DEMmax-DEMmin)을 넣을 것.
            const float defaultWorldXZ = 200f;
            const float defaultWorldY = 30f;
            var go = BuildTerrain(path, res, defaultWorldXZ, defaultWorldY);
            if (go != null) { Selection.activeGameObject = go; EditorGUIUtility.PingObject(go); }
        }

        public static GameObject BuildTerrain(string rawPath, int res, float worldXZ, float worldY)
        {
            // 해상도 2^n+1 검증 (33/65/.../4097). gdal_translate -outsize 1025 1025 권장.
            int n = res - 1;
            if (n <= 0 || (n & (n - 1)) != 0)
            {
                Debug.LogError($"[ParkGolf] 해상도 {res}가 2^n+1이 아닙니다. gdal_translate -outsize 513/1025/2049 로 다시 만드세요.");
                return null;
            }

            byte[] bytes = File.ReadAllBytes(rawPath);
            if (bytes.Length < res * res * 2)
            {
                Debug.LogError("[ParkGolf] RAW 길이가 부족합니다.");
                return null;
            }

            float[,] little = Decode(bytes, res, bigEndian: false);
            float[,] big    = Decode(bytes, res, bigEndian: true);
            bool useBig = Roughness(big) < Roughness(little);   // 자동 엔디안 선택
            float[,] heights = useBig ? big : little;

            FlipVertical(heights, res);                          // 자동 상하 보정

            var td = new TerrainData { heightmapResolution = res };
            if (td.heightmapResolution != res)
            {
                Debug.LogError($"[ParkGolf] Unity가 해상도를 {td.heightmapResolution}로 클램프했습니다(요청 {res}). 입력 해상도를 2^n+1로 맞추세요.");
                return null;
            }
            td.size = new Vector3(worldXZ, worldY, worldXZ);
            td.SetHeights(0, 0, heights);

            var go = Terrain.CreateTerrainGameObject(td);
            go.name = Path.GetFileNameWithoutExtension(rawPath) + "_Terrain";

            const string dir = "Assets/ParkGolf/Heightmaps";
            EnsureFolder(dir);
            AssetDatabase.CreateAsset(td, AssetDatabase.GenerateUniqueAssetPath($"{dir}/{go.name}.asset"));
            AssetDatabase.SaveAssets();

            Debug.Log($"[ParkGolf] DEM 임포트 완료 — {res}×{res}, byteOrder={(useBig ? "big" : "little")}-endian(자동), size={td.size}.");
            return go;
        }

        // 16-bit RAW → 정규화 높이[y,x] (0~1).
        static float[,] Decode(byte[] b, int res, bool bigEndian)
        {
            var h = new float[res, res];
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    int idx = (y * res + x) * 2;
                    ushort v = bigEndian
                        ? (ushort)((b[idx] << 8) | b[idx + 1])
                        : (ushort)((b[idx + 1] << 8) | b[idx]);
                    h[y, x] = v / 65535f;
                }
            return h;
        }

        // 인접 셀 차이 합 — 엉뚱한 엔디안은 값이 극단적으로 튀어 이 값이 매우 커진다.
        static float Roughness(float[,] h)
        {
            int res = h.GetLength(0);
            int step = Mathf.Max(1, res / 128);
            double sum = 0;
            for (int y = step; y < res; y += step)
                for (int x = step; x < res; x += step)
                    sum += Mathf.Abs(h[y, x] - h[y - step, x]) + Mathf.Abs(h[y, x] - h[y, x - step]);
            return (float)sum;
        }

        static void FlipVertical(float[,] h, int res)
        {
            for (int y = 0; y < res / 2; y++)
                for (int x = 0; x < res; x++)
                {
                    float tmp = h[y, x];
                    h[y, x] = h[res - 1 - y, x];
                    h[res - 1 - y, x] = tmp;
                }
        }

        static int GuessSquareResolution(long byteLen)
        {
            long samples = byteLen / 2;
            int r = (int)Math.Round(Math.Sqrt(samples));
            return (long)r * r == samples ? r : -1;
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
