using System;
using System.Collections.Generic;
using UnityEngine;

namespace WitcherRightVersion.Localization
{
    public static class GameLocalization
    {
        public const string LanguageKey = "settings.language";
        public const int EnglishLanguage = 0;
        public const int RussianLanguage = 1;

        public static event Action LanguageChanged;

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            { "Elder Voytsekh", "Староста Войцех" },
            { "Marta Lozovaya", "Марта Лозовая" },
            { "Boris the Smith", "Кузнец Борис" },
            { "Radek the Trader", "Торговец Радек" },
            { "Elsa Cherntravka", "Эльза Чернотравка" },
            { "Ivar Sedoy", "Ивар Седой" },
            { "Ghost of the Girl", "Призрак девушки" },
            { "Orten", "Ортен" },
            { "Talk", "Говорить" },
            { "Listen", "Слушать" },
            { "Inspect", "Осмотреть" },
            { "Take", "Взять" },
            { "Read", "Прочитать" },
            { "Touch", "Коснуться" },
            { "Craft", "Создать" },
            { "Confront", "Противостоять" },
            { "Buy supplies", "Купить припасы" },
            { "Take reward", "Забрать награду" },
            { "Accept", "Принять" },
            { "Choose truth", "Выбрать правду" },
            { "Choose lie", "Выбрать ложь" },
            { "Choose sacrifice", "Выбрать жертву" },

            { "Witcher. The whole road is open before you now: village, forest, swamp, ash tract. But the contract is still simple. Find what drags people into the reeds.", "Ведьмак. Теперь перед тобой открыты все дороги: деревня, лес, болото и пепельный тракт. Но контракт прост: найди то, что утаскивает людей в камыши." },
            { "Because fear needs a shape. Give it teeth, call it a beast, and people sleep. Start south if you want coin.", "Страху нужен облик. Дай ему зубы, назови зверем, и люди снова уснут. Если хочешь монет, иди на юг." },
            { "Good. Speak with Marta, then follow the road into the Black Swamp. Bring proof before asking for reward.", "Хорошо. Поговори с Мартой, затем иди по дороге к Чёрному Болоту. Принеси доказательство, прежде чем требовать награду." },
            { "You came back from the reeds. Tell me the thing is dead.", "Ты вернулся из камышей. Скажи, что тварь мертва." },
            { "Then the village can call this finished. The swamp gave us a beast, and you cut it down.", "Значит, деревня может считать дело законченным. Болото породило зверя, а ты его зарубил." },
            { "Careful. A clean ending is a mercy in a place like this.", "Осторожнее. В таком месте простой конец сам по себе милость." },
            { "Take your coin, your experience, and Marta's antitoxin recipe. Spend them before questions spend you.", "Забирай монеты, опыт и рецепт противоядия Марты. Потрать их раньше, чем вопросы истратят тебя." },
            { "The road is quiet. The village prefers quiet.", "Дорога тиха. Деревня любит тишину." },
            { "I accept the swamp contract.", "Я принимаю контракт на болотную тварь." },
            { "Why is the village so eager to blame the swamp?", "Почему деревня так охотно обвиняет болото?" },
            { "Later.", "Позже." },
            { "I accept. But I will look deeper.", "Я согласен. Но разберусь глубже." },
            { "Not yet.", "Ещё нет." },
            { "I am going.", "Я отправляюсь." },
            { "The drowner is dead. Here is your proof.", "Утопец мёртв. Вот доказательство." },
            { "The swamp is guilty. Pay me.", "Виновато болото. Плати." },
            { "This is not just a monster. Something is wrong here.", "Дело не только в чудовище. Здесь что-то не так." },
            { "I will take the reward. I am not done looking.", "Я заберу награду, но расследование не закончено." },
            { "Contract complete.", "Контракт завершён." },
            { "For now.", "Пока что." },

            { "So Voytsekh sent you south. Good. Walk with open eyes: black slime, torn reeds, tracks too heavy for a man.", "Значит, Войцех отправил тебя на юг. Хорошо. Смотри внимательно: чёрная слизь, сломанный камыш, следы слишком тяжёлые для человека." },
            { "Look for claw cuts, poisoned slime, and cloth caught on the reeds. When all three agree, draw silver.", "Ищи следы когтей, ядовитую слизь и ткань на камышах. Когда все три улики сойдутся, обнажай серебро." },
            { "People blame Elsa because a single witch is easier than a village full of liars. Survive the monster first; truth comes later.", "Люди винят Эльзу, потому что одна ведьма проще целой деревни лжецов. Сначала переживи встречу с чудовищем, правда будет потом." },
            { "I took the contract. What should I look for?", "Я взял контракт. Что мне искать?" },
            { "Why does the swamp remember the dead?", "Почему болото помнит мёртвых?" },
            { "Then I start with the tracks.", "Тогда начну со следов." },
            { "Tell me about the curse.", "Расскажи о проклятии." },
            { "I will inspect the swamp.", "Я осмотрю болото." },
            { "Enough for now.", "Пока достаточно." },

