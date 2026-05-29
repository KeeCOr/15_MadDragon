# MadDragon 이미지/UI 리소스 목록

**생성일:** 2026-05-28
**방향:** 밝은 Clash-like 모바일 RTS, 파랑/금색 아군과 빨강/금색 적 진영, 따뜻한 석재와 장난감 같은 3D 실루엣

## 우선 적용 리소스

| 구분 | 파일 | 용도 | 비고 |
| --- | --- | --- | --- |
| 건물/장식 아틀라스 | `Assets/_Game/Art/Generated/Transparent/maddragon_building_atlas_transparent.png` | 성, 타워, 성벽, 병영, 마법탑, 금광, 엘릭서 저장소, 공방, 나무/바위 장식 | 개별 오브젝트로 크롭해 월드 프리팹/아이콘에 사용 |
| 유닛 카드 아틀라스 | `Assets/_Game/Art/Generated/Transparent/maddragon_unit_card_atlas_transparent.png` | 기사, 궁수, 마법사, 기병, 공성 기술자, 힐러, 드래곤 카드 이미지 | 하단 병력 카드와 해금/상점 카드에 사용 |
| 자원/마법/액션 아이콘 | `Assets/_Game/Art/Generated/Transparent/maddragon_icon_atlas_transparent.png` | 골드, 엘릭서, 트로피, 전투, 건설, 배치, 강화, 정보, 화염구, 번개, 치유, 빙결, 분노, 방어, 설정 | HUD, 버튼, 자원바, 마법 슬롯에 사용 |
| UI 키트 아틀라스 | `Assets/_Game/Art/Generated/Transparent/maddragon_ui_kit_atlas_transparent.png` | 상단 자원바, 버튼, 카드 프레임, 주문 슬롯, 원형 프레임, 배지, 탭바, 미니맵 프레임 | UGUI Image/Sliced Sprite 후보 |

## 원본 생성본

크로마키 배경이 남아 있는 원본은 같은 폴더에 보관한다.

- `Assets/_Game/Art/Generated/maddragon_building_atlas.png`
- `Assets/_Game/Art/Generated/maddragon_unit_card_atlas.png`
- `Assets/_Game/Art/Generated/maddragon_icon_atlas.png`
- `Assets/_Game/Art/Generated/maddragon_ui_kit_atlas.png`

## 다음 분할 권장안

1. 완료: 건물 아틀라스에서 성/타워/성벽/마법탑/자원 건물을 개별 PNG로 크롭해 `Assets/Resources/MadDragonArt/Buildings`에 배치했다.
2. 완료: 유닛 카드 아틀라스는 7개 카드 이미지로 분리해 `Assets/Resources/MadDragonArt/Units`에 배치했고, 병력 구매 버튼에 썸네일로 연결했다.
3. 완료: 아이콘 아틀라스는 자원/마법/액션 아이콘으로 분리해 `Assets/Resources/MadDragonArt/Icons`에 배치했고, 마법 구매 버튼에 연결했다.
4. 진행 후보: UI 키트는 `Assets/Resources/MadDragonArt/Ui`에 분리해 두었고, 다음 단계에서 버튼/패널 프레임을 9-slice Sprite로 설정해 모바일 HUD 전체에 적용한다.

## 적용 상태

- `GeneratedArtLibrary`가 `Resources/MadDragonArt` 아래 PNG를 런타임 Sprite로 로드한다.
- `GeneratedArtBillboard`가 월드에 붙은 건물 스프라이트를 카메라 방향으로 회전시킨다.
- `TestBootstrap`의 성, 타워, 성벽, 병영, 마법탑, 자원 건물은 기존 primitive 콜라이더/AI를 유지하고, 렌더링만 생성 이미지 파사드로 보강한다.
- 공격 준비 화면의 병력 구매 버튼과 마법 구매 버튼에는 생성 이미지 썸네일을 표시한다.

## 생성 프롬프트 요약

- 건물: 밝은 Clash-like 3D 이소메트릭 건물 아틀라스, 파랑/빨강 진영성, 성벽, 타워, 병영, 마법탑, 금광, 엘릭서, 공방, 자연 장식.
- 유닛: 기사, 궁수, 마법사, 기병, 공성 기술자, 힐러, 파란 드래곤의 모바일 카드용 캐릭터 아트.
- 아이콘: 자원, 전투/건설/배치/강화/정보, 마법, 방어, 설정 아이콘.
- UI 키트: 석재/금속 프레임, 파랑/빨강 버튼, 카드/슬롯/탭/미니맵 프레임.
