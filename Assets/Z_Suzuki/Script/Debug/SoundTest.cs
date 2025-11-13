using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SoundManager.Instance.PlaySE("Œˆ’èƒ{ƒ^ƒ“‚ğ‰Ÿ‚·1");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SoundManager.Instance.PlayBGM("yBGMWz‚â‚³‚µ‚¢•—y‚Ù‚Ì‚Ú‚ÌŒnz(nc770)");
        }
    }
}
