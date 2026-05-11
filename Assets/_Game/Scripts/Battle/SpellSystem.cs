// Assets/_Game/Scripts/Battle/SpellSystem.cs
// 마법 구매 및 시전 관리 정적 시스템
using UnityEngine;

namespace MedievalRTS.Battle
{
    public enum SpellType { Fireball, Lightning, Heal, Freeze, Rage }

    public static class SpellSystem
    {
        public struct SpellDef
        {
            public string name, desc;
            public int    goldCost, maxCharges;
            public Color  uiColor;
        }

        public static readonly SpellDef[] Defs = new SpellDef[]
        {
            new SpellDef { name="화염구", desc="범위 3m 폭발 120 데미지",  goldCost=0, maxCharges=1, uiColor=new Color(1f,0.35f,0.05f)  },
            new SpellDef { name="번개",   desc="가장 가까운 적 200 데미지", goldCost=0, maxCharges=1, uiColor=new Color(0.5f,0.6f,1f)    },
            new SpellDef { name="치유",   desc="가장 가까운 아군 HP +300",  goldCost=0, maxCharges=1, uiColor=new Color(0.2f,0.85f,0.4f) },
            new SpellDef { name="빙결",   desc="전 적 이동속도 -50%  5초", goldCost=0, maxCharges=1, uiColor=new Color(0.3f,0.8f,1f)    },
            new SpellDef { name="분노",   desc="전 아군 공격력 +50%  8초", goldCost=0, maxCharges=1, uiColor=new Color(1f,0.2f,0.2f)    },
        };

        private static readonly int[] _charges = new int[5];

        public static int  GetCharges(SpellType t) => _charges[(int)t];
        public static bool HasCharge(SpellType t)  => _charges[(int)t] > 0;
        public static bool CanBuyMore(SpellType t) => _charges[(int)t] < Defs[(int)t].maxCharges;
        public static int  BuyCost(SpellType t)    => Defs[(int)t].goldCost;

        /// <summary>골드를 소모해 차지를 1 증가. 성공 시 true.</summary>
        public static bool TryBuy(SpellType t, ref int gold)
        {
            int i = (int)t;
            if (_charges[i] >= Defs[i].maxCharges || gold < Defs[i].goldCost) return false;
            gold -= Defs[i].goldCost;
            _charges[i]++;
            return true;
        }

        /// <summary>차지를 1 소모. 차지가 있을 때만 true 반환.</summary>
        public static bool UseCharge(SpellType t)
        {
            int i = (int)t;
            if (_charges[i] <= 0) return false;
            _charges[i]--;
            return true;
        }

        public static void Reset()
        {
            for (int i = 0; i < _charges.Length; i++) _charges[i] = 0;
        }
    }
}
