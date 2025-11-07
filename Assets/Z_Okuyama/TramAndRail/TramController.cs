using UnityEngine;
using UnityEngine.Splines;

public class TramController : MonoBehaviour
{
	//Enum==================================================
	enum TStartMode
	{
		Front,
		Back,
	}


	//Variable==================================================
	Spline _spline;
	[Header("RailAndTram")]
	[SerializeField] SplineContainer _splineContainer;
	[SerializeField] GameObject _tramObject;
	[SerializeField] RailBuilder _railBuilder;
	[SerializeField, Min(0.1f)] Vector3 _tramSize = new Vector3(1f, 1f, 1f);

	[Header("StartPosition")]
	[SerializeField, Range(0f, 1f)] float _tStart = 0f;
	[SerializeField] TStartMode _tStartMode = TStartMode.Front;

	[Header("Move")]
	//速度
	const float TramSpeedMin = 0f;
	const float TramSpeedMax = 20f;
	//[SerializeField, Range(TramSpeedMin, TramSpeedMax)] float _tramSpeedDefault = 1f;
	[SerializeField, Range(TramSpeedMin, TramSpeedMax)] float _tramSpeed = 1f;
	//[SerializeField] bool _tramSpeedDebug = false;

	//カーブ
	//[SerializeField, Min(0f)]
	float _tauRot = 0.05f;//時間定数
	Vector3 _fwdSmoothed;
	Vector3 _upSmoothed;
	bool _rotInit;

	float _tFront;
	float _tBack;
	float _tramLength;


	//Start==================================================
	private void Start()
	{
		//GetComponent
		_spline = _splineContainer.Spline;

		//SetVariable
		//_tramSpeed = _tramSpeedDefault;
		_tramLength = SplineUtility.CalculateLength(_spline, transform.localToWorldMatrix);
		_rotInit = false;

		//tの前面と後面位置合わせ
		float tSub = _tramSize.z / _tramLength;
		switch (_tStartMode)
		{
			case TStartMode.Front:
				_tFront = _tStart;
				_tBack = _tFront - tSub;
				if (_tBack < 0f)
				{
					_tBack += 1f;
				}

				break;

			case TStartMode.Back:
				_tBack = _tStart;
				_tFront = _tBack + tSub;
				if (1f < _tFront)
				{
					_tFront -= 1f;
				}

				break;

			default:
				break;
		}
	}


	//Update==================================================
	private void Update()
	{
		/*
		if (_tramSpeedDebug && _tramSpeed != _tramSpeedDefault)
		{
			_tramSpeed = _tramSpeedDefault;
		}
		*/
		AddT();
		SetTramPosition();
	}


	void AddT()
	{
		float move = Time.deltaTime * _tramSpeed * 0.1f;

		_tFront += move;
		_tBack += move;

		_tFront %= 1f;
		_tBack %= 1f;
	}

	void SetTramPosition()
	{
		Transform transform = _tramObject.transform;

		//位置
		Vector3 frontPos = _splineContainer.EvaluatePosition(_tFront);
		Vector3 backPos = _splineContainer.EvaluatePosition(_tBack);
		Vector3 pos = (frontPos + backPos) * 0.5f;
		if (_railBuilder)
		{
			pos.y = pos.y + _railBuilder.GetRailThickness();
		}

		transform.position = pos;

		//接線/回転
		float tCenter = MidTNormalized(_tFront, _tBack);
		Vector3 forwardRaw = (frontPos - backPos).normalized;

		//Up計算
		Vector3 upRaw;
		if (_railBuilder)
		{
			Vector3 left = _railBuilder.GetLeftWorldAt(tCenter);
			Vector3 right = _railBuilder.GetRightWorldAt(tCenter);
			Vector3 rightVec = (right - left).normalized;
			upRaw = Vector3.Cross(forwardRaw, rightVec).normalized;
		}
		else
		{
			upRaw = _splineContainer.EvaluateUpVector(tCenter);
			upRaw = upRaw.normalized;
		}

		float s = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(1e-4f, _tauRot));

		if (!_rotInit)
		{
			_fwdSmoothed = forwardRaw;
			_upSmoothed = upRaw;
			_rotInit = true;
		}
		else
		{
			//スムージング⇒一旦なし
			//_fwdSmoothed = Vector3.Slerp(_fwdSmoothed, forwardRaw, s).normalized;
			_fwdSmoothed = forwardRaw;
			_upSmoothed = Vector3.Slerp(_upSmoothed, upRaw, s).normalized;

			//直交化
			var right = Vector3.Cross(_upSmoothed, _fwdSmoothed).normalized;
			_upSmoothed = Vector3.Cross(_fwdSmoothed, right).normalized;
		}

		transform.rotation = Quaternion.LookRotation(_fwdSmoothed, _upSmoothed);
	}


	//計算系==================================================
	//tからForward取得
	/*
	Vector3 GetForward(float t, float dtT)
	{
		float t0 = Mathf.Repeat(t - dtT, 1f);
		float t1 = Mathf.Repeat(t + dtT, 1f);

		Vector3 p0 = _splineContainer.EvaluatePosition(t0);
		Vector3 p1 = _splineContainer.EvaluatePosition(t1);

		Vector3 fwd = (p1 - p0);
		if (fwd.sqrMagnitude < 1e-8f) return _fwdSmoothed != Vector3.zero ? _fwdSmoothed : transform.forward;
		return fwd.normalized;
	}
	*/

	//角度に直して最短差分
	float MidTNormalized(float tA, float tB)
	{
		float a = tA * 360f;
		float b = tB * 360f;
		float mid = b + Mathf.DeltaAngle(b, a) * 0.5f;
		return Mathf.Repeat(mid / 360f, 1f);
	}
}
