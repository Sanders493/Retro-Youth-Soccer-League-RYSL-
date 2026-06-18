using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls audio volume through UI buttons or sliders.
/// </summary>
public class AudioVolumeButton : MonoBehaviour
{
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource soundEffectAudioSource;
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private float volumeStep = 0.1f;

    public float CurrentVolume
    {
        get; private set;
    }

    /// <summary>
    /// Sets the starting audio volume.
    /// </summary>
    private void Start()
    {
        CurrentVolume = AudioListener.volume;

        if (volumeSlider != null)
        {
            volumeSlider.value = CurrentVolume;
        }
    }

    /// <summary>
    /// Increases the game audio volume.
    /// </summary>
    public void IncreaseVolume()
    {
        SetVolume(CurrentVolume + volumeStep);
    }

    /// <summary>
    /// Decreases the game audio volume.
    /// </summary>
    public void DecreaseVolume()
    {
        SetVolume(CurrentVolume - volumeStep);
    }

    /// <summary>
    /// Mutes all game audio.
    /// </summary>
    public void MuteVolume()
    {
        SetVolume(0f);
    }

    /// <summary>
    /// Sets game audio volume from a slider.
    /// </summary>
    /// <param name="volume">The selected volume value from 0 to 1.</param>
    public void SetVolume(float volume)
    {
        CurrentVolume = Mathf.Clamp01(volume);

        AudioListener.volume = CurrentVolume;

        if (musicAudioSource != null)
        {
            musicAudioSource.volume = CurrentVolume;
        }

        if (soundEffectAudioSource != null)
        {
            soundEffectAudioSource.volume = CurrentVolume;
        }

        if (volumeSlider != null)
        {
            volumeSlider.value = CurrentVolume;
        }
    }
}