            { "If the swamp chews your armor, do not blame the armor. Blame the witcher who walked in underprepared.", "Если болото прогрызёт твою броню, не вини броню. Вини ведьмака, который пришёл неподготовленным." },
            { "There is an old blade in the forest camp. Bring it back and I will reforge steel worth carrying.", "В лесном лагере остался старый клинок. Принеси его, и я перекую сталь, которую не стыдно носить." },
            { "Good metal remembers hands better than people do. Take the improved steel sword.", "Хороший металл помнит руки лучше людей. Забирай улучшенный стальной меч." },
            { "Need work done?", "Есть работа?" },
            { "I brought the old camp blade.", "Я принёс клинок из старого лагеря." },
            { "I will look for it.", "Я поищу его." },
            { "Fair trade.", "Честная сделка." },

            { "Do not draw steel. If Voytsekh sent you, he sent you to kill the only witness who still remembers the first version.", "Не обнажай сталь. Если тебя послал Войцех, он послал тебя убить единственного свидетеля, который ещё помнит первую версию." },
            { "Orten made them a kinder memory. The girl became a witch. The killers became saviors. The swamp became a grave that could still speak.", "Ортен подарил им более удобную память. Девушка стала ведьмой, убийцы стали спасителями, а болото превратилось в могилу, которая всё ещё умеет говорить." },
            { "Then take the reed charm. It will not open the tower alone, but the ruins will know you came for the buried version.", "Тогда возьми камышовый оберег. Сам он не откроет башню, но руины поймут, что ты пришёл за похороненной версией." },
            { "Of course. Villages are very good at hiring clean hands for dirty endings.", "Конечно. Деревни отлично умеют нанимать чистые руки для грязных концовок." },
            { "Tell me what the mirror did.", "Расскажи, что сделало зеркало." },
            { "I will protect you from the elder.", "Я защищу тебя от старосты." },
            { "The village wants you taken in.", "Деревня требует выдать тебя." },
            { "Help me reach the tower.", "Помоги мне добраться до башни." },
            { "I need proof, not faith.", "Мне нужны доказательства, а не вера." },
            { "I will use it.", "Я воспользуюсь им." },
            { "This ends cleanly.", "Я закончу это быстро." },

            { "You call it a lie because you arrived after the blood dried. I call it surgery. A village cannot live while staring at its own wound.", "Ты зовёшь это ложью, потому что пришёл, когда кровь уже высохла. Я называю это операцией. Деревня не может жить, вечно глядя на собственную рану." },
            { "Then bring a better ending, witcher. Truth, comfort, or fire. The mirror only obeys a hand that has chosen.", "Тогда принеси лучший конец, ведьмак: правду, утешение или огонь. Зеркало подчиняется лишь руке, которая сделала выбор." },
            { "A practical monster hunter. Voytsekh would have liked you before fear taught him manners.", "Практичный охотник на чудовищ. Ты бы понравился Войцеху до того, как страх научил его вежливости." },
            { "Break it, then. But when the curse leaves, it will take what it still owns.", "Тогда разбей его. Но когда проклятие уйдёт, оно заберёт с собой всё, что ещё считает своим." },
            { "Your surgery made a curse.", "Твоя операция породила проклятие." },
            { "Maybe the lie saved them.", "Возможно, ложь спасла их." },
            { "I will break the mirror.", "Я разобью зеркало." },
            { "Leave.", "Уйти." },
            { "I have chosen enough.", "Я уже достаточно выбрал." },
            { "This version survives.", "Эта версия выживет." },
            { "Better a cruel end than an endless rot.", "Лучше жестокий конец, чем бесконечное гниение." },

            { "If you came for the hunter, you found what is left of him: a man who learned the forest is quieter than the village.", "Если ты пришёл за охотником, то нашёл то, что от него осталось: человека, понявшего, что лес тише деревни." },
            { "Then I owe you one shot when the elder's people raise their bows.", "Тогда я должен тебе один точный выстрел, когда люди старосты поднимут луки." },
            { "I can still use your eyes on the Ash Road.", "Твои глаза ещё пригодятся мне на Пепельном тракте." },
            { "Stay hidden then.", "Тогда оставайся в укрытии." },
            { "I will remember that.", "Я это запомню." },

