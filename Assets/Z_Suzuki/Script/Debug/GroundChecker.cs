using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class GroundChecker : MonoBehaviour
{
    [SerializeField, Header("CastRayの発生元を少し上にずらす距離")] private float _Epsilon = 0.005f;

    [SerializeField, Header("Colliderと地面の距離がこの値より小さければ接地と判定")] private float _loatingDistance = 0.01f;

    [SerializeField, Header("どのレイヤーのオブジェクトに当たったら")] private LayerMask _GroundLayerMask;

    private CapsuleCollider _Collider;
    private bool _IsGround = false;
    private bool _IsRoof = false;

    private RaycastHit _Hit;

    //実際には接地していないが、接地しているということにするブール    例えば地面から離れてから何フレームかジャンプ出来るようにするイメージ
    private bool _IsGroundFake = false;
    [SerializeField, Header("接地している判定の持続猶予フレーム")] int _IsGroundFrame = 5;
    private int _IsGroundCount = 0;

    private float _HalfHeight;
    private float _Radius;
    private float _Distacce;

    void Start()
    {
        _Collider = GetComponent<CapsuleCollider>();
        _HalfHeight = (_Collider.height * 0.5f) * transform.lossyScale.y;
        _Radius = _Collider.radius * transform.lossyScale.x;
        _Distacce = (_HalfHeight - _Radius) + _loatingDistance + _Epsilon;
    }

    private void FixedUpdate()
    {
        _IsGround = Physics.SphereCast(
            _Collider.transform.position + new Vector3(0, _Epsilon, 0),
            _Radius,
            Vector3.down,
            out _Hit,
            _Distacce,
            _GroundLayerMask);

        _IsRoof = Physics.SphereCast(
            _Collider.transform.position - new Vector3(0, _Epsilon, 0),
            _Radius,
            Vector3.up,
            out _Hit,
            _Distacce,
            _GroundLayerMask);

        //------------------------------------------------
        //接地猶予フレームのカウント
        //------------------------------------------------
        if (!_IsGround)//空中に出るとカウントが進む
        {
            ++_IsGroundCount;
        }
        else if (_IsGroundCount != 0)//接地したらリセット
        {
            _IsGroundCount = 0;
        }


        if (_IsGroundCount > _IsGroundFrame)//猶予フレームを超えると、偽の接地判定もfalse
        {
            _IsGroundFake = false;
        }
        if ((!_IsGroundFake) && (_IsGround))
        {
            _IsGroundFake = true;
        }

    }

    public bool GetIsGroundFake() { return _IsGroundFake; }

    public bool GetIsGround() { return _IsGround; }

    public bool GetIsRoof() { return _IsRoof; }
}
