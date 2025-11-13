using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRecordReplay : MonoBehaviour
{
    [SerializeField, Header("記録を再現させるオブジェクト")] GameObject TargetObject;
    [SerializeField, Header("再生速度(倍率)")] float ReplaySpeedMultiplier = 1.0f;


    private Animator _animator;
    private Dictionary<int, List<bool>> _animationDatas;
    private bool _isPlaying = false;
    private int _currentFrameIndex = 0;


    void Start()
    {
        if (TargetObject == null)
        {
            Debug.LogError("記録を再現させるオブジェクトがアタッチされていません " + gameObject.name);
        }

        _animator = TargetObject.GetComponentInChildren<Animator>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && _isPlaying == false)
            StartCoroutine(StartReplay());
    }


    private IEnumerator StartReplay()
    {
        _animationDatas = ObjectAnimationLogger.Instance.GetAnimationBoolList();

        Debug.Log("リプレイ再生開始");
        _isPlaying = true;

        while (_isPlaying && _currentFrameIndex < _animationDatas[Animator.StringToHash("run")].Count)
        {
            foreach (var data in _animationDatas)
            {
                _animator.SetBool(data.Key, data.Value[_currentFrameIndex]);
            }

            float recordInterval = ObjectAnimationLogger.Instance.GetRecordInterval();
            _currentFrameIndex++;

            yield return new WaitForSeconds(recordInterval / ReplaySpeedMultiplier);
        }

        _isPlaying = false;
        Debug.Log("リプレイ再生終了");
    }
}