            { "They gave me a witch's name after they killed me. The mirror learned it. The village learned it. Only the well kept the first sound.", "После убийства они дали мне имя ведьмы. Зеркало запомнило его. Деревня запомнила его. Лишь колодец сохранил первое имя." },
            { "Not a monster. Not Elsa. Men with clean doorways and dirty hands. The elder sealed the order; Orten made mercy from forgetting.", "Не чудовище. Не Эльза. Люди с чистыми порогами и грязными руками. Староста скрепил приказ печатью, а Ортен сделал из забвения милосердие." },
            { "It remembers my pulse. Carry it to the final road, and the mirror will have to answer to the first version.", "Он помнит мой пульс. Отнеси его к последней дороге, и зеркалу придётся ответить перед первой версией." },
            { "I found your medallion.", "Я нашёл твой медальон." },
            { "Who killed you?", "Кто тебя убил?" },
            { "Fade back.", "Вернись в тень." },
            { "Then I need the seal and the diary.", "Тогда мне нужны печать и дневник." },
            { "I will carry the truth.", "Я понесу правду." },

            { "Truth Ending", "Концовка: Правда" },
            { "Corrected Story Ending", "Концовка: Исправленная история" },
            { "Sacrifice Ending", "Концовка: Жертва" },
            { "Ending Reached", "Концовка достигнута" },
            { "Reynard exposes the first lie buried under Velemar. The curse begins to break, but Heather Ford must now survive the truth it tried to erase.", "Рейнард раскрывает первую ложь, похороненную под Велемаром. Проклятие начинает рушиться, но Вересковому Броду теперь придётся пережить правду, которую он пытался стереть." },
            { "Reynard lets the elder seal the useful version of history. The village survives, but the mirror keeps one more polished lie.", "Рейнард позволяет старосте закрепить удобную версию истории. Деревня выживает, но зеркало сохраняет ещё одну отполированную ложь." },
            { "Reynard destroys the mirror's heart. The curse breaks hard and fast, taking the weakest infected with it before silence returns.", "Рейнард уничтожает сердце зеркала. Проклятие рушится быстро и жестоко, забирая слабейших заражённых, прежде чем возвращается тишина." },
            { "Reynard's choice changes Velemar. The final version of the story is now written.", "Выбор Рейнарда меняет Велемар. Последняя версия истории написана." },

            { "Contract: Beast from the Swamp", "Контракт: Зверь из болота" },
            { "Mirror of Truth", "Зеркало Правды" },
            { "Voice from the Well", "Голос из колодца" },
            { "Exile", "Изгнанница" },
            { "Right Version", "Правильная версия" },
            { "Drowner Nest", "Логово утопцев" },
            { "Missing Hunter", "Пропавший охотник" },
            { "Smith's Debt", "Долг кузнеца" },

            { "Old Steel Sword", "Старый стальной меч" },
            { "Silver Witcher Sword", "Серебряный меч ведьмака" },
            { "Improved Steel Sword", "Улучшенный стальной меч" },
            { "Leather Witcher Armor", "Кожаная ведьмачья броня" },
            { "Reinforced Armor", "Укреплённая броня" },
            { "Swamp Cloak", "Болотный плащ" },
            { "Swallow", "Ласточка" },
            { "Antitoxin", "Противоядие" },
            { "Food", "Еда" },
            { "Field Ration", "Паёк" },
            { "Ash Salt", "Пепельная соль" },
            { "Swallow Grass", "Ласточкина трава" },
            { "Bogweed", "Болотник" },
            { "Iron Ore", "Железная руда" },
            { "Wolf Pelt", "Шкура волка" },
            { "Wolf Fang", "Клык волка" },
            { "Drowner Slime", "Слизь утопца" },
            { "None", "Нет" },
            { "Inventory", "Инвентарь" },
            { "Overview", "Обзор" },
            { "Gear", "Снаряжение" },
            { "Items", "Предметы" },
            { "Crafting", "Крафт" },
            { "Level", "Уровень" },
            { "Skill points", "Очки навыков" },
            { "XP", "Опыт" },
            { "Coins", "Монеты" },
            { "Equipped", "Экипировано" },
            { "Equipped weapon", "Экипированное оружие" },
            { "Active quest", "Активный квест" },
            { "Weapons", "Оружие" },
            { "Armor", "Броня" },
            { "Consumables", "Расходники" },
            { "Resources", "Ресурсы" },
            { "Quest items", "Квестовые предметы" },
            { "Known recipes", "Известные рецепты" },
            { "No recipes available.", "Рецептов пока нет." },
            { "[READY] ", "[ГОТОВО] " },
            { "[LOCKED] ", "[ЗАКРЫТО] " },
            { "Needs", "Нужно" },

