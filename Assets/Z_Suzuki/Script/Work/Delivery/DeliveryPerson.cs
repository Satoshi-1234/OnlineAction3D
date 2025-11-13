using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryPerson : MonoBehaviour
{
    [SerializeField] private DeliveryManager DeliveryManager;


    void Start()
    {
        if (DeliveryManager == null)
        {
            Debug.LogError("DeliveryManagerがアタッチされていません " + gameObject.name);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DeliveryPoint"))
        {
            DeliveryPoint deliveryPoint = other.gameObject.GetComponent<DeliveryPoint>();

            if (deliveryPoint == null)
            {
                return;
            }

            if (DeliveryManager.GetCurrentDeliveryPoint() != deliveryPoint)
            {
                Debug.Log("ここは配達先ではありません");
                return;
            }

            DeliveryManager.DeliveryCompleted();
        }
    }
}
