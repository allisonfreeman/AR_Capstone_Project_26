using UnityEngine;
using UnityEngine.Video;

public class SceneVideoLoader : MonoBehaviour
{
    public VideoClip videoForThisScene;
    public Material materialForThisScene;
    void Start()
    {
        if (SessionPanelManager.Instance != null)
        {
            if (materialForThisScene != null)
            {
                SessionPanelManager.Instance.UpdateScreenMaterial(materialForThisScene);
            }

            // Then update and play the video if one exists
            if (videoForThisScene != null)
            {
                SessionPanelManager.Instance.UpdateVideo(videoForThisScene);
            }
        }
    }
}