            { "Thunder", "Гром" },
            { "Cat", "Кошка" },
            { "Ash Bomb", "Бомба «Пепел»" },
            { "Light Bomb", "Бомба «Свет»" },
            { "Undead Oil", "Масло против нежити" },
            { "Bog Creature Oil", "Масло против болотных тварей" },
            { "Hanged Man Oil", "Масло против людей" },
            { "Old Camp Blade", "Клинок старого лагеря" },
            { "Mirror Shard", "Осколок зеркала" },
            { "Messenger Note", "Записка гонца" },
            { "Ritual Key", "Ритуальный ключ" },
            { "Orten Diary", "Дневник Ортена" },
            { "Girl's Medallion", "Медальон девушки" },
            { "Reed Charm", "Камышовый оберег" },
            { "Elder's Seal", "Печать старосты" },

            { "Merchant", "Торговец" },
            { "Buy", "Купить" },
            { "Supplies", "Припасы" },
            { "Crafting station", "Место крафта" },
            { "Evidence", "Улика" },
            { "Quest object", "Квестовый объект" },
            { "Object", "Объект" },
            { "Interact", "Взаимодействовать" },
            { "Travel", "Переход" },
            { "Path", "Тропа" },
            { "Ash Road", "Пепельный тракт" },
            { "Final truth altar", "Финальный алтарь правды" },

            { "Claw marks", "Следы когтей" },
            { "Black slime trail", "След чёрной слизи" },
            { "Torn cloth", "Обрывок ткани" },
            { "Drowner nest notice", "Объявление о логове утопцев" },
            { "Drowner nest reward cache", "Тайник награды за логово" },
            { "Marta's herb basket", "Корзина трав Марты" },
            { "Forge supplies", "Кузнечные припасы" },
            { "Alchemy table: Swallow", "Алхимический стол: Ласточка" },
            { "Alchemy table: Antitoxin", "Алхимический стол: Противоядие" },
            { "Boris's forge: Reinforced Armor", "Кузница Бориса: укреплённая броня" },
            { "Abandoned hunter camp", "Заброшенный лагерь охотника" },
            { "Blood trail", "Кровавый след" },
            { "Broken knife", "Сломанный нож" },
            { "Hunter's hidden pouch", "Тайный кошель охотника" },
            { "Old village well", "Старый деревенский колодец" },
            { "Girl's medallion", "Медальон девушки" },
            { "Orten's diary", "Дневник Ортена" },
            { "Mirror shard cache", "Тайник осколков зеркала" },
            { "Reed charm mark", "Знак камышового оберега" },
            { "Ghost memory", "Воспоминание призрака" },
            { "Elder's sealed order", "Запечатанный приказ старосты" },
            { "World final truth altar", "Финальный алтарь правды" },
            { "World final corrected-story altar", "Финальный алтарь исправленной истории" },
            { "World final sacrifice altar", "Финальный алтарь жертвы" },
            { "Elsa's ward", "Защитный знак Эльзы" },
            { "Voytsekh's control post", "Пост людей Войцеха" },
            { "Medallion proof", "Доказательство медальоном" },
            { "Orten's diary proof", "Доказательство дневником Ортена" },

