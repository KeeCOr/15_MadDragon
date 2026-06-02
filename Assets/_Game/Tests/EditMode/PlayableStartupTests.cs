using NUnit.Framework;
using UnityEditor;
using MedievalRTS.Core;

public class PlayableStartupTests
{
    [Test]
    public void FirstEnabledBuildScene_IsPlayableCampaignHub()
    {
        var scenes = EditorBuildSettings.scenes;

        Assert.IsNotEmpty(scenes);
        Assert.IsTrue(scenes[0].enabled);
        Assert.AreEqual("Assets/_Game/Scenes/05_TestBattle.unity", scenes[0].path);
    }

    [Test]
    public void GameManager_UsesExistingBuildSceneNames()
    {
        Assert.AreEqual("01_MainMenu", GameManager.SceneNameForState(GameState.MainMenu));
        Assert.AreEqual("02_BaseBuilder", GameManager.SceneNameForState(GameState.BaseBuilder));
        Assert.AreEqual("05_TestBattle", GameManager.SceneNameForState(GameState.Battle));
        Assert.AreEqual("04_Result", GameManager.SceneNameForState(GameState.Result));
    }
}
