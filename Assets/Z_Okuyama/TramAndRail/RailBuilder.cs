#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

//更新対応、MeshRenderer追加
[ExecuteAlways, RequireComponent(typeof(MeshRenderer))]
public class RailBuilder : MonoBehaviour
{

	//Enum==================================================
	enum TwistMode
	{
		WorldUp,
		ParallelTransport
	}

	//Struct==================================================
	struct SamplePoint
	{
		public Vector3 pos, tan, up;
		public float distance, t;
	}

	[System.Serializable]
	struct RailSettings
	{
		public float maxStep, minStep;
		public float gaugeHalf;
		public float railWidth, railThickness;
		public int twistMode;

		//比較
		public bool Equals(RailSettings o)
		{
			return maxStep == o.maxStep && minStep == o.minStep
				&& gaugeHalf == o.gaugeHalf && railWidth == o.railWidth
				&& railThickness == o.railThickness && twistMode == o.twistMode;
		}
	}


	//Variable==================================================

	Spline _spline;

	[SerializeField] SplineContainer _splineContainer;

	const float kEpsilon = 1e-8f;

	//サンプリングとかオフセットなどの計算系
	[Header("Sampling")]
	[SerializeField, Range(0.1f, 1f)] float _maxStep = 0.5f;
	[SerializeField, Range(0.05f, 0.5f)] float _minStep = 0.1f;
	[SerializeField, Range(1f, 10f)] float _maxAngle = 3f;
	[SerializeField] TwistMode _twistMode = TwistMode.ParallelTransport;

	const float kNarrowGaugeRailwayHalf = 0.5335f;//狭軌の半分

	//Railのメッシュ生成関係
	[Header("Rail")]
	[SerializeField, Range(0.1f, 1f)] float _railTrackWidthHalf = kNarrowGaugeRailwayHalf;
	[SerializeField, Range(0.01f, 0.5f)] float _railWidth = 0.1f;//横幅
	[SerializeField, Range(0.01f, 0.5f)] float _railThickness = 0.05f;//厚さ


	//更新関係
	RailSettings _prev;
	bool _needsRebuild;

#if UNITY_EDITOR
	long _lastSplineSig;
#endif

	//PublicFunction==================================================
	//GetVariable
	public float GetRailThickness() { return _railThickness; }
	public float GetRailTrackWidthHalf() { return _railTrackWidthHalf; }

	//GetVector
	public Vector3 GetRailLeftPosWorld(float t)
	{
		Vector3 center = _splineContainer.EvaluatePosition(Mathf.Repeat(t, 1f));
		Vector3 fwd = _splineContainer.EvaluateTangent(Mathf.Repeat(t, 1f));
		fwd = fwd.normalized;
		if (fwd.sqrMagnitude < kEpsilon) fwd = Vector3.forward;
		Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
		return center - right * _railTrackWidthHalf;
	}
	public Vector3 GetRailRightPosWorld(float t)
	{
		Vector3 center = _splineContainer.EvaluatePosition(Mathf.Repeat(t, 1f));
		Vector3 fwd = _splineContainer.EvaluateTangent(Mathf.Repeat(t, 1f));
		fwd = fwd.normalized;
		if (fwd.sqrMagnitude < kEpsilon) fwd = Vector3.forward;
		Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
		return center + right * _railTrackWidthHalf;
	}


	//OnEnable==================================================
	void OnEnable()
	{
		if (_splineContainer)
		{
			_spline = _splineContainer.Spline;
		}
		MarkDirtyAndScheduleRebuild();

#if UNITY_EDITOR
		//スプライン変更を監視
		_lastSplineSig = ComputeSplineSignature(_spline);
		EditorApplication.update += EditorSplineWatcher;
#endif
	}

	//OnDisable==================================================
	void OnDisable()
	{
#if UNITY_EDITOR
		//監視終了
		EditorApplication.update -= EditorSplineWatcher;
#endif
	}

	//OnValidate==================================================
	void OnValidate()
	{
		//変更があった場合、更新
		RailSettings now = Capture();
		if (!_prev.Equals(now))
		{
			_prev = now;
			MarkDirtyAndScheduleRebuild();
		}
	}

	//設定取得
	RailSettings Capture()
	{
		return new RailSettings
		{
			maxStep = _maxStep,
			minStep = _minStep,
			gaugeHalf = _railTrackWidthHalf,
			railWidth = _railWidth,
			railThickness = _railThickness,
			twistMode = (int)_twistMode
		};
	}

	void MarkDirtyAndScheduleRebuild()
	{
		_needsRebuild = true;
#if UNITY_EDITOR
		EditorApplication.delayCall += RebuildIfNeeded;
#endif
	}

