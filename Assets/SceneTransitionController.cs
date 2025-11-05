using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionController : MonoBehaviour
{
    // This public method now accepts the scene name as a parameter
    public void GoToNextScene(string sceneName)
    {
        // Start the scene loading coroutine, passing the received sceneName
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. Start the loading operation without immediately activating the new scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 2. Add a visual fade (optional but recommended for comfort)
        float fadeTime = 0.5f; // Duration of your fade animation/pause
        yield return new WaitForSeconds(fadeTime); 

        // 3. Wait until the scene has loaded (progress is near 0.9)
        while (operation.progress < 0.9f)
        {
            // You can add logic here to display a progress bar if desired
            yield return null;
        }

        // 4. Activate the newly loaded scene
        operation.allowSceneActivation = true;
    }
}