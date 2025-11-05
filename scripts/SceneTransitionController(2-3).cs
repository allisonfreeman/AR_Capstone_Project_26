using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionController : MonoBehaviour
{
    // The name of the scene to load next (e.g., "Scene2_VideoStep2")
    public string nextSceneName = "Scene3_VideoStep3";

    // Call this method from an Interactable (like a button)
    public void GoToNextScene()
    {
        // Start the scene loading coroutine
        StartCoroutine(LoadSceneAsync(nextSceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. Start the loading operation without immediately activating the new scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 2. Add a visual fade (optional but recommended for comfort)
        //    NOTE: You would typically use an MRTK component (like a non-core UI element 
        //    or an extension) here to fade the screen to black before the loop.
        //    Since we don't have a specific fade component, we'll simulate a slight pause.
        
        float fadeTime = 0.5f; // Duration of your fade animation/pause
        yield return new WaitForSeconds(fadeTime); 

        // 3. Wait until the scene has loaded (progress is near 0.9)
        while (operation.progress < 0.9f)
        {
            // The loading screen content/visuals can be updated here
            yield return null;
        }

        // 4. Activate the newly loaded scene
        operation.allowSceneActivation = true;
    }
}