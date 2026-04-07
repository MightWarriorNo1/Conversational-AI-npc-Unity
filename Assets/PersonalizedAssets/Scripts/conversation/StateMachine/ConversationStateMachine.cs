using UnityEngine;

public class ConversationStateMachine : MonoBehaviour
{
    public ConversationStates CurrentState { get; private set; } = ConversationStates.Idle;

    public bool TrySetState(ConversationStates newState)
    {
        if (!IsValidTransition(CurrentState, newState))
        {
            Debug.LogWarning($"Transición inválida: {CurrentState} → {newState}");
            return false;
        }
        CurrentState = newState;
        Debug.Log($"Estado: {CurrentState}");
        return true;
    }

    private bool IsValidTransition(ConversationStates from, ConversationStates to)
    {
        return (from, to) switch
        {
            (ConversationStates.Idle, ConversationStates.Recording) => true,
            (ConversationStates.Recording, ConversationStates.Transcribing) => true,
            (ConversationStates.Transcribing, ConversationStates.WaitingLLM) => true,
            (ConversationStates.WaitingLLM, ConversationStates.Speaking) => true,
            (ConversationStates.Speaking, ConversationStates.Idle) => true,
            _ => false        
        };
    }
}