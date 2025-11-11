using TMPro;
using UnityEngine;


public class DestinationMarker : MonoBehaviour
{
    [SerializeField, Header("マーカーを表示するカメラ")] private Camera DisplayCamera;
    [SerializeField, Header("距離を表示するテキスト")] private TextMeshProUGUI DistanceText;
    [SerializeField, Header("マーカーオブジェクト")] private RectTransform MarkerTransform;
    [SerializeField, Header("画面端からの余白")] private float ScreenEdgeMargin = 50.0f;

    
    public void SetTarget(Transform target) { _targetTransform = target; }
    public Transform GetTarget() { return _targetTransform; }


    private Transform _targetTransform;


    private void Start()
    {
        if (DisplayCamera == null)
        {
            Debug.LogError("カメラが設定されていません " + gameObject.name);
            return;
        }

        if (DistanceText == null)
        {
            Debug.LogError("距離表示用のテキストが設定されていません" + gameObject.name);
            return;
        }

        if (MarkerTransform == null)
        {
            Debug.LogError("マーカーオブジェクトが設定されていません" + gameObject.name);
            return;
        }

        MarkerTransform.gameObject.SetActive(true);
    }


    void Update()
    {
        MarkerPositionUpdate();
        DistanceTextUpdate();
    }


    private void MarkerPositionUpdate()
    {
        if (_targetTransform == null)
        {
            return;
        }

        Vector3 screenPos = DisplayCamera.WorldToScreenPoint(_targetTransform.position);

        if (screenPos.z > 0)
        {
            //マーカーが画面端からはみ出さないように調整する
            float clampedX = Mathf.Clamp(screenPos.x, ScreenEdgeMargin, Screen.width - ScreenEdgeMargin);
            float clampedY = Mathf.Clamp(screenPos.y, ScreenEdgeMargin, Screen.height - ScreenEdgeMargin);

            MarkerTransform.position = new Vector3(clampedX, clampedY, 0);
        }
        else
        {
            //ターゲットがカメラの後ろにある状態でスクリーン座標に変換した場合、
            //座標が負の方向になってしまうため、-1をかけて正の方向にする
            screenPos *= -1;

            //画面中心からターゲット座標への方向を求める
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2;
            Vector3 dir = (screenPos - screenCenter).normalized;

            //画面中央から画面端までの距離を求める
            float maxDistance = Mathf.Sqrt(screenCenter.x * screenCenter.x + screenCenter.y * screenCenter.y);
            
            //画面中心からターゲット座標への方向ベクトルを画面端まで伸ばし、
            //そこに画面中央の座標を足してマーカー位置を決定する
            Vector3 markerPos = screenCenter + dir * maxDistance;

            //マーカーが画面端からはみ出さないように調整する
            float clampedX = Mathf.Clamp(markerPos.x, ScreenEdgeMargin, Screen.width - ScreenEdgeMargin);
            float clampedY = Mathf.Clamp(markerPos.y, ScreenEdgeMargin, Screen.height - ScreenEdgeMargin);

            MarkerTransform.position = new Vector3(clampedX, clampedY, 0);
        }
    }


    private void DistanceTextUpdate()
    {
        if (_targetTransform == null)
        {
            return;
        }
        float distance = Vector3.Distance(DisplayCamera.transform.position, _targetTransform.position);
        DistanceText.text = $"{(int)distance}m";
    }
}
