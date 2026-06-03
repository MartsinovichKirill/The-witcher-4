using UnityEngine;
using UnityEngine.UI;
using WitcherRightVersion.Core;
using WitcherRightVersion.Quest;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Dialogue
{
    public sealed class DialogueService : MonoBehaviour
    {
        private const int MaxChoices = 4;

        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text[] choiceTexts;

        private DialogueNode[] activeNodes;
        private DialogueNode currentNode;

        public static DialogueService Instance { get; private set; }
        public bool IsDialogueOpen => dialogueRoot != null && dialogueRoot.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CloseDialogue();
        }

        private void Update()
        {
            if (!IsDialogueOpen || currentNode == null)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseDialogue();
                return;
            }

            for (var i = 0; i < MaxChoices; i++)
            {
                if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                {
                    SelectChoice(i);
                    return;
                }
            }
        }

        public void StartDialogue(DialogueNode[] nodes, string startNodeId)
        {
            if (nodes == null || nodes.Length == 0)
            {
                Debug.LogWarning("Dialogue has no nodes.", this);
                return;
            }

            activeNodes = nodes;
            ShowNode(string.IsNullOrWhiteSpace(startNodeId) ? nodes[0].Id : startNodeId);
            InteractionPromptUI.Instance?.HidePrompt();
        }

        public void CloseDialogue()
        {
            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }

            currentNode = null;
            activeNodes = null;
        }

        private void ShowNode(string nodeId)
        {
            var node = FindNode(nodeId);
            if (node == null)
            {
                CloseDialogue();
                return;
            }

            currentNode = node;

            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(true);
            }

            if (speakerText != null)
            {
                speakerText.text = node.SpeakerName;
            }

            if (bodyText != null)
            {
                bodyText.text = node.Body;
            }

            UpdateChoices(node.Choices);
        }

        private DialogueNode FindNode(string nodeId)
        {
            if (activeNodes == null)
            {
                return null;
            }

            for (var i = 0; i < activeNodes.Length; i++)
            {
                if (activeNodes[i].Id == nodeId)
                {
                    return activeNodes[i];
                }
            }

            Debug.LogWarning($"Dialogue node not found: {nodeId}", this);
            return null;
        }

        private void UpdateChoices(DialogueChoice[] choices)
        {
            if (choiceTexts == null)
            {
                return;
            }

            for (var i = 0; i < choiceTexts.Length; i++)
            {
                var choiceText = choiceTexts[i];
                if (choiceText == null)
                {
                    continue;
                }

                if (choices != null && i < choices.Length)
                {
                    choiceText.gameObject.SetActive(true);
                    choiceText.text = $"{i + 1}. {choices[i].Text}";
                }
                else
                {
                    choiceText.gameObject.SetActive(false);
                }
            }
        }

        private void SelectChoice(int index)
        {
            var choices = currentNode.Choices;
            if (choices == null || index < 0 || index >= choices.Length)
            {
                return;
            }

            var choice = choices[index];
            if (!string.IsNullOrWhiteSpace(choice.FlagToSet))
            {
                DecisionFlagService.Instance?.SetFlag(choice.FlagToSet);
            }

            if (!string.IsNullOrWhiteSpace(choice.QuestAction))
            {
                QuestService.Instance?.RunAction(choice.QuestAction);
            }

            if (choice.ClosesDialogue)
            {
                CloseDialogue();
                return;
            }

            ShowNode(choice.NextNodeId);
        }
    }
}
