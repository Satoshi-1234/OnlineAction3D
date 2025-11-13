using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadTest : MonoBehaviour
{
    [SerializeField, Header("変更先のシーン名")] string ChangeSceneName = string.Empty;
    [SerializeField, Header("シーン変更キー")] KeyCode SceneChangeKey = KeyCode.Space;
    

    void Update()
    {
        if (Input.GetKeyDown(SceneChangeKey))
        {
            SceneLoader.LoadScene(ChangeSceneName);
        }
    }
}
