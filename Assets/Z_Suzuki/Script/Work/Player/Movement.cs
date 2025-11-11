using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Movement : MonoBehaviour
{
    [SerializeField, Header("ゼンマイ第一段階の最大速度(秒速)")] private float MainspringStageFirstSpeed = 7.0f;
    [SerializeField, Header("ゼンマイ第一段階の加速度(秒速)")] private float MainspringStageFirstAcceleration = 15.0f;
    [SerializeField, Header("ゼンマイ第二段階の最大速度(秒速)")] private float MainspringStageSecondSpeed = 10.0f;
    [SerializeField, Header("ゼンマイ第二段階の加速度(秒速)")] private float MainspringStageSecondAcceleration = 20.0f;
    [SerializeField, Header("ゼンマイ第三段階の最大速度(秒速)")] private float MainspringStageThirdSpeed = 15.0f;
    [SerializeField, Header("ゼンマイ第三段階の加速度(秒速)")] private float MainspringStageThirdAcceleration = 30.0f;
    [SerializeField, Range(0.0f, 1.0f), Header("慣性の保存率")] private float InertialVelocityPreserve = 0.5f;
    [SerializeField, Range(0.0f, 1.0f), Header("障害物にぶつかったときの速度減衰率")] private float CollisionVelocityDamping = 0.5f;

    private float _speed;
    private float _acceleration;

    private Rigidbody _Rigidbody;
    private bool _IsGrounded = false;
    private MainspringStage _mainspringStage = MainspringStage.Zeroth;

    void Awake()
    {
        _Rigidbody = GetComponent<Rigidbody>();
    }


    // 移動処理
    public void Move(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        Vector3 moveDir;
        float maxSpeed;
        Vector3 velocityXZ;
        Vector3 finalVelocityXZ;
        float acceleration;
        float currentSpeedInDirection;
        float targetSpeed;
        Vector3 newMoveVelocity;
        Vector3 inertialVelocity;

        //進む速度を計算
        moveDir = new Vector3(direction.x, 0.0f, direction.z).normalized;
        velocityXZ = new Vector3(_Rigidbody.linearVelocity.x, 0.0f, _Rigidbody.linearVelocity.z);
        currentSpeedInDirection = Vector3.Dot(velocityXZ, moveDir);
        acceleration = _acceleration;
        targetSpeed = currentSpeedInDirection + acceleration * Time.fixedDeltaTime;
        maxSpeed = _speed;
        targetSpeed = Mathf.Min(targetSpeed, maxSpeed);

        //慣性を計算
        newMoveVelocity = moveDir * targetSpeed;
        inertialVelocity = velocityXZ - moveDir * currentSpeedInDirection;
        inertialVelocity *= InertialVelocityPreserve;

        //最終的な速度を計算
        finalVelocityXZ = newMoveVelocity + inertialVelocity;

        _Rigidbody.linearVelocity = new Vector3(finalVelocityXZ.x, _Rigidbody.linearVelocity.y, finalVelocityXZ.z);
    }


    // 衝突時に減速する処理
    public void Deceleration()
    {
        _Rigidbody.linearVelocity *= CollisionVelocityDamping;
    }


    // プレイヤーを完全に停止させる
    public void Stop()
    {
        _Rigidbody.linearVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        _acceleration = 0.0f;
        _speed = 0.0f;
        _mainspringStage = MainspringStage.Zeroth;
    }

    // ゼンマイの段階を上げる
    public void MainspringStageUp()
    {
        switch (_mainspringStage)
        {
            case MainspringStage.Zeroth:
                {
                    _speed = MainspringStageFirstSpeed;
                    _acceleration = MainspringStageFirstAcceleration;
                    _mainspringStage = MainspringStage.First;
                    break;
                }
            case MainspringStage.First:
                {
                    _speed = MainspringStageSecondSpeed;
                    _acceleration = MainspringStageSecondAcceleration;
                    _mainspringStage = MainspringStage.Second;
                    break;
                }
            case MainspringStage.Second:
                {
                    _speed = MainspringStageThirdSpeed;
                    _acceleration = MainspringStageThirdAcceleration;
                    _mainspringStage = MainspringStage.Third;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }


    // ゼンマイの段階を下げる
    public void MainspringStageDown()
    {
        switch (_mainspringStage)
        {
            case MainspringStage.First:
                {
                    _speed = MainspringStageFirstSpeed;
                    _acceleration = 0.0f;
                    _mainspringStage = MainspringStage.Zeroth;
                    break;
                }
            case MainspringStage.Second:
                {
                    _speed = MainspringStageFirstSpeed;
                    _acceleration = MainspringStageFirstAcceleration;
                    _mainspringStage = MainspringStage.First;
                    break;
                }
            case MainspringStage.Third:
                {
                    _speed = MainspringStageSecondSpeed;
                    _acceleration = MainspringStageSecondAcceleration;
                    _mainspringStage = MainspringStage.Second;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }


    public void SetIsGround(bool g) { _IsGrounded = g; }
    public bool GetIsGround() { return _IsGrounded; }
}
