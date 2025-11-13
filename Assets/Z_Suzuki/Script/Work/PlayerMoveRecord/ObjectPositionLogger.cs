using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ObjectPositionLogger : SingletonBase<ObjectPositionLogger>
{
    [SerializeField, Header("ç¿ïWÇãLò^Ç∑ÇÈä‘äu(ïb)")] float RecordInterval = 1.0f;


    public List<Vector3> GetPositionList() { return _positionDatas; }
    public float GetRecordInterval() { return RecordInterval; }


    private Transform _targetObject;
    private List<Vector3> _positionDatas = new List<Vector3>();
    private float _intervalCount = 0.0f;


    private void FixedUpdate()
    {
        if (_targetObject == null)
        {
            Initialized();
        }

        PositionRecord(Time.fixedDeltaTime);
    }


    private void PositionRecord(float deltaTime)
    {
        _intervalCount += deltaTime;

        if (_intervalCount >= RecordInterval)
        {
            _intervalCount = 0.0f;
            _positionDatas.Add(_targetObject.position);
            Debug.Log(_positionDatas[_positionDatas.Count - 1]);
        }
    }


    private void Initialized()
    {
        if (NetworkClient.localPlayer.GetComponent<NetworkPlayerController>())
        {
            _targetObject = NetworkClient.localPlayer.transform;
        }
    }
}
