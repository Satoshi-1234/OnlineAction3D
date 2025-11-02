using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadMonitor : MonoBehaviour
{

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(WaitForStart(scene));
    }

    private IEnumerator WaitForStart(Scene scene)
    {
        yield return null; // Start() 実行待ち
        Debug.Log($"[SceneLoadMonitor] シーン「{scene.name}」の Start() がすべて完了しました。");
    }
}
