using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class WhisperAPISTT : MonoBehaviour
{
    [SerializeField] private APIConfig _apiConfig;
    private const string API_URL = "https://api.openai.com/v1/audio/transcriptions";    
    private const string MODEL = "whisper-1";
    private const string LANGUAGE = "es";

    private ConversationEventSystem _eventSystem;
    private ConversationStateMachine _stateMachine;

    private void Awake()
    {
        _eventSystem = GetComponent<ConversationEventSystem>();
        _stateMachine = GetComponent<ConversationStateMachine>();
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
        Debug.Log("Transcribiendo con Whisper API...");
        string text = await TranscribeAsync(recording);
        Debug.Log($"Transcripción: {text}");
        _stateMachine.TrySetState(ConversationStates.WaitingLLM);
        _eventSystem.OnTranscriptionComplete.Invoke(text);
    }

    private async Task<string> TranscribeAsync(AudioClip recording)
    {
        byte[] wavData = AudioClipConverter.ToWav(recording);

        WWWForm form = new();
        form.AddBinaryData("file", wavData, "recording.wav", "audio/wav");
        form.AddField("model", MODEL);
        form.AddField("language", LANGUAGE);

        using UnityWebRequest request = UnityWebRequest.Post(API_URL, form);
        request.SetRequestHeader("Authorization", $"Bearer {_apiConfig.openAIKey}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error Whisper API: {request.error}");
            return string.Empty;
        }

        var response = JsonConvert.DeserializeObject<WhisperResponse>(request.downloadHandler.text);
        return response?.text ?? string.Empty;
    }

    [System.Serializable]
    private class WhisperResponse
    {
        public string text;
    }
}