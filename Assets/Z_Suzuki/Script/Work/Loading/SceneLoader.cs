using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static event Action<float> OnProgressUpdated;
    public static event Action OnLoadCompleted;
    [SerializeField, Header("ƒ[ƒh‚É‚©‚©‚éÅ’áŽžŠÔ(•b)")] private static float DelayAfterLoad = 2.0f;


    public static void LoadScene(string sceneName)
    {
        _targetSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }


    private static string _targetSceneName;


    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }


    private IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_targetSceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            OnProgressUpdated?.Invoke(progress);

            if (asyncLoad.progress >= 0.9f)
            {
                break;
            }

            yield return null;
        }

        OnProgressUpdated?.Invoke(1.0f);
        OnLoadCompleted?.Invoke();

        yield return new WaitForSeconds(DelayAfterLoad);

        asyncLoad.allowSceneActivation = true;
    }
}
