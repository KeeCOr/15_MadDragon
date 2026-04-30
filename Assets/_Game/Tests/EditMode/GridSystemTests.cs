// Assets/_Game/Tests/EditMode/GridSystemTests.cs
using NUnit.Framework;
using MedievalRTS.Grid;
using UnityEngine;

public class GridSystemTests
{
    private GridSystem _grid;

    [SetUp]
    public void SetUp()
    {
        _grid = new GridSystem(10, 10);
    }

    [Test]
    public void CanPlace_EmptyGrid_1x1_ReturnsTrue()
    {
        Assert.IsTrue(_grid.CanPlace(0, 0, new Vector2Int(1, 1)));
    }

    [Test]
    public void CanPlace_OutOfBounds_ReturnsFalse()
    {
        Assert.IsFalse(_grid.CanPlace(9, 9, new Vector2Int(2, 2)));
    }

    [Test]
    public void CanPlace_NegativeCoord_ReturnsFalse()
    {
        Assert.IsFalse(_grid.CanPlace(-1, 0, new Vector2Int(1, 1)));
    }

    [Test]
    public void Place_Then_CanPlace_SameCell_ReturnsFalse()
    {
        _grid.Place(0, 0, new Vector2Int(2, 2));
        Assert.IsFalse(_grid.CanPlace(0, 0, new Vector2Int(1, 1)));
        Assert.IsFalse(_grid.CanPlace(1, 1, new Vector2Int(1, 1)));
    }

    [Test]
    public void Remove_AfterPlace_CellBecomesAvailable()
    {
        _grid.Place(3, 3, new Vector2Int(2, 2));
        _grid.Remove(3, 3, new Vector2Int(2, 2));
        Assert.IsTrue(_grid.CanPlace(3, 3, new Vector2Int(2, 2)));
    }

    [Test]
    public void CanPlace_AdjacentToPlaced_ReturnsTrue()
    {
        _grid.Place(0, 0, new Vector2Int(2, 2));
        Assert.IsTrue(_grid.CanPlace(2, 0, new Vector2Int(1, 1)));
    }

    [Test]
    public void IsOccupied_AfterPlace_ReturnsTrue()
    {
        _grid.Place(5, 5, new Vector2Int(1, 1));
        Assert.IsTrue(_grid.IsOccupied(5, 5));
    }
}
