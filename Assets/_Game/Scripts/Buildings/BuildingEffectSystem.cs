// Assets/_Game/Scripts/Buildings/BuildingEffectSystem.cs
// 특수 건물 레벨과 효과를 관리하는 정적 시스템
using System.Collections.Generic;
using UnityEngine;

namespace MedievalRTS.Buildings
{
    /// <summary>특수 건물 종류</summary>
    public enum SpecialBuildingType
    {
        Blacksmith,       // 대장간: 유닛 비용 감소
        Tavern,           // 여인숙: 새 유닛 해금
        TrainingGround,   // 훈련소: 유닛 스탯 강화
        Fortress,         // 요새: 건물 내구도 증가
        Treasury,         // 금고: 건물 파괴 시 자원 손실 감소 (생존 시)
        Workshop,         // 공방: 잠긴 건물 해금
    }

    public static class BuildingEffectSystem
    {
        public const int MaxLevel = 3;

        // 건물 명칭 (UI 표시용)
        public static readonly string[] Names =
        {
            "대장간", "여인숙", "훈련소", "요새", "금고", "공방"
        };

        // 건물 간략 효과 설명 (레벨 무관)
        public static readonly string[] EffectDescs =
        {
            "유닛 비용 -10/20/30%",
            "새 유닛 해금",
            "유닛 데미지·HP +10/25/45%",
            "아군 건물 HP +20/40/70%",
            "자원 손실 -30/50/70%",
            "잠긴 건물 해금",
        };

        // 건물별 레벨당 비용 [건물][레벨 0→1, 1→2, 2→3]
        private static readonly int[][] _costs =
        {
            new[] { 100, 200, 350 },  // Blacksmith
            new[] { 150, 300, 500 },  // Tavern
            new[] { 150, 300, 500 },  // TrainingGround
            new[] { 200, 400, 700 },  // Fortress
            new[] { 150, 300, 500 },  // Treasury
            new[] { 200, 400, 700 },  // Workshop
        };

        private static readonly int[] _levels = new int[6];

        // Treasury 생존 여부 (외부에서 갱신)
        public static bool TreasuryAlive { get; set; } = false;

        // ── 레벨 조회/업그레이드 ──────────────────────────────────────
        public static int GetLevel(SpecialBuildingType type) => _levels[(int)type];

        public static bool CanUpgrade(SpecialBuildingType type)
            => _levels[(int)type] < MaxLevel;

        public static int GetUpgradeCost(SpecialBuildingType type)
        {
            int lv = _levels[(int)type];
            return lv < MaxLevel ? _costs[(int)type][lv] : int.MaxValue;
        }

        public static void Upgrade(SpecialBuildingType type)
        {
            int i = (int)type;
            if (_levels[i] < MaxLevel) _levels[i]++;
        }

        public static void Reset()
        {
            for (int i = 0; i < _levels.Length; i++) _levels[i] = 0;
            TreasuryAlive = false;
        }

        // ── 공방: 특수 건물 해금 ──────────────────────────────────────
        // Blacksmith / Tavern / Workshop: 항상 개방
        // TrainingGround / Treasury: Workshop Lv1+
        // Fortress: Workshop Lv2+
        public static bool IsBuildingUnlocked(SpecialBuildingType type)
        {
            int ws = _levels[(int)SpecialBuildingType.Workshop];
            return type switch
            {
                SpecialBuildingType.Blacksmith     => true,
                SpecialBuildingType.Tavern         => true,
                SpecialBuildingType.Workshop       => true,
                SpecialBuildingType.TrainingGround => ws >= 1,
                SpecialBuildingType.Treasury       => ws >= 1,
                SpecialBuildingType.Fortress       => ws >= 2,
                _                                  => true,
            };
        }

        // ── 여인숙: 유닛 해금 (unitDefIndex 기준) ──────────────────────
        // 0~3: 기본(기사/궁수/마법사/정찰병)   항상 가능
        // 4: 기병  → Tavern Lv1+
        // 5: 공성기 → Tavern Lv2+
        public static bool IsUnitUnlocked(int unitDefIndex)
        {
            int lv = _levels[(int)SpecialBuildingType.Tavern];
            if (unitDefIndex < 4) return true;
            if (unitDefIndex == 4) return lv >= 1;
            if (unitDefIndex == 5) return lv >= 2;
            return false;
        }

        // ── 대장간: 유닛 비용 배율 ───────────────────────────────────
        public static float GetCostMultiplier()
            => 1f - _levels[(int)SpecialBuildingType.Blacksmith] * 0.10f;

        // ── 훈련소: 유닛 스탯 배율 ───────────────────────────────────
        public static float GetUnitStatMultiplier()
        {
            return _levels[(int)SpecialBuildingType.TrainingGround] switch
            {
                0 => 1.00f, 1 => 1.10f, 2 => 1.25f, _ => 1.45f,
            };
        }

        // ── 요새: 건물 HP 배율 ──────────────────────────────────────
        public static float GetBuildingHPMultiplier()
        {
            return _levels[(int)SpecialBuildingType.Fortress] switch
            {
                0 => 1.00f, 1 => 1.20f, 2 => 1.40f, _ => 1.70f,
            };
        }

        // ── 금고: 자원 손실 배율 (Treasury가 살아있을 때만 적용) ───────
        // 높을수록 손실 감소 (1.0 = 손실 없음 감소, 0.3 = 70% 감소)
        public static float GetResourceLossMultiplier()
        {
            if (!TreasuryAlive) return 1f;
            return _levels[(int)SpecialBuildingType.Treasury] switch
            {
                0 => 1.00f, 1 => 0.70f, 2 => 0.50f, _ => 0.30f,
            };
        }
    }
}
