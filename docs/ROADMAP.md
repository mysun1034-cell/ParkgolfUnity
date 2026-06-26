# 파크골프 — ROADMAP (다음 할 일 인덱스)

> 신규·열린 작업 행 추가(**append**). 닫힌 항목은 `docs/작업_레지스트리.md`에만. 현재 상태 단일진실 = 레지스트리.

## A. 작업 규율 이식 (§1) — 진행 중
- [x] 1. 자산 위생 (.gitignore + LFS) — T-002 ✅ `a1edb39`
- [x] 2. 문서 세트 (CLAUDE.md + docs/) — T-003 ✅
- [ ] 3. `ParkGolf.Core` asmdef 분리 (순수로직 → EditMode 테스트 가능화) — T-004
- [ ] 4. `scripts/check.ps1` (Unity 감지·미설치 SKIPPED 정직) — T-005
- [ ] 5. git 훅 (commit-msg: Co-Authored-By 차단+trailer / pre-commit: check) — T-006
- [ ] 6. CI — 보류(Unity 미설치·비용 미정) — T-007

## B. Unity 실환경 (설치 후)
- [ ] Unity 설치·프로젝트 생성(버전·RP 확정) → `Assets/ParkGolf` 드롭인 → 실제 컴파일·렌더 검증 — T-008
- [ ] 샘플 코스 JSON 임포트 → `CourseBuilder` Build → 9홀 렌더 확인
- [ ] DEM `.raw` → Terrain 임포트(한강변 코스 1개) 실측 검증
- [ ] 코스 생성 **결정성·불변식** EditMode 테스트 + **골든마스터** 스냅샷
- [ ] **성능 예산** 수치 확정(목표 기기 기준) → CLAUDE.md §성능 채우기

## C. 게임플레이 (후속)
- [ ] 공 물리·타격·홀인 판정(컵 내경 0.2m) · 스코어(파 기준 33타)
- [ ] 그린/벙커/해저드 메시·머티리얼 · 프롭 산포(Kenney/Quaternius CC0)
- [ ] 적대적 검증: 코스 플레이 가능성(막힌 홀·도달불가 지형) 점검
