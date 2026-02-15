using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.ARFoundation;

public class SpawnAnchorFromSelection : MonoBehaviour
{
    public GameObject prefab;
   public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
   public ARAnchorManager anchorManager;

    void Start()
    {
       rayInteractor.selectEntered.AddListener(SpawnAnchor); 
    }
    public async void SpawnAnchor(BaseInteractionEventArgs args)
    {
        rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit);

        Pose hitPose = new Pose(hit.point, Quaternion.LookRotation(-hit.normal));

        var result = await anchorManager.TryAddAnchorAsync(hitPose);

        bool success = result.TryGetResult(out var anchor);

        if(success)
        {
            GameObject spawnedPrefab = Instantiate(prefab, anchor.pose.position, anchor.pose.rotation);
            spawnedPrefab.transform.parent = anchor.transform;
        }

    }
}
