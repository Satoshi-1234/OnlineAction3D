using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAnimationLogger : SingletonBase<ObjectAnimationLogger>
{
    [SerializeField, Header("ÉAÉjÉÅÅ[ÉVÉáÉìÇãLò^Ç∑ÇÈä‘äu(ïb)")] float RecordInterval = 1.0f;


    public Dictionary<int, List<bool>> GetAnimationBoolList() { return _animationDatas; }
    public float GetRecordInterval() { return RecordInterval; }
    

    private Dictionary<int, List<bool>> _animationDatas = new Dictionary<int, List<bool>>();
    private List<int> _hashs = new List<int>();
    private Animator _targetObject;
    private float _intervalCount = 0.0f;


    private void Start()
    {
        _hashs.Add(Animator.StringToHash("run"));
        _hashs.Add(Animator.StringToHash("jump"));
        _hashs.Add(Animator.StringToHash("runJump"));

        foreach (var hash in _hashs)
        {
            _animationDatas.Add(hash, new List<bool>());
        }
    }


    void FixedUpdate()
    {
        if (_targetObject == null)
        {
            Initialized();
        }

        AnimationRecord(Time.fixedDeltaTime);
    }


    private void AnimationRecord(float deltaTime)
    {
        _intervalCount += deltaTime;

        if (_intervalCount >= RecordInterval)
        {
            _intervalCount = 0.0f;
            foreach (var hash in _hashs)
            {
                _animationDatas[hash].Add(_targetObject.GetBool(hash));
                Debug.Log(hash + " : " + _animationDatas[hash].Count);
            }
            
        }
    }


    private void Initialized()
    {
        if (NetworkClient.localPlayer.GetComponent<NetworkPlayerController>())
        {
            _targetObject = NetworkClient.localPlayer.GetComponent<AnimationUpdater>().GetAnimator();
        }
    }
}
