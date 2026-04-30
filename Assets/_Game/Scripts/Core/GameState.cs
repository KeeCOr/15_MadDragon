// Assets/_Game/Scripts/Core/GameState.cs
namespace MedievalRTS.Core
{
    public enum GameState { MainMenu, BaseBuilder, Battle, Result }

    public readonly struct GameStateChangedEvent
    {
        public readonly GameState State;
        public GameStateChangedEvent(GameState state) => State = state;
    }

    public readonly struct GoldChangedEvent
    {
        public readonly int Amount;
        public GoldChangedEvent(int amount) => Amount = amount;
    }

    public readonly struct BattleEndedEvent
    {
        public readonly bool Victory;
        public readonly int Stars;
        public readonly BattleStats Stats;
        public BattleEndedEvent(bool victory, int stars, BattleStats stats)
        {
            Victory = victory;
            Stars = stars;
            Stats = stats;
        }
    }

    public struct BattleStats
    {
        public bool Victory;
        public int UnitsLost;
        public float TimeElapsed;
    }

    public readonly struct UnitDiedEvent
    {
        public readonly Units.Unit Unit;
        public UnitDiedEvent(Units.Unit unit) => Unit = unit;
    }

    public readonly struct BuildingDestroyedEvent
    {
        public readonly Buildings.Building Building;
        public BuildingDestroyedEvent(Buildings.Building building) => Building = building;
    }
}
