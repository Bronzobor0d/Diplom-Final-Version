using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMusic : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private AudioSource _audio;
    [SerializeField] private Slider _sliderMusic;
    [SerializeField] private Text _volumeMusicText;
    [SerializeField] private List<AudioClip> _musics;

    [Header("Keys")]
    [SerializeField] private string _saveVolumeKey;

    [Header("Parameters")]
    [SerializeField] private float _volume;
    private int _indexLastMusic = -1;

    #region MONO

    private void Awake()
    {
        if (_musics.Count == 1)
        {
            _audio.clip = _musics[0];
            _audio.loop = true;
        }
        else
        {
            _audio.loop = false;
            _audio.clip = RandomMusic();
            _audio.Play();
        }
        if (PlayerPrefs.HasKey(_saveVolumeKey))
        {
            _volume = PlayerPrefs.GetFloat(_saveVolumeKey);
            _audio.volume = _volume;
            _sliderMusic.value = _volume;
        }
        else
        {
            _volume = 0.5f;
            PlayerPrefs.SetFloat(_saveVolumeKey, _volume);
            _audio.volume = _volume;
        }
    }

    #endregion

    #region BODY

    private void LateUpdate()
    {
        _volume = _sliderMusic.value;
        if (_audio.volume != _volume)
        {
            PlayerPrefs.SetFloat(_saveVolumeKey, _volume);
        }
        _volumeMusicText.text = Mathf.Round(_volume * 100) + "%";
        _audio.volume = _volume;

        if (!_audio.isPlaying)
        {
            _audio.clip = RandomMusic();
            _audio.Play();
        }
    }

    #endregion

    #region CALLBACKS

    private AudioClip RandomMusic()
    {
        while (true)
        {
            int indexMusic = Random.Range(0, _musics.Count);
            if (indexMusic != _indexLastMusic)
            {
                _indexLastMusic = indexMusic;
                break;
            }
        }
        return _musics[_indexLastMusic];
    }

    #endregion
}
