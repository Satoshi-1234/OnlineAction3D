using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class RickshawObstacles : ObstaclesBase
{
    [SerializeField, Header("進む速度")] private float MoveSpeed = 5.0f;
    [SerializeField, Header("壁を検知するレイの長さ")] private float RaycastDistance = 1.5f;
    [SerializeField, Header("壁として認識するレイヤー")] private LayerMask WallLayer;
    [SerializeField, Header("反転する速度")] private float TurnSpeed = 10f;


    enum RickshawState
    {
        Move,
        Turn
    }


    private RickshawState _currentState = RickshawState.Move;
    private Vector3 _turnDirection;

    protected override void DoUpdate()
    {
        switch(_currentState)
        {
            case RickshawState.Move:
                MoveState();
                break;
            case RickshawState.Turn:
                TurnState();
                break;
        }
    }

    private void MoveState()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirectionForward = transform.forward;

        Debug.DrawRay(rayOrigin, rayDirectionForward * RaycastDistance, Color.red);

        //壁があるかチェックし、あった場合反転状態に移行
        if (Physics.Raycast(rayOrigin, rayDirectionForward, out RaycastHit hit, RaycastDistance, WallLayer))
        {
            _currentState = RickshawState.Turn;
            _turnDirection = -transform.forward;
        }

        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }


    private void TurnState()
    {
        Vector3 targetDirection = _turnDirection;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, TurnSpeed * Time.deltaTime);

        //目標の方向に近づいたら移動状態に移行
        if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        {
            _currentState = RickshawState.Move;
        }
    }
}
