# 파크골프(ParkGolf, Unity) — CLAUDE.md

> Unity 파크골프 게임. 이 파일은 **에이전트·기여자가 매 세션 먼저 읽는 운영 규칙 정본**이다.
> **현재 상태의 단일 진실 = `docs/작업_레지스트리.md`**(+코드). 설계 근거 = `파크골프_Unity맵_설계_2026-06-26.md`.
> 규칙 전문(full text)의 정본은 이 파일 — 타 문서는 요약·포인터(복붙 금지=드리프트원).

## ⚖️ 규약 우선순위 (위가 항상 이김)
1. 이 CLAUDE.md의 절대 규칙
2. `docs/작업_레지스트리.md` (현재 상태 단일 진실)
3. Claude 기본 동작
- 방법론(TDD·체계적 디버깅)은 최대 활용하되, 위 규칙과 충돌 시 규칙이 이긴다. 예외 없음.

## 🚫 절대 규칙 (MUST / MUST NOT)
1. **결정성(절차생성 핵심)**: 같은 seed → 같은 코스. 생성/검증 로직은 **순수함수**(UnityEngine 비의존, `ParkGolf.Core`).
   EditMode로 **결정성 + 불변식**(겹침 없음·티↔홀 연결성·플레이 가능성) 검증. 대표 seed는 **골든마스터 스냅샷**으로 회귀 가드.
2. **추측 금지 / 근거 주석**: 매직넘버(물리·코스생성·난이도·경사 임계·성능예산)는 **값+근거 주석** 의무. 근거 없으면
   "휴리스틱·튜닝값(임시)" 정직 표기(없는 근거 지어내기 금지). **파크골프 규정 수치**는 출처 확보 전 단정 구현 금지.
3. **성능 예산**: 목표 프레임·드로우콜·코스 생성시간을 **수치+근거**로 정함. 초과 시 결함으로 레지스트리 등재.
4. **에셋 라이선스**: CC0/무료 에셋은 출처·라이선스·다운로드일을 `docs/에셋_출처.md`에 기록. **라이선스 불명 도입 금지**.
   **`.meta`는 에셋과 함께 커밋**.
5. **아키텍처(테스트 가능성)**: 순수 로직 = `ParkGolf.Core`(UnityEngine 비의존). MonoBehaviour는 **얇은 글루**.
6. **검증 게이트**: 커밋 전 `scripts/check` GREEN(컴파일0 · EditMode · 가능 시 PlayMode). 실패 시 커밋 금지.
   *Unity 미설치 환경에선 해당 단계 "SKIPPED" 정직 표기 — 통과 위장 금지.*
7. **단일 진실**: 현재 상태 = 코드 + `docs/작업_레지스트리.md`. "됐나/남았나"는 **먼저 grep**. 단일진실 문서는
   **append만**(덮어쓰기·삭제 금지, 정정도 옛 내용 보존+정정행 추가).
8. **커밋**: 항목당 1커밋. 본문 = **[무엇][왜][어떻게(파일/씬/프리팹)][검증][영향]** + `Unity:`/`OS:` trailer.
   **한국어**. **Co-Authored-By 절대 금지(단독 크레딧)**. 씬·프리팹·.asset은 diff가 안 보이니 *변경내용을 본문에 명시*.
9. **push 전 `git fetch`/`rebase`**(멀티머신). `origin`만 push.

## 🏗️ 아키텍처 (목표 구조)
```
ParkgolfUnity/                 (= 이 repo. Unity 설치 후 프로젝트 루트로 확장)
  Assets/ParkGolf/
    Core/      순수 C#(UnityEngine 비의존) — 코스생성·검증·규준. EditMode 대상   [asmdef ParkGolf.Core]
    Runtime/   MonoBehaviour 글루·메시/스플라인/Terrain 생성                    [asmdef ParkGolf.Runtime]
    Editor/    임포터(JSON·DEM)·베이크·인스펙터 버튼                            [asmdef ParkGolf.Editor]
    Tests/     EditMode/PlayMode 테스트                                       [asmdef ParkGolf.Tests]
    Samples/   샘플 코스 JSON
  docs/        작업_레지스트리 · 결정_로그 · ROADMAP · HANDOFF · 에셋_출처
  scripts/     check.ps1 (로컬==판정 게이트)
  .githooks/   commit-msg · pre-commit (core.hooksPath)
  파크골프_Unity맵_설계_2026-06-26.md   설계·소싱·검증된 API·규준 정본
```
> 현재(부트스트랩): Unity 프로젝트 미생성·Unity 미설치. `Assets/ParkGolf`는 드롭인 패키지 형태. 진행은 레지스트리·ROADMAP 참조.

