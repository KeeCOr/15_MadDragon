using NUnit.Framework;
using MedievalRTS.Campaign;
using MedievalRTS.Core;
using MedievalRTS.Data;
using UnityEngine;

public class StarRatingTests
{
    private StageData _stage;

    [SetUp]
    public void SetUp()
    {
        _stage = ScriptableObject.CreateInstance<StageData>();
        _stage.battleDuration = 120f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_stage);
    }

    [Test]
    public void Calculate_Defeat_Returns0()
    {
        var stats = new BattleStats { Victory = false };
        Assert.AreEqual(0, StarRatingSystem.Calculate(stats, _stage));
    }

    [Test]
    public void Calculate_Victory_WithLosses_SlowClear_Returns1()
    {
        var stats = new BattleStats { Victory = true, UnitsLost = 3, TimeElapsed = 110f };
        Assert.AreEqual(1, StarRatingSystem.Calculate(stats, _stage));
    }

    [Test]
    public void Calculate_Victory_NoLosses_SlowClear_Returns2()
    {
        var stats = new BattleStats { Victory = true, UnitsLost = 0, TimeElapsed = 110f };
        Assert.AreEqual(2, StarRatingSystem.Calculate(stats, _stage));
    }

    [Test]
    public void Calculate_Victory_NoLosses_FastClear_Returns3()
    {
        // 120 * 0.7 = 84초 이내
        var stats = new BattleStats { Victory = true, UnitsLost = 0, TimeElapsed = 80f };
        Assert.AreEqual(3, StarRatingSystem.Calculate(stats, _stage));
    }

    [Test]
    public void Calculate_Victory_WithLosses_FastClear_Returns2()
    {
        // 빠르지만 손실 있음 → 별 1(승리) + 별 1(빠름) = 2
        var stats = new BattleStats { Victory = true, UnitsLost = 2, TimeElapsed = 80f };
        Assert.AreEqual(2, StarRatingSystem.Calculate(stats, _stage));
    }
}
