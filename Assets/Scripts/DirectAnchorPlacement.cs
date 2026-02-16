using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // Updated namespace for Interactors

public class DirectAnchorPlacement : MonoBehaviour
{
    public GameObject prefab;
    // Changed to DirectInteractor
    public XRDirectInteractor directInteractor; 
    public ARAnchorManager anchorManager;

    void Start()
    {
        // Still listens for the "Select" event (Trigger pull or Air-tap)
        directInteractor.selectEntered.AddListener(SpawnAnchorAtHand); 
    }

    public async void SpawnAnchorAtHand(BaseInteractionEventArgs args)
    {
        // 1. Get the current pose of the hand/interactor instead of a ray hit
        // We use the attachTransform as it's the precise "grab point"
        Vector3 spawnPosition = directInteractor.attachTransform.position;
        Quaternion spawnRotation = directInteractor.attachTransform.rotation;

        Pose hitPose = new Pose(spawnPosition, spawnRotation);

        // 2. Request the system to create a spatial anchor at this hand position
        var result = await anchorManager.TryAddAnchorAsync(hitPose);

        // 3. Check if the anchor was successfully created
        if (result.status.IsSuccess())
        {
            var anchor = result.value;

            // 4. Instantiate and parent the prefab to the anchor
            // This "glues" the grid to the physical spot your hand just touched
            GameObject spawnedPrefab = Instantiate(prefab, anchor.pose.position, anchor.pose.rotation);
            spawnedPrefab.transform.SetParent(anchor.transform, true);
            
            Debug.Log("Anchor created directly at hand position!");
        }
        else
        {
            Debug.LogError($"Failed to create anchor: {result.status}");
        }
    }
}