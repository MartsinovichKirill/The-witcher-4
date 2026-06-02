# PROJECT MEMORY: The Witcher 4: Right Version

## Purpose
This file is the shared memory of the project. Read it before every new development stage and update it after every completed system, quest, scene, or important decision.

The goal is to avoid losing context, changing core decisions by accident, or building several unrelated systems at the same time.

## Current Project Status
- Current workspace: `G:\The witcher 2`
- Current stage: Unity skeleton created
- Current focus: prepare Stage 2 main menu and settings stub
- Next focus: implement `MainMenuScene` menu stub
- Unity project status: created
- Git status: local repository initialized; Git is available through Visual Studio bundled `git.exe`, not through PATH
- Unity status: Unity Hub is installed; Unity Editor `6000.4.9f1` is installed at `C:\Program Files\Unity\Hub\Editor\6000.4.9f1\Editor\Unity.exe`
- Unity modules detected: `windowsstandalonesupport`, `WebGLSupport`
- Visual Studio status: Visual Studio exists on this PC

## Fixed Game Identity
- Title: `The Witcher 4: Right Version`
- Type: educational non-commercial fan RPG
- Engine: Unity LTS
- Language: C#
- Platform: Windows
- Camera: third person
- Mode: single-player
- First playable target: vertical slice, 20-30 minutes
- Full planned game shape: 5 connected zones, 8 quests, 3 endings
- Main development rule: first make a small complete playable slice, then expand

## Creative Direction
The player controls a new witcher named Reynard. He arrives in the cursed region of Velemar after taking a contract from the village elder. At first the job looks simple: talk to the elder, inspect traces, go to the swamp, kill the monster, return for payment.

The investigation reveals that the monster is only a symptom. Years ago, villagers and the mage Orten used the Mirror of Truth to rewrite people's memory and hide the murder of an innocent girl. The "right version" of history saved the guilty people, but the buried truth became a curse.

Core theme: truth can heal the world, but it can also destroy the life people built on lies.

## Main Story Facts
- Region: Velemar
- Main village: Heather Ford / Vereskovy Brod
- Main hero: Reynard, a new witcher
- Main artifact: Mirror of Truth
- Hidden crime: murder of an innocent girl, rewritten by magic
- Public false story: the girl was a witch and her death saved the village
- Real consequence: swamp rot, undead, disappearances, disease, ghostly voice from the well

## Story Acts
1. Contract
   - Reynard arrives in Vereskovy Brod.
   - Elder Voytsekh gives the swamp beast contract.
   - Player learns village layout, dialogue, investigation, and first combat.

2. Investigation
   - Player explores Old Forest and Black Swamp.
   - Player finds traces, ritual stones, the messenger note, Elsa's hut, and first mirror shards.
   - Elsa stops looking like the real villain.

3. Tower
   - Player opens Tower Ruins.
   - Player finds Orten's diary, mirror hall, laboratory, and the ghost of the murdered girl.
   - Truth about the crime becomes clear.

4. Consequences
   - Player goes to Ash Road.
   - Final area changes depending on decisions.
   - Player chooses one of three endings.

## Endings
| Ending | Required / Expected Condition | Result |
|---|---|---|
| Truth | Girl's medallion found and enough evidence collected | The crime is exposed. Curse is lifted. Village morally collapses. |
| Corrected History / Lie | Player supports the elder or lacks evidence | Memory is rewritten again. Village is saved, but the lie becomes stronger. |
| Sacrifice | Orten's diary found or mirror destruction path unlocked | Mirror is destroyed. Curse ends, but some infected villagers die. |

## World Map
```text
                         [Tower Ruins]
                              |
[Old Forest] ----- [Vereskovy Brod] ----- [Ash Road]
                              |
                         [Black Swamp]
```

## Scenes
| Scene | Purpose | Status |
|---|---|---|
| `MainMenuScene` | Main menu, settings, load game | planned |
| `VillageScene` | Vereskovy Brod hub | planned |
| `ForestScene` | Old Forest investigation and early enemies | planned |
| `SwampScene` | Black Swamp, Elsa, drowner nest, poison | planned |
| `TowerRuinsScene` | Truth reveal, Orten, mirror hall | planned |
| `AshRoadScene` | Final consequences and endings | planned |

## Vertical Slice Scope
The vertical slice must be a small complete version of the game loop:

`contract -> conversation -> traces -> combat -> choice -> reward -> save/load`

