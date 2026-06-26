using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ParkGolf.EditorTools
{
    /// <summary>
    /// 씬에서 SplineContainer를 편집(Unity 스플라인 툴이 거기서만 동작) → 선택한 컨테이너를
    /// HoleDefinition 에셋으로 "Bake". centerline 깊은 복사 + 티/컵을 스플라인 양끝으로 설정.
    /// </summary>
    public static class SplineBakeUtility
    {
        [MenuItem("ParkGolf/Bake Selected SplineContainer → Hole Definition")]
        public static void Bake()
        {
            var go = Selection.activeGameObject;
            var sc = go != null ? go.GetComponent<SplineContainer>() : null;
            if (sc == null || sc.Spline == null)
            {
                Debug.LogWarning("[ParkGolf] 씬에서 SplineContainer가 붙은 오브젝트를 선택하세요.");
                return;
            }

            var hole = ScriptableObject.CreateInstance<HoleDefinition>();
            hole.centerline = CopySpline(sc.Spline);

            // 티/컵 = 스플라인 양끝(컨테이너 월드 좌표)
            if (sc.Evaluate(0f, out var p0, out _, out _)) hole.teePosition = (Vector3)p0;
            if (sc.Evaluate(1f, out var p1, out _, out _)) hole.cupPosition = (Vector3)p1;

            const string dir = "Assets/ParkGolf/Holes";
            EnsureFolder(dir);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{dir}/{go.name}.asset");
            AssetDatabase.CreateAsset(hole, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = hole;
            EditorGUIUtility.PingObject(hole);
            Debug.Log($"[ParkGolf] '{go.name}' → HoleDefinition Bake 완료: {path}");
        }

        static Spline CopySpline(Spline src)
        {
            var s = new Spline();
            foreach (var knot in src) s.Add(knot);
            s.Closed = src.Closed;
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
