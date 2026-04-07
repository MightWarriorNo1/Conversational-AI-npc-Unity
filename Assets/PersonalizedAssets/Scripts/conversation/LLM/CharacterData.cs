using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Conversation/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [TextArea(5, 15)]
    public string systemPrompt;
}