using UnityEngine;

public class ResetBoolOnEnter : StateMachineBehaviour
{
    [SerializeField] private string boolParameterName;

    // ���̃X�e�[�g�ɓ������u�ԂɌĂяo�����
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(boolParameterName))
        {
            animator.SetBool(boolParameterName, false);
        }
    }
}