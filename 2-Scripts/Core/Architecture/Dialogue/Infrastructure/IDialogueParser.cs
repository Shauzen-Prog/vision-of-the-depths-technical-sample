public interface IDialogueParser
{
    DialogueConversation ParseFromJson(string dialogueId, string json);
}
