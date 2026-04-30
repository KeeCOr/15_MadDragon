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

    // TODO: Replace object with Units.Unit once Units.Unit is implemented (Task N)
    public readonly struct UnitDiedEvent
    {
        public readonly object Unit;
        public UnitDiedEvent(object unit) => Unit = unit;
    }

    // TODO: Replace object with Buildings.Building once Buildings.Building is implemented (Task N)
    public readonly struct BuildingDestroyedEvent
    {
        public readonly object Building;
        public BuildingDestroyedEvent(object building) => Building = building;
    }
}
