using System;
using UnityEngine;

namespace ParkGolf
{
    // ── 실측 대량수입용 JSON DTO ──
    // ★ JsonUtility는 최상위 배열을 못 읽으므로, 반드시 CourseDTO(객체)로 감싼 JSON을 쓴다.
    //    더 복잡한 스키마가 필요하면 com.unity.nuget.newtonsoft-json(Json.NET)으로 교체.

    [Serializable]
    public class Vec3
    {
        public float x, y, z;
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class HoleDTO
    {
        public int holeNumber = 1;
        public int par = 3;
        public float distanceMeters = 50f;
        public float fairwayWidth = 4f;
        public string hazard = "None";     // "None" | "Sand" | "Water"
        public Vec3[] centerlinePoints;     // 위성 트레이싱/실측 점들 → 스플라인으로 변환
        public Vec3 tee;
        public Vec3 cup;
    }

    [Serializable]
    public class CourseDTO
    {
        public string name = "새 코스";
        public HoleDTO[] holes;
    }
}
