# MadDragon 다음 개선 지시서

## 현재 상태

- 2026-09-21 소스에서 전투 상단 HUD가 시간·목표 기지 HP·보상 세 영역으로 분리됐다.
- 순수 표시 모델의 독립 스모크는 11/11 통과했지만 Unity EditMode/씬/빌드는 미검증이다.
- 목표 예시 화면은 `docs/design-references/2026-09-21-battle-hud-three-zones.png`에 있다.
- v0.6.2에서 결과 패널이 레이드 손실 요약과 다음 행동 힌트를 표시한다.
- `RaidResultPanelText`, `RaidResultPanelTextTests`가 추가됐다.
- Windows portable 실행파일은 `MadDragon_v0.6.2_portable.exe`로 갱신됐다.

## 검증 근거

- 독립 C# 검증: `_temp/raid_summary_logic_check_20260703` 실행 결과 4개 시나리오 통과.
- Unity batchmode 컴파일: `Logs/EditModeTests_result_panel_final_20260703.log`, `Logs/EditModeTests_result_panel_second_final_20260703.log` 모두 return code 0.
- Unity Windows 빌드: `Logs/BuildWindows_v062_20260703.log` return code 0.

## 남은 개선 후보

1. Unity가 다시 사용 가능할 때 16:9·18:9·20:9에서 세 영역 겹침과 색상 대비를 먼저 확인한다.
2. 이미 존재하는 `BattlePriorityModel.Build`의 위험·자원·즉시 행동 정보를 실제 HUD 또는 지휘 패널에 연결한다.
3. `TestBootstrap.cs`의 오래된 한글 깨짐 문자열은 인코딩을 보존하는 방식으로만 정리한다.

## 주의

- `TestBootstrap.cs`는 혼합 인코딩 흔적이 있어 PowerShell `Set-Content`로 전체 저장하면 파일이 손상될 수 있다.
- 해당 파일 수정은 바이트 기반 치환 또는 Unity/IDE 저장 인코딩 확인 후 진행한다.

## 2026-09-18 프로젝트별 고유 개선 3개
> Unity 제외 조건: 이번 반영은 설계·씬·검증 명세이며 런타임 구현 완료를 뜻하지 않는다.

1. 모바일 HUD를 위험·자원·즉시 행동 세 영역으로 축소
2. 드래곤 성장 선택의 전투 변화량 미리보기
3. 첫 보스까지의 목표와 실패 후 재도전 보너스 명확화
