using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField, Header("ローディング中に表示するテキストUI")] private TextMeshProUGUI LoadingTextUI;
    [SerializeField, Header("ローディング中に表示するテキスト")] private string LoadingText = "Now Loading";
    [SerializeField, Header("ローディング中に動かす画像")] private Image LodingImage;
    [SerializeField, Header("ローディングの進捗を示すスライダー")] private Slider LoadingSlider;
    [SerializeField, Header("テキストが変化する間隔(秒)")] private float ChangeTextInterval = 1.0f;
    [SerializeField, Header("テキストに表示させるドットの最大数")] private int MaxDotCount = 3;


    private float _loadingTimeCount = 0;
    private int _dotCount = 0;


    private void UpdateProgressBar(float progress)
    {
        LoadingSlider.value = progress;
    }


    private void LoadingImageUpdate(float deltaTime)
    {
        LodingImage.transform.Rotate(0f, 0f, -200f * Time.fixedDeltaTime);
    }


    private void LoadingTextUpdate(float deltaTime)
    {
        _loadingTimeCount += Time.fixedDeltaTime;

        if (_loadingTimeCount < ChangeTextInterval)
        {
            return;
        }

        _loadingTimeCount = 0;
        _dotCount++;
        if (_dotCount > MaxDotCount)
        {
            _dotCount = 0;
        }
        LoadingTextUI.text = LoadingText + new string('.', _dotCount);
    }


    private void OnEnable()
    {
        SceneLoader.OnProgressUpdated += UpdateProgressBar;
    }


    private void Start()
    {
        if (LoadingTextUI== null)
        {
            Debug.LogError("LoadingTextがアタッチされていません " + gameObject.name);
            return;
        }

        if (LodingImage == null)
        {
            Debug.LogError("LodingImageがアタッチされていません " + gameObject.name);
            return;
        }

        if (LoadingSlider == null)
        {
            Debug.LogError("LoadingSliderがアタッチされていません " + gameObject.name);
            return;
        }

        LoadingTextUI.text = LoadingText;
    }


    private void FixedUpdate()
    {
        LoadingImageUpdate(Time.fixedDeltaTime);
        LoadingTextUpdate(Time.fixedDeltaTime);
    }
}
