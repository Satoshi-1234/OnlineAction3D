//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UIElements;

//public class WalkerObstacles : ObstaclesBase
//{
//    enum WalkerState
//    {
//        None,
//        Search,
//        Walk,
//    }


//    [SerializeField, Header("進む速度")] private float MoveSpeed = 5.0f;
//    [SerializeField, Header("回転する速度")] private float RotationSpeed = 10.0f;
//    [SerializeField, Header("壁を検知するレイの長さ")] private float RaycastDistance = 1.5f;
//    [SerializeField, Header("壁から離れる力の強さ")] private float WallAvoidanceStrength = 2f;
//    [SerializeField, Header("壁との距離")] private float IdealWallDistance = 1f;
//    [SerializeField, Header("壁として認識するオブジェクト")] private Transform WallTransform;
//    [SerializeField, Header("壁として認識するレイヤー")] private LayerMask WallLayer;
//    [SerializeField, Header("時計回りにする")] private bool IsClockwise = false;


//    private WalkerState _currentState = WalkerState.None;
//    private Quaternion _targetRotation;


//    protected override void DoUpdate()
//    {
//        //switch(_currentState)
//        //{
//        //    case WalkerState.None:
//        //        _currentState = WalkerState.Search;
//        //        break;

//        //    case WalkerState.Search:
//        //        Search();
//        //        break;

//        //    case WalkerState.Walk:
//        //        Walk();
//        //        break;
//        //}

//        Vector3 rayOrigin = transform.position;
//        Vector3 rayDirectionLeft = -transform.right;
//        Vector3 rayDirectionForward = transform.forward;

//        Debug.DrawRay(rayOrigin, rayDirectionLeft * RaycastDistance, Color.red);
//        Debug.DrawRay(rayOrigin, rayDirectionForward * RaycastDistance, Color.blue);

//        RaycastHit hit;

//        //オブジェクトから見て左側に壁があるかチェックし、あった場合壁に平行な方向に進む。
//        if (Physics.Raycast(rayOrigin, rayDirectionLeft, out hit, RaycastDistance, WallLayer))
//        {
//            //壁に平行な進行方向を計算
//            Vector3 targetDirection = Vector3.Cross(hit.normal, Vector3.up);

//            // 計算した進行方向が、現在の進行方向と真逆を向いていないかチェック
//            if (Vector3.Dot(targetDirection, transform.forward) < 0)
//            {
//                targetDirection *= -1;
//            }

//            //壁との距離を設定した値に保つ
//            float distanceError = hit.distance - IdealWallDistance;
//            targetDirection -= hit.normal * distanceError * WallAvoidanceStrength;

//            //計算した進行方向に向かって回転
//            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
//        }
//        //左側に壁がなく、前方に壁があった場合右に回転して回避
//        else if (Physics.Raycast(rayOrigin, rayDirectionForward, out hit, RaycastDistance, WallLayer))
//        {
//            transform.Rotate(0, RotationSpeed * Time.deltaTime * 10f, 0);
//        }
//        //左側に壁が無かった場合左に回転して壁を探す
//        else
//        {
//            transform.Rotate(0, -RotationSpeed * Time.deltaTime * 10f, 0);
//        }

//        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
//    }


//    private void Search()
//    {
//        Vector3 rayOrigin = transform.position;
//        Vector3 rayDirectionSide;
//        Vector3 rayDirectionForward = transform.forward;

//        if (IsClockwise)
//        {
//            rayDirectionSide = -transform.right;
//        }
//        else
//        {
//            rayDirectionSide = transform.right;
//        }

//        Debug.DrawRay(rayOrigin, rayDirectionSide * RaycastDistance, Color.red);
//        Debug.DrawRay(rayOrigin, rayDirectionForward * RaycastDistance, Color.blue);

//        RaycastHit hit;

//        if (Physics.Raycast(rayOrigin, rayDirectionForward, out hit, RaycastDistance) &&
//            hit.transform.gameObject == WallTransform.gameObject)
//        {
//            _currentState = WalkerState.Walk;
//            return;
//        }

//        Vector3 wallPosition = new Vector3(WallTransform.position.x, transform.position.y, WallTransform.position.z);
//        Vector3 direction = (wallPosition - transform.position).normalized;

//        Quaternion targetRotation = Quaternion.LookRotation(direction);
//        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

//        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
//    }


//    private void Walk()
//    {
//        Vector3 rayOrigin = transform.position;
//        Vector3 rayDirectionSide;
//        Vector3 rayDirectionForward = transform.forward;

//        if (IsClockwise)
//        {
//            rayDirectionSide = -transform.right;
//        }
//        else
//        {
//            rayDirectionSide = transform.right;
//        }

//        Debug.DrawRay(rayOrigin, rayDirectionSide * RaycastDistance, Color.red);
//        Debug.DrawRay(rayOrigin, rayDirectionForward * RaycastDistance, Color.blue);

