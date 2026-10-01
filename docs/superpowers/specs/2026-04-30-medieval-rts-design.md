# Medieval RTS — Design Spec
**Date:** 2026-04-30
**Reference:** Mad Rocket: Fog of War (full clone, medieval fantasy reskin)
**Platform:** Android + WebGL (iOS 추후)
**Engine:** Unity 6 LTS, URP

---

## 1. 개요

Mad Rocket: Fog of War의 게임 메커니즘을 그대로 재현하되, 우주/SF 테마를 중세 판타지로 교체한다. 싱글플레이어 캠페인 MVP를 먼저 완성하고, 이후 멀티플레이어를 추가한다.

---

## 2. 전체 아키텍처

### 프로젝트 디렉토리 구조

```
15_MD/                          ← Unity 프로젝트 루트
├── Assets/
│   ├── _Game/
│   │   ├── Scenes/
│   │   │   ├── MainMenu
│   │   │   ├── Battle
│   │   │   └── BaseBuilder
│   │   ├── Scripts/
│   │   │   ├── Core/
│   │   │   ├── Units/
│   │   │   ├── Base/
│   │   │   ├── FogOfWar/
│   │   │   └── UI/
│   │   └── ScriptableObjects/
│   └── [AssetPacks]/
├── Packages/
└── ProjectSettings/
```

### 씬 흐름

```
MainMenu → BaseBuilder → Battle → Result → (MainMenu 또는 다음 스테이지)
```

---

## 3. 핵심 게임 메커니즘

### 전투 루프

1. **안개 상태** — 전투 시작 시 적 기지 전체가 Fog of War로 가려짐
2. **정찰** — Scout 유닛을 먼저 투입해 건물 위치 파악
3. **유닛 배치** — 화면 하단 배치 존에서 유닛 드래그&드롭으로 전장 투입
4. **자동 전투** — 투입된 유닛은 AI로 자동 이동/공격; 플레이어는 추가 유닛 투입만
5. **승패 판정** — 적 Castle 파괴 시 승리 / 시간 초과 또는 전멸 시 패배

### BaseBuilder (방어 페이즈)

- 그리드 기반 건물 자유 배치
- 배치 가능 건물: 방어탑, 성벽, 자원 건물
- 저장된 레이아웃이 상대 공격 시 그대로 사용됨

### Fog of War

- 적 기지 전체를 초기에 안개로 가림
- 유닛 이동 시 주변 시야 반경만큼 안개 걷힘
- Scout 유닛은 일반 유닛보다 넓은 시야 반경 보유
- 구현: 에셋스토어 Top-down Fog of War 시스템 우선 사용

### 자원 시스템

- **골드**: 유닛 소환 비용, 전투 중 시간에 따라 자동 충전
- **마나**: 스킬 사용 비용 (2차 MVP에 추가)

---

## 4. 유닛 구성

| 유닛 | 역할 | Mad Rocket 대응 | 특징 |
|------|------|----------------|------|
| Knight (기사) | 탱커, 근접 | Heavy Trooper | 고체력, 느림 |
| Archer (궁수) | 원거리 딜러 | Sniper | 낮은 체력, 건물 특효 없음 |
| Catapult (투석기) | 광역, 건물 특효 | Rocket Launcher | 느리지만 건물 파괴 특화 |
| Scout (정찰기병) | 정찰 특화 | Drone | 빠름, 낮은 전투력, 넓은 시야 |
| Mage (마법사) | 광역 스킬 | Bomb unit | 범위 피해, 높은 비용 |

### UnitData ScriptableObject 필드

```
int hp
int damage
float speed
int goldCost
float attackRange
float sightRange
UnitType type
Sprite icon
GameObject prefab
```

---

## 5. 건물 구성

| 건물 | 역할 |
|------|------|
| Castle | 본부, 파괴 시 패배 |
| Archer Tower | 자동 원거리 방어 |
| Cannon Tower | 느리지만 강력한 방어 |
| Wall | 경로 차단용 성벽 |
| Barracks | AI 유닛 생성 (싱글플레이 AI용) |
| Gold Mine | 골드 생산 건물 |

### BuildingData ScriptableObject 필드

```
int hp
Vector2Int gridSize
int goldCost
bool isDefenseTower
float attackRange (방어탑만)
int attackDamage (방어탑만)
GameObject prefab
```

---

## 6. 싱글플레이어 AI & 캠페인

### 캠페인 구조

- 월드맵에서 스테이지 선택 (MVP: 10~15스테이지)
- AI 기지: 디자이너가 미리 배치한 고정 레이아웃 (ScriptableObject로 저장)
- AI 공격: 일정 주기 웨이브로 유닛 투입, 난이도별 조합 변화

### 난이도 스케일

```
스테이지 1-3:   느린 웨이브, 기본 유닛(Knight, Archer)만
스테이지 4-7:   복합 유닛, 방어탑 추가
스테이지 8-10:  타이트한 시간제한, 강화 유닛 포함
```

### 별점 시스템

- 스테이지 클리어 시 별 1~3개 획득 (Mad Rocket 동일)
- 3개 기준: 클리어, 유닛 손실 적음, 제한 시간 내 클리어
- 획득한 별로 새 유닛 언락 또는 업그레이드 포인트 획득

### 유닛 업그레이드

- 유닛별 레벨 1~3
- 레벨업 시 UnitData 수치 비례 스케일 (레벨 × 배율)

---

## 7. 세이브 시스템

- 저장 방식: JSON 파일 (로컬, `Application.persistentDataPath`)
- 저장 항목:
  - 스테이지 진행도 및 별점
  - 유닛 레벨
  - 기지 레이아웃

---

## 8. 기술 스택

| 항목 | 선택 |
|------|------|
| Unity 버전 | Unity 6 LTS |
| Render Pipeline | URP |
| Input System | New Input System (터치 + 마우스) |
| 빌드 타겟 | Android, WebGL |

### 추천 에셋 카테고리

| 카테고리 | 용도 |
|---------|------|
| Fantasy Kingdom / Medieval Village 팩 | 건물, 성벽, 타워 |
| Fantasy Characters 팩 | 유닛 모델 + 애니메이션 |
| Top-down Fog of War 시스템 | 안개 쉐이더 |
| Terrain & Tilemap 팩 | 전장 배경 |
| Fantasy UI Pack | HUD, 버튼, 아이콘 |

---

## 9. 코드 아키텍처

- `GameManager` (싱글턴): 게임 상태(전투 시작/종료/일시정지) 관리
- `EventBus`: 유닛/건물 간 이벤트 디커플링 (UnityEvent 또는 C# Action)
- `ScriptableObject`: 모든 밸런스 데이터 외부화 (코드 수정 없이 밸런싱 가능)
- `GridSystem`: BaseBuilder용 그리드 배치 로직

---

## 10. MVP 개발 로드맵

```
1주차: Unity 프로젝트 세팅 + 에셋 임포트 + 기본 씬 구성
2주차: GridSystem + BaseBuilder 구현
3주차: 유닛 AI (이동, 타겟팅, 공격) + 전투 시스템
4주차: Fog of War 통합
5주차: 캠페인 스테이지 10개 제작
6주차: UI + 프로그레션 (별점, 업그레이드) 시스템
7주차: Android 빌드 + 폴리싱 + 버그픽스
```

---

## 11. 범위 밖 (MVP 이후)

- 멀티플레이어 (온라인 PvP)
- iOS 빌드
- 마나 / 스킬 시스템
- 클랜/소셜 기능
- 인앱 결제
