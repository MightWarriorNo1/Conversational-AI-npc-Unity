using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ElevenLabsTTS : MonoBehaviour
{
    private const string API_URL = "https://api.elevenlabs.io/v1/text-to-speech";

    [SerializeField] private APIConfig _apiConfig;
    [SerializeField] private string _model_id = "eleven_turbo_v2_5";
    [SerializeField] private float _stability = 0.5f;
    [SerializeField] private float _similarity_boost = 0.75f;

    private AudioSource _audioSource;
    private ConversationEventSystem _eventSystem;
    private ConversationStateMachine _stateMachine;

    private void Awake()
    {
        _eventSystem = GetComponent<ConversationEventSystem>();
        _stateMachine = GetComponent<ConversationStateMachine>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _eventSystem.OnResponseComplete.AddListener(HandleResponseComplete);
    }

    private void OnDisable()
    {
        _eventSystem.OnResponseComplete.RemoveListener(HandleResponseComplete);
    }

    private async void HandleResponseComplete(string text)
    {
        Debug.Log("Convirtiendo texto a voz...");
        AudioClip clip = await GetAudioAsync(text);
        if (clip == null) return;

        _stateMachine.TrySetState(ConversationStates.Speaking);
        _audioSource.clip = clip;
        _audioSource.Play();
        _eventSystem.OnAudioReady.Invoke(clip);

        await WaitForAudioAsync(clip);
        _stateMachine.TrySetState(ConversationStates.Idle);
    }

    private async Task<AudioClip> GetAudioAsync(string text)
    {
        string url = $"{API_URL}/{_apiConfig.elevenLabsVoiceId}";

        var requestBody = new ElevenLabsRequest
        {
            text = text,
            model_id = _model_id,
            voice_settings = new VoiceSettings
            {
                stability = _stability,
                similarity_boost = _similarity_boost
            }
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        // Primera petición para comprobar errores
        using UnityWebRequest checkRequest = new(url, "POST");
        checkRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        checkRequest.downloadHandler = new DownloadHandlerBuffer();
        checkRequest.SetRequestHeader("Content-Type", "application/json");
        checkRequest.SetRequestHeader("xi-api-key", _apiConfig.elevenLabsKey);

        var checkOperation = checkRequest.SendWebRequest();
        while (!checkOperation.isDone)
            await Task.Yield();

        if (checkRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error ElevenLabs: {checkRequest.error}");
            Debug.LogError($"Respuesta: {checkRequest.downloadHandler.text}");
            return null;
        }

        // Segunda petición para obtener el audio
        using UnityWebRequest audioRequest = new(url, "POST");
        audioRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        audioRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        audioRequest.SetRequestHeader("Content-Type", "application/json");
        audioRequest.SetRequestHeader("xi-api-key", _apiConfig.elevenLabsKey);

        var audioOperation = audioRequest.SendWebRequest();
        while (!audioOperation.isDone)
            await Task.Yield();

        if (audioRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error audio: {audioRequest.error}");
            return null;
        }

        return DownloadHandlerAudioClip.GetContent(audioRequest);
    }
    
    private async Task WaitForAudioAsync(AudioClip clip)
    {
        float duration = clip.length;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            await Task.Yield();
        }
    }

    // Clases JSON
    private class ElevenLabsRequest
    {
        public string text;
        public string model_id;
        public VoiceSettings voice_settings;
    }

    private class VoiceSettings
    {
        public float stability;
        public float similarity_boost;
    }
}