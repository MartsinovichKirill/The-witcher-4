# ROADMAP: The Witcher 4: Right Version

## Rule For Every Stage
Before starting a stage:
1. Read `PROJECT_MEMORY.md`.
2. Work on only one main system.
3. Keep the change small enough to test.
4. After finishing, update `PROJECT_MEMORY.md`.

## Current Milestone
Milestone: Vertical Slice

Goal: make a 20-30 minute playable version with the full loop:

`contract -> conversation -> traces -> combat -> choice -> reward -> save/load`

## Short Development Plan
### Stage 0: Project Memory
Status: done

Tasks:
- Create `PROJECT_MEMORY.md`.
- Create `ROADMAP.md`.
- Store fixed story, systems, items, NPCs, quests, flags, and current status.

Acceptance:
- Both files exist.
- Next work can start by reading them.

### Stage 1: Unity Project Skeleton
Status: done

Tasks:
- Create Unity project in `G:\The witcher 2`.
- Create folders:
  - `Assets/Scenes`
  - `Assets/Scripts`
  - `Assets/Scripts/Core`
  - `Assets/Scripts/Player`
  - `Assets/Scripts/Combat`
  - `Assets/Scripts/Dialogue`
  - `Assets/Scripts/Quest`
  - `Assets/Scripts/Inventory`
  - `Assets/Scripts/Crafting`
  - `Assets/Scripts/Save`
  - `Assets/Scripts/UI`
  - `Assets/Prefabs`
  - `Assets/ScriptableObjects`
  - `Assets/Art`
  - `Assets/Audio`
  - `Assets/Materials`
  - `Assets/Docs`
- Create initial scenes:
  - `MainMenuScene`
  - `VillageScene`

Acceptance:
- Unity opens the project.
- Scenes exist.
- Folder structure exists.
- No gameplay systems are implemented yet.

### Stage 2: Main Menu And Settings Stub
Status: done

Tasks:
- Build `MainMenuScene`.
- Add buttons:
  - New Game
  - Continue
  - Settings
  - Exit
- New Game loads `VillageScene`.
- Continue can be disabled or show "No save yet" until save system exists.
- Settings panel contains volume, music toggle, resolution placeholder, graphics placeholder.

Acceptance:
- Game starts at main menu.
- New Game enters the village scene.
- Settings panel opens and closes.
- Exit button quits in build or logs in editor.

### Stage 3: Player Movement And Third-Person Camera
Status: done

Tasks:
- Add placeholder player capsule or simple model.
- Implement `PlayerController`.
- Add walking, running, rotation.
- Add third-person camera follow.
- Add basic ground collision.

Acceptance:
- Player can move in `VillageScene`.
- Camera follows behind the player.
- Player cannot fall through ground.
- No combat, quests, or inventory in this stage.

### Stage 4: Interaction System
Status: next

Tasks:
- Implement `InteractionController`.
- Add interact prompt.
- Add interactable interface for NPCs, traces, doors, and objects.
- Add simple test object in village.

Acceptance:
- Player can approach object.
- Prompt appears.
- Pressing interaction key triggers object response.
- System is reusable for NPC dialogue and traces.

### Stage 5: Dialogue With Elder Voytsekh
Status: planned

Tasks:
- Implement simple `DialogueService`.
- Add Elder Voytsekh NPC.
- Add first dialogue:
  - greet player
  - explain swamp contract
  - offer contract
  - allow accept choice
- On accept, set flag `acceptedSwampContract`.

Acceptance:
- Player can talk to elder.
- Dialogue choices work.
- Accepting contract stores the flag.
- No quest UI is required yet unless minimal text is faster.

### Stage 6: QuestService And First Quest
Status: planned

Tasks:
- Implement `QuestService`.
- Add quest state enum.
- Add first quest: `Contract: Beast from the Swamp`.
- Stages:
  1. Talk to Elder Voytsekh.
  2. Speak with Marta.
  3. Find swamp traces.
  4. Kill the drowner.
  5. Return to Elder.
  6. Choose response.
  7. Receive reward.

Acceptance:
- Quest starts from elder dialogue.
- Quest can advance by scripted events.
- Current quest stage can be printed to screen or debug log.
- Memory update records the new flag and reward.

### Stage 7: Dialogue With Marta
Status: planned

Tasks:
- Add Marta NPC.
- Add dialogue explaining:
  - swamp poison
  - strange curse symptoms
  - possible use of Antitoxin
- Advance first quest stage after conversation.

