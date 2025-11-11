using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRecordReplay : MonoBehaviour
{
    [SerializeField, Header("記録を再現させるオブジェクト")] GameObject TargetObject;
    [SerializeField, Header("再生速度(倍率)")] float ReplaySpeedMultiplier = 1.0f;
    [SerializeField, Header("再現させるオブジェクトのY座標")] float TargetObjectPositionY = 5.0f;


    private List<Vector3> _positionDatas;
    private bool _isPlaying = false;
    private int _currentFrameIndex = 0;


    void Start()
    {
        if (TargetObject == null)
        {
            Debug.LogError("記録を再現させるオブジェクトがアタッチされていません " + gameObject.name);
        }

        _positionDatas = ObjectPositionLogger.Instance.GetPositionList();
        StartCoroutine(StartReplay());
    }

    
    private IEnumerator StartReplay()
    {
        Debug.Log("リプレイ再生開始");
        _isPlaying = true;

        while (_isPlaying && _currentFrameIndex < _positionDatas.Count)
        {
            Vector3 currentPosition = _positionDatas[_currentFrameIndex];
            TargetObject.transform.position = new Vector3(currentPosition.x, TargetObjectPositionY, currentPosition.z);
            float recordInterval = ObjectPositionLogger.Instance.GetRecordInterval();
            _currentFrameIndex++;

            yield return new WaitForSeconds(recordInterval / ReplaySpeedMultiplier);
        }

        _isPlaying = false;
        Debug.Log("リプレイ再生終了");
    }
}
