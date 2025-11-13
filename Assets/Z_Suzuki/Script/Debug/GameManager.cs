using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonBase<GameManager>
{
    [SerializeField] private KeyCode PauseKey = KeyCode.Space;
    [SerializeField] private KeyCode DebugEndKey = KeyCode.Escape;


    public bool GetIsPaused() { return _isPaused; }

    
    private bool _isPaused = false;


    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Update()
    {
        if (Input.GetKeyDown(PauseKey))
        {
            _isPaused = !_isPaused;
            Cursor.visible = !Cursor.visible;
            if (Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        if (Input.GetKeyDown(DebugEndKey))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
