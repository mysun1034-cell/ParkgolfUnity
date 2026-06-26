# 파크골프 — HANDOFF (세션 / 멀티머신 인계)

> 마지막 상태 + 다음 한 수. **append**(과거 인계 보존). 새 머신은 `git fetch`/`rebase` 후 작업.

## 2026-06-26
- **마지막 상태**: repo `github.com/mysun1034-cell/ParkgolfUnity`(main)에 — 설계문서 + `ParkGolf` 드롭인 패키지(T-001) + 자산위생 .gitignore/LFS(T-002) + 문서 세트 CLAUDE.md·docs/(T-003) 커밋·push 완료. **이 머신에 Unity 미설치** → 실제 컴파일·렌더 미검증.
- **다음 한 수**: §1 항목 3 = `ParkGolf.Core` asmdef 분리 — `ParkGolfStandards`(이미 UnityEngine 비의존) 등 순수로직을 Core로 옮겨 EditMode 단위테스트 토대 마련(T-004).
- **주의**:
  1. 새 머신: `git fetch`/`rebase` 후 작업, `origin`만 push.
  2. Unity 설치 시 **버전·RP를 CLAUDE.md §기술스택에 기입**.
  3. `core.hooksPath` 전환 시 **LFS 훅 우회 주의**(T-006) — `.githooks/`에 LFS 훅 호출 포함.
  4. 커밋 = **한국어 5W1H + `Unity:`/`OS:` trailer, Co-Authored-By 금지**.
  5. medium/low 신뢰도 규준 수치는 코드 주석에 "잠정" 표기(추측금지).