## 🧰 기술 스택
Unity(**버전 미정** — 설치 후 확정·여기 기재) · C# · com.unity.splines · (RP 미정, URP 권장) · Git + Git LFS · Windows/PowerShell.

## ▶️ 빌드·테스트·실행 (Windows / PowerShell)
- 검증 게이트: `powershell -File scripts/check.ps1` (로컬 == 판정).
- EditMode(예): `Unity -batchmode -projectPath . -runTests -testPlatform EditMode -quit`
> Unity 미설치 시 check.ps1은 Unity 단계를 SKIPPED로 보고하고 컴파일/형식 검사만 수행.

## ✅ 검증 게이트
`scripts/check.ps1` 하나가 로컬 == 판정: ①컴파일 0 ②EditMode ③(가능 시)PlayMode 스모크. 실패 시 비0 종료(커밋 금지).
씬/프리팹 변경은 추가로 **에디터 수동 확인**(렌더·플레이) 후 결과를 커밋 본문에 기재.

## 📝 커밋 규약 · 작업 절차
- **작업 진행 형식**: 착수 전 **[문제 정의 → 해결방법 → 기대효과]** 명시. 항목 단위로 **하나씩 순차**, **완료마다 보고**.
  모호하면 코드 전에 질문(**한 번에 1개**).
- **D-D-R-R**: ①정의(무엇·왜·`file:line`·재현) → ②해결방법(대안·트레이드오프·영향범위; 위험 크면 분리·확인) →
  ③기록(레지스트리 착수 전 기재 + 빨간불 재현 테스트 선행) → ④해결(구현 → check GREEN → 빨간불을 회귀가드로 →
  레지스트리 닫고 커밋ID → fetch/rebase → push).
- **DoD**: 빨간불→초록 회귀가드 + check GREEN + 레지스트리 닫힘(커밋ID) + 5W1H 커밋.
- **과잉 엔지니어링 회피**: 오타·1줄 수정에 전체 의식 강제 금지. 규율은 갖추되 무게는 작업 크기에 맞게.

## 📌 단일진실 문서 갱신 규칙 (append만)
`작업_레지스트리.md`(결함·작업 행, 닫을 때 커밋ID) · `결정_로그.md`(ADR-lite 한 줄) · `ROADMAP.md`(다음 할 일) ·
`에셋_출처.md`(에셋 도입 시). **덮어쓰기·삭제 금지**, 정정도 옛 내용 보존+정정행.

## 🏌️ 도메인 컨텍스트 (파크골프 규준 — 출처/신뢰도)
- 9홀 = 파3×4 + 파4×4 + 파5×1 = **33타**(high). 18홀 66타.
- 홀 거리 **상한**: 파3 ≤60m / 파4 ≤100m / 파5 ≤150m(high). 하한은 시설 재량(휴리스틱).
- 페어웨이 폭 ≥2m·그린 지름 ≥5m(medium) · 홀컵 내경 200mm(high) · 공 60mm/~90g(high/medium) · 클럽 ≤86cm/≤600g, 1인 1클럽.
- 상세·출처·신뢰도 = `파크골프_Unity맵_설계_2026-06-26.md §4`. medium/low 값은 코드 주석에 "잠정" 표기.

## 🔍 적대적 검증 (마일스톤마다)
"이 코스가 실제로 플레이 가능한가 / 막힌 홀·도달 불가 지형 없나"를 **생성기 제작자와 다른 시각**으로 점검. 발견은 레지스트리로.

## 🚫 가져오지 않는 것 (CareLink 도메인 특화 — 무관)
멀티테넌트 · 금액 반올림 · 한국어 에러코드 · 소프트삭제 · DB 마이그레이션 · 서버 시크릿 가드.

## 📚 참조 문서
- 상태/다음: `docs/작업_레지스트리.md`(단일 진실) · `docs/ROADMAP.md`
- 설계·API·규준: `파크골프_Unity맵_설계_2026-06-26.md` · 패키지 `Assets/ParkGolf/README.md`
- 인계·결정: `docs/HANDOFF.md` · `docs/결정_로그.md` · 에셋: `docs/에셋_출처.md`