//        RaycastHit hitForward;
//        RaycastHit hitSide;

//        if (Physics.Raycast(rayOrigin, rayDirectionForward, out hitForward, RaycastDistance) &&
//            hitForward.transform.gameObject == WallTransform.gameObject)
//        {
//            Vector3 targetDirection = Vector3.Cross(hitForward.normal, Vector3.up);

//            if (IsClockwise == false)
//            {
//                targetDirection *= -1;
//            }

//            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
//            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
//            transform.rotation = targetRotation;
//        }
//        else
//        {


//            if (Physics.Raycast(rayOrigin, rayDirectionSide, out hitSide, RaycastDistance) &&
//                hitSide.transform.gameObject == WallTransform.gameObject)
//            {

//                Vector3 targetDirection = Vector3.Cross(hitSide.normal, Vector3.up);

//                if (IsClockwise == false)
//                {
//                    targetDirection *= -1;
//                }

//                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
//                //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
//                transform.rotation = targetRotation;
//            }
//            else
//            {
//                if (IsClockwise)
//                {
//                    transform.Rotate(0, -RotationSpeed * 10.0f * Time.deltaTime, 0);
//                }
//                else
//                {
//                    transform.Rotate(0, RotationSpeed * 10.0f * Time.deltaTime, 0);
//                }
//            }
//        }

//        //if (Physics.Raycast(rayOrigin, rayDirectionSide, out hit, RaycastDistance) ||
//        //    Physics.Raycast(rayOrigin, rayDirectionForward, out hit, RaycastDistance) &&
//        //    hit.transform.gameObject == WallTransform.gameObject)
//        //{
//        //    if (Physics.Raycast(rayOrigin, rayDirectionSide, out hit, RaycastDistance) &&
//        //        Physics.Raycast(rayOrigin, rayDirectionForward, out hit, RaycastDistance))
//        //    {


//        //    }

//        //    Vector3 targetDirection = Vector3.Cross(hit.normal, Vector3.up);

//        //    if (IsClockwise == false)
//        //    {
//        //        targetDirection *= -1;
//        //    }

//        //    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
//        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
//        //}


//        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);

//        //float distance = Vector3.Distance(WallTransform.position, transform.position);

//        //if (distance > RaycastDistance)
//        //{
//        //    _oldState = _currentState;
//        //    _currentState = WalkerState.Search;
//        //    return;
//        //}
//    }


//    private void Start()
//    {
//        if (WallTransform == null)
//        {
//            Debug.LogError("WallTransformが設定されていません " + gameObject.name);
//        }
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WalkerObstacles : ObstaclesBase
{
    [SerializeField, Header("進む速度")] private float MoveSpeed = 5.0f;
    [SerializeField, Header("回転する速度")] private float RotationSpeed = 10.0f;
    [SerializeField, Header("壁を検知するレイの長さ")] private float RaycastDistance = 1.5f;
    [SerializeField, Header("壁から離れる力の強さ")] private float WallAvoidanceStrength = 2f;
    [SerializeField, Header("壁との距離")] private float IdealWallDistance = 1f;
    [SerializeField, Header("壁として認識するレイヤー")] private LayerMask WallLayer;


    protected override void DoUpdate()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirectionLeft = -transform.right;
        Vector3 rayDirectionForward = transform.forward;

        Debug.DrawRay(rayOrigin, rayDirectionLeft * RaycastDistance, Color.red);
        Debug.DrawRay(rayOrigin, rayDirectionForward * RaycastDistance, Color.blue);

        RaycastHit hit;

        //オブジェクトから見て左側に壁があるかチェックし、あった場合壁に平行な方向に進む。
        if (Physics.Raycast(rayOrigin, rayDirectionLeft, out hit, RaycastDistance, WallLayer))
        {
            //壁に平行な進行方向を計算
            Vector3 targetDirection = Vector3.Cross(hit.normal, Vector3.up);

            // 計算した進行方向が、現在の進行方向と真逆を向いていないかチェック
            if (Vector3.Dot(targetDirection, transform.forward) < 0)
            {
                targetDirection *= -1;
            }

            //壁との距離を設定した値に保つ
            float distanceError = hit.distance - IdealWallDistance;
            targetDirection -= hit.normal * distanceError * WallAvoidanceStrength;

            //計算した進行方向に向かって回転
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
        //左側に壁がなく、前方に壁があった場合右に回転して回避
        else if (Physics.Raycast(rayOrigin, rayDirectionForward, out hit, RaycastDistance, WallLayer))
        {
            transform.Rotate(0, RotationSpeed * Time.deltaTime * 10f, 0);
        }
        //左側に壁が無かった場合左に回転して壁を探す
        else
        {
            transform.Rotate(0, -RotationSpeed * Time.deltaTime * 10f, 0);
        }

        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }
}
