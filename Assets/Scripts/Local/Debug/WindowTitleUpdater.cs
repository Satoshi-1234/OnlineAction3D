using UnityEngine;
using System.Runtime.InteropServices; // Windows�̋@�\���Ăяo�����߂ɕK�v

public class WindowTitleUpdater : MonoBehaviour
{
    // CustomNetworkManager�ւ̎Q�Ƃ��C���X�y�N�^�[����ݒ�
    [Header("�Q��")]
    [Tooltip("�o�[�W���������擾���邽�߂�CustomNetworkManager�������ɐݒ肵�܂�")]
    public CustomNetworkManager customNetworkManager;

    [Header("�ݒ�")]
    [Tooltip("FPS�̍X�V�p�x�i�b�j")]
    public float fpsUpdateInterval = 0.5f;

    // FPS�v�Z�p�̕ϐ�
    private float accum = 0;
    private int frames = 0;
    private float timeLeft;
    private string fpsString = "";

    // Windows API���Ăяo�����߂̏���
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);

    private System.IntPtr windowHandle;
#endif

    void Awake()
    {
        // ���̃I�u�W�F�N�g���V�[�����܂����ł��j������Ȃ��悤�ɂ���
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        timeLeft = fpsUpdateInterval;

        // Windows�r���h�̏ꍇ�̂݁A�E�B���h�E�̃n���h�����擾
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        // Unity�̃f�t�H���g�E�B���h�E�� "UnityWndClass" ���g���ăE�B���h�E��T��
        windowHandle = FindWindow("UnityWndClass", null);
#endif
    }

    void Update()
    {
        // --- FPS�̌v�Z ---
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        // ���Ԋu��FPS��������X�V
        if (timeLeft <= 0.0)
        {
            float fps = accum / frames;
            fpsString = $"{fps:F2} FPS"; // �����_�ȉ�2���ŕ\��

            // ���̌v�Z�̂��߂Ƀ��Z�b�g
            timeLeft = fpsUpdateInterval;
            accum = 0.0f;
            frames = 0;
        }

        // --- �E�B���h�E�^�C�g���̍X�V ---
        if (CustomNetworkManager.Instance != null && !string.IsNullOrEmpty(fpsString))
        {
            string baseTitle = Application.productName;
            string newTitle = $"{baseTitle} - v{CustomNetworkManager.Instance.version} ({fpsString})"; // Instance�o�R�Ńo�[�W�������擾

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            SetWindowText(windowHandle, newTitle);
#endif
        }
    }
}