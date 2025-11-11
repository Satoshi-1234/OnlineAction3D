using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField, Header("初期カメラ")] Camera InitialCamera;
    [SerializeField, Header("シーンに存在するカメラリスト")] Camera[] CameraList;


    public Camera GetActiveCamera() {  return _activeCamera; }


    public void MainCameraChange(Camera nextCamera)
    {
        _activeCamera.gameObject.SetActive(false);

        foreach (Camera camera in CameraList)
        {
            if (camera ==  nextCamera)
            {
                _activeCamera = camera;
                camera.gameObject.SetActive(true);
                break;
            }
        }
    }


    private Camera _activeCamera;


    void Start()
    {
        if (InitialCamera ==  null)
        {
            Debug.LogError("初期カメラがアタッチされていません " + gameObject.name);
        }
        if (CameraList.Length == 0)
        {
            Debug.LogError("カメラリストにカメラが一つもアタッチされていません " + gameObject.name);
        }

        foreach (Camera camera in CameraList)
        {
            if (camera != InitialCamera)
            {
                camera.gameObject.SetActive(false);
            }
            else
            {
                _activeCamera = camera;
                camera.gameObject.SetActive(true);
            }
        }

        if (_activeCamera == null)
        {
            Debug.LogError("カメラリストに初期カメラに設定したカメラがアタッチされていません " + gameObject.name);
        }
    }
}
