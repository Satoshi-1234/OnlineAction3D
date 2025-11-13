using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;


public class SplineCartEaseSpeed : MonoBehaviour
{
    [SerializeField, Header("対象のSplineCart")] private CinemachineSplineCart DollyCart;
    [SerializeField, Header("SplineCartの開始時速度")] private float StartSpeed = 10.0f;
    [SerializeField, Header("SplineCartの終了時速度")] private float EndSpeed = 1.0f;
    [SerializeField, Header("速度の遷移の仕方")] private AnimationCurve SpeedCurve = AnimationCurve.Linear(0, 0, 1, 1);


    public bool IsCompleted() { return _isCompleted; }


    private const int FLOOR_DIGITS = 100;
    private SplineContainer _dollyTrack;
    private float _splineLength = 0.0f;
    private bool _isCompleted = false;
    private float _cartSpeed = 0.0f;
    private float _intermediary = 0.0f;


    void Start()
    {
        if (DollyCart == null)
        {
            Debug.LogError("CinemachineSplineCartがアタッチされていません " + gameObject.name);
            return;
        }
        if (DollyCart.Spline == null)
        {
            Debug.LogError("アタッチされているCinemachineSplineCartに、SplineContainerが設定されていません " + gameObject.name);
            return;
        }

        if (DollyCart.PositionUnits != PathIndexUnit.Distance)
        {
            DollyCart.PositionUnits = PathIndexUnit.Distance;
        }

        _dollyTrack = DollyCart.Spline as SplineContainer;
        _splineLength = _dollyTrack.CalculateLength();
        _cartSpeed = StartSpeed;
    }


    void Update()
    {
        SplineCartMove(Time.deltaTime);
    }


    private void SplineCartMove(float deltaTime)
    {
        float progress = DollyCart.SplinePosition / _splineLength;

        _intermediary = SpeedCurve.Evaluate(progress);
        _intermediary = Mathf.Floor(_intermediary * FLOOR_DIGITS) / FLOOR_DIGITS;
        _cartSpeed = Mathf.Lerp(StartSpeed, EndSpeed, _intermediary);

        DollyCart.SplinePosition += _cartSpeed * deltaTime;

        if (_intermediary >= 1 && _isCompleted == false)
        {
            _isCompleted = true;
        }
    }
}
