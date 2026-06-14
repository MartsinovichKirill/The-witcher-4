using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace WitcherRightVersion.Editor
{
    /// <summary>
    /// Builds Mecanim AnimatorControllers from the clips already embedded in the
    /// Quaternius FBX (discovered via AnimationProbe), and marks locomotion clips as
    /// looping. Generic rigs; gameplay code drives Speed/Attack/Dodge/Dead params.
    /// Idempotent: re-running rebuilds the controllers in place.
    /// </summary>
    public static class CharacterAnimationSetup
    {
        public const string KnightPath = "Assets/Art/External/Quaternius_Knight/Knight Character by @Quaternius/FBX/KnightCharacter.fbx";
        public const string SkeletonPath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX/Skeleton.fbx";
        public const string SlimePath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX/Slime.fbx";
        public const string BatPath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX/Bat.fbx";
        public const string DragonPath = "Assets/Art/External/Quaternius_AnimatedMonsters/FBX/Dragon.fbx";
        // OpenGameArt RPG characters (NPCs, bandits, enforcers) share one rig + clip set.
        public const string RpgWarriorPath = "Assets/Art/External/OpenGameArt_RPGCharacters/FBX/Warrior.fbx";

        public const string ControllerDir = "Assets/Generated/Animators";
        public const string ReynardController = ControllerDir + "/ReynardKnight.controller";
        public const string SkeletonController = ControllerDir + "/SkeletonGuard.controller";
        public const string DrownerController = ControllerDir + "/DrownerSlime.controller";
        public const string BatController = ControllerDir + "/BatFlyer.controller";
        public const string DragonController = ControllerDir + "/DragonFlyer.controller";
        public const string RpgController = ControllerDir + "/RpgCharacter.controller";

        [MenuItem("Tools/Witcher Right Version/Setup Character Animations")]
        public static void Setup()
        {
            // Loop the locomotion clips; leave one-shot clips (attack/death) alone.
            ConfigureLooping(KnightPath, name => NameMatches(name, "idle", "walk", "run") && !name.ToLower().Contains("attack"));
            ConfigureLooping(SkeletonPath, name => NameMatches(name, "idle", "running", "walk"));
            ConfigureLooping(SlimePath, name => NameMatches(name, "idle", "walk"));
            ConfigureLooping(BatPath, name => NameMatches(name, "flying"));
            ConfigureLooping(DragonPath, name => NameMatches(name, "flying"));
            ConfigureLooping(RpgWarriorPath, name => (name == "CharacterArmature|Idle" || name == "CharacterArmature|Walk" || name == "CharacterArmature|Run"));

            if (!AssetDatabase.IsValidFolder("Assets/Generated"))
            {
                AssetDatabase.CreateFolder("Assets", "Generated");
            }
            if (!AssetDatabase.IsValidFolder(ControllerDir))
            {
                AssetDatabase.CreateFolder("Assets/Generated", "Animators");
            }

            BuildHumanoidController(ReynardController, KnightPath,
                idle: "HumanArmature|Idle",
                walk: "HumanArmature|Walking",
                run: "HumanArmature|Run",
                attack: "HumanArmature|Run_swordAttack",
                dodge: "HumanArmature|Roll_sword",
                death: "HumanArmature|Death",
                runThreshold: 5f);

            BuildHumanoidController(SkeletonController, SkeletonPath,
                idle: "SkeletonArmature|Skeleton_Idle",
                walk: "SkeletonArmature|Skeleton_Running",
                run: "SkeletonArmature|Skeleton_Running",
                attack: "SkeletonArmature|Skeleton_Attack",
                dodge: null,
                death: "SkeletonArmature|Skeleton_Death",
                runThreshold: 99f);

            BuildHumanoidController(DrownerController, SlimePath,
                idle: "Armature|Slime_Idle",
                walk: "Armature|Slime_Walk",
                run: "Armature|Slime_Walk",
                attack: "Armature|Slime_Attack",
                dodge: null,
                death: "Armature|Slime_Death",
                runThreshold: 99f);

            // Shared RPG-character controller (Idle/Walk/Run/Attack/Death). NPCs stay in
            // Idle; bandits/enforcers are driven by EnemyAnimatorDriver. The shared
            // CharacterArmature means Warrior's clips animate any RPG model placed.
            BuildHumanoidController(RpgController, RpgWarriorPath,
                idle: "CharacterArmature|Idle",
                walk: "CharacterArmature|Walk",
                run: "CharacterArmature|Run",
                attack: "CharacterArmature|Sword_Attack",
                dodge: "CharacterArmature|Roll",
                death: "CharacterArmature|Death",
                runThreshold: 5f);

            // Simple single-state looping flight controllers for ambient flying monsters.
            BuildLoopController(BatController, BatPath, "BatArmature|Bat_Flying");
            BuildLoopController(DragonController, DragonPath, "DragonArmature|Dragon_Flying");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("CharacterAnimationSetup: controllers built.");
        }

        // One looping state, no parameters: makes a dormant flier animate continuously.
        private static void BuildLoopController(string controllerPath, string fbxPath, string clipName)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var clip = LoadClip(fbxPath, clipName);
            var state = controller.layers[0].stateMachine.AddState("Fly");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
        }

        private static bool NameMatches(string clipName, params string[] tokens)
        {
            var lower = clipName.ToLower();
            return tokens.Any(t => lower.Contains(t));
        }

        private static void ConfigureLooping(string fbxPath, System.Func<string, bool> shouldLoop)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"CharacterAnimationSetup: no ModelImporter at {fbxPath}");
                return;
            }

            // Seed explicit clip definitions from the default takes, then flag loopers.
            var defaults = importer.defaultClipAnimations;
            if (defaults == null || defaults.Length == 0)
            {
                return;
            }

            for (var i = 0; i < defaults.Length; i++)
            {
                var wantLoop = shouldLoop(defaults[i].name);
                defaults[i].loopTime = wantLoop;
            }

            importer.clipAnimations = defaults;
            importer.SaveAndReimport();
        }

        private static AnimationClip LoadClip(string fbxPath, string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return null;
            }

            var clip = AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => c != null && c.name == clipName && !c.name.StartsWith("__preview__"));
            if (clip == null)
            {
                Debug.LogWarning($"CharacterAnimationSetup: clip '{clipName}' not found in {fbxPath}");
            }

            return clip;
        }

        private static void BuildHumanoidController(string controllerPath, string fbxPath,
            string idle, string walk, string run, string attack, string dodge, string death, float runThreshold)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dodge", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;

            var idleClip = LoadClip(fbxPath, idle);
            var walkClip = LoadClip(fbxPath, walk);
            var runClip = LoadClip(fbxPath, run);
            var attackClip = LoadClip(fbxPath, attack);
            var dodgeClip = LoadClip(fbxPath, dodge);
            var deathClip = LoadClip(fbxPath, death);

            var idleState = sm.AddState("Idle");
            idleState.motion = idleClip;
            sm.defaultState = idleState;

            var walkState = sm.AddState("Walk");
            walkState.motion = walkClip;

            var runState = sm.AddState("Run");
            runState.motion = runClip;

            // Idle <-> Walk on Speed.
            AddTransition(idleState, walkState, "Speed", AnimatorConditionMode.Greater, 0.1f, 0.12f);
            AddTransition(walkState, idleState, "Speed", AnimatorConditionMode.Less, 0.1f, 0.15f);
            // Walk <-> Run on Speed threshold (skip if same clip / no run split).
            if (runThreshold < 50f)
            {
                AddTransition(walkState, runState, "Speed", AnimatorConditionMode.Greater, runThreshold, 0.12f);
                AddTransition(runState, walkState, "Speed", AnimatorConditionMode.Less, runThreshold, 0.15f);
            }

            // Attack from any state.
            if (attackClip != null)
            {
                var attackState = sm.AddState("Attack");
                attackState.motion = attackClip;
                var toAttack = sm.AddAnyStateTransition(attackState);
                toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
                toAttack.duration = 0.06f;
                toAttack.canTransitionToSelf = false;
                var attackExit = attackState.AddExitTransition();
                attackExit.hasExitTime = true;
                attackExit.exitTime = 0.85f;
                attackExit.duration = 0.12f;
            }

            // Dodge/Roll from any state.
            if (dodgeClip != null)
            {
                var dodgeState = sm.AddState("Dodge");
                dodgeState.motion = dodgeClip;
                var toDodge = sm.AddAnyStateTransition(dodgeState);
                toDodge.AddCondition(AnimatorConditionMode.If, 0f, "Dodge");
                toDodge.duration = 0.05f;
                toDodge.canTransitionToSelf = false;
                var dodgeExit = dodgeState.AddExitTransition();
                dodgeExit.hasExitTime = true;
                dodgeExit.exitTime = 0.85f;
                dodgeExit.duration = 0.1f;
            }

            // Death from any state on Dead bool.
            if (deathClip != null)
            {
                var deathState = sm.AddState("Death");
                deathState.motion = deathClip;
                var toDeath = sm.AddAnyStateTransition(deathState);
                toDeath.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
                toDeath.duration = 0.1f;
                toDeath.canTransitionToSelf = false;
            }

            EditorUtility.SetDirty(controller);
        }

        private static void AddTransition(AnimatorState from, AnimatorState to, string param, AnimatorConditionMode mode, float threshold, float duration)
        {
            var t = from.AddTransition(to);
            t.AddCondition(mode, threshold, param);
            t.hasExitTime = false;
            t.duration = duration;
        }
    }
}
