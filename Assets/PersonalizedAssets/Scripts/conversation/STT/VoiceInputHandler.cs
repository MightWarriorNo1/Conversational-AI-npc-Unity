using UnityEngine;
using UnityEngine.InputSystem;

public class VoiceInputHandler : MonoBehaviour
{
    [SerializeField] private InputActionReference _recordAction;
    private ConversationEventSystem _eventSystem;
    private ConversationStateMachine _stateMachine;

    private AudioClip _recording;
    private const int SampleRate = 16000;
    private const int MaxRecordingSeconds = 10;

    private void Awake()
    {
        _eventSystem = GetComponent<ConversationEventSystem>();
        _stateMachine = GetComponent<ConversationStateMachine>();
    }

    private void OnEnable()
    {
        _recordAction.action.started += OnRecordStarted;
        _recordAction.action.canceled += OnRecordCanceled;
        _recordAction.action.Enable();
    }

    private void OnDisable()
    {
        _recordAction.action.started -= OnRecordStarted;
        _recordAction.action.canceled -= OnRecordCanceled;
        _recordAction.action.Disable();
    }

    private void OnRecordStarted(InputAction.CallbackContext ctx)
    {
        if (!_stateMachine.TrySetState(ConversationStates.Recording)) return;
        _recording = Microphone.Start(null, false, MaxRecordingSeconds, SampleRate);
        Debug.Log("Grabando...");
    }

    private void OnRecordCanceled(InputAction.CallbackContext ctx)
    {
        if (!_stateMachine.TrySetState(ConversationStates.Transcribing)) return;
        Microphone.End(null);
        Debug.Log("Grabación terminada");
        _eventSystem.OnRecordingComplete.Invoke(_recording);
    }
}