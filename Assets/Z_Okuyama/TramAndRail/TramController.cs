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

	[Header("Move")]
	//速度
	const float kTramSpeedMax = 20f;
	[SerializeField, Range(-kTramSpeedMax, kTramSpeedMax)] float _tramSpeed = 1f;

	[Header("Curve")]

	[SerializeField, Range(0.1f, 1f)] float _curveSpeedNorm = 0.5f;
	[SerializeField, Range(0.001f, 1f)] float _curveSpeedSmooth = 0.05f;

	[SerializeField, Range(0.001f, 10f)] float _curveLength = 1.0f;
	[SerializeField, Range(1, 10)] int _samplesPerSide = 4;//片側サンプル数
	[SerializeField, Range(0.1f, 20)] float _maxAnglePerStep = 12f;//ステップの上限
	float _movePrev;

	float _rotSmoothTimeConst = 0.05f;//時間定数
	Vector3 _fwdSmoothed;
	Vector3 _upSmoothed;
	bool _rotInit;

	float _tFrontNorm;
	float _tFBackNorm;
	float _tramLength;


	//Start==================================================
	private void Start()
	{
		//GetComponent
		_spline = _splineContainer.Spline;

		//SetVariable
		_tramLength = SplineUtility.CalculateLength(_spline, transform.localToWorldMatrix);
		_rotInit = false;

		//tの前面と後面位置合わせ
		float tSub = _tramSize.z / _tramLength;
		_tFrontNorm = Mathf.Repeat(_tStart + tSub * 0.5f, 1f);
		_tFBackNorm = Mathf.Repeat(_tStart - tSub * 0.5f, 1f);
	}


	//Update==================================================
	private void FixedUpdate()
	{
		AddT();
		SetTramPosition();
	}


	void AddT()
	{
		const float kSpeedScale = 0.1f;

		float t = GetForwardT(_tFrontNorm, _tFBackNorm, _tramSpeed);
		float curve = ((1 - GetCurveSharpness(t, _curveLength, _samplesPerSide))
			+ (1 - GetCurveSharpness(
				(_tramSpeed > 0) ? _tFrontNorm : _tFBackNorm,
				_tramLength / (_samplesPerSide * 0.5f),
				_samplesPerSide)))
			* 0.5f;


		//移動速度
		float speed = curve * _tramSpeed
			+ (1 - curve) * _tramSpeed * _curveSpeedNorm;

		//移動距離
		float move = Time.deltaTime * speed * kSpeedScale;
		float smoothFactor = 1f - Mathf.Exp(-_curveSpeedSmooth * Time.deltaTime * 60f);
		move = Mathf.Lerp(_movePrev, move, smoothFactor);
		_movePrev = move;

		//移動
		_tFrontNorm = Mathf.Repeat(_tFrontNorm + move, 1f);
		_tFBackNorm = Mathf.Repeat(_tFBackNorm + move, 1f);
	}

	void SetTramPosition()
	{
		Transform transform = _tramObject.transform;

		//位置
		Vector3 frontPos = _splineContainer.EvaluatePosition(_tFrontNorm);
		Vector3 backPos = _splineContainer.EvaluatePosition(_tFBackNorm);
		Vector3 pos = (frontPos + backPos) * 0.5f;
		if (_railBuilder)
		{
			pos.y = pos.y + _railBuilder.GetRailThickness();
		}

		transform.position = pos;

		//接線/回転
		float tCenter = MidTNormalized(_tFrontNorm, _tFBackNorm);
		Vector3 forwardRaw = (frontPos - backPos).normalized;

		//Up計算
		Vector3 upRaw;
		if (_railBuilder)
		{
			Vector3 left = _railBuilder.GetRailLeftPosWorld(tCenter);
			Vector3 right = _railBuilder.GetRailRightPosWorld(tCenter);
			Vector3 rightVec = (right - left).normalized;
			upRaw = Vector3.Cross(forwardRaw, rightVec).normalized;
		}
		else
		{
			upRaw = _splineContainer.EvaluateUpVector(tCenter);
			upRaw = upRaw.normalized;
		}

		float s = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(1e-4f, _rotSmoothTimeConst));

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

	//角度に直して最短差分
	float MidTNormalized(float tA, float tB)
	{
		float a = tA * 360f;
		float b = tB * 360f;
		float mid = b + Mathf.DeltaAngle(b, a) * 0.5f;
		return Mathf.Repeat(mid / 360f, 1f);
	}

	float GetForwardT(float tFrontNorm, float tBackNorm, float speed)
	{
		bool front = (speed > 0f);
		float t = (front) ? tFrontNorm : tBackNorm;
		float sub = -Mathf.Abs(tFrontNorm - tBackNorm) + _curveLength * _samplesPerSide * 0.01f;

		t += (front) ? sub : -sub;
		t = Mathf.Repeat(t, 1f);

		return t;
	}

	//カーブ度合い
	float GetCurveSharpness(float t, float curveLength, int samples)
	{
		const float minDt = 0.0005f;
		const float maxDt = 0.06f;
		const float eps = 1e-6f;

		// 距離→t
		float length = UnityEngine.Splines.SplineUtility.CalculateLength(_spline, transform.localToWorldMatrix);
		if (length < eps || samples <= 0) return 0f;
		float dt = Mathf.Clamp(curveLength / length, minDt, maxDt);

		//端から端に一筆書きサンプル
		int steps = samples * 2;
		float tStart = Mathf.Repeat(t - samples * dt, 1f);

		//接線の取得関数
		Vector3 GetTangent(float tt)
		{
			var tan = (Vector3)UnityEngine.Splines.SplineUtility.EvaluateTangent(_spline, tt);  // ← 行列なしの3引数版
			tan.y = 0f;
			return tan.sqrMagnitude > eps ? tan.normalized : Vector3.forward;
		}


		// 度の積算
		float totalDeg = 0f;
		Vector3 prevTan = GetTangent(tStart);

		for (int i = 1; i <= steps; i++)
		{
			float ti = Mathf.Repeat(tStart + i * dt, 1f);
			Vector3 tan = GetTangent(ti);

			float cos = Mathf.Clamp(Vector3.Dot(prevTan, tan), -1f, 1f);
			float ang = Mathf.Acos(cos) * Mathf.Rad2Deg; // 0..180
			totalDeg += ang;

			prevTan = tan;
		}

		//平均
		float avgDeg = totalDeg / steps;
		float sharp = Mathf.Clamp01(avgDeg / _maxAnglePerStep);

		return sharp;
	}


}
