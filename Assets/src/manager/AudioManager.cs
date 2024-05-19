
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioClipData footstepAudio;
    public AudioClipData punchAudio;
    public AudioClipData worldHitSounds;
    public AudioClipData UI_Sounds;
    public float masterVolume = 0.5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayClipDataSoundRandom(AudioClipData clipData, string groupName, Vector3 position, float delay = 0f)
    {
        if (clipData == null)
        {
            Debug.Log("AudioManager: AudioClip data does not exist");
            return;
        }
        AudioClip[] selectedClips = clipData.GetClipsForGroup(groupName);
        AudioClip clip = null;
        if (selectedClips != null && selectedClips.Length > 0)
        {
            clip = selectedClips[Random.Range(0, selectedClips.Length)];
        }
        PlayClipAtPosition(clip, position, clipData.GetSoundLevelForGroup(groupName));
    }

    public void PlayClipDataSound(AudioClipData clipData, string groupName, Vector3 position, int index)
    {
        if (clipData.GetClipsForGroup(groupName) == null)
        {
            Debug.Log("AudioManager: " + groupName + " does not exist");
            return;
        }
        AudioClip[] selectedClips = clipData.GetClipsForGroup(groupName);
        if (selectedClips.Length <= 0)
        {
            Debug.Log("AudioManager: " + selectedClips + " does not exist");
            return;
        }
        PlayClipAtPosition(selectedClips[index], position, clipData.GetSoundLevelForGroup(groupName));
    }

    private void PlayClipAtPosition(AudioClip clip, Vector3 position, AudioClipData.SoundLevel soundLevel)
    {
        if (clip == null) return;
        GameObject tempAudioSource = new GameObject("TempAudio");

        Vector3 listenerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        if (position == Vector3.zero) tempAudioSource.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
        else tempAudioSource.transform.position = position;

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.pitch = Random.Range(0.98f, 1.02f);

        float volumeForSoundLevel = GetVolumeForSoundLevel(soundLevel);
        audioSource.maxDistance = 50;
        float distanceFactor = CalculateVolumeFactorByDistance(position, listenerPosition, audioSource.maxDistance);
        audioSource.volume = volumeForSoundLevel * distanceFactor * masterVolume;

        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.spatialBlend = 1;
        audioSource.Play();

        Destroy(tempAudioSource, clip.length);
    }

    public GameObject PlayLoopingClipAtPosition(AudioClip clip, Vector3 position, AudioClipData.SoundLevel soundLevel)
    {
        if (clip == null) return null;
        GameObject tempAudioSource = new GameObject("TempAudio");

        Vector3 listenerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        if (position == Vector3.zero) tempAudioSource.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
        else tempAudioSource.transform.position = position;

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.pitch = Random.Range(0.98f, 1.02f);

        float volumeForSoundLevel = GetVolumeForSoundLevel(soundLevel);
        audioSource.maxDistance = 50;
        float distanceFactor = CalculateVolumeFactorByDistance(position, listenerPosition, audioSource.maxDistance);
        audioSource.volume = volumeForSoundLevel * distanceFactor * masterVolume;

        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.loop = true;
        audioSource.spatialBlend = 1;
        audioSource.Play();

        return tempAudioSource;
    }


    private float CalculateVolumeFactorByDistance(Vector3 sourcePosition, Vector3 listenerPosition, float maxDistance)
    {
        float distance = Vector3.Distance(sourcePosition, listenerPosition);
        // Example calculation, adjust based on your needs
        // Ensures the volume is 1 at 0 distance and linearly decreases to 0 at maxDistance or beyond.
        float volumeFactor = 1 - (distance / maxDistance);
        return Mathf.Clamp(volumeFactor, 0, 1); // Ensure the volume factor is between 0 and 1
    }

    private float GetVolumeForSoundLevel(AudioClipData.SoundLevel soundLevel)
    {
        switch (soundLevel)
        {
            case AudioClipData.SoundLevel.Loud:
                return 1.0f; // Maximum volume
            case AudioClipData.SoundLevel.Regular:
                return 0.7f; // Regular volume
            case AudioClipData.SoundLevel.Soft:
                return 0.5f; // Reduced volume
            case AudioClipData.SoundLevel.Quiet:
                return 0.3f; // Reduced volume
            default:
                return 0.7f; // Default to regular volume
        }
    }
}
