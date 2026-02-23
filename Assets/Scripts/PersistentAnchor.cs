using UnityEngine;
using MixedReality.Toolkit.SpatialManipulation; // MRTK3 Namespace

public class PersistentAnchor : MonoBehaviour
{
    // Static reference allows other scripts to find this panel easily
    public static PersistentAnchor Instance { get; private set; }

    private ObjectManipulator manipulator;

    void Awake()
    {
        // Singleton Logic: Ensures only one panel exists and persists across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        manipulator = GetComponent<ObjectManipulator>();
    }

    public void LockPanel()
    {
        if (manipulator != null)
        {
            // Disables the ability to grab/move the panel in MRTK3
            manipulator.enabled = false;
            Debug.Log("Panel locked for this session.");
        }
        
        // Optional: Change the material color or icon to show it is locked
    }

    public void UnlockPanel()
    {
        if (manipulator != null)
        {
            manipulator.enabled = true;
            Debug.Log("Panel unlocked for adjustment.");
        }
    }
}