	//メッシュ更新
	void RebuildIfNeeded()
	{
		if (!_needsRebuild) { return; }
		_needsRebuild = false;
		if (!_splineContainer || _splineContainer.Spline == null) { return; }

		//サンプリング
		List<SamplePoint> samples = BuildSamples(_splineContainer.Spline);

		//オフセット
		SamplePoint[] left, right;
		ComputeOffsetSamples(samples, _railTrackWidthHalf, out left, out right);

		//メッシュ生成
		Mesh leftMesh = BuildRailPrismMesh(left, right, _railWidth * 0.5f, _railThickness, true);
		Mesh rightMesh = BuildRailPrismMesh(right, left, _railWidth * 0.5f, _railThickness, false);

		ApplyToChild("RailLeft", leftMesh);
		ApplyToChild("RailRight", rightMesh);
	}

	//Editor==================================================
#if UNITY_EDITOR
	//変更フラグ
	void EditorSplineWatcher()
	{
		if (!_splineContainer) { return; }
		Spline spline = _splineContainer.Spline;
		if (spline == null) { return; }

		long sig = ComputeSplineSignature(spline);
		if (sig != _lastSplineSig)
		{
			_lastSplineSig = sig;
			MarkDirtyAndScheduleRebuild();
		}
	}

	//Splineハッシュ値化⇒高速化(変更検知用)
	//FNV-1a参考
	long ComputeSplineSignature(Spline spline)
	{
		if (spline == null) return 0;

		//FNV-1a定数
		const long FNV_OFFSET_BASIS = 1469598103934665603L;
		const long FNV_PRIME = 1099511628211L;

		//float→64bit値
		static long MixFloat(long h, float f)
		{
			h ^= (long)math.aslong(f);
			h *= FNV_PRIME;
			return h;
		}

		unchecked
		{
			long hash = FNV_OFFSET_BASIS;//ハッシュ値

			//ノット数
			int count = spline.Count;
			hash ^= count;
			hash *= FNV_PRIME;

			for (int i = 0; i < count; i++)
			{
				var knot = spline[i];

				//位置
				hash = MixFloat(hash, knot.Position.x);
				hash = MixFloat(hash, knot.Position.y);
				hash = MixFloat(hash, knot.Position.z);

				//接線
				hash = MixFloat(hash, knot.TangentIn.x);
				hash = MixFloat(hash, knot.TangentIn.y);
				hash = MixFloat(hash, knot.TangentIn.z);

				hash = MixFloat(hash, knot.TangentOut.x);
				hash = MixFloat(hash, knot.TangentOut.y);
				hash = MixFloat(hash, knot.TangentOut.z);

				//回転
				var quat = knot.Rotation.value;
				hash = MixFloat(hash, quat.x);
				hash = MixFloat(hash, quat.y);
				hash = MixFloat(hash, quat.z);
				hash = MixFloat(hash, quat.w);
			}

			//ループ
			hash = MixFloat(hash, spline.Closed ? 1f : 0f);

			return hash;
		}
	}
#endif


	//Rail==================================================

	//サンプリング
	List<SamplePoint> BuildSamples(Spline spline)
	{
		List<SamplePoint> list = new List<SamplePoint>();
		float total = SplineUtility.CalculateLength(
			spline, (float4x4)_splineContainer.transform.localToWorldMatrix);

		float dist = 0f;
		Vector3 prevFwd = Vector3.forward;

		while (dist < total)
		{
			float t = SplineUtility.ConvertIndexUnit(
				spline, dist, PathIndexUnit.Distance, PathIndexUnit.Normalized);

			Vector3 pos = _splineContainer.transform.TransformPoint(
				(Vector3)SplineUtility.EvaluatePosition(spline, t));
			Vector3 tan = ((Vector3)SplineUtility.EvaluateTangent(spline, t)).normalized;

			list.Add(new SamplePoint
			{
				pos = pos,
				tan = tan,
				up = Vector3.up,
				distance = dist,
				t = t
			});

			float ang = Vector3.Angle(prevFwd, tan);
			float step = Mathf.Lerp(_maxStep, _minStep, Mathf.InverseLerp(0f, _maxAngle, ang));
			dist += Mathf.Max(step, _minStep);
			prevFwd = tan;
		}

		//ループ処理
		if (spline.Closed && 1 < list.Count)
		{
			list.Add(list[0]);
			SamplePoint sp = list[^1];
			sp.up = list[0].up; list[^1] = sp;//ねじれ合わせ
		}
		return list;
	}


