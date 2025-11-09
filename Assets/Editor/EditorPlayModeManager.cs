using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Unityエディタの再生モードを管理するクラス
/// 1. どのシーンで再生ボタンを押しても、強制的に「Title」シーンから開始する
/// 2. 再生停止時、再生前に開いていたシーンに自動的に戻す
/// </summary>
[InitializeOnLoad]
public static class EditorPlayModeManager
{
    private const string TITLE_SCENE_PATH = "Assets/Scenes/Network/Client.unity";

    private const string PREVIOUS_SCENE_KEY = "EditorPlayModeManager_PreviousScenePath";

    // メニューバーに追加する項目名
    private const string MENU_NAME = "Tools/Force Start from Title Scene";
    // ON/OFF設定を保存するためのキー
    private const string FORCE_START_KEY = "EditorPlayModeManager_ForceStart";

    // デフォルトで機能を有効にするか (true = 有効)
    private const bool IS_ENABLED_BY_DEFAULT = true;
    static EditorPlayModeManager()
    {
        // 再生モードの状態が変更されたときにメソッドを購読
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary>
    /// メニューアイテム (Tools/Force Start from Title Scene) をクリックした時の動作
    /// </summary>
    [MenuItem(MENU_NAME)]
    private static void ToggleForceStart()
    {
        // 現在の設定を取得し、反転させて保存
        bool isEnabled = EditorPrefs.GetBool(FORCE_START_KEY, IS_ENABLED_BY_DEFAULT);
        EditorPrefs.SetBool(FORCE_START_KEY, !isEnabled);
    }

    /// <summary>
    /// メニューアイテムの状態（チェックマーク）を管理
    /// </summary>
    [MenuItem(MENU_NAME, true)]
    private static bool ValidateToggleForceStart()
    {
        // 設定を読み込み、メニューのチェック状態に反映
        bool isEnabled = EditorPrefs.GetBool(FORCE_START_KEY, IS_ENABLED_BY_DEFAULT);
        Menu.SetChecked(MENU_NAME, isEnabled);
        return true; // メニューアイテムは常に有効（グレーアウトさせない）
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        bool isEnabled = EditorPrefs.GetBool(FORCE_START_KEY, IS_ENABLED_BY_DEFAULT);
        switch (state)
        {
            // ------------------------------------
            // 1. 再生ボタンが押された時 (EditMode -> PlayMode に入る直前)
            // ------------------------------------
            case PlayModeStateChange.ExitingEditMode:
                if (!isEnabled)
                {
                    // 以前のシーンパスが残っていると停止時に予期せず戻るためクリア
                    EditorPrefs.DeleteKey(PREVIOUS_SCENE_KEY);
                    break; // このswitch-caseを抜ける
                }
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