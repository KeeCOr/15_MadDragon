# Toony RTS Visual Integration Design

## Goal

MadDragon의 현재 테스트 전투 루프를 유지하면서 `ToonyTinyPeople/TT_RTS/TT_RTS_Standard` 에셋을 유닛, 건물, 전장 장식, 전투 FX에 최대한 적용한다.

## Approach

기존 게임 로직, 콜라이더, HP, AI, 입력 흐름은 보존한다. Toony RTS 프리팹과 FBX는 런타임에서 시각 자식 오브젝트로 부착하고, 원래 primitive 렌더러는 숨긴다. 에셋을 찾지 못하면 기존 primitive/생성 이미지 비주얼이 유지되도록 fallback한다.

## Asset Mapping

- Knight: `TT_Swordman` 또는 `TT_HeavySwordman`
- Archer: `TT_Archer`
- Mage: `TT_Mage`
- Scout: `TT_Scout` 또는 `TT_Light_Infantry`
- Cavalry: `TT_Light_Cavalry`
- Catapult: `machines/TT_Catapult_lvl1`
- Player/Enemy castle: `models/buildings/TownHall.FBX`, `Keep.FBX`
- Wall: `models/buildings/Wall_A_wall.FBX`, `Wall_B_wall.FBX`
- Tower: `models/buildings/Tower_A.FBX`, `Tower_B.FBX`, `Tower_C.FBX`
- Barracks/production: `Stables.FBX`, `Workshop.FBX`, `Market.FBX`
- Mage tower/shrine: `MageTower.FBX`, `Temple.FBX`
- Resource buildings: `Granary.FBX`, `Farm.FBX`, `LumberMill.FBX`
- Decorations: banner prefabs, small building props, existing SimpleNaturePack foliage
- FX: `FX_Building_burning`, `FX_Building_Destroyed_mid`, `FX_machine_destroyed`, `FX_Blood`

## Implementation Boundaries

- Add a `ToonyRtsVisualLibrary` that centralizes asset paths and editor/runtime-safe loading.
- Add a `ToonyRtsVisualApplier` that attaches visual children, strips unwanted gameplay components, disables colliders on visual-only children, and applies consistent scale/offset.
- Modify `TestBootstrap` only at creation points: after unit/building primitive or prefab creation, call the applier.
- Add EditMode tests for key Toony asset availability and fallback behavior.

## Testing

- EditMode tests must verify at least one unit prefab, one machine prefab, one building model, one banner, and one FX prefab are loadable in the editor.
- Full EditMode suite must pass.
- Windows build must complete and the portable root copy must be refreshed.

## Risks

- Toony prefabs may include colliders, animators, or scripts that should not control gameplay. Visual children must be sanitized.
- Some FBX building imports may have unusual local scale or pivot. Offsets and scales must be per asset key, not guessed globally.
- Runtime `Resources.Load` will not load arbitrary AssetDatabase paths in player builds. The first implementation should use editor-assigned or build-included references generated through a Resources manifest, or keep AssetDatabase-only tests separate from runtime logic. For the current prototype, direct editor asset loading is acceptable only in editor helpers; player runtime needs fallback-safe paths.
