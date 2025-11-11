using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclesBase : MonoBehaviour
{
    [SerializeField, Header("”j‰óŽ‘±ŽžŠÔ")] private int DestructionDuration = 3;
    [SerializeField, Header("”j‰óŽž‚ÌSE(ƒtƒ@ƒCƒ‹–¼)")] private string DestructionSE = "";
    private float _destructionTimer = 0.0f;
    private bool _isDestruction = false;


    public void Destruction()
    {
        _isDestruction = true;
        SoundManager.Instance.PlaySE(DestructionSE, transform.position);
    }


    protected virtual void DoUpdate() { }


    protected virtual void DoFixedUpdate() { }


    private void Update()
    {
        DoUpdate();
    }


    private void FixedUpdate()
    {
        if (_isDestruction)
        {
            _destructionTimer += Time.fixedDeltaTime;
            if (_destructionTimer > DestructionDuration)
            {
                _isDestruction = false;
            }
        }

        DoFixedUpdate();
    }
}
