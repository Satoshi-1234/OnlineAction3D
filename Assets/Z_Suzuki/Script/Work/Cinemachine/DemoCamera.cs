using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoCamera : MonoBehaviour
{
    [SerializeField, Header("カメラマネージャー")] private CameraManager CameraManager;
    [SerializeField, Header("ここに設定したDollyCartの移動が完了したらカメラ切替")] private SplineCartEaseSpeed DollyCartEaseSpeed;
    [SerializeField, Header("変更先のカメラ")] private Camera ChangeCamera;


    void Start()
    {
        if (CameraManager ==  null)
        {
            Debug.LogError("カメラマネージャーがアタッチされていません " + gameObject.name);
        }
        if (DollyCartEaseSpeed == null)
        {
            Debug.LogError("DollyCartがアタッチされていません " + gameObject.name);
        }
        if (ChangeCamera == null)
        {
            Debug.LogError("変更先のカメラがアタッチされていません " + gameObject.name);
        }
    }

    
    void Update()
    {
        if (DollyCartEaseSpeed.IsCompleted())
        {
            CameraManager.MainCameraChange(ChangeCamera);
        }
    }
}
