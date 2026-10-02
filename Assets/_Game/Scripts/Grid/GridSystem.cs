// Assets/_Game/Scripts/Grid/GridSystem.cs
using UnityEngine;

namespace MedievalRTS.Grid
{
    public class GridSystem
    {
        private readonly int _width;
        private readonly int _height;
        private readonly bool[,] _occupied;

        public GridSystem(int width, int height)
        {
            _width = width;
            _height = height;
            _occupied = new bool[width, height];
        }

        public bool CanPlace(int x, int y, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
            {
                int cx = x + dx, cy = y + dy;
                if (cx < 0 || cx >= _width || cy < 0 || cy >= _height) return false;
                if (_occupied[cx, cy]) return false;
            }
            return true;
        }

        public void Place(int x, int y, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                _occupied[x + dx, y + dy] = true;
        }

        public void Remove(int x, int y, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
            for (int dy = 0; dy < size.y; dy++)
                _occupied[x + dx, y + dy] = false;
        }

        public bool IsOccupied(int x, int y) => _occupied[x, y];
    }
}
