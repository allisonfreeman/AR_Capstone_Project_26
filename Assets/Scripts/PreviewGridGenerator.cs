using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PreviewGridGenerator : MonoBehaviour
{
    public Transform topLeftSphere;
    public Transform bottomRightSphere;
    public GameObject gridPrefab; 

    public List<string> gridLabels = new List<string>() 
    { 
        "A1", "A2", "A3", "A4",
        "B1", "B2", "B3", "B4"
    };

    private GameObject[] currentGrid;

    // This method is for your "Clear" button
    public void ClearGrid()
    {
        if (currentGrid != null)
        {
            foreach (var obj in currentGrid) 
            {
                if (obj != null) Destroy(obj);
            }
            currentGrid = null;
        }
    }

    public void GenerateGrid()
    {
        // First, clear any old grid so they don't stack up
        ClearGrid();

        int columns = 4;
        int rows = 2;
        currentGrid = new GameObject[columns * rows];

        Vector3 startPos = topLeftSphere.position;
        Vector3 endPos = bottomRightSphere.position;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float tCol = (columns > 1) ? (float)c / (columns - 1) : 0;
                float tRow = (rows > 1) ? (float)r / (rows - 1) : 0;

                float x = Mathf.Lerp(startPos.x, endPos.x, tCol);
                float z = Mathf.Lerp(startPos.z, endPos.z, tRow);
                Vector3 spawnPos = new Vector3(x, startPos.y, z);

                GameObject newSphere = Instantiate(gridPrefab, spawnPos, Quaternion.identity);

                int index = r * columns + c;
                TextMeshPro textComp = newSphere.GetComponentInChildren<TextMeshPro>();

                if (textComp != null && index < gridLabels.Count)
                {
                    textComp.text = gridLabels[index];
                }

                currentGrid[index] = newSphere;
            }
        }
    }
}