using System;

namespace WitcherRightVersion.Dialogue
{
    [Serializable]
    public sealed class DialogueChoice
    {
        public string Text;
        public string NextNodeId;
        public string FlagToSet;
        public string QuestAction;
        public bool ClosesDialogue;

        public DialogueChoice(string text, string nextNodeId, string flagToSet = "", bool closesDialogue = false, string questAction = "")
        {
            Text = text;
            NextNodeId = nextNodeId;
            FlagToSet = flagToSet;
            QuestAction = questAction;
            ClosesDialogue = closesDialogue;
        }
    }
}
