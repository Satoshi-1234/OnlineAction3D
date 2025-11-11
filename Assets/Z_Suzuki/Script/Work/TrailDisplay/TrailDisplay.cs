using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailDisplay : MonoBehaviour
{
    [SerializeField] private LineRenderer LineRenderer;
    [SerializeField] private float PointsPerSecond = 100.0f;


    void Start()
    {
        var positions = ObjectPositionLogger.Instance.GetPositionList();

        LineRenderer.positionCount = positions.Count;
        LineRenderer.SetPositions(positions.ToArray());

        StartCoroutine(AnimateTrail());
    }

    
    void Update()
    {
        
    }


    private IEnumerator AnimateTrail()
    {
        List<Vector3> positions = ObjectPositionLogger.Instance.GetPositionList();

        // Line Rendererの初期化
        LineRenderer.positionCount = 0;

        // 1点あたりにかける待機時間
        float waitTime = (PointsPerSecond <= 0) ? 0 : (1.0f / PointsPerSecond);

        // 最初の点を設定
        LineRenderer.positionCount = 1;
        LineRenderer.SetPosition(0, positions[0]);

        // 2点目から順に、座標リストの最後までループ
        for (int i = 1; i < positions.Count; i++)
        {
            // 頂点の数を増やし、新しい座標を設定
            LineRenderer.positionCount = i + 1;
            LineRenderer.SetPosition(i, positions[i]);

            // waitTimeが0より大きい場合のみ待機する
            if (waitTime > 0)
            {
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                // 速度が速すぎる場合はフレームを待つ
                yield return null;
            }
        }

        Debug.Log("軌跡の描画完了");
    }
}