            { "Deep claw cuts in the mud point toward the south pool.", "Глубокие следы когтей в грязи указывают к южной заводи." },
            { "Marta should explain what to look for first.", "Сначала стоит спросить Марту, что искать." },
            { "The slime bubbles like something alive. The trail bends toward the drowned reeds.", "Слизь пузырится, будто живая. След уходит к затопленным камышам." },
            { "The slime looks wrong, but Reynard needs Marta's warning first.", "Слизь выглядит странно, но Рейнарду сначала нужна подсказка Марты." },
            { "A strip of wet cloth hangs from the reeds. Someone was dragged deeper.", "На камышах висит мокрый лоскут. Кого-то утащили глубже." },
            { "This cloth is just a rag until Reynard knows the swamp signs.", "Пока Рейнард не знает болотные признаки, это просто тряпка." },
            { "Contract accepted: clear the drowner nest in the Black Swamp.", "Контракт принят: зачистить логово утопцев в Чёрном Болоте." },
            { "The notice board is already marked.", "Объявление уже отмечено." },
            { "The village cache pays out for the cleared nest.", "Деревенский тайник выдаёт награду за очищенное логово." },
            { "The cache is sealed until the nest is cleared.", "Тайник закрыт, пока логово не зачищено." },
            { "Resources gained: Swallow Grass, Field Ration, Bogweed.", "Получены ресурсы: ласточкина трава, паёк, болотник." },
            { "Resources gained: Iron Ore, Wolf Pelt, Drowner Slime.", "Получены ресурсы: железная руда, шкура волка, слизь утопца." },
            { "The camp is torn open. Someone fled west into the trees.", "Лагерь разорван. Кто-то бежал на запад, в деревья." },
            { "The camp waits in silence.", "Лагерь молчит." },
            { "Fresh blood marks the moss.", "Свежая кровь пятнает мох." },
            { "The trail makes more sense after the camp is inspected.", "След станет понятнее после осмотра лагеря." },
            { "The blade snapped against bone or old iron.", "Клинок сломался о кость или старое железо." },
            { "A broken knife, but not yet a story.", "Сломанный нож, но пока не история." },
            { "You find coin and a note: Ivar survived the first night.", "Ты находишь монеты и записку: Ивар пережил первую ночь." },
            { "The pouch stays hidden until the trail is understood.", "Кошель останется скрыт, пока следы не будут поняты." },
            { "A voice rises from the stones: find what she wore when the village named her a witch.", "Из камней поднимается голос: найди то, что было на ней, когда деревня назвала её ведьмой." },
            { "The well is silent now.", "Колодец пока молчит." },
            { "The medallion is old, scratched, and still warm near the mirror rot. Truth ending evidence recorded.", "Медальон старый, исцарапанный и всё ещё тёплый рядом с зеркальной гнилью. Улика для концовки Правды записана." },
            { "Orten wrote how the mirror rewrites memory. Sacrifice ending evidence recorded.", "Ортен описал, как зеркало переписывает память. Улика для концовки Жертвы записана." },
            { "The shard cache cracks. The mirror loses one anchor in the living world.", "Тайник осколков трескается. Зеркало теряет одну опору в живом мире." },
            { "The tower stones answer the reed charm. The ruin route is marked for Reynard.", "Камни башни отвечают камышовому оберегу. Путь к руинам отмечен для Рейнарда." },
            { "A girl's voice names the well, the seal, and the men who called murder mercy.", "Голос девушки называет колодец, печать и людей, назвавших убийство милосердием." },
            { "The memory will not answer without the girl's medallion.", "Воспоминание не ответит без медальона девушки." },
            { "The elder's seal names the people who paid Orten to bury the first story.", "Печать старосты указывает людей, заплативших Ортену за похороны первой истории." },
            { "Elsa has marked a safer path through the ash.", "Эльза отметила более безопасный путь через пепел." },
            { "The elder's people stand bolder after Elsa is handed over.", "Люди старосты держатся увереннее после выдачи Эльзы." },
            { "The girl's medallion answers the Truth altar.", "Медальон девушки отзывается у алтаря Правды." },
            { "Orten's notes explain how to break the Mirror without asking it for another lie.", "Записи Ортена объясняют, как разбить Зеркало, не прося у него новой лжи." },
            { "Bought Radek's supply pack: Food, Ash Salt, Swallow Grass, Iron Ore.", "Куплен набор Радека: еда, пепельная соль, ласточкина трава, железная руда." },
            { "Radek wants 10 coins for the supply pack.", "Радек просит 10 монет за набор припасов." },
            { "Radek has no more packed supplies today.", "Сегодня у Радека больше нет готовых наборов." }
        };

        public static int CurrentLanguage => Mathf.Clamp(PlayerPrefs.GetInt(LanguageKey, EnglishLanguage), EnglishLanguage, RussianLanguage);
        public static bool IsRussian => CurrentLanguage == RussianLanguage;

        public static void SetLanguage(int languageIndex)
        {
            var normalizedLanguage = Mathf.Clamp(languageIndex, EnglishLanguage, RussianLanguage);
            var changed = CurrentLanguage != normalizedLanguage;

            PlayerPrefs.SetInt(LanguageKey, normalizedLanguage);
            PlayerPrefs.Save();

            if (changed)
            {
                LanguageChanged?.Invoke();
            }
        }

        public static string Text(string english)
        {
            if (!IsRussian || string.IsNullOrEmpty(english))
            {
                return english;
            }

            return Russian.TryGetValue(english, out var translated) ? translated : english;
        }

        public static string Select(string english, string russian)
        {
            return IsRussian ? russian : english;
        }
    }
}