Vertical slice content:
- partial `VillageScene`
- small swamp combat area
- one main quest: `Contract: Beast from the Swamp`
- NPCs: Elder Voytsekh, Marta Lozovaya
- enemy: one drowner
- investigation objects: traces near the road/swamp
- one choice: trust elder's version or question it
- reward: 50 XP, 20 coins, Antitoxin recipe
- save/load support

## Locations And Important Objects
### Vereskovy Brod
- Player spawn point: starts new game.
- Elder's house: main quest, story conversations, elder route.
- Marta's house: healing, herbs, alchemy introduction.
- Boris's forge: weapon and armor upgrades.
- Radek's shop: food, potions, resources, trophy sale.
- Contract board: side quests.
- Well: starts `Voice from the Well`.
- Player chest: item storage.
- Alchemy table: potions, oils, bombs.
- Campfire: rest, heal, autosave.
- Forest exit: available early.
- Swamp exit: unlocked after elder conversation.
- Ash Road exit: unlocked near finale.

### Old Forest
- Forest trail: main route.
- Hunter camp: `Missing Hunter`.
- Wolf den: wolves and wolf resources.
- Old altar: ritual evidence.
- Dead messenger: `Messenger Note`.
- Broken cart: loot container.
- Herb clearings: swallow herb.
- Drowner cave: optional combat area.
- Fast travel sign: unlock after first visit.

### Black Swamp
- Rotten bridge: swamp entrance.
- Poison bogs: periodic poison damage.
- Elsa's hut: major NPC and choice branch.
- Drowner nest: `Drowner Nest` quest.
- Ritual stones: main quest evidence.
- Dead tree: ghost encounter at night.
- Rare herbs: bogweed and ash salt.
- Sunken chest: exploration reward.
- Tower passage: opened by ritual key.

### Tower Ruins
- Tower gate: requires ritual key.
- Library: Orten diary.
- Laboratory: recipes for Light bomb and undead oil.
- Lever room: simple puzzle.
- Mirror hall: truth reveal.
- Mirror shards: quest items.
- Ghost girl: key story conversation.
- Orten arena: boss/dialogue encounter.

### Ash Road
- Burned road: final route.
- Survivor camp: NPC reactions to choices.
- Elder's ambush: appears if player opposes elder.
- Witcher campfire: final autosave.
- Final altar: ending selection.
- Three final points: Truth, Lie, Sacrifice.

## NPC Database
| NPC | Location | Role | Functions | Status |
|---|---|---|---|---|
| Reynard | player | main hero | movement, combat, signs, inventory, quests, choices, save/load | planned |
| Elder Voytsekh | village | quest giver and hidden culprit | gives main contract, pushes blame onto Elsa, supports Lie ending | planned |
| Marta Lozovaya | village | healer and alchemy mentor | heals, sells herbs, opens alchemy, explains curse | planned |
| Elsa Chernotravka | swamp | exile and truth holder | gives rare recipes, helps open tower, affects finale | planned |
| Ivar the Gray | forest / ash road | hunter rival | combat hints, ally or enemy based on decisions | planned |
| Boris | village forge | smith | improves weapons/armor, gives `Smith's Debt` | planned |
| Radek | village shop | trader | sells food/potions/resources, buys trophies | planned |
| Ghost Girl | well / tower | murdered victim | gives medallion, reveals crime | planned |
| Orten | tower | antagonist | explains Mirror, defends false history, final threat | planned |

## Enemy Database
| Enemy | Code Type | Location | Behavior | Loot |
|---|---|---|---|---|
| Wolf | `Beast` | Old Forest | fast melee attacker, low health | wolf hide, wolf fang |
| Drowner | `Monster` | Swamp / cave | slower, stronger melee, can poison | drowner slime |
| Skeleton Guard | `Undead` | Tower Ruins | durable, slow weapon attacks | undead bone |
| Wraith | `Specter` | well / swamp / tower | dash movement, weak to Light bomb | essence / undead bone |
| Bandit | `Human` | Ash Road | group weapon attacks | coins, food |
| Swamp Boss | `Monster` | Black Swamp | high health, heavy attacks | trophy, ritual key |

## Combat Design Memory
- Combat style: simplified PvE action combat.
- Light attack: quick, low damage.
- Heavy attack: slower, higher damage.
- Block: reduces damage from `Human` and `Beast`; weaker against `Monster`.
- Dodge: short backward/side dash with 1-2 second cooldown.
- Steel sword: bonus against `Human` and `Beast`.
- Silver sword: bonus against `Monster`, `Undead`, `Specter`.
- Igni sign: frontal magic damage with cooldown.
- Aard sign: push/interruption for weak enemies with cooldown.
- Oils: temporary bonus against one enemy type.
- Bombs: consumable area effect.
- Enemy death: XP plus loot chance.

