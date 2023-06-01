using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsHelper : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeName;

    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle musicToggle;
    public const string VibrationKey = "Vibration";
    public const string MusicKey = "Music";
    public static SettingsHelper Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        vibrationToggle.isOn = PlayerPrefs.HasKey(VibrationKey);
        musicToggle.isOn = PlayerPrefs.HasKey(MusicKey);
    }

    private void TurnOffMusic()
    {
        audioMixer.SetFloat(musicVolumeName, -80);
    }

    private void TurnOnMusic()
    {
        audioMixer.SetFloat(musicVolumeName, -5);
    }

    public void OnVibrationToggle(bool val)
    {
        if (val)
            PlayerPrefs.SetInt(VibrationKey, 1);
        else PlayerPrefs.DeleteKey(VibrationKey);
    }

    public void OnMusicToggle(bool val)
    {
        if (val)
        {
            PlayerPrefs.SetInt(MusicKey, 1);
            Instance.TurnOnMusic();
        }
        else
        {
            PlayerPrefs.DeleteKey(MusicKey);
            Instance.TurnOffMusic();
        }
    }

    public bool IsVibrationOn()
    {
        return PlayerPrefs.HasKey(VibrationKey);
    }
}