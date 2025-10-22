using UnityEngine;

// �K�v�ȃR���|�[�l���g�������ŃA�^�b�`����
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(AnimationUpdater))]
public class LocalPlayerController : MonoBehaviour
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
        // --- �ڒn���� ---
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (_isGrounded)
        {
            _animationUpdater.ResetJumpFlags();
        }

        // --- ���͎擾 ---
        _rotateInput = Input.GetAxis("Horizontal");
        _moveInput = Input.GetAxis("Vertical");

        // --- �A�j���[�V�����̍X�V ---
        bool isPressingForward = _moveInput > 0.1f; // 0.1f�̂������l�ŃA�i���O�X�e�B�b�N�̔����ȌX���𖳎�
        _animationUpdater.SetRunning(isPressingForward);

        // --- �W�����v���� ---
        // GetButtonDown�͖��t���[���Ă΂��Update���Ń`�F�b�N����
        HandleJump();
    }

    // �������Z��FixedUpdate�Ŏ��s
    private void FixedUpdate()
    {
        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        transform.Rotate(0f, _rotateInput * rotateSpeed * Time.fixedDeltaTime, 0f);
    }

    private void HandleMovement()
    {
        // �����I�Ȉړ��͓���(_moveInput)�����ɍs���i�����͕ύX�Ȃ��j
        if (_moveInput > 0.1f)
        {
            if (_rigidbody.linearVelocity.magnitude < maxSpeed)
            {
                _rigidbody.AddForce(transform.forward * moveForce, ForceMode.Force);
            }
        }
        // W�L�[��������Ă��炸�A���ڒn���Ă���ꍇ
        else if (_isGrounded)
        {
            // ���������̑��x�������I��0�ɂ��A�L�����N�^�[���u�s�^�b�v�Ǝ~�߂�
            // ��������(Y)�̑��x�͂��̂܂܂ɂ��āA�W�����v��d�͂ɉe�����o�Ȃ��悤�ɂ���
            _rigidbody.linearVelocity = new Vector3(0f, _rigidbody.linearVelocity.y, 0f);
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _animationUpdater.TriggerJump();
        }
    }
}