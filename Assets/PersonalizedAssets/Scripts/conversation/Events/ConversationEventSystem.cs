using UnityEngine;
using UnityEngine.Events;

public class ConversationEventSystem : MonoBehaviour
{
    public UnityEvent<AudioClip> OnRecordingComplete = new();
    public UnityEvent<string> OnTranscriptionComplete = new();
    public UnityEvent<string> OnResponseComplete = new();
    public UnityEvent<AudioClip> OnAudioReady = new();
}