	//オフセット
	void ComputeOffsetSamples(List<SamplePoint> center, float halfGauge,
							  out SamplePoint[] left, out SamplePoint[] right)
	{
		int n = center.Count;
		left = new SamplePoint[n];
		right = new SamplePoint[n];

		Vector3 prevRight = Vector3.right;
		for (int i = 0; i < n; i++)
		{
			Vector3 fwd = center[i].tan;
			Vector3 r = Vector3.Cross(Vector3.up, fwd).normalized;//right
			if (_twistMode == TwistMode.ParallelTransport && 0 < i)
			{
				r = (prevRight - fwd * Vector3.Dot(prevRight, fwd)).normalized;
			}

			Vector3 up = Vector3.Cross(fwd, r).normalized;

			left[i] = center[i]; left[i].pos -= r * halfGauge; left[i].up = up;
			right[i] = center[i]; right[i].pos += r * halfGauge; right[i].up = up;

			prevRight = r;
		}
	}


	//メッシュ生成
	Mesh BuildRailPrismMesh(SamplePoint[] own, SamplePoint[] opp, float halfWidth, float thick, bool upMode)
	{
		int n = own.Length;
		Vector3[] verts = new Vector3[n * 8];
		Vector2[] uvs = new Vector2[n * 8];

		//上外0, 上内1, 下外2, 下外3
		int TopOut(int i) => i * 8 + 0;
		int TopIn(int i) => i * 8 + 1;
		int BotOut(int i) => i * 8 + 2;
		int BotIn(int i) => i * 8 + 3;

		float total = Mathf.Max(own[n - 1].distance, 0.0001f);

		for (int i = 0; i < n; i++)
		{
			Vector3 right = (opp[i].pos - own[i].pos).normalized;
			Vector3 fwd = own[i].tan.normalized;
			Vector3 up = Vector3.Cross(fwd, right).normalized;

			Vector3 c = own[i].pos;

			Vector3 pTo, pTi, pBo, pBi;
			if (upMode)
			{
				pBo = c + right * halfWidth;
				pBi = c - right * halfWidth;
				pTo = pBo + up * thick;
				pTi = pBi + up * thick;
			}
			else
			{
				up *= -1;
				pBo = c - right * halfWidth;
				pBi = c + right * halfWidth;
				pTo = pBo + up * thick;
				pTi = pBi + up * thick;
			}

			verts[TopOut(i)] = transform.InverseTransformPoint(pTo);
			verts[TopIn(i)] = transform.InverseTransformPoint(pTi);
			verts[BotOut(i)] = transform.InverseTransformPoint(pBo);
			verts[BotIn(i)] = transform.InverseTransformPoint(pBi);

			float v = own[i].distance / total;
			uvs[TopOut(i)] = new Vector2(1, v);
			uvs[TopIn(i)] = new Vector2(0, v);
			uvs[BotOut(i)] = new Vector2(1, v);
			uvs[BotIn(i)] = new Vector2(0, v);
		}

		int trisPerBand = (n - 1) * 6;
		int[] tris = new int[trisPerBand * 4];
		int w = 0;

		//上面
		for (int i = 0; i < n - 1; i++)
		{
			tris[w++] = TopIn(i); tris[w++] = TopOut(i + 1); tris[w++] = TopOut(i);
			tris[w++] = TopIn(i); tris[w++] = TopIn(i + 1); tris[w++] = TopOut(i + 1);
		}
		//下面
		for (int i = 0; i < n - 1; i++)
		{
			tris[w++] = BotIn(i); tris[w++] = BotOut(i); tris[w++] = BotOut(i + 1);
			tris[w++] = BotIn(i); tris[w++] = BotOut(i + 1); tris[w++] = BotIn(i + 1);
		}
		//外側
		for (int i = 0; i < n - 1; i++)
		{
			tris[w++] = TopOut(i); tris[w++] = BotOut(i + 1); tris[w++] = BotOut(i);
			tris[w++] = TopOut(i); tris[w++] = TopOut(i + 1); tris[w++] = BotOut(i + 1);
		}
		//内側
		for (int i = 0; i < n - 1; i++)
		{
			tris[w++] = TopIn(i); tris[w++] = BotIn(i); tris[w++] = BotIn(i + 1);
			tris[w++] = TopIn(i); tris[w++] = BotIn(i + 1); tris[w++] = TopIn(i + 1);
		}

		Mesh mesh = new Mesh { name = "RailPrism" };
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.triangles = tris;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}


	//RailMeshの設定
	void ApplyToChild(string name, Mesh mesh)
	{
		Transform t = transform.Find(name);
		if (t == null)
		{
			GameObject go = new GameObject(name);
			go.transform.SetParent(transform, false);
			t = go.transform;
			t.gameObject.AddComponent<MeshFilter>();
			MeshRenderer mr = t.gameObject.AddComponent<MeshRenderer>();
			MeshRenderer pmr = GetComponent<MeshRenderer>();
			if (pmr)
			{
				mr.sharedMaterials = pmr.sharedMaterials;
			}
		}
		t.GetComponent<MeshFilter>().sharedMesh = mesh;
	}



}
