using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    [SerializeField, Header("配達先リスト")] private List<DeliveryPoint> DeliveryPointList = new List<DeliveryPoint>();
    [SerializeField, Header("マーカーオブジェクト")] private DestinationMarker MarkerObject;


    public int GetDeliveryCompletedCount() { return _deliveryCompletedCount; }
    public DeliveryPoint GetCurrentDeliveryPoint() { return _currentDeliveryPoint; }


    public void StartDelivery()
    {
        int randIndex = Random.Range(0, DeliveryPointList.Count);
        _currentDeliveryPoint = DeliveryPointList[randIndex];
        MarkerObject.SetTarget(_currentDeliveryPoint.transform);
        Debug.Log("配達先: " + _currentDeliveryPoint.name);
        Debug.Log("インデックス: " + randIndex);
    }


    public void DeliveryCompleted()
    {
        Debug.Log("配達完了: " + _currentDeliveryPoint.name);
        if (_oldDeliveryPoint != null)
        {
            Debug.Log("配達先に一つ前に配達した場所が追加されました: " + _oldDeliveryPoint.name);
        }
        DeliveryPointList.Add(_oldDeliveryPoint);
        _oldDeliveryPoint = _currentDeliveryPoint;
        DeliveryPointList.Remove(_currentDeliveryPoint);
        _deliveryCompletedCount++;
        Debug.Log("配達完了数: " + _deliveryCompletedCount);
    }


    private DeliveryPoint _currentDeliveryPoint = null;
    private DeliveryPoint _oldDeliveryPoint = null;
    private int _deliveryCompletedCount = 0;


    void Start()
    {
        if (MarkerObject ==  null)
        {
            Debug.LogError("マーカーオブジェクトがアタッチされていません " + gameObject.name);
            return;
        }
        if (DeliveryPointList.Count < 1)
        {
            Debug.LogError("配達先リストに配達先が設定されていません " + gameObject.name);
            return;
        }
        StartDelivery();
    }
}
