using MedievalRTS.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class CampaignHubScreenTests
{
    private GameObject _canvas;

    [SetUp]
    public void SetUp()
    {
        _canvas = new GameObject("Canvas", typeof(Canvas));
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(_canvas);
    }

    [Test]
    public void PlayButton_StartsBattleDirectly()
    {
        bool battleStarted = false;
        bool prepOpened = false;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var screen = new CampaignHubScreen(_canvas, font, () => battleStarted = true, () => prepOpened = true, () => { });

        screen.Root.transform.Find("Play").GetComponent<Button>().onClick.Invoke();

        Assert.IsTrue(battleStarted);
        Assert.IsFalse(prepOpened);
    }

    [Test]
    public void PrepButton_OpensAttackPreparation()
    {
        bool battleStarted = false;
        bool prepOpened = false;
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var screen = new CampaignHubScreen(_canvas, font, () => battleStarted = true, () => prepOpened = true, () => { });

        screen.Root.transform.Find("Prep").GetComponent<Button>().onClick.Invoke();

        Assert.IsTrue(prepOpened);
        Assert.IsFalse(battleStarted);
    }
}
