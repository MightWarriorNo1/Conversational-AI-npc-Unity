using UnityEngine;

[CreateAssetMenu(fileName = "APIConfig", menuName = "Conversation/API Config")]
public class APIConfig : ScriptableObject
{
    public string openAIKey;
    public string elevenLabsKey;
    public string elevenLabsVoiceId;
}