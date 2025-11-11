using UnityEngine;

public class ActionObjectBase : MonoBehaviour
{
    public bool GetActionable() { return _isActionable; }
    public void SetActionable(bool actionable) { _isActionable = actionable; }


    //アクション時にオブジェクトに何らかの処理を行わせたい場合は、この関数をオーバーライドして記述する
    public virtual void Action() { }


    //FixedUpdateで行いたい処理はこの関数をオーバーライドして記述する
    protected virtual void DoFixedUpdate() { }


    private bool _isActionable = false;
    private bool _oldIsActionable = false;


    private void FixedUpdate()
    {
        DoFixedUpdate();

        if (_isActionable == _oldIsActionable)
        {
            return;
        }

        if (_isActionable)
        {
            Debug.Log(gameObject.name + " is Actionable");
        }
        else
        {
            Debug.Log(gameObject.name + " is Not Actionable");
        }
        _oldIsActionable = _isActionable;
    }
}
