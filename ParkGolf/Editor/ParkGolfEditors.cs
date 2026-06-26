using UnityEditor;
using UnityEngine;

namespace ParkGolf.EditorTools
{
    /// <summary>인스펙터에 "Build Mesh" 버튼 추가.</summary>
    [CustomEditor(typeof(RibbonMeshBuilder))]
    public class RibbonMeshBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Mesh"))
            {
                var b = (RibbonMeshBuilder)target;
                var mesh = b.Build();
                if (mesh != null) EditorUtility.SetDirty(b);
            }
        }
    }

    /// <summary>인스펙터에 "Build Course" 버튼 추가.</summary>
    [CustomEditor(typeof(CourseBuilder))]
    public class CourseBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Build Course"))
            {
                var b = (CourseBuilder)target;
                b.BuildCourse();
                EditorUtility.SetDirty(b);
            }
        }
    }

    /// <summary>인스펙터에 "Scatter Props" 버튼 추가.</summary>
    [CustomEditor(typeof(PropScatterer))]
    public class PropScattererEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Scatter Props"))
            {
                var s = (PropScatterer)target;
                s.Scatter();
                EditorUtility.SetDirty(s);
            }
        }
    }
}
