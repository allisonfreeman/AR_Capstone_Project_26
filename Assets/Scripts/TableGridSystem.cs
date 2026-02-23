using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;
using TMPro;

public class TableGridSystem : MonoBehaviour
{
    [Header("Components")]
    public XRDirectInteractor directInteractor;
    public ARAnchorManager anchorManager;
    public TextMeshProUGUI statusText;

    [Header("Prefabs")]
    public GameObject markerPrefab;
    public GameObject previewPrefab;
    public GameObject cellPrefab;

    private Vector3 pointA;
    private GameObject previewInstance;
    private int step = 0;
    private List<GameObject> markers = new List<GameObject>();

    void Start()
    {
        statusText.text = "Step 1: Pinch Top-Left Corner";
        directInteractor.selectEntered.AddListener(HandleSelect);
    }

    void Update()
    {
        if (step == 1 && previewInstance != null)
        {
            UpdatePreview(directInteractor.attachTransform.position);
        }
    }

    private void HandleSelect(BaseInteractionEventArgs args)
    {
        Vector3 handPos = directInteractor.attachTransform.position;

        if (step == 0)
        {
            pointA = handPos;
            SpawnMarker(handPos);
            previewInstance = Instantiate(previewPrefab);
            step = 1;
            statusText.text = "Step 2: Pinch Bottom-Right Corner";
        }
        else if (step == 1)
        {
            SpawnMarker(handPos);
            Destroy(previewInstance);
            CreateGrid(handPos);
            step = 2;
            statusText.text = "Grid Complete!";
        }
    }

    void SpawnMarker(Vector3 pos)
    {
        markers.Add(Instantiate(markerPrefab, pos, Quaternion.identity));
    }

    void UpdatePreview(Vector3 handPos)
    {
        previewInstance.transform.position = (pointA + handPos) / 2f;
        previewInstance.transform.localScale = new Vector3(
            Mathf.Abs(pointA.x - handPos.x), 
            0.01f, 
            Mathf.Abs(pointA.z - handPos.z));
    }

    async void CreateGrid(Vector3 pointB)
    {
        // 1. Anchor the entire system to the first point
        var result = await anchorManager.TryAddAnchorAsync(new Pose(pointA, Quaternion.identity));
        if (result.status.IsSuccess())
        {
            GameObject root = new GameObject("GridRoot");
            root.transform.SetParent(result.value.transform, false);

            // 2. Calculate cell math
            float totalWidth = pointB.x - pointA.x;
            float totalLength = pointB.z - pointA.z;
            float cellW = totalWidth / 5f;
            float cellL = totalLength / 3f;

            for (int z = 0; z < 3; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    GameObject cell = Instantiate(cellPrefab, root.transform);
                    // Position cells relative to the Top-Left anchor
                    cell.transform.localPosition = new Vector3(
                        (x * cellW) + (cellW / 2), 
                        0, 
                        (z * cellL) + (cellL / 2));
                    
                    cell.transform.localScale = new Vector3(Mathf.Abs(cellW), 0.01f, Mathf.Abs(cellL));
                }
            }
        }
    }
}