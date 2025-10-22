using UnityEngine;
using System.Runtime.InteropServices; // Windowsの機能を呼び出すために必要

public class WindowTitleUpdater : MonoBehaviour
{
    // CustomNetworkManagerへの参照をインスペクターから設定
    [Header("参照")]
    [Tooltip("バージョン情報を取得するためにCustomNetworkManagerをここに設定します")]
    public CustomNetworkManager customNetworkManager;

    [Header("設定")]
    [Tooltip("FPSの更新頻度（秒）")]
    public float fpsUpdateInterval = 0.5f;

    // FPS計算用の変数
    private float accum = 0;
    private int frames = 0;
    private float timeLeft;
    private string fpsString = "";

    // Windows APIを呼び出すための準備
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

    private System.IntPtr windowHandle;
#endif

    void Awake()
    {
        // このオブジェクトがシーンをまたいでも破棄されないようにする
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        timeLeft = fpsUpdateInterval;

        // Windowsビルドの場合のみ、ウィンドウのハンドルを取得
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // Unityのデフォルトウィンドウ名 "UnityWndClass" を使ってウィンドウを探す
        windowHandle = FindWindow("UnityWndClass", null);
#endif
    }

    void Update()
    {
        // --- FPSの計算 ---
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // 一定間隔でFPS文字列を更新
        if (timeLeft <= 0.0)
        {
            float fps = accum / frames;
            fpsString = $"{fps:F2} FPS"; // 小数点以下2桁で表示

            // 次の計算のためにリセット
            timeLeft = fpsUpdateInterval;
            accum = 0.0f;
            frames = 0;
        }

        // --- ウィンドウタイトルの更新 ---
        if (CustomNetworkManager.Instance != null && !string.IsNullOrEmpty(fpsString))
        {
            string baseTitle = Application.productName;
            string newTitle = $"{baseTitle} - v{CustomNetworkManager.Instance.version} ({fpsString})"; // Instance経由でバージョンを取得

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowText(windowHandle, newTitle);
#endif
        }
    }
}