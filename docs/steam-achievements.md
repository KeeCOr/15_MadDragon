# MadDragon — Steam Achievements

---

## Stats

| API Name | Type | Description |
|----------|------|-------------|
| `STAT_WAVES_CLEARED` | INT | Total waves cleared |
| `STAT_RUNS_COMPLETED` | INT | Total sessions completed |
| `STAT_UNITS_DEPLOYED` | INT | Total units deployed |
| `STAT_SPELLS_CAST` | INT | Total spells cast |
| `STAT_ENEMIES_DEFEATED` | INT | Total enemies defeated |
| `STAT_PERFECT_WAVES` | INT | Waves cleared with no base damage |

---

## Achievements

| API Name | EN Name | KO Name | How to Unlock |
|----------|---------|---------|---------------|
| `ACH_FIRST_BATTLE` | First Defense | 첫 방어 | Survive your first wave |
| `ACH_WAVE_3` | Holdout | 수성 | Survive 3 waves in a session |
| `ACH_WAVE_5` | Siege Breaker | 공방 돌파 | Survive all 5 waves |
| `ACH_FIRST_SPELL` | Arcane Support | 마법 지원 | Cast your first spell |
| `ACH_KNIGHT_DEPLOY` | Iron Wall | 철의 벽 | Deploy knights as your frontline and survive the wave |
| `ACH_ARCHER_SNIPE` | Long Range Command | 원거리 지휘 | Win a wave using archer-majority lineup |
| `ACH_MAGE_POWER` | Arcane Barrage | 마법 포격 | Win a wave using mage-majority lineup |
| `ACH_NO_LOSS` | Perfect Defense | 완벽한 방어 | Survive a wave without any base damage |
| `ACH_PERFECT_5` | Fortress Commander | 요새 지휘관 | Win all 5 waves without any base damage |
| `ACH_SPELL_MASTER` | Spellweaver | 마법사령관 | Cast 50 spells total across all sessions |
| `ACH_SURVEY_ACE` | Scout's Eye | 정찰병의 눈 | Correctly read incoming threat composition before 3 consecutive waves |
| `ACH_ADAPT_WIN` | Adaptive Tactics | 적응 전술 | Win after changing your formation mid-session |
| `ACH_HUNDRED_ENEMIES` | Dragon's Bane | 드래곤의 천적 | Defeat 100 enemies total |
| `ACH_VETERAN` | Seasoned Commander | 노련한 지휘관 | Complete 20 sessions |

---

## Implementation Notes

- Steam API: `ISteamUserStats`
- `ACH_NO_LOSS` requires tracking base HP delta per wave
- `ACH_PERFECT_5` requires all 5 waves having zero base damage in one session
- `ACH_SURVEY_ACE` requires a correct-prediction counter (compare player's pre-wave guess against actual incoming composition)
- `ACH_ADAPT_WIN` requires detecting a formation change event during an active session
- All achievements unlockable in offline single-player
- Replace App ID 480 with real Steamworks App ID before submission
