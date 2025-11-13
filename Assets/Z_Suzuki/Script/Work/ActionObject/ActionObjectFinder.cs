using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionObjectFinder : MonoBehaviour
{
    [SerializeField, Header("カメラ")] private Camera Camera;
    [SerializeField, Header("検索する距離")] private float SearchDistance = 10.0f;
    [SerializeField, Header("アクションオブジェクトのタグ")] private string ActionObjectTag = "";


    //視界に入っていて、なおかつ指定したアクションクラスのオブジェクトのなかで、
    //画面中央に最も近いオブジェクトを返す
    public GameObject GetActionObjectInCenterView<T>() where T : ActionObjectBase
    {
        List<GameObject> actionObjects = GetActionObjectsInView<T>();
        float minDistance = float.MaxValue;
        GameObject closestObject = null;

        Vector2 screenCenter = new Vector2(Screen.width / 2.0f, Screen.height / 2.0f);

        foreach (var actionObject in actionObjects)
        {
            Vector3 screenPos = Camera.WorldToScreenPoint(actionObject.transform.position);
            float distance = Vector2.Distance(screenCenter, screenPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = actionObject;
            }
        }

        return closestObject;
    }


    //指定したアクションクラスのオブジェクトが視界に入っているかを判定し、
    //視界に入っている全てのオブジェクトを返す
    public List<GameObject> GetActionObjectsInView<T>() where T : ActionObjectBase
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera);
        GameObject[] targets = GameObject.FindGameObjectsWithTag(ActionObjectTag);
        List<GameObject> foundObjects = new List<GameObject>();

        foreach (var target in targets)
        {
            //ここでアクションクラスを取得
            T actionObject = target.GetComponent<T>();
            if (actionObject == null)
            {
                continue;
            }

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                continue;
            }

            //Boundsが視錐台内に入っているかを判定
            if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds) == false)
            {
                continue;
            }

            //対象からカメラまでの距離を取得
            Vector3 targetPos = renderer.bounds.center;
            Vector3 cameraPos = Camera.transform.position;
            float sqrDistance = (cameraPos - targetPos).sqrMagnitude;

            //対象がアクション可能距離内に入っているかを判定
            if (sqrDistance < SearchDistance * SearchDistance == false)
            {
                continue;
            }

            Vector3 cameraToTargetRay = targetPos - cameraPos;

            if (Physics.Raycast(cameraPos, cameraToTargetRay, out RaycastHit hit, SearchDistance))
            {
                if (hit.collider.gameObject != target)
                {
                    continue;
                }
            }

            //カメラから対象までの間に障害物がない場合
            foundObjects.Add(target);
        }

        return foundObjects;
    }


    void Start()
    {
        if (Camera == null)
        {
            Debug.LogError("カメラがアタッチされていません " + gameObject.name);
        }

        if (string.IsNullOrEmpty(ActionObjectTag))
        {
            Debug.LogError("アクションオブジェクトのタグが設定されていません " + gameObject.name);
        }
    }
}