## Item Database
| Item | Type | Function |
|---|---|---|
| Old Steel Sword | Weapon | starting weapon for humans and beasts |
| Witcher Silver Sword | Weapon | starting weapon for monsters |
| Improved Steel Sword | Weapon | higher damage against `Human` / `Beast` |
| Improved Silver Sword | Weapon | higher damage against `Monster` / `Undead` / `Specter` |
| Old Camp Blade | Quest / Weapon | unlocks smith upgrades |
| Leather Witcher Armor | Armor | base protection |
| Reinforced Armor | Armor | increased protection |
| Swamp Cloak | Armor | poison and bog resistance |
| Tracker Amulet | Accessory | improved resource find chance |
| Swallow | Potion | restores health |
| Thunder | Potion | temporary damage boost |
| Cat | Potion | improves dark vision |
| Antitoxin | Potion | removes poison |
| Food | Consumable | slow healing out of combat |
| Ash Bomb | Bomb | group damage |
| Light Bomb | Bomb | weakens specters |
| Undead Oil | Oil | bonus vs `Undead` / `Specter` |
| Bog Creature Oil | Oil | bonus vs drowners and swamp boss |
| Hanged Man Oil | Oil | bonus vs `Human` |
| Swallow Herb | Resource | healing recipe |
| Bogweed | Resource | antitoxin and oils |
| Drowner Slime | Resource | swamp recipes |
| Undead Bone | Resource | undead recipes |
| Wolf Hide | Resource | armor and sale |
| Wolf Fang | Resource | weapon and Thunder |
| Iron Ore | Resource | forge upgrades |
| Silver Shard | Resource | silver sword upgrade |
| Ash Salt | Resource | bombs |
| Mirror Shard | Quest Item | main mystery |
| Messenger Note | Quest Item | leads toward tower |
| Ritual Key | Quest Item | opens tower ruins |
| Orten Diary | Quest Item | explains Mirror |
| Girl's Medallion | Quest Item | unlocks/strengthens Truth ending |
| Elder's Seal | Quest Item | proves village guilt |

## Crafting Recipes
### Alchemy Table
- Swallow = Swallow Herb + Food
- Thunder = Wolf Fang + Ash Salt
- Cat = Bogweed + Mirror Shard
- Antitoxin = Bogweed + Drowner Slime
- Ash Bomb = Ash Salt + Iron Ore
- Light Bomb = Ash Salt + Undead Bone
- Undead Oil = Undead Bone + Bogweed
- Bog Creature Oil = Drowner Slime + Bogweed
- Hanged Man Oil = Wolf Fang + Iron Ore

### Forge
- Improved Steel Sword = Old Steel Sword + Iron Ore + Wolf Fang
- Improved Silver Sword = Witcher Silver Sword + Silver Shard + Undead Bone
- Reinforced Armor = Leather Witcher Armor + Wolf Hide + Iron Ore
- Swamp Cloak = Wolf Hide + Bogweed + Drowner Slime

## Quest Database
| Quest | Start | Giver | Objectives | Reward | Affects Ending |
|---|---|---|---|---|---|
| Contract: Beast from the Swamp | Village | Elder | talk, find traces, kill drowner, return | 50 XP, 20 coins, Antitoxin recipe | yes |
| The Right Version | after first contract | story | collect note, medallion, diary, elder seal | XP, tower access | yes |
| Mirror of Truth | tower / ash road | story | reach altar, choose final decision | ending | yes |
| Missing Hunter | board / forest | board | find Ivar, decide his fate | XP, coins, wolf fang | partial |
| Smith's Debt | forge | Boris | find old camp blade | weapon upgrades | no |
| Voice from the Well | well | ghost | find medallion, speak with spirit | medallion, XP | yes |
| The Exile | swamp | Elsa / elder | protect or betray Elsa | recipe or coins | yes |
| Drowner Nest | board | board | clear cave | slime, XP, rare resource | no |

