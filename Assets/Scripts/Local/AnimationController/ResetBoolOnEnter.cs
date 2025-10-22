using UnityEngine;

public class ResetBoolOnEnter : StateMachineBehaviour
{
    [SerializeField] private string boolParameterName;

    // このステートに入った瞬間に呼び出される
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(boolParameterName))
        {
            animator.SetBool(boolParameterName, false);
        }
    }
}