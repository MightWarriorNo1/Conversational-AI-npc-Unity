using UnityEngine;

public class LipSyncHelper : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private ConversationEventSystem _eventSystem;

    private void Awake()
    {        
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _eventSystem.OnAudioReady.AddListener(ConfigureAvatar);
    }

    private void OnDisable()
    {
        _eventSystem.OnAudioReady.RemoveListener(ConfigureAvatar);
    }

    private void ConfigureAvatar(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
