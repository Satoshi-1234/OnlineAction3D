using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class Jump : MonoBehaviour
{
    [SerializeField, Header("ジャンプで上昇する高さ")] float _jumpheight = 5.0f;
    [SerializeField, Header("ジャンプで上昇するスピード倍率")] private float _jumpupspeedscare = 1.5f;
    [SerializeField, Header("ジャンプで下降するスピード倍率")] private float _jumpfallspeedscare = 1.5f;

    private Rigidbody _rigidbody;

    private float _jumpvelocity;

    private bool _is_ground;

    private float _basegravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _basegravity = Mathf.Abs(Physics.gravity.y);

        _rigidbody = GetComponent<Rigidbody>();

        CalculateJumpVelocity();
    }

    public void StartJump()
    {
        _rigidbody.linearVelocity = new Vector3(_rigidbody.linearVelocity.x, _jumpvelocity, _rigidbody.linearVelocity.z);
    }

    public void QuitJump()
    {
    }

    private void FixedUpdate()
    {
        float jumpspeedscale;
        if (_is_ground)
        {
            jumpspeedscale = 1.0f;
        }
        else if (0 < _rigidbody.linearVelocity.y)
        {
            jumpspeedscale = _jumpupspeedscare;
        }
        else
        {
            jumpspeedscale = _jumpfallspeedscare;
        }

        float ag = -_basegravity * jumpspeedscale;

        _rigidbody.linearVelocity += Vector3.up * ag * Time.fixedDeltaTime;
    }

    // 指定の高さまで飛ぶジャンプの初速
    private void CalculateJumpVelocity()
    {
        _jumpvelocity = Mathf.Sqrt(2 * (_basegravity * _jumpupspeedscare) * _jumpheight);
    }

    void OnValidate()
    {
        CalculateJumpVelocity();
    }

    public void SetIsGround(bool isground)
    {
        _is_ground = isground;
    }
}
