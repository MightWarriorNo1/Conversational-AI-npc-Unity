using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class OllamaLLMService : MonoBehaviour
{
    private const string API_URL = "http://localhost:11434/api/chat";
    private const string MODEL = "llama3.2";

    [SerializeField] private CharacterData _characterData;

    private ConversationEventSystem _eventSystem;
    private ConversationStateMachine _stateMachine;
    private List<OllamaMessage> _conversationHistory = new();

    private void Awake()
    {
        _eventSystem = GetComponent<ConversationEventSystem>();
        _stateMachine = GetComponent<ConversationStateMachine>();
    }

    private void OnEnable()
    {
        _eventSystem.OnTranscriptionComplete.AddListener(HandleTranscriptionComplete);
    }

    private void OnDisable()
    {
        _eventSystem.OnTranscriptionComplete.RemoveListener(HandleTranscriptionComplete);
    }

    private async void HandleTranscriptionComplete(string userMessage)
    {
        Debug.Log("Enviando al LLM...");
        string response = await GetResponseAsync(userMessage);
        Debug.Log($"Respuesta: {response}");
        _stateMachine.TrySetState(ConversationStates.WaitingLLM);
        _eventSystem.OnResponseComplete.Invoke(response);
    }

    private async Task<string> GetResponseAsync(string userMessage)
    {
        // Añadir mensaje del usuario al historial
        _conversationHistory.Add(new OllamaMessage
        {
            role = "user",
            content = userMessage
        });

        var requestBody = new OllamaRequest
        {
            model = MODEL,
            stream = false,
            messages = BuildMessages()
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new(API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error Ollama: {request.error}");
            return string.Empty;
        }

        var response = JsonConvert.DeserializeObject<OllamaResponse>(request.downloadHandler.text);
        string assistantMessage = response?.message?.content ?? string.Empty;

        _conversationHistory.Add(new OllamaMessage
        {
            role = "assistant",
            content = assistantMessage
        });

        return assistantMessage;
    }

    private List<OllamaMessage> BuildMessages()
    {
        var messages = new List<OllamaMessage>();

        // System prompt del personaje
        messages.Add(new OllamaMessage
        {
            role = "system",
            content = _characterData.systemPrompt
        });

        // Historial de conversación
        messages.AddRange(_conversationHistory);

        return messages;
    }

    // Clases para serializar JSON
    private class OllamaRequest
    {
        public string model;
        public bool stream;
        public List<OllamaMessage> messages;
    }

    private class OllamaMessage
    {
        public string role;
        public string content;
    }

    private class OllamaResponse
    {
        public OllamaMessage message;
    }
}