using System.Collections.Generic;
using MedievalRTS.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MobileBattleHudTests
{
    private GameObject _canvas;
    private Font _font;

    [SetUp]
    public void SetUp()
    {
        _canvas = new GameObject("Canvas", typeof(Canvas));
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_canvas);
    }

    [Test]
    public void CommandButtons_AreNamedAndTouchSized()
    {
        var hud = new MobileBattleHud(_canvas, _font);

        AssertCommandButton(hud, "Rally");
        AssertCommandButton(hud, "Attack");
        AssertCommandButton(hud, "Hold");
        AssertCommandButton(hud, "Spells");
    }

    [Test]
    public void CommandButtons_InvokeExpectedCommands()
    {
        var received = new List<MobileBattleHud.CommandKind>();
        var hud = new MobileBattleHud(_canvas, _font);
        hud.SetCommandHandler(received.Add);

        Click(hud, "Rally");
        Click(hud, "Attack");
        Click(hud, "Hold");
        Click(hud, "Spells");

        CollectionAssert.AreEqual(
            new[]
            {
                MobileBattleHud.CommandKind.Rally,
                MobileBattleHud.CommandKind.Attack,
                MobileBattleHud.CommandKind.Hold,
                MobileBattleHud.CommandKind.Spells
            },
            received);
    }

    [Test]
    public void Refresh_UsesCompactBattleStatusText()
    {
        var hud = new MobileBattleHud(_canvas, _font);

        hud.Refresh(87, 1234, 560, 12);

        Assert.AreEqual("TIME  87s", FindTopText(hud, "Time").text);
        Assert.AreEqual("OBJECTIVE  BASE HP  1,234", FindTopText(hud, "Objective").text);
        Assert.AreEqual("REWARD  +560G  +12 HONOR", FindTopText(hud, "Reward").text);
    }

    [Test]
    public void Refresh_UsesWarningAndCriticalTimeTones()
    {
        var hud = new MobileBattleHud(_canvas, _font);

        hud.Refresh(24, 1000, 0, 0);
        Assert.AreEqual(MobileHudTheme.Gold, FindTopText(hud, "Time").color);

        hud.Refresh(9, 1000, 0, 0);
        Assert.AreEqual(MobileHudTheme.Danger, FindTopText(hud, "Time").color);
    }

    [Test]
    public void StatusModel_ClampsInvalidValuesForSafeDisplay()
    {
        var status = BattlePriorityModel.BuildHudStatus(-3, -10, -20, -1);

        Assert.AreEqual("TIME  0s", status.TimeText);
        Assert.AreEqual("OBJECTIVE  BASE HP  0", status.ObjectiveText);
        Assert.AreEqual("REWARD  +0G  +0 HONOR", status.RewardText);
        Assert.AreEqual(BattleTimeTone.Critical, status.TimeTone);
    }

    private static void AssertCommandButton(MobileBattleHud hud, string name)
    {
        var buttonTransform = hud.Root.transform.Find($"QuickBar/{name}");
        Assert.IsNotNull(buttonTransform, $"Missing {name} command button.");
        Assert.IsNotNull(buttonTransform.GetComponent<Button>(), $"{name} must be a Button.");
        Assert.GreaterOrEqual(buttonTransform.GetComponent<RectTransform>().rect.height, 44f, $"{name} touch target is too short.");
    }

    private static Text FindTopText(MobileBattleHud hud, string name)
    {
        return hud.Root.transform.Find($"TopStatus/{name}").GetComponent<Text>();
    }

    private static void Click(MobileBattleHud hud, string name)
    {
        hud.Root.transform.Find($"QuickBar/{name}").GetComponent<Button>().onClick.Invoke();
    }
}
