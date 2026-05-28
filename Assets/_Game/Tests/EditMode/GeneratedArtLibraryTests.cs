using MedievalRTS.Visuals;
using NUnit.Framework;

public class GeneratedArtLibraryTests
{
    [Test]
    public void GeneratedArtResources_AreLoadable()
    {
        Assert.IsTrue(GeneratedArtLibrary.HasSprite("Buildings/player_castle"));
        Assert.IsTrue(GeneratedArtLibrary.HasSprite("Units/knight"));
        Assert.IsTrue(GeneratedArtLibrary.HasSprite("Icons/gold"));
        Assert.IsTrue(GeneratedArtLibrary.HasSprite("Ui/button_blue"));
    }
}
