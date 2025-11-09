using UnityEditor;
using UnityEditor.SceneManagement;
//using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

/// <summary>
/// Unityエディタの再生モードを管理するクラス
/// 1. どのシーンで再生ボタンを押しても、強制的に「Title」シーンから開始する
/// 2. 再生停止時、再生前に開いていたシーンに自動的に戻す
/// </summary>
[InitializeOnLoad]
public static class EditorPlayModeManager
{
    // ★★★★★ チーム全員でここのパスを統一してください ★★★★★
    private const string TITLE_SCENE_PATH = "Assets/Scenes/Network/Client.unity";

    private const string PREVIOUS_SCENE_KEY = "EditorPlayModeManager_PreviousScenePath";

    static EditorPlayModeManager()
    {
        // 再生モードの状態が変更されたときにメソッドを購読
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            // ------------------------------------
            // 1. 再生ボタンが押された時 (EditMode -> PlayMode に入る直前)
            // ------------------------------------
            case PlayModeStateChange.ExitingEditMode:
                // 現在のシーンがTitleシーン *ではない* 場合
                string currentScenePath = SceneManager.GetActiveScene().path;
                if (currentScenePath != TITLE_SCENE_PATH)
                {
                    // 現在のシーンパスを保存
                    EditorPrefs.SetString(PREVIOUS_SCENE_KEY, currentScenePath);

                    // シーンに変更があれば保存を促す
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        // Titleシーンを開いてから再生を開始
                        EditorSceneManager.OpenScene(TITLE_SCENE_PATH);
                    }
                    else
                    {
                        // ユーザーがキャンセルした場合、再生を中止
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    // 既にTitleシーンにいる場合は、何も保存しない
                    EditorPrefs.DeleteKey(PREVIOUS_SCENE_KEY);
                }
                break;

            // ------------------------------------
            // 2. 停止ボタンが押された時 (PlayMode -> EditMode に戻った直後)
            // ------------------------------------
            case PlayModeStateChange.EnteredEditMode:
                string previousScenePath = EditorPrefs.GetString(PREVIOUS_SCENE_KEY);

                // 保存されていたパスがある場合
                if (!string.IsNullOrEmpty(previousScenePath) && previousScenePath != SceneManager.GetActiveScene().path)
                {
                    // 元のシーンに戻す
                    EditorSceneManager.OpenScene(previousScenePath);
                    EditorPrefs.DeleteKey(PREVIOUS_SCENE_KEY);
                }
                break;
        }
    }
}