using UnityEngine;
using Mirror; // Mirror�̋@�\���g�����߂ɕK�{
using System.Collections.Generic;
// �K�v�ȃR���|�[�l���g�������ŃA�^�b�`
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AnimationUpdater))]
public class NetworkPlayerController : NetworkBehaviour // NetworkBehaviour���p��
{
    [Header("�ړ��E��]���x")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float rotateSpeed = 200.0f;
    [SerializeField] private float maxSpeed = 5.0f;

    [Header("�W�����v��")]
    [SerializeField] private float jumpForce = 5.0f;

    [Header("�ڒn����")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    private Rigidbody _rigidbody;
    private AnimationUpdater _animationUpdater;

    // --- �l�b�g���[�N�����p�̕ϐ� ---
    // [SyncVar]�́A�T�[�o�[��̂��̕ϐ��̒l���ύX�����ƁA�����I�ɑS�N���C�A���g�ɓ��������
    // hook��ݒ肷��ƁA�l���ύX���ꂽ�Ƃ��Ɏw�肵�����\�b�h���N���C�A���g���ŌĂяo�����
    [SyncVar(hook = nameof(OnRunningChanged))]
    private bool _isSyncedRunning;
    // ���̃L�����N�^�[���ǂ̎����ɑ����Ă��邩������ID
    [SyncVar]
    public uint matchId = 0;
    // --- ���[�J���v���C���[�p�̕ϐ� ---
    private bool _isGrounded;
    private float _moveInput;
    private float _rotateInput;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animationUpdater = GetComponent<AnimationUpdater>();
    }

    private void Update()
    {
        // ���̃I�u�W�F�N�g���A����PC�ő��삵�Ă���v���C���[�̂��̂łȂ���΁A�ȍ~�̏����͍s��Ȃ�
        // ����ɂ��A���l�̃L�����N�^�[�������̓��͂œ������Ă��܂��̂�h��
        if (!isLocalPlayer) return;

        // --- �ڒn���� ---
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded)
        {
            _animationUpdater.ResetJumpFlags();
        }

        // --- ���͎擾 ---
        _rotateInput = Input.GetAxis("Horizontal");
        _moveInput = Input.GetAxis("Vertical");

        // --- ����A�j���[�V�����̓��� ---
        bool isPressingForward = _moveInput > 0.1f;
        // �����Ԃ��ω��������`�F�b�N
        if (isPressingForward != _isSyncedRunning)
        {
            // �T�[�o�[�ɑ����Ԃ̕ύX�𖽗߂���
            CmdSetRunning(isPressingForward);
        }

        // --- �W�����v ---
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            // �T�[�o�[�ɃW�����v�������Ƃ𖽗߂���
            CmdTriggerJump();
            // ���[�J���ł͑����ɕ����I�ȃW�����v�����s���āA����̃��X�|���X��ǂ�����
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        // ���[�J���v���C���[�̕����I�Ȉړ��Ɖ�]�́A��Ɏ��g�Ōv�Z����
        if (!isLocalPlayer) return;

        transform.Rotate(0f, _rotateInput * rotateSpeed * Time.fixedDeltaTime, 0f);

        if (_moveInput > 0.1f && _rigidbody.linearVelocity.magnitude < maxSpeed)
        {
            _rigidbody.AddForce(transform.forward * moveForce, ForceMode.Force);
        }
    }

    // ============== �l�b�g���[�N���� ==============

    #region �����Ԃ̓���
    /// <summary>
    /// [Command] ����: �N���C�A���g����T�[�o�[�֑����閽��
    /// �����Ԃ��T�[�o�[�ɒʒm���܂��B
    /// </summary>
    [Command]
    private void CmdSetRunning(bool isRunning)
    {
        _isSyncedRunning = isRunning;
    }

    /// <summary>
    /// SyncVar�t�b�N: _isSyncedRunning�̒l���T�[�o�[����N���C�A���g�ɓ������ꂽ�Ƃ��ɌĂ΂��
    /// </summary>
    private void OnRunningChanged(bool oldVal, bool newVal)
    {
        // �S�ẴN���C�A���g�i�������܂ށj�ő���A�j���[�V�������X�V
        _animationUpdater.SetRunning(newVal);
    }
    #endregion

    #region �W�����v�̓���
    /// <summary>
    /// [Command] ����: �N���C�A���g����T�[�o�[�փW�����v���͂�ʒm���܂��B
    /// </summary>
    [Command]
    private void CmdTriggerJump()
    {
        // �T�[�o�[���S�N���C�A���g�ɃW�����v�A�j���[�V�����̍Đ��𖽗߂���
        RpcTriggerJump();
    }

    /// <summary>
    /// [ClientRpc] ����: �T�[�o�[����S�N���C�A���g�֑����閽��
    /// �S�ẴN���C�A���g�ŃW�����v�A�j���[�V�������Đ����܂��B
    /// </summary>
    [ClientRpc]
    private void RpcTriggerJump()
    {
        // ���̃v���C���[�̃W�����v�͕������Z�𔺂�Ȃ��i�ʒu������NetworkTransform���s���j
        // �A�j���[�V�����̃g���K�[�������Ăяo��
        _animationUpdater.TriggerJump();
    }
    #endregion
}