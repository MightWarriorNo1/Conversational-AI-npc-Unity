using UnityEngine;
using Whisper;
using System.Threading.Tasks;

public class WhisperLocalStt : MonoBehaviour
{
    private WhisperManager _whisperManager;
    private ConversationEventSystem _eventSystem;

    private void Awake()
    {
        _whisperManager = GetComponent<WhisperManager>();
        _eventSystem = GetComponent<ConversationEventSystem>();
    }

    private void OnEnable()
    {
        _eventSystem.OnRecordingComplete.AddListener(HandleRecordingComplete);
    }

    private void OnDisable()
    {
        _eventSystem.OnRecordingComplete.RemoveListener(HandleRecordingComplete);
    }

    private async void HandleRecordingComplete(AudioClip recording)
    {
        Debug.Log("Transcribiendo...");
        string text = await TranscribeAsync(recording);
        Debug.Log($"Transcripción: {text}");
        _eventSystem.OnTranscriptionComplete.Invoke(text);
    }

    public async Task<string> TranscribeAsync(AudioClip recording)
    {
        var amplified = AmplifyAudioClip(recording);
        var result = await _whisperManager.GetTextAsync(amplified);
        return result?.Result ?? string.Empty;
    }

    private AudioClip AmplifyAudioClip(AudioClip clip, float gain = 3f)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        for (int i = 0; i < samples.Length; i++)
            samples[i] = Mathf.Clamp(samples[i] * gain, -1f, 1f);

        AudioClip amplified = AudioClip.Create(clip.name, clip.samples,
                              clip.channels, clip.frequency, false);
        amplified.SetData(samples, 0);
        return amplified;
    }
}