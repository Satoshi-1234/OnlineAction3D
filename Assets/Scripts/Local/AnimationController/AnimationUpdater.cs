using UnityEngine;

public class AnimationUpdater : MonoBehaviour
{
    private Animator _animator;

    // パフォーマンス向上のため、パラメータ名をハッシュ値に変換しておく
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int JumpHash = Animator.StringToHash("jump");
    private static readonly int RunJumpHash = Animator.StringToHash("runJump");

    private void Awake()
    {
        // 自身にアタッチされたAnimatorを取得
        _animator = GetComponentInChildren<Animator>();
    }

    /// <summary>
    /// 走り状態を更新します。移動制御スクリプトから毎フレーム呼び出します。
    /// </summary>
    /// <param name="isRunning">走っている状態ならtrue</param>
    public void SetRunning(bool isRunning)
    {
        _animator.SetBool(RunHash, isRunning);
    }

    /// <summary>
    /// ジャンプを開始します。ジャンプ入力があった瞬間に呼び出します。
    /// </summary>
    public void TriggerJump()
    {
        // 現在の走り状態を取得し、適切なジャンプアニメーションを再生
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
    /// 全てのジャンプ関連フラグをリセットします。接地時などに呼び出します。
    /// </summary>
    public void ResetJumpFlags()
    {
        _animator.SetBool(JumpHash, false);
        _animator.SetBool(RunJumpHash, false);
    }
}