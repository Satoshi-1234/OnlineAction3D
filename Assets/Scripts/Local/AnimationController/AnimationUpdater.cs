using UnityEngine;

public class AnimationUpdater : MonoBehaviour
{
    private Animator _animator;

    // �p�t�H�[�}���X����̂��߁A�p�����[�^�����n�b�V���l�ɕϊ����Ă���
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int RunJumpHash = Animator.StringToHash("runJump");

    private void Awake()
    {
        // ���g�ɃA�^�b�`���ꂽAnimator���擾
        _animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// �����Ԃ��X�V���܂��B�ړ�����X�N���v�g���疈�t���[���Ăяo���܂��B
    /// </summary>
    /// <param name="isRunning">�����Ă����ԂȂ�true</param>
    public void SetRunning(bool isRunning)
    {
        _animator.SetBool(RunHash, isRunning);
    }

    /// <summary>
    /// �W�����v���J�n���܂��B�W�����v���͂��������u�ԂɌĂяo���܂��B
    /// </summary>
    public void TriggerJump()
    {
        // ���݂̑����Ԃ��擾���A�K�؂ȃW�����v�A�j���[�V�������Đ�
        if (_animator.GetBool(RunHash))
        {
            _animator.SetBool(RunJumpHash, true);
        }
        else
        {
            _animator.SetBool(JumpHash, true);
        }
    }

    /// <summary>
    /// �S�ẴW�����v�֘A�t���O�����Z�b�g���܂��B�ڒn���ȂǂɌĂяo���܂��B
    /// </summary>
    public void ResetJumpFlags()
    {
        _animator.SetBool(JumpHash, false);
        _animator.SetBool(RunJumpHash, false);
    }
}