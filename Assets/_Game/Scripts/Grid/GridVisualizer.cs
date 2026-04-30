// Assets/_Game/Scripts/Grid/GridVisualizer.cs
using UnityEngine;

namespace MedievalRTS.Grid
{
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private int width = 20;
        [SerializeField] private int height = 20;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);

        private void OnDrawGizmos()
        {
            Gizmos.color = gridColor;
            for (int x = 0; x <= width; x++)
                Gizmos.DrawLine(
                    transform.position + new Vector3(x * cellSize, 0, 0),
                    transform.position + new Vector3(x * cellSize, 0, height * cellSize));
            for (int y = 0; y <= height; y++)
                Gizmos.DrawLine(
                    transform.position + new Vector3(0, 0, y * cellSize),
                    transform.position + new Vector3(width * cellSize, 0, y * cellSize));
        }
    }
}
