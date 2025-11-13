using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollow : MonoBehaviour
{
    [SerializeField, Header("追従するオブジェクト")] private Transform Target;
    [SerializeField, Header("プレイヤーに合わせてマップを回転するかどうか")] private bool IsRotation = true;


    void Start()
    {
        if (Target == null)
        {
            Debug.LogError("追従するオブジェクトが見つかりません " + gameObject.name);
        }
    }

    void FixedUpdate()
    {
        if (Target == null)
        {
            return;
        }

        transform.position = new Vector3(Target.position.x, transform.position.y, Target.position.z);

        if (IsRotation)
        {
            transform.rotation = new Quaternion(0.0f, Target.rotation.y, 0.0f, Target.rotation.w);
        }
    }
}
