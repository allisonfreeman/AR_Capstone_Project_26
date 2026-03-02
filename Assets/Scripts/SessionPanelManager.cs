using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SessionPanelManager : MonoBehaviour
{
    public static SessionPanelManager Instance { get; private set; }

    [Header("Button Groups")]
    public GameObject setupButtonsContainer;
    public GameObject navigationButtonsContainer;

    [Header("Screen References")]
    public MeshRenderer screenRenderer;

    public VideoPlayer videoPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Subscribe to the sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe when destroyed to avoid memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we are in the Setup scene
        if (scene.name == "UserSetup")
        {
            setupButtonsContainer.SetActive(true);
            navigationButtonsContainer.SetActive(false);
        }
        else
        {
            // For all other scenes (Procedure steps)
            setupButtonsContainer.SetActive(false);
            navigationButtonsContainer.SetActive(true);
        }
    }
    public void LoadNext()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
    }

    public void LoadPrevious()
    {
        int prevIndex = SceneManager.GetActiveScene().buildIndex - 1;
        if (prevIndex >= 0)
        {
            SceneManager.LoadScene(prevIndex);
        }
    }

    public void UpdateScreenMaterial(Material newMat)
    {
        if (screenRenderer != null && newMat != null)
        {
            screenRenderer.material = newMat;
            
        }
    }

    public void UpdateVideo(VideoClip newClip)
    {
        if (videoPlayer != null && newClip != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = newClip;
            videoPlayer.Play();
        }
    }

    public void ToggleVideo()
    {
        if (videoPlayer == null) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            
        }
        else
        {
            videoPlayer.Play();
            
        }
    }
    public void RewindTenSeconds()
    {
        if (videoPlayer == null) return;

        // Calculate new time, ensuring it doesn't go below 0
        float newTime = (float)videoPlayer.time - 10f;
        videoPlayer.time = Mathf.Max(newTime, 0f);
        
        
    }

    public void FastForwardTenSeconds()
    {
        if (videoPlayer == null) return;

        // Calculate new time, ensuring it doesn't exceed video length
        float newTime = (float)videoPlayer.time + 10f;
        float videoLength = (float)videoPlayer.length;
        
        videoPlayer.time = Mathf.Min(newTime, videoLength);
        
        
    }
}