Acceptance:
- Player can talk to Marta after accepting contract.
- Quest advances to trace investigation.
- Marta does not yet need full shop/heal system.

### Stage 8: Trace Investigation
Status: planned

Tasks:
- Add trace objects between village and swamp area.
- Interacting with traces advances quest.
- Trace examples:
  - claw marks
  - slime trail
  - torn cloth
- After enough traces, unlock drowner encounter.

Acceptance:
- Player can inspect traces.
- Quest stage changes after required traces.
- Player is guided toward combat area.

### Stage 9: Basic Combat
Status: planned

Tasks:
- Implement `CombatController`.
- Add health for player and enemy.
- Add light attack.
- Add enemy damage.
- Add death handling.
- Add first drowner enemy.

Acceptance:
- Player can damage drowner.
- Drowner can damage player.
- Drowner dies.
- Flag `killedFirstDrowner` is set.
- Quest advances to return stage.

### Stage 10: Combat Expansion For Vertical Slice
Status: planned

Tasks:
- Add heavy attack.
- Add block.
- Add dodge.
- Add simple sword type handling.
- Add one sign, preferably Aard or Igni.

Acceptance:
- Player has at least light attack, dodge, and one sign.
- Block works or is clearly deferred to MVP.
- Combat remains simple and stable.

### Stage 11: Choice And Reward
Status: planned

Tasks:
- Return to elder after drowner kill.
- Add choice:
  - "The swamp is guilty. Pay me."
  - "This is not just a monster. Something is wrong here."
- First choice supports elder version.
- Second choice sets `questionedElderVersion`.
- Give reward:
  - 50 XP
  - 20 coins
  - Antitoxin recipe
- Set `receivedAntitoxinRecipe`.

Acceptance:
- First quest can be completed.
- Reward is granted.
- Choice flag is saved in game state.
- Player sees a clear completion message.

### Stage 12: Minimal Inventory
Status: planned

Tasks:
- Implement `InventoryService`.
- Add coins, recipe unlock, and simple item list.
- Add starting swords.
- Add basic UI or debug panel for inventory.

Acceptance:
- Reward items/coins persist while playing.
- Player has steel and silver sword records.
- Inventory can be inspected.

### Stage 13: Save And Load
Status: planned

Tasks:
- Implement `SaveService`.
- Save:
  - current scene
  - player position
  - health
  - quest state
  - decision flags
  - inventory basics
- Add 1 autosave first.
- Add 3 manual slots after autosave works.

Acceptance:
- Player can complete part of vertical slice, save, exit play mode or reload, and continue.
- Quest flags survive load.
- `acceptedSwampContract`, `questionedElderVersion`, `killedFirstDrowner`, and `receivedAntitoxinRecipe` can be persisted.

### Stage 14: Vertical Slice Polish
Status: planned

Tasks:
- Add simple UI for health and active quest.
- Add placeholder sound effects.
- Add fog/light mood for village and swamp.
- Add tutorial hints only if needed.
- Test full 20-30 minute flow.

Acceptance:
- A player can complete the vertical slice without developer help.
- No blocker bugs in the first quest.
- Save/load works.
- The project can move to MVP.

## MVP After Vertical Slice
Only start after vertical slice is stable.

MVP tasks:
- Add second zone: Old Forest or expanded Black Swamp.
- Add 2 more quests.
- Add basic crafting.
- Add basic inventory UI.
- Add XP and level.
- Add one working ending.
- Build Windows executable.

MVP acceptance:
- Main menu works.
- 2 zones are playable.
- 3 quests are complete.
- Combat, dialogue, inventory, XP, and save/load work.
- One ending can be reached.
- Build runs without Unity Editor.

## Alpha After MVP
Alpha tasks:
- Add all 5 zones.
- Add all 8 quests.
- Add all NPCs.
- Add all ending logic.
- Add final decision dependencies.
- Replace worst placeholders.

Alpha acceptance:
- Full game is playable from start to one of three endings.
- Bugs are allowed, blockers are not acceptable.

## Beta After Alpha
Beta tasks:
- Fix critical bugs.
- Balance combat, rewards, and crafting.
- Improve UI.
- Improve sound and atmosphere.
- Verify save/load across scenes.

Beta acceptance:
- Full playthrough has no blockers.
- All endings are reachable.
- Saves do not break progress.

## Release
Release tasks:
- Final Windows build.
- Final test pass.
- Prepare material for PZ/documentation after game is stable.

Release acceptance:
- Build is stable.
- Main quest and side quests complete correctly.
- UI, controls, saving, and endings work.
- Ready for demonstration.
