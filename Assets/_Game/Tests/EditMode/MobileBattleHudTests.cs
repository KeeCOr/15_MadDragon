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

        var topStatus = hud.Root.transform.Find("TopStatus/Label").GetComponent<Text>();
        StringAssert.Contains("87s", topStatus.text);
        StringAssert.Contains("HP 1234", topStatus.text);
        StringAssert.Contains("+560G", topStatus.text);
        StringAssert.Contains("+12 Honor", topStatus.text);
    }

    private static void AssertCommandButton(MobileBattleHud hud, string name)
    {
        var buttonTransform = hud.Root.transform.Find($"QuickBar/{name}");
        Assert.IsNotNull(buttonTransform, $"Missing {name} command button.");
        Assert.IsNotNull(buttonTransform.GetComponent<Button>(), $"{name} must be a Button.");
        Assert.GreaterOrEqual(buttonTransform.GetComponent<RectTransform>().rect.height, 44f, $"{name} touch target is too short.");
    }

    private static void Click(MobileBattleHud hud, string name)
    {
        hud.Root.transform.Find($"QuickBar/{name}").GetComponent<Button>().onClick.Invoke();
    }
}
