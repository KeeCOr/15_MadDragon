# MadDragon 작업 규칙

## 빌드 및 실행파일 배치

지시사항 수행 완료 후 반드시 아래 순서로 실행한다.

### 빌드 방법

Unity 프로젝트. Unity Editor에서 빌드하거나 아래 CLI 명령 사용:

```bash
# Unity 설치 경로에 맞게 조정
"C:/Program Files/Unity/Hub/Editor/{버전}/Editor/Unity.exe" \
  -batchmode -quit \
  -projectPath "C:/Development/15_MD" \
  -buildWindows64Player "C:/Development/15_MD/release/MadDragon.exe"
```

Unity CLI 빌드가 어려운 경우 사용자에게 Unity Editor에서 직접 빌드 요청.

### 실행파일 배치
- 루트에 배치: `C:/Development/15_MD/MadDragon_v{버전}_portable.exe`

## 기획서 최신화

기능 추가/변경 후 반드시 업데이트:
- `docs/MadDragon_기획서.md`
- `docs/MadDragon_기획서.html`
