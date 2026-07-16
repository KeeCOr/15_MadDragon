# MadDragon 업데이트 내역서

## 2026-07-03 / v0.6.2 Result Panel Raid Feedback

- 프로토타입 결과 패널이 전투 승패만 보여주던 상태에서, 레이드 손실 요약과 다음 행동 힌트를 함께 보여주도록 연결했다.
- `RaidResultPanelText`를 추가해 결과 화면 제목/본문 문구를 Unity UI와 분리했다.
- `TestBootstrap.EndGame`은 `BuildCurrentRaidForecast()` -> `RaidResultSummaryModel.Create()` -> `RaidResultPanelText.Create()` 흐름으로 결과 패널 텍스트를 구성한다.
- 결과 본문에는 파괴 건물 수, 획득 골드, 획득 무공, 저장 자원 손실, 지갑 자원 손실, 총 손실, 다음 방어 행동 힌트가 포함된다.
- `RaidResultPanelTextTests`를 추가해 승리 제목, 패배 손실 요약, 음수 보상 보정/요약 누락 폴백을 검증했다.
- 프로젝트 버전을 `0.6.2`로 올리고 Windows portable 산출물을 갱신했다.

## 2026-07-03 / Raid Summary Independent Logic Check

- Unity Test Runner XML 생성을 여러 차례 재시도했지만 XML 파일은 생성되지 않았다.
- Unity batchmode는 스크립트 컴파일과 Windows 빌드를 return code 0으로 종료했다.
- 대체 검증으로 `_temp/raid_summary_logic_check_20260703`의 독립 .NET 콘솔 프로젝트가 실제 소스 파일을 링크해 4개 시나리오를 통과했다.
- 확인 출력: `RaidResultSummaryModel independent C# check passed: 4 scenarios`.

## 2026-07-02 / Raid Result Summary Model

- `RaidResultSummaryModel`을 추가해 레이드 결과를 플레이어가 읽기 쉬운 문구 구조로 변환했다.
- `RaidResultSummaryModelTests`로 방어 성공, 좁은 침투, 본진 파괴 케이스의 손실/힌트 문구를 고정했다.

## 2026-07-02 / Raid Defense Readability Recheck

- 기존 구현을 확인했다: `RaidLossCalculator`, `RaidForecast`, `ResourceStorageSystem`, `CampaignHubScreen`, `BaseManagementScreen`, `TestBootstrap.BuildCurrentRaidForecast`.
- 남은 구현 과제는 계산 자체가 아니라 결과/위험 문구를 실제 UI에 명확히 연결하는 것이었다.

## 2026-06-28 / v0.6.1 Representative Resource Refresh

- `Assets/Resources/MadDragonArt/Buildings/player_castle.png`를 같은 경로에서 교체했다.
- `GeneratedArtLibrary` 경로와 Unity `.meta`를 유지해 기존 프리팹 참조 안정성을 보존했다.
- 원본 백업: `_temp/asset_backups/2026-06-28_player_castle/player_castle.original.png`.