## Decision Flags
| Flag | Meaning | Used By |
|---|---|---|
| `acceptedSwampContract` | player accepted first contract | first quest progression |
| `questionedElderVersion` | player doubted elder during vertical slice choice | later truth route |
| `killedFirstDrowner` | vertical slice enemy defeated | reward and quest completion |
| `receivedAntitoxinRecipe` | player got first recipe | crafting unlock |
| `ElsaProtected` | Elsa protected | tower help and finale |
| `ElsaBetrayed` | Elsa betrayed | elder route stronger |
| `MedallionFound` | girl's medallion found | Truth ending |
| `OrtenDiaryFound` | Orten diary found | Sacrifice ending / mirror understanding |
| `IvarSaved` | Ivar saved | Ash Road support |
| `IvarDead` | Ivar dead or abandoned | no support |
| `SmithQuestCompleted` | forge quest complete | improved weapons |
| `DrownerNestCleared` | swamp safer | fewer swamp enemies |
| `VillageTruthExposed` | truth route advanced | final state |
| `MayorSupported` | elder supported | Lie ending easier |

## Technical Architecture Memory
Unity scenes:
- `MainMenuScene`
- `VillageScene`
- `ForestScene`
- `SwampScene`
- `TowerRuinsScene`
- `AshRoadScene`

Main C# systems:
- `PlayerController`: movement, run, rotation, camera
- `CombatController`: attacks, block, dodge, signs, damage
- `EnemyAI`: patrol, chase, attack, death
- `InteractionController`: NPCs, items, traces, chests, doors
- `DialogueService`: dialogue nodes, choices, quest starts
- `QuestService`: stages, objectives, decision flags
- `InventoryService`: items, equipment, consumables
- `CraftingService`: recipes and crafting stations
- `SaveService`: 3 manual slots and autosave
- `UIController`: HUD, inventory, journal, menu
- `SettingsService`: audio, graphics, resolution

Data types:
- `ItemDefinition`
- `EnemyDefinition`
- `QuestDefinition`
- `DialogueDefinition`
- `RecipeDefinition`
- `LocationDefinition`

Enums:
- `ItemType`: Weapon, Armor, Potion, Bomb, Oil, Resource, QuestItem
- `EnemyType`: Human, Beast, Monster, Undead, Specter
- `QuestState`: NotStarted, Active, Completed, TurnedIn, Failed
- `CraftingStationType`: AlchemyTable, Forge
- `EndingType`: Truth, Lie, Sacrifice

## Save System Memory
Save slots:
- 3 manual slots
- 1 autosave

Save file must store:
- current scene
- player position
- health
- level
- XP
- skill points
- inventory
- equipment
- quest states
- decision flags
- unlocked locations
- important NPC states
- settings

Autosave triggers:
- campfires
- key quest completion
- before finale

## Asset Policy
Allowed asset sources:
- Unity Asset Store
- Mixamo animations
- Quaternius RPG props and environment
- Kenney fantasy town kit
- Sketchfab only after manual license check

Do not use:
- ripped models from The Witcher
- Geralt/Ciri direct models
- unclear license assets
- copyrighted original game files

Visual style:
- stylized low-poly dark fantasy
- dark green forests
- brown wood/earth
- swamp tones
- gray fog
- gold UI accents
- red danger/blood accents
- purple Mirror magic

## What Must Not Change Without A Good Reason
- Engine remains Unity.
- Language remains C#.
- First target remains vertical slice.
- Do not expand beyond 5 zones and 8 quests before MVP is playable.
- Do not work on several major systems at the same time.
- Do not replace the new witcher hero with Geralt or Ciri.
- Do not add online/PvP/server database to first version.
- Do not use ripped Witcher assets.
- Do not rewrite story facts without updating this memory file.

## Current Done Log
- Created `PROJECT_MEMORY.md`.
- Created `ROADMAP.md`.
- Added Unity-focused `.gitignore`.
- Initialized local Git repository.
- Created first commit: `3f31b0a Initialize project memory and roadmap`.
- Confirmed Unity Hub is installed.
- Confirmed Unity Editor `6000.4.9f1` is installed.
- Confirmed Windows Standalone support is installed.
- Created Unity project skeleton in `G:\The witcher 2`.
- Created initial Unity folders under `Assets`.
- Created `Assets/Scenes/MainMenuScene.unity`.
- Created `Assets/Scenes/VillageScene.unity`.
- Added both initial scenes to Unity Build Settings.
- Added `Assets/Editor/ProjectSkeletonBuilder.cs` skeleton utility.
- Added `Assets/Docs/STAGE_1_SKELETON.md`.

## Current Work
- Stage 1 is complete.
- Next concrete step: Stage 2, build `MainMenuScene` with a main menu and settings stub.

## Update Log Template
Use this format after every completed part:

```text
Date:
Completed:
Files/Scenes changed:
New flags:
New items:
New quests/stages:
Notes:
Next step:
```
