using UnityEngine;

public class AmbientAudioController : MonoBehaviour
{
    private AudioSource ambientAudio;
    private const string VolumePrefKey = "AmbientVolume";

    void Start()
    {
        ambientAudio = GetComponent<AudioSource>();

        // Set initial volume based on saved PlayerPrefs value or default to max
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        ambientAudio.volume = savedVolume;
    }
}
