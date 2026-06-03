using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WitcherRightVersion.Combat;
using WitcherRightVersion.Core;
using WitcherRightVersion.Dialogue;
using WitcherRightVersion.Interaction;
using WitcherRightVersion.Inventory;
using WitcherRightVersion.Player;
using WitcherRightVersion.Quest;
using WitcherRightVersion.Save;
using WitcherRightVersion.UI;

namespace WitcherRightVersion.Editor
{
    public static class VillageSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/VillageScene.unity";

        [MenuItem("Tools/Witcher Right Version/Build Village Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            AssetDatabase.Refresh();

            RemoveIfExists("Reynard_Player");
            RemoveIfExists("ThirdPersonCamera");
            RemoveIfExists("VillageMovementTestArea");
            RemoveIfExists("VillageBlockoutGround");
            RemoveIfExists("VillagePropRoot");
            RemoveIfExists("InteractionCanvas");
            RemoveIfExists("DialogueCanvas");
            RemoveIfExists("QuestCanvas");
            RemoveIfExists("HealthCanvas");
            RemoveIfExists("InventoryCanvas");
            RemoveIfExists("InteractionDemoRoot");
            RemoveIfExists("RuntimeServices");

            CreateRuntimeServices();
            CreateLighting();
            var ground = CreateGround();
            var player = CreatePlayer();
            CreateCamera(player.transform);
            CreateMovementMarkers(ground.transform);
            CreateVillageProps();
            CreateInteractionDemoObjects();
            CreateInteractionCanvas();
            CreateDialogueCanvas();
            CreateQuestCanvas();
            CreateHealthCanvas();
            CreateInventoryCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static GameObject CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "VillageBlockoutGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(12f, 1f, 12f);

            var material = CreateMaterial("Assets/Materials/VillageGround.mat", new Color(0.16f, 0.22f, 0.15f, 1f));
            ground.GetComponent<Renderer>().sharedMaterial = material;
            return ground;
        }

        private static GameObject CreatePlayer()
        {
            var player = new GameObject("Reynard_Player");
            player.name = "Reynard_Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.05f, 0f);
            player.transform.rotation = Quaternion.identity;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2.05f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 1.02f, 0f);
            controller.stepOffset = 0.35f;
            controller.slopeLimit = 45f;

            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionController>();
            var health = player.AddComponent<Health>();
            health.Configure("Reynard", 120f);
            player.AddComponent<CombatController>();
            CreatePlayerVisual(player.transform);
            return player;
        }

        private static void CreatePlayerVisual(Transform player)
        {
            var knight = InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/KnightCharacter.fbx",
                "ReynardKnightModel",
                player,
                new Vector3(0f, -0.02f, 0f),
                Quaternion.Euler(0f, 180f, 0f),
                Vector3.one);

            if (knight == null)
            {
                CreateFallbackPlayerVisual(player);
                return;
            }

            InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/Sword.fbx",
                "ReynardSteelSword_Visual",
                player,
                new Vector3(-0.32f, 1.2f, -0.18f),
                Quaternion.Euler(65f, 0f, 25f),
                new Vector3(0.9f, 0.9f, 0.9f));

            InstantiateModel(
                "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/ShortSword.fbx",
                "ReynardSilverSword_Visual",
                player,
                new Vector3(0.32f, 1.12f, -0.18f),
                Quaternion.Euler(65f, 0f, -25f),
                new Vector3(0.9f, 0.9f, 0.9f));
        }

        private static void CreateFallbackPlayerVisual(Transform player)
        {
            var fallback = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fallback.name = "ReynardPlaceholderCapsule";
            fallback.transform.SetParent(player, false);
            fallback.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            fallback.transform.localRotation = Quaternion.identity;
            fallback.transform.localScale = Vector3.one;

            var collider = fallback.GetComponent<CapsuleCollider>();
            Object.DestroyImmediate(collider);

            var material = CreateMaterial("Assets/Materials/ReynardPlaceholder.mat", new Color(0.22f, 0.25f, 0.23f, 1f));
            fallback.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateCamera(Transform target)
        {
            var cameraObject = new GameObject("ThirdPersonCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 4f, -6f);
            cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 500f;

            cameraObject.AddComponent<AudioListener>();
            var follow = cameraObject.AddComponent<ThirdPersonCamera>();
            follow.Target = target;
        }

        private static void CreateLighting()
        {
            var light = Object.FindAnyObjectByType<Light>();
            if (light == null)
            {
                var lightObject = new GameObject("Directional Light");
                light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
            }

            light.name = "VillageSunLight";
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.26f, 1f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.38f, 0.42f, 0.38f, 1f);
            RenderSettings.fogDensity = 0.012f;
        }

        private static void CreateMovementMarkers(Transform parent)
        {
            var root = new GameObject("VillageMovementTestArea");
            root.transform.position = Vector3.zero;

            CreateMarker(root.transform, "NorthMarker", new Vector3(0f, 0.5f, 10f), new Color(0.53f, 0.42f, 0.18f, 1f));
            CreateMarker(root.transform, "EastMarker", new Vector3(10f, 0.5f, 0f), new Color(0.42f, 0.16f, 0.12f, 1f));
            CreateMarker(root.transform, "SouthMarker", new Vector3(0f, 0.5f, -10f), new Color(0.18f, 0.32f, 0.18f, 1f));
            CreateMarker(root.transform, "WestMarker", new Vector3(-10f, 0.5f, 0f), new Color(0.22f, 0.2f, 0.28f, 1f));
        }

        private static void CreateMarker(Transform parent, string name, Vector3 position, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, true);
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(1f, 1f, 1f);
            marker.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{name}.mat", color);
        }

        private static void CreateVillageProps()
        {
            var root = new GameObject("VillagePropRoot");
            root.transform.position = Vector3.zero;

            PlaceProp(root.transform, "VillageRoad_01", "road.fbx", new Vector3(0f, 0.02f, -4f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRoad_02", "road.fbx", new Vector3(0f, 0.02f, 0f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRoad_03", "road.fbx", new Vector3(0f, 0.02f, 4f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageCart_01", "cart.fbx", new Vector3(-3.6f, 0f, 2.4f), Quaternion.Euler(0f, 35f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageFence_01", "fence.fbx", new Vector3(3.2f, 0f, 2.8f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageFence_02", "fence-broken.fbx", new Vector3(3.2f, 0f, 4.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageLantern_01", "lantern.fbx", new Vector3(-1.9f, 0f, 3.8f), Quaternion.identity, Vector3.one);
            PlaceProp(root.transform, "VillageRock_01", "rock-small.fbx", new Vector3(4.4f, 0f, -2.6f), Quaternion.Euler(0f, 20f, 0f), Vector3.one);
            PlaceProp(root.transform, "VillageBanner_01", "banner-red.fbx", new Vector3(-4.4f, 0f, -1.4f), Quaternion.Euler(0f, -35f, 0f), Vector3.one);
        }

        private static void CreateInteractionDemoObjects()
        {
            var root = new GameObject("InteractionDemoRoot");
            root.transform.position = Vector3.zero;

            CreateElderDialogue(root.transform);
            CreateMartaDialogue(root.transform);

            CreateSwampTraceObjects(root.transform);
            CreateFirstDrowner(root.transform);
        }

        private static void CreateSwampTraceObjects(Transform parent)
        {
            CreateInteractableTrace(
                parent,
                "SwampTrace_ClawMarks",
                "Claw marks",
                "Inspect",
                QuestService.ActionSwampTracesFound,
                "Deep claw cuts in the mud. Too wide for a wolf. Something dragged itself toward the reeds.",
                "These claw marks will matter after Marta explains what to look for.",
                new Vector3(0.8f, 0.08f, 3.2f),
                new Vector3(0.9f, 0.04f, 0.55f),
                new Color(0.18f, 0.12f, 0.08f, 1f));

            CreateInteractableTrace(
                parent,
                "SwampTrace_SlimeTrail",
                "Black slime trail",
                "Inspect",
                QuestService.ActionSwampTracesFound,
                "Black slime bubbles under the grass. Marta was right: the trail is poisoned.",
                "The slime looks wrong, but Reynard needs Marta's warning first.",
                new Vector3(1.9f, 0.08f, 4.35f),
                new Vector3(0.55f, 0.04f, 1.1f),
                new Color(0.08f, 0.16f, 0.11f, 1f));

            CreateInteractableTrace(
                parent,
                "SwampTrace_TornCloth",
                "Torn cloth",
                "Inspect",
                QuestService.ActionSwampTracesFound,
                "A strip of wet cloth hangs on the reeds. The victim was pulled toward deeper water.",
                "This cloth is just a rag until Reynard knows the swamp signs.",
                new Vector3(3.0f, 0.08f, 5.2f),
                new Vector3(0.45f, 0.04f, 0.45f),
                new Color(0.32f, 0.29f, 0.22f, 1f));
        }

        private static void CreateElderDialogue(Transform parent)
        {
            var elder = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            elder.name = "ElderVoytsekh_Prototype";
            elder.transform.SetParent(parent, true);
            elder.transform.position = new Vector3(2.2f, 1f, -1.7f);
            elder.transform.rotation = Quaternion.identity;
            elder.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            elder.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/ElderVoytsekh_Prototype.mat", new Color(0.22f, 0.18f, 0.13f, 1f));

            var dialogue = elder.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Elder Voytsekh",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Elder Voytsekh",
                        "Witcher. You came for the notice, then listen well. Something from the swamp drags people off the southern road.",
                        new[]
                        {
                            new DialogueChoice("Tell me about the contract.", "contract"),
                            new DialogueChoice("Why blame the swamp so quickly?", "doubt"),
                            new DialogueChoice("I will come back later.", "", "", true)
                        }),
                    new DialogueNode(
                        "contract",
                        "Elder Voytsekh",
                        "Find the tracks near the reeds, kill the beast, and return with proof. Fifty crowns when the road is safe again.",
                        new[]
                        {
                            new DialogueChoice("I accept the contract.", "accepted", "acceptedSwampContract", false, QuestService.ActionStartSwampContract),
                            new DialogueChoice("I need more answers first.", "doubt"),
                            new DialogueChoice("Not now.", "", "", true)
                        }),
                    new DialogueNode(
                        "doubt",
                        "Elder Voytsekh",
                        "Because I have buried three villagers this month. If you want old stories, ask Marta. If you want coin, go south.",
                        new[]
                        {
                            new DialogueChoice("I accept the contract.", "accepted", "acceptedSwampContract", false, QuestService.ActionStartSwampContract),
                            new DialogueChoice("I will speak with Marta first.", "", "", true)
                        }),
                    new DialogueNode(
                        "accepted",
                        "Elder Voytsekh",
                        "Good. Start at the muddy road south of the village. And witcher... do not stir up fears you cannot put back down.",
                        new[]
                        {
                            new DialogueChoice("I will find your beast.", "", "", true)
                        }),
                    new DialogueNode(
                        "return",
                        "Elder Voytsekh",
                        "You are back. I heard the thing screaming near the reeds. Tell me the road is clear.",
                        new[]
                        {
                            new DialogueChoice("The drowner is dead. Here is your proof.", "choice_response", "", false, QuestService.ActionReturnedToElder),
                            new DialogueChoice("Not yet.", "", "", true)
                        }),
                    new DialogueNode(
                        "choice_response",
                        "Elder Voytsekh",
                        "Then the village can breathe. The swamp gave us a beast, and you cut it down. That is the end of it.",
                        new[]
                        {
                            new DialogueChoice("The swamp is guilty. Pay me.", "reward", "MayorSupported", false, QuestService.ActionAcceptedElderVersion),
                            new DialogueChoice("This is not just a monster. Something is wrong here.", "warning_after_contract", "questionedElderVersion", false, QuestService.ActionQuestionedElderVersion)
                        }),
                    new DialogueNode(
                        "warning_after_contract",
                        "Elder Voytsekh",
                        "Careful, witcher. People here need sleep more than old wounds. Take your coin and leave buried things buried.",
                        new[]
                        {
                            new DialogueChoice("I will take the reward. But I am not done looking.", "reward")
                        }),
                    new DialogueNode(
                        "reward",
                        "Elder Voytsekh",
                        "Fifty experience worth of work, twenty crowns, and Marta's antitoxin recipe. Spend them quietly.",
                        new[]
                        {
                            new DialogueChoice("Contract complete.", "", "", true, QuestService.ActionRewardReceived)
                        }),
                    new DialogueNode(
                        "completed",
                        "Elder Voytsekh",
                        "The road is quieter now. Let us hope it stays that way.",
                        new[]
                        {
                            new DialogueChoice("For now.", "", "", true)
                        })
                },
                "return",
                "completed");
        }

        private static void CreateMartaDialogue(Transform parent)
        {
            var marta = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            marta.name = "MartaLozovaya_Prototype";
            marta.transform.SetParent(parent, true);
            marta.transform.position = new Vector3(-2.4f, 1f, -1.3f);
            marta.transform.rotation = Quaternion.identity;
            marta.transform.localScale = new Vector3(0.75f, 1f, 0.75f);
            marta.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/MartaLozovaya_Prototype.mat", new Color(0.16f, 0.24f, 0.17f, 1f));

            var dialogue = marta.AddComponent<DialogueInteractable>();
            dialogue.Configure(
                "Marta Lozovaya",
                "Talk",
                "start",
                new[]
                {
                    new DialogueNode(
                        "start",
                        "Marta Lozovaya",
                        "You walk like someone hired by Voytsekh. If he sent you south, do not trust dry boots and clean stories.",
                        new[]
                        {
                            new DialogueChoice("I took the swamp contract. What do you know?", "swamp_poison", "", false, QuestService.ActionMartaSpoken),
                            new DialogueChoice("Why does the village fear the swamp?", "curse"),
                            new DialogueChoice("I need herbs and remedies.", "antitoxin"),
                            new DialogueChoice("Later.", "", "", true)
                        }),
                    new DialogueNode(
                        "swamp_poison",
                        "Marta Lozovaya",
                        "The mud itself bites now. Look for black slime on the road, torn reeds, and tracks that sink too deep. If you smell rot before you see water, draw silver.",
                        new[]
                        {
                            new DialogueChoice("Then I start with the tracks.", "", "", true),
                            new DialogueChoice("Tell me about antidotes.", "antitoxin"),
                            new DialogueChoice("Why is this happening?", "curse")
                        }),
                    new DialogueNode(
                        "curse",
                        "Marta Lozovaya",
                        "A normal swamp kills slowly. This one remembers. People call Elsa a witch because it is easier than asking what the village buried.",
                        new[]
                        {
                            new DialogueChoice("That sounds bigger than a monster.", "warning"),
                            new DialogueChoice("Back to the contract.", "swamp_poison", "", false, QuestService.ActionMartaSpoken),
                            new DialogueChoice("Enough for now.", "", "", true)
                        }),
                    new DialogueNode(
                        "warning",
                        "Marta Lozovaya",
                        "It is. But first survive the thing in the reeds. Truth is useless if you drown before reaching it.",
                        new[]
                        {
                            new DialogueChoice("Show me how to stay alive.", "antitoxin"),
                            new DialogueChoice("I will inspect the tracks.", "", "", true)
                        }),
                    new DialogueNode(
                        "antitoxin",
                        "Marta Lozovaya",
                        "Boil bogweed with a little monster slime and keep it bitter. Proper antitoxin comes later, once you bring proof from the swamp.",
                        new[]
                        {
                            new DialogueChoice("I will remember that.", "", "", true),
                            new DialogueChoice("Tell me about the tracks.", "swamp_poison", "", false, QuestService.ActionMartaSpoken)
                        })
                });
        }

        private static void CreateInteractableTrace(Transform parent, string objectName, string displayName, string prompt, string questAction, string successMessage, string blockedMessage, Vector3 position, Vector3 scale, Color color)
        {
            var trace = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trace.name = objectName;
            trace.transform.SetParent(parent, true);
            trace.transform.position = position;
            trace.transform.rotation = Quaternion.identity;
            trace.transform.localScale = scale;
            trace.GetComponent<Renderer>().sharedMaterial = CreateMaterial($"Assets/Materials/{objectName}.mat", color);

            var interactable = trace.AddComponent<QuestProgressInteractable>();
            interactable.Configure(displayName, prompt, questAction, successMessage, blockedMessage);
        }

        private static void CreateFirstDrowner(Transform parent)
        {
            var drowner = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            drowner.name = "FirstDrowner_Prototype";
            drowner.transform.SetParent(parent, true);
            drowner.transform.position = new Vector3(5.2f, 1f, 6.1f);
            drowner.transform.rotation = Quaternion.Euler(0f, -130f, 0f);
            drowner.transform.localScale = new Vector3(0.9f, 0.85f, 0.9f);
            drowner.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/FirstDrowner_Prototype.mat", new Color(0.08f, 0.18f, 0.14f, 1f));

            var health = drowner.AddComponent<Health>();
            health.Configure("First drowner", 72f);

            var ai = drowner.AddComponent<EnemyAI>();
            ai.Configure("Drowner", true, "killedFirstDrowner", QuestService.ActionFirstDrownerKilled);
        }

        private static void CreateInteractionCanvas()
        {
            var canvasObject = new GameObject("InteractionCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var promptRoot = CreatePanel(canvasObject.transform, "InteractionPrompt", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(520f, 86f), new Vector2(0f, 76f), new Color(0.05f, 0.045f, 0.035f, 0.86f));
            var title = CreateText(promptRoot.transform, "PromptTitle", new Vector2(0f, 0.42f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(-36f, -16f), new Vector2(0f, -10f), 22, TextAnchor.MiddleLeft, new Color(0.94f, 0.84f, 0.58f, 1f));
            var action = CreateText(promptRoot.transform, "PromptAction", new Vector2(0f, 0f), new Vector2(1f, 0.58f), new Vector2(0.5f, 0f), new Vector2(-36f, -12f), new Vector2(0f, 10f), 18, TextAnchor.MiddleLeft, Color.white);

            var messageRoot = CreatePanel(canvasObject.transform, "InteractionMessage", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(720f, 64f), new Vector2(0f, 178f), new Color(0.08f, 0.07f, 0.055f, 0.9f));
            var message = CreateText(messageRoot.transform, "MessageText", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-40f, -18f), Vector2.zero, 19, TextAnchor.MiddleCenter, new Color(0.95f, 0.92f, 0.84f, 1f));

            var prompt = canvasObject.AddComponent<InteractionPromptUI>();
            SetSerializedObjectReference(prompt, "promptRoot", promptRoot);
            SetSerializedObjectReference(prompt, "titleText", title);
            SetSerializedObjectReference(prompt, "actionText", action);
            SetSerializedObjectReference(prompt, "messageRoot", messageRoot);
            SetSerializedObjectReference(prompt, "messageText", message);
        }

        private static void CreateDialogueCanvas()
        {
            var canvasObject = new GameObject("DialogueCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 70;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var dialogueRoot = CreatePanel(
                canvasObject.transform,
                "DialoguePanel",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(1180f, 330f),
                new Vector2(0f, 54f),
                new Color(0.045f, 0.04f, 0.033f, 0.94f));

            var speaker = CreateText(
                dialogueRoot.transform,
                "DialogueSpeaker",
                new Vector2(0f, 0.76f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-72f, -28f),
                new Vector2(0f, -18f),
                26,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.78f, 0.42f, 1f));

            var body = CreateText(
                dialogueRoot.transform,
                "DialogueBody",
                new Vector2(0f, 0.42f),
                new Vector2(1f, 0.78f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-72f, -18f),
                new Vector2(0f, -4f),
                22,
                TextAnchor.UpperLeft,
                new Color(0.94f, 0.91f, 0.84f, 1f));

            var choices = new Text[4];
            for (var i = 0; i < choices.Length; i++)
            {
                choices[i] = CreateText(
                    dialogueRoot.transform,
                    $"DialogueChoice_{i + 1}",
                    new Vector2(0f, 0.08f + i * 0.085f),
                    new Vector2(1f, 0.18f + i * 0.085f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(-72f, -6f),
                    Vector2.zero,
                    19,
                    TextAnchor.MiddleLeft,
                    Color.white);
            }

            var service = canvasObject.AddComponent<DialogueService>();
            SetSerializedObjectReference(service, "dialogueRoot", dialogueRoot);
            SetSerializedObjectReference(service, "speakerText", speaker);
            SetSerializedObjectReference(service, "bodyText", body);
            SetSerializedArrayReferences(service, "choiceTexts", choices);
        }

        private static void CreateQuestCanvas()
        {
            var canvasObject = new GameObject("QuestCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var hudRoot = CreatePanel(
                canvasObject.transform,
                "QuestHudPanel",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(560f, 122f),
                new Vector2(-44f, -44f),
                new Color(0.045f, 0.04f, 0.032f, 0.88f));

            var title = CreateText(
                hudRoot.transform,
                "QuestHudTitle",
                new Vector2(0f, 0.58f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-44f, -16f),
                new Vector2(0f, -10f),
                21,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.78f, 0.42f, 1f));

            var objective = CreateText(
                hudRoot.transform,
                "QuestHudObjective",
                new Vector2(0f, 0f),
                new Vector2(1f, 0.62f),
                new Vector2(0.5f, 0f),
                new Vector2(-44f, -16f),
                new Vector2(0f, 10f),
                18,
                TextAnchor.MiddleLeft,
                new Color(0.93f, 0.9f, 0.82f, 1f));

            var hud = canvasObject.AddComponent<QuestHudUI>();
            SetSerializedObjectReference(hud, "hudRoot", hudRoot);
            SetSerializedObjectReference(hud, "titleText", title);
            SetSerializedObjectReference(hud, "objectiveText", objective);
        }

        private static void CreateHealthCanvas()
        {
            var canvasObject = new GameObject("HealthCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 45;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var hudRoot = CreatePanel(
                canvasObject.transform,
                "HealthHudPanel",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(420f, 92f),
                new Vector2(44f, -44f),
                new Color(0.045f, 0.04f, 0.032f, 0.88f));

            var barBack = CreatePanel(
                hudRoot.transform,
                "HealthBarBack",
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(-44f, 22f),
                new Vector2(0f, 20f),
                new Color(0.14f, 0.05f, 0.04f, 1f));

            var barFillObject = CreatePanel(
                barBack.transform,
                "HealthBarFill",
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.68f, 0.11f, 0.08f, 1f));

            var fillImage = barFillObject.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 1f;

            var healthText = CreateText(
                hudRoot.transform,
                "HealthHudText",
                new Vector2(0f, 0.48f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-44f, -14f),
                new Vector2(0f, -10f),
                21,
                TextAnchor.MiddleLeft,
                new Color(0.96f, 0.88f, 0.77f, 1f));

            var hud = canvasObject.AddComponent<HealthHudUI>();
            SetSerializedObjectReference(hud, "healthText", healthText);
            SetSerializedObjectReference(hud, "healthFill", fillImage);
        }

        private static void CreateInventoryCanvas()
        {
            var canvasObject = new GameObject("InventoryCanvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            var panel = CreatePanel(
                canvasObject.transform,
                "InventoryPanel",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(500f, 440f),
                new Vector2(44f, -44f),
                new Color(0.045f, 0.04f, 0.033f, 0.93f));

            var content = CreateText(
                panel.transform,
                "InventoryContent",
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f),
                new Vector2(-48f, -42f),
                Vector2.zero,
                19,
                TextAnchor.UpperLeft,
                new Color(0.93f, 0.9f, 0.82f, 1f));

            var hud = canvasObject.AddComponent<InventoryHudUI>();
            SetSerializedObjectReference(hud, "panelRoot", panel);
            SetSerializedObjectReference(hud, "contentText", content);
        }

        private static void CreateRuntimeServices()
        {
            var services = new GameObject("RuntimeServices");
            services.AddComponent<DecisionFlagService>();
            services.AddComponent<PlayerRewardService>();
            services.AddComponent<InventoryService>();
            services.AddComponent<QuestService>();
            services.AddComponent<SaveService>();
        }

        private static void PlaceProp(Transform parent, string objectName, string modelName, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var prop = InstantiateModel(
                $"Assets/Art/External/Kenney_FantasyTownKit/Models/FBX format/{modelName}",
                objectName,
                parent,
                position,
                rotation,
                scale);

            if (prop != null)
            {
                return;
            }

            var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            placeholder.name = objectName + "_Placeholder";
            placeholder.transform.SetParent(parent, false);
            placeholder.transform.localPosition = position + Vector3.up * 0.3f;
            placeholder.transform.localRotation = rotation;
            placeholder.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            placeholder.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Assets/Materials/VillagePropPlaceholder.mat", new Color(0.28f, 0.22f, 0.14f, 1f));
        }

        private static GameObject InstantiateModel(string assetPath, string objectName, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning($"Model asset not found: {assetPath}");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
            if (instance == null)
            {
                instance = Object.Instantiate(asset);
            }

            instance.name = objectName;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            instance.transform.localScale = localScale;
            return instance;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                material.color = color;
                return material;
            }

            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;

            var image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, int fontSize, TextAnchor alignment, Color color)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void SetSerializedObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            serializedObject.FindProperty(propertyName).objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSerializedArrayReferences(Object target, string propertyName, Object[] values)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            property.arraySize = values.Length;

            for (var i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemoveIfExists(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }
    }
}
