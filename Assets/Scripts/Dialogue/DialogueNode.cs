using System;

namespace WitcherRightVersion.Dialogue
{
    [Serializable]
    public sealed class DialogueNode
    {
        public string Id;
        public string SpeakerName;
        public string Body;
        public DialogueChoice[] Choices;

        public DialogueNode(string id, string speakerName, string body, DialogueChoice[] choices)
        {
            Id = id;
            SpeakerName = speakerName;
            Body = body;
            Choices = choices;
        }
    }
}
