using System;

namespace WitcherRightVersion.Dialogue
{
    [Serializable]
    public sealed class DialogueChoice
    {
        public string Text;
        public string NextNodeId;
        public string FlagToSet;
        public bool ClosesDialogue;

        public DialogueChoice(string text, string nextNodeId, string flagToSet = "", bool closesDialogue = false)
        {
            Text = text;
            NextNodeId = nextNodeId;
            FlagToSet = flagToSet;
            ClosesDialogue = closesDialogue;
        }
    }
}
