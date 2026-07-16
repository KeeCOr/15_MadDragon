# MadDragon 다음 개선 지시서

## 현재 상태

- v0.6.2에서 결과 패널이 레이드 손실 요약과 다음 행동 힌트를 표시한다.
- `RaidResultPanelText`, `RaidResultPanelTextTests`가 추가됐다.
- Windows portable 실행파일은 `MadDragon_v0.6.2_portable.exe`로 갱신됐다.

## 검증 근거

- 독립 C# 검증: `_temp/raid_summary_logic_check_20260703` 실행 결과 4개 시나리오 통과.
- Unity batchmode 컴파일: `Logs/EditModeTests_result_panel_final_20260703.log`, `Logs/EditModeTests_result_panel_second_final_20260703.log` 모두 return code 0.
- Unity Windows 빌드: `Logs/BuildWindows_v062_20260703.log` return code 0.

## 남은 개선 후보

1. `TestBootstrap.cs`의 오래된 한글 깨짐 문자열을 안전하게 정리한다.
2. Unity Test Runner XML이 생성되지 않는 원인을 분리한다.
3. 결과 패널 텍스트를 모바일 UI 스타일에 맞춰 2열 정보 카드 또는 아이콘 라벨로 정돈한다.

## 주의

- `TestBootstrap.cs`는 혼합 인코딩 흔적이 있어 PowerShell `Set-Content`로 전체 저장하면 파일이 손상될 수 있다.
- 해당 파일 수정은 바이트 기반 치환 또는 Unity/IDE 저장 인코딩 확인 후 진행한다.