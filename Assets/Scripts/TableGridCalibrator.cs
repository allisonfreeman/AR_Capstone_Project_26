using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class TableGridCalibrator : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor;
    public ARAnchorManager anchorManager;
    public GameObject cellPrefab; // The small grid cell prefab
    public GameObject previewPlanePrefab; // A translucent plane/quad
    public TextMeshProUGUI instructionText;

    private Vector3 pointTopLeft;
    private GameObject previewObject;
    private int step = 0;

    void Start()
    {
        instructionText.text = "Pinch the Top-Left corner";
        directInteractor.selectEntered.AddListener(OnSelect);
    }

    void Update()
    {
        // Rubber Band Effect
        if (step == 1 && previewObject != null)
        {
            UpdatePreview(directInteractor.attachTransform.position);
        }
    }

    private void OnSelect(BaseInteractionEventArgs args)
    {
        Vector3 currentPos = directInteractor.attachTransform.position;

        if (step == 0)
        {
            pointTopLeft = currentPos;
            previewObject = Instantiate(previewPlanePrefab);
            step = 1;
            instructionText.text = "Pinch the Bottom-Right corner";
        }
        else if (step == 1)
        {
            Destroy(previewObject);
            CreateFinalGrid(currentPos);
            step = 2;
            instructionText.text = "Grid Confirmed!";
        }
    }

    void UpdatePreview(Vector3 currentHandPos)
    {
        // Find the center between the start point and current hand
        Vector3 center = (pointTopLeft + currentHandPos) / 2f;
        previewObject.transform.position = center;

        // Calculate size
        float width = Mathf.Abs(pointTopLeft.x - currentHandPos.x);
        float length = Mathf.Abs(pointTopLeft.z - currentHandPos.z);

        // Adjust scale (Assuming a 1x1 Unity Plane/Cube)
        previewObject.transform.localScale = new Vector3(width, 0.01f, length);
    }

    async void CreateFinalGrid(Vector3 pointBottomRight)
    {
        Pose anchorPose = new Pose(pointTopLeft, Quaternion.identity);
        var result = await anchorManager.TryAddAnchorAsync(anchorPose);

        if (result.status.IsSuccess())
        {
            var anchor = result.value;
            GameObject gridContainer = new GameObject("TableGrid");
            gridContainer.transform.SetParent(anchor.transform, false);

            float totalWidth = pointBottomRight.x - pointTopLeft.x;
            float totalLength = pointBottomRight.z - pointTopLeft.z;

            float cellW = totalWidth / 5f;
            float cellL = totalLength / 3f;

            for (int z = 0; z < 3; z++)
            {
                for (int x = 0; x < 5; x++)
                {
                    // Center the cell within its bounds
                    Vector3 lPos = new Vector3((x * cellW) + (cellW / 2), 0, (z * cellL) + (cellL / 2));
                    GameObject cell = Instantiate(cellPrefab, gridContainer.transform);
                    cell.transform.localPosition = lPos;
                    cell.transform.localScale = new Vector3(Mathf.Abs(cellW), 0.01f, Mathf.Abs(cellL));
                }
            }
        }
    }
}