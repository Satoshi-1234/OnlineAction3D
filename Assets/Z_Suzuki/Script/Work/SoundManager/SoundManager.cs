using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonBase<SoundManager>
{
    [SerializeField] private AudioMixer AudioMixer;
    [SerializeField] private AudioMixerGroup SEGroup;
    [SerializeField] private AudioMixerGroup BGMGroup;
    [SerializeField] private AudioClip[] SEAudioClips;
    [SerializeField] private AudioClip[] BGMAudioClips;


    //SEを再生する関数
    //AudioSourceを指定する関数と音声ファイル名を指定する関数があります
    //ファイル名と座標（Vector3）を指定することで立体音響で再生することができます
    public void PlaySE(AudioSource source)
    {
        source.outputAudioMixerGroup = SEGroup;
        source.PlayOneShot(source.clip);
    }
    public void PlaySE(string clipName)
    {
        AudioSource playSource = GetFreeSEAudioSource();
        if (playSource == null)
        {
            return;
        }
        AudioClip playClip = GetSEAudioClip(clipName);
        if (playClip == null)
        {
            return;
        }

        playSource.outputAudioMixerGroup = SEGroup;
        playSource.spatialBlend = 0.0f;
        playSource.PlayOneShot(playClip);
    }


    public void PlaySE(string clipName, Vector3 playPosition)
    {
        AudioSource playSource = GetFreeSEAudioSource();
        if (playSource == null)
        {
            return;
        }
        AudioClip playClip = GetSEAudioClip(clipName);
        if (playClip == null)
        {
            return;
        }

        playSource.outputAudioMixerGroup = SEGroup;
        playSource.spatialBlend = 1.0f;
        playSource.transform.position = playPosition;
        playSource.PlayOneShot(playClip);
    }


    //BGMを再生する関数
    //ファイル名を指定して再生することができます
    //基本BGMは立体音響使わないと思ったので座標指定して再生する関数は実装してないです
    public void PlayBGM(string clipName)
    {
        AudioSource playSource = GetFreeBGMAudioSource();
        if (playSource == null)
        {
            return;
        }
        AudioClip playClip = GetBGMAudioClip(clipName);
        if (playClip == null)
        {
            return;
        }

        playSource.outputAudioMixerGroup = BGMGroup;
        playSource.spatialBlend = 0.0f;
        playSource.clip = playClip;
        playSource.Play();
    }


    //Scene上の全ての音を止める関数
    //オブジェクトにアタッチされているAudioSourceも含めた、全ての再生されている音を止めます
    public void StopAllSound()
    {
        AudioSource[] stopSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        for (int i = 0; i < _bgmSources.Length; i++)
        {
            if (_bgmSources[i].isPlaying)
            {
                _bgmSources[i].Stop();
            }
        }
        for (int i = 0; i < _seSources.Length; i++)
        {
            if (_seSources[i].isPlaying)
            {
                _seSources[i].Stop();
            }
        }
        for (int i = 0; i < stopSources.Length; i++)
        {
            if (stopSources[i].isPlaying)
            {
                stopSources[i].Stop();
            }
        }
    }


    //音を止める
    //AudioSourceを指定して再生されている音を止めることができます
    public void StopSound(AudioSource stopSoundSource)
    {
        if (stopSoundSource.isPlaying)
        {
            stopSoundSource.Stop();
        }
        else
        {
            Debug.Log("指定したAudioSourceは音が再生されていません");
        }
    }


    //BGMを止める関数
    //再生されている全てのBGMを止める関数と指定したファイル名のBGMを止める関数を実装してます
    public void StopAllBGM()
    {
        for (int i = 0; i < _bgmSources.Length; i++)
        {
            if (_bgmSources[i].isPlaying)
            {
                _bgmSources[i].Stop();
            }
        }
    }
    public void StopBGM(string stopBGMName)
    {
        for (int i = 0; i < _bgmSources.Length; i++)
        {
            if (_bgmSources[i].clip == null)
            {
                continue;
            }

            if (_bgmSources[i].clip.name == stopBGMName)
            {
                if (_bgmSources[i].isPlaying)
                {
                    _bgmSources[i].Stop();
                    return;
                }
            }
        }

        Debug.Log("BGMが見つかりませんでした");
    }


    //SEを止める関数
    //再生されている全てのSEを止める関数と指定したファイル名のSEを止める関数を実装してます
    public void StopAllSE()
    {
        for (int i = 0; i < _seSources.Length; i++)
        {
            if (_seSources[i].isPlaying)
            {
                _seSources[i].Stop();
            }
        }
    }
    public void StopSE(string stopSEName)
    {
        for (int i = 0; i < _seSources.Length; i++)
        {
            if (_seSources[i].clip == null)
            {
                continue;
            }

            if (_seSources[i].clip.name == stopSEName)
            {
                if (_seSources[i].isPlaying)
                {
                    _seSources[i].Stop();
                    return;
                }
            }
        }

        Debug.Log("SEが見つかりませんでした");
    }


    //音量を設定する関数
    //音量を０～１で設定できます
    //マスター音量、BGM音量、SE音量それぞれ設定できます
    public void SetVolumeMaster(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1.0f)) * 20f;
        AudioMixer.SetFloat("Master", dB);
    }
    public void SetVolumeBGM(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1.0f)) * 20f;
        AudioMixer.SetFloat("BGM", dB);
    }
    public void SetVolumeSE(float volume)
    {
        float dB = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1.0f)) * 20f;
        AudioMixer.SetFloat("SE", dB);
    }


    public float GetVolumeMaster()
    {
        float volume;
        AudioMixer.GetFloat("Master", out volume);
        return Mathf.Pow(10, volume / 20);
    }
    public float GetVolumeBGM()
    {
        float volume;
        AudioMixer.GetFloat("BGM", out volume);
        return Mathf.Pow(10, volume / 20);
    }
    public float GetVolumeSE()
    {
        float volume;
        AudioMixer.GetFloat("SE", out volume);
        return Mathf.Pow(10, volume / 20);
    }


    /*************ここから下の関数は基本SoundManager内だけで使用するものなので気にしなくてもOK*************/
    private const int SE_MAX = 10;
    private const int BGM_MAX = 1;
    private AudioSource[] _seSources = new AudioSource[SE_MAX];
    private AudioSource[] _bgmSources = new AudioSource[BGM_MAX];


    protected override void DoAwake()
    {
        for (int i = 0; i < _seSources.Length; i++)
        {
            _seSources[i] = gameObject.AddComponent<AudioSource>();
        }
        for (int i = 0; i < _bgmSources.Length; i++)
        {
            _bgmSources[i] = gameObject.AddComponent<AudioSource>();
        }
    }


    private AudioSource GetFreeSEAudioSource()
    {
        AudioSource ret = null;

        for (int i = 0; i<_seSources.Length; i++)
        {
            if (!_seSources[i].isPlaying)
            {
                ret = _seSources[i];
                break;
            }
        }
        if (ret == null)
        {
            Debug.Log("現在再生されているSEの数が、同時に再生できるSEの数を超えているか、\n" +
                      "もしくは同時に再生できるSEの数が正しく設定されていません");
        }

        return ret;
    }


    private AudioSource GetFreeBGMAudioSource()
    {
        AudioSource ret = null;

        for (int i = 0; i < _bgmSources.Length; i++)
        {
            if (!_bgmSources[i].isPlaying)
            {
                ret = _bgmSources[i];
                break;
            }
        }
        if (ret == null)
        {
            Debug.Log("現在再生されているBGMの数が、同時に再生できるBGMの数を超えているか、\n" +
                      "もしくは同時に再生できるBGMの数が正しく設定されていません");
        }

        return ret;
    }


    private AudioClip GetBGMAudioClip(string bgmName)
    {
        AudioClip ret = null;

        for (int i = 0; i < BGMAudioClips.Length; i++)
        {
            if (BGMAudioClips[i].name == bgmName)
            {
                ret = BGMAudioClips[i];
                break;
            }
        }
        if (ret == null)
        {
            Debug.Log("BGMが見つかりませんでした");
        }

        return ret;
    }


    private AudioClip GetSEAudioClip(string seName)
    {
        AudioClip ret = null;

        for (int i = 0; i < SEAudioClips.Length; i++)
        {
            if (SEAudioClips[i].name == seName)
            {
                ret = SEAudioClips[i];
                break;
            }
        }
        if (ret == null)
        {
            Debug.Log("SEが見つかりませんでした");
            Debug.Log("指定したSEの名前は: " + seName);
        }

        return ret;
    }
}
