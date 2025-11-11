using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPositionLogger : Singleton<ObjectPositionLogger>
{
    [SerializeField, Header("移動を追跡するオブジェクト")] Transform TrackObject;
    [SerializeField, Header("座標を記録する間隔(秒)")] float RecordInterval = 1.0f;


    public List<Vector3> GetPositionList() { return _positionDatas; }
    public float GetRecordInterval() { return RecordInterval; }


    private List<Vector3> _positionDatas = new List<Vector3>();
    private float _intervalCount = 0.0f;


    private void Start()
    {
        if (TrackObject == null)
        {
            Debug.LogError("移動を追跡するオブジェクトがアタッチされていません " + gameObject.name);
        }
    }


    private void FixedUpdate()
    {
        if (TrackObject == null)
        {
            return;
        }

        _intervalCount += Time.fixedDeltaTime;

        if (_intervalCount >= RecordInterval)
        {
            _intervalCount = 0.0f;
            _positionDatas.Add(TrackObject.position);
        }
    }
}
