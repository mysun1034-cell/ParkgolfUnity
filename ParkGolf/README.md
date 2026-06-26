# ParkGolf — Unity 절차생성 파이프라인 (드롭인 패키지)

"코스 = 데이터(스플라인 + 폭 + 설정값)"로 정의하고, 한 번 만든 파이프라인에 데이터만 입력해 코스를 양산하는 Unity 스크립트 모음.
설계 배경·근거·규준 출처는 상위 폴더의 `파크골프_Unity맵_설계_2026-06-26.md` 참조.

> ⚠️ 이 코드는 Unity 없는 환경에서 작성되어 **컴파일·실행 검증은 아직 안 됨**. 공식 API 시그니처는 문서로 확인했으나,
> 첫 임포트 시 콘솔 오류가 나면 README 하단 "알려진 버전 의존" 항목을 먼저 확인할 것.

---

## 1. 설치

1. **의존 패키지**: Package Manager에서 `Splines`(com.unity.splines) 설치. (Mathematics는 함께 따라옴.)
   - DEM 임포터는 코어 TerrainData만 써서 추가 패키지 불필요.
   - JSON 임포터는 내장 `JsonUtility` 사용 → Newtonsoft 불필요(복잡 스키마로 갈 때만 com.unity.nuget.newtonsoft-json).
2. 이 `ParkGolf` 폴더를 통째로 프로젝트의 `Assets/` 아래로 복사.
   - `Runtime/`·`Editor/`에 asmdef 2개(`ParkGolf.Runtime`·`ParkGolf.Editor`)가 들어 있어, **asmdef를 쓰는 프로젝트든 안 쓰는 프로젝트든 Editor/Runtime 분리가 올바르게 동작**한다(Editor 코드가 플레이어 빌드에 안 섞임). 단순 폴더규약만 원하면 두 `.asmdef`를 지워도 `Editor/` 폴더 이름 규약으로 동작.
   - asmdef가 `Unity.Splines`를 참조하므로, **Splines 미설치 상태면 어셈블리 전체가 컴파일 오류**가 난다 → 1번을 먼저 끝낼 것.
3. 권장 Unity: 2022 LTS(splines 2.3~2.5) 또는 Unity 6(splines 2.6~2.8). 핵심 API 시그니처는 2.0~2.8 동일.

---

## 2. 폴더 구성

```
ParkGolf/
  Runtime/
    Data/        HoleDefinition · CourseDefinition · CourseDTO · ParkGolfStandards
    Generation/  RibbonMeshFactory · RibbonMeshBuilder · CourseBuilder · PropScatterer
  Editor/        CourseJsonImporter(B) · DemHeightmapImporter(C) · SplineBakeUtility · ParkGolfEditors(버튼)
  Samples/       sample_course.json
```

---

## 3. 사용법

### A. 손작성 → 코스 생성
1. 씬 빈 오브젝트에 `SplineContainer` 추가 → 스플라인 툴로 홀 중심선 그리기.
2. 메뉴 **ParkGolf ▸ Bake Selected SplineContainer → Hole Definition** → `Assets/ParkGolf/Holes`에 HoleDefinition 생성(티/컵은 스플라인 양끝 자동).
3. **Create ▸ ParkGolf ▸ Course Definition**으로 코스 만들고 `holes`에 홀들 배열로 등록.
4. 씬 오브젝트에 `CourseBuilder` 추가 → `course`·프리팹(컵/깃대/티, Kenney CC0)·페어웨이 머티리얼 지정 → 인스펙터 **Build Course**.
   - 단일 면만 빠르게 보려면 `RibbonMeshBuilder`(MeshFilter/Renderer 포함 오브젝트)에서 **Build Mesh**.

### B. 실측 자동수입(JSON → 코스)
1. 위성영상에서 홀 중심선·티·컵 좌표를 트레이싱해 `sample_course.json` 형식으로 작성.
2. 메뉴 **ParkGolf ▸ Import Course from JSON…** → CourseDefinition + HoleDefinition 에셋 자동 생성(점들 → 부드러운 스플라인 변환).
3. 그 CourseDefinition을 `CourseBuilder`에 물려 **Build Course**.
   - ★ JSON 최상위는 반드시 객체(`{ "name":..., "holes":[...] }`). 최상위 배열은 JsonUtility가 못 읽음.

### C. DEM 실측 지형
1. (Unity 밖) GDAL로 한국 DEM 5m → 16-bit RAW (설계문서 §3.3 명령 참조):
   ```bash
   gdalwarp -t_srs EPSG:5186 -r bilinear input.img dem.tif
   gdalwarp -te xmin ymin xmax ymax dem.tif crop.tif
   gdal_translate -ot UInt16 -scale -of ENVI -outsize 1025 1025 crop.tif heightmap.raw
   gdalinfo -mm crop.tif   # → 표고 max-min(m)을 worldY로
   ```
2. `heightmap.raw`를 프로젝트 안에 두고 메뉴 **ParkGolf ▸ Import DEM Heightmap (.raw)…** 선택.
   - 바이트오더(little/big)·상하반전을 **자동 처리**. 해상도는 2^n+1(1025 등)이어야 함.
   - 기본 size는 (200, 30, 200) → `DemHeightmapImporter.BuildTerrain(path, res, worldXZ, worldY)`를 직접 호출해 실제 gdalinfo 값으로 넣으면 정확.
3. 지형 베이스(DEM) 위에 A/B의 페어웨이 메시를 얹는 하이브리드 권장(5m 격자는 미세 굴곡 못 살림).

---

## 4. 무료 CC0 에셋(프리팹 채우기)

- 깃대·홀컵·코스 타일: **Kenney Minigolf Kit**(CC0)
- 나무·풀·바위: **Quaternius**(CC0, 무료 68개) · **Kenney Nature Kit**(CC0)
- 잔디 텍스처/조명: **ambientCG · Poly Haven**(CC0)
- 라이선스 함정/외주 시세는 설계문서 §2·§5 참조.

---

## 5. 알려진 버전 의존 / 확인 포인트

- 임포터는 `BezierKnot.Rotation`을 `quaternion.identity`로 명시 설정함(생성자가 이미 identity라 안전망) — 곡선이 이상하면 이 부분과 탄젠트 핸들 길이를 확인.
- 리본 메시가 뒤집혀(아래를 향해) 보이면 `RibbonMeshFactory.FlipIfFacingDown`이 처리하지만, 그래도 이상하면 `right` 부호/삼각형 winding 점검.
- `SplineUtility.Evaluate`/`CalculateLength`는 확장 메서드(`this T spline`) — `using UnityEngine.Splines;` 필수.
- DEM 자동 엔디안 선택은 휴리스틱(거칠기 비교) — 드물게 오판하면 `BuildTerrain`에서 `useBig`를 수동 고정.
