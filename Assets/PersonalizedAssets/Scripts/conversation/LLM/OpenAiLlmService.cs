using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

public class OpenAiLlmService : MonoBehaviour
{
    [SerializeField] private APIConfig _apiConfig;
    private const string API_URL = "https://api.openai.com/v1/chat/completions";
    private const string MODEL = "gpt-4o-mini";

    [SerializeField] private CharacterData _characterData;

    private ConversationEventSystem _eventSystem;
    private ConversationStateMachine _stateMachine;
    private List<OpenAIMessage> _conversationHistory = new();

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
        Debug.Log("Enviando a OpenAI...");
        string response = await GetResponseAsync(userMessage);
        Debug.Log($"Respuesta: {response}");
        _stateMachine.TrySetState(ConversationStates.Speaking);
        _eventSystem.OnResponseComplete.Invoke(response);
    }

    private async Task<string> GetResponseAsync(string userMessage)
    {
        _conversationHistory.Add(new OpenAIMessage
        {
            role = "user",
            content = userMessage
        });

        var requestBody = new OpenAIRequest
        {
            model = MODEL,
            messages = BuildMessages()
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new(API_URL, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {_apiConfig.openAIKey}");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error OpenAI: {request.error}");
            return string.Empty;
        }

        var response = JsonConvert.DeserializeObject<OpenAIResponse>(request.downloadHandler.text);
        string assistantMessage = response?.choices?[0]?.message?.content ?? string.Empty;

        _conversationHistory.Add(new OpenAIMessage
        {
            role = "assistant",
            content = assistantMessage
        });

        return assistantMessage;
    }

    private List<OpenAIMessage> BuildMessages()
    {
        var messages = new List<OpenAIMessage>();

        messages.Add(new OpenAIMessage
        {
            role = "system",
            content = _characterData.systemPrompt
        });

        messages.AddRange(_conversationHistory);
        return messages;
    }

    // Clases JSON
    private class OpenAIRequest
    {
        public string model;
        public List<OpenAIMessage> messages;
    }

    private class OpenAIMessage
    {
        public string role;
        public string content;
    }

    private class OpenAIResponse
    {
        public List<OpenAIChoice> choices;
    }

    private class OpenAIChoice
    {
        public OpenAIMessage message;
    }
}