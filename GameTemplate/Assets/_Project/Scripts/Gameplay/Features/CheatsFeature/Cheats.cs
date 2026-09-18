using _Project.Scripts.Gameplay.Extensions;
using _Project.Scripts.Gameplay.Features.CommonFeature.Services;
using _Project.Scripts.Gameplay.Features.CrowdFeature.Components;
using _Project.Scripts.Gameplay.Features.CurrencyFeature.Services;
using _Project.Scripts.Gameplay.Features.EntityViewFeature;
using _Project.Scripts.Gameplay.Features.LevelFeature.Services;
using _Project.Scripts.Gameplay.Features.LoggerFeature;
#if USE_LOCALIZATION
using _Project.Scripts.Gameplay.Features.LocalizationFeature;
using UnityEngine.Localization;
#endif
#if GAMEPUSH_ENABLED
using GamePush;
#endif
using QFSW.QC;
using Scellecs.Morpeh;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.Scripts.Gameplay.Features.CheatsFeature
{
    public static class Cheats
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeOnLoad() =>
            DebugManager.instance.enableRuntimeUI = false;

        [Command("game.clear-world", "Clears the world entities")]
        public static void ClearWorld()
        {
            World.Default.ClearWorld();
            L.Log("World cleared");
        }

        [Command("game.get-entity-id", "Gets the ID of the entity attached to a GameObject by name")]
        public static int GetEntityIdByName(string gameObjectName)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);
            if (gameObject == null)
            {
                L.LogError($"GameObject with name '{gameObjectName}' not found.");
                return -1;
            }

            EntityView entityView = gameObject.GetComponent<EntityView>();
            if (entityView == null)
            {
                L.LogError($"GameObject '{gameObjectName}' does not have an EntityView component.");
                return -1;
            }

            Entity entity = entityView.Entity;
            if (entity.IsNullOrDisposed())
            {
                L.LogError("Entity is null or disposed.");
                return -1;
            }

            L.Log($"Entity ID for '{gameObjectName}': {entity.Id}");
            return entity.Id;
        }

        [MenuItem("Cheats/Game State/Trigger Victory")]
        [Command("game.win", "Forces level victory")]
        public static void Win()
        {
            GameStateMachine gameStateMachine = AllServices.Instance.Get<GameStateMachine>();
            if (gameStateMachine == null)
            {
                L.LogError("GameStateMachine not found.");
                return;
            }

            LevelService levelService = AllServices.Instance.Get<LevelService>();
            if (levelService == null)
            {
                L.LogError("LevelService not found.");
                return;
            }

            levelService.SetLevelCompleted(levelService.CurrentLevelIndex);

            gameStateMachine.Ctx.GameEndReason = GameEndingState.Reason.FINISH_REACHED;
            gameStateMachine.Fire(GameplayEvent.ENTER_GAME_ENDING);

            L.Log("Triggered victory state transition");
        }
        
        [Command("game.add-money", "Adds a specified amount of money")]
        public static void AddMoney(int amount = 1000)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            currencyService.AddMoney(amount);
            L.Log($"Added {amount} money. New total: {currencyService.Money}");
        }

        [Command("game.add-silver", "Adds a specified amount of silver")]
        public static void AddSilver(int amount = 1000)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            currencyService.AddSilver(amount);
            L.Log($"Added {amount} silver. New total: {currencyService.Silver}");
        }

        [Command("game.add-crystals", "Adds a specified amount of crystals")]
        public static void AddCrystals(int amount = 100)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            currencyService.AddGems(amount);
            L.Log($"Added {amount} crystals. New total: {currencyService.Gems}");
        }

        [Command("game.set-money", "Sets money to a specified amount")]
        public static void SetMoney(int amount)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            int currentMoney = currencyService.Money;
            int difference = amount - currentMoney;

            currencyService.AddMoney(difference);
            L.Log($"Set money to {amount}");
        }

        [Command("game.set-silver", "Sets silver to a specified amount")]
        public static void SetSilver(int amount)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            currencyService.SetSilver(amount);
            L.Log($"Set silver to {amount}");
        }

        [Command("game.set-crystals", "Sets crystals to a specified amount")]
        public static void SetCrystals(int amount)
        {
            CurrencyService currencyService = AllServices.Instance.Get<CurrencyService>();
            if (currencyService == null)
            {
                L.LogError("CurrencyService not found.");
                return;
            }

            int currentCrystals = currencyService.Gems;
            int difference = amount - currentCrystals;

            currencyService.AddGems(difference);
            L.Log($"Set crystals to {amount}");
        }

#if UNITY_EDITOR
        [MenuItem("Cheats/Crowd/Add 1 Member")]
        [Command("crowd.add-member", "Adds 1 crowd member")]
        public static void AddCrowdMember() => AddCrowdMembers(1);

        [MenuItem("Cheats/Crowd/Add 5 Members")]
        [Command("crowd.add-members-5", "Adds 5 crowd members")]
        public static void AddCrowdMembers5() => AddCrowdMembers(5);

        [MenuItem("Cheats/Crowd/Add 10 Members")]
        [Command("crowd.add-members-10", "Adds 10 crowd members")]
        public static void AddCrowdMembers10() => AddCrowdMembers(10);

        [MenuItem("Cheats/Crowd/Add 20 Members")]
        [Command("crowd.add-members-20", "Adds 20 crowd members")]
        public static void AddCrowdMembers20() => AddCrowdMembers(20);

        [Command("crowd.add-members", "Adds a specified number of crowd members")]
        private static void AddCrowdMembers(int count = 5)
        {
            World world = World.Default;
            Filter crowds = world.Filter.With<CrowdTag>().Build();

            Entity crowd = crowds.FirstOrDefault();
            if (crowd.IsNullOrDisposed())
            {
                L.LogError("Crowd not found.");
                return;
            }

            world
                .GetRequest<AddCrowdMembersRequest>()
                .Publish(new AddCrowdMembersRequest
                {
                    Count = count,
                    AnimationType = CrowdMemberAnimationType.JUMP
                }, allowNextFrame: true);

            L.Log($"Requested to add {count} crowd members");
        }

        [MenuItem("Cheats/Crowd/Remove 5 Members")]
        [Command("crowd.remove-members-5", "Removes 5 crowd members")]
        public static void RemoveCrowdMembers5() => RemoveCrowdMembers(5);

        [MenuItem("Cheats/Crowd/Remove 10 Members")]
        [Command("crowd.remove-members-10", "Removes 10 crowd members")]
        public static void RemoveCrowdMembers10() => RemoveCrowdMembers(10);

        [Command("crowd.remove-members", "Removes a specified number of crowd members")]
        private static void RemoveCrowdMembers(int count = 5)
        {
            World world = World.Default;
            Filter crowds = world.Filter.With<CrowdTag>().Build();

            Entity crowd = crowds.First();
            if (crowd.IsNullOrDisposed())
            {
                L.LogError("Crowd not found.");
                return;
            }

            world
                .GetRequest<RemoveCrowdMembersRequest>()
                .Publish(new RemoveCrowdMembersRequest
                {
                    Count = count,
                    SpecificMembers = null
                }, allowNextFrame: true);

            L.Log($"Requested to remove {count} crowd members");
        }

        public static void RemoveSpecificCrowdMembers(Entity[] members)
        {
            World world = World.Default;
            
            world
                .GetRequest<RemoveCrowdMembersRequest>()
                .Publish(new RemoveCrowdMembersRequest
                {
                    SpecificMembers = members,
                    Count = 0
                }, allowNextFrame: true);
        }

        [MenuItem("Cheats/Crowd/Double Population")]
        [Command("crowd.double-population", "Doubles the crowd population")]
        public static void DoubleCrowdPopulation() => MultiplyCrowdPopulation(2.0f);

        [MenuItem("Cheats/Crowd/Triple Population")]
        [Command("crowd.triple-population", "Triples the crowd population")]
        public static void TripleCrowdPopulation() => MultiplyCrowdPopulation(3.0f);

        [MenuItem("Cheats/Crowd/Half Population")]
        [Command("crowd.half-population", "Halves the crowd population")]
        public static void HalfCrowdPopulation() => MultiplyCrowdPopulation(0.5f);

        [Command("crowd.multiply-population", "Multiplies the crowd population by a specified ratio")]
        private static void MultiplyCrowdPopulation(float ratio = 2.0f)
        {
            World world = World.Default;
            Filter crowds = world.Filter.With<CrowdTag>().Build();

            Entity crowd = crowds.FirstOrDefault();
            if (crowd.IsNullOrDisposed())
            {
                L.LogError("Crowd not found.");
                return;
            }

            world
                .GetRequest<MultiplyCrowdRequest>()
                .Publish(new MultiplyCrowdRequest
                {
                    Ratio = ratio
                }, allowNextFrame: true);

            L.Log($"Requested to multiply crowd population by {ratio}");
        }

#if USE_LOCALIZATION
        [Command("game.lang.change", "Changes the game language using locale code (e.g. 'en', 'fr', 'de', 'es', 'ru')")]
        public static void ChangeLanguage(string localeCode)
        {
            LocalizationService localizationService = AllServices.Instance.Get<LocalizationService>();
            if (localizationService == null)
            {
                L.LogError("LocalizationService not found.");
                return;
            }

            if (!localizationService.IsInitialized)
            {
                L.LogError("LocalizationService is not initialized yet.");
                return;
            }

            List<Locale> availableLocales = localizationService.GetAvailableLocales();
            bool localeFound = false;

            foreach (var locale in availableLocales)
            {
                if (locale.Identifier.Code.Equals(localeCode, StringComparison.OrdinalIgnoreCase))
                {
                    localeFound = true;
                    break;
                }
            }

            if (!localeFound)
            {
                L.LogError($"Locale '{localeCode}' not found. Use 'lang.list' to see available languages.");
                return;
            }

            Locale originalLocale = localizationService.GetCurrentLocale();

            localizationService.ChangeLanguage(localeCode);

            Locale newLocale = localizationService.GetCurrentLocale();
            L.Log($"Language changed from '{originalLocale.LocaleName}' to '{newLocale.LocaleName}'");
        }
#endif

        [MenuItem("Cheats/Game State/Trigger Defeat")]
        [Command("game.trigger-defeat", "Triggers the defeat state transition")]
        public static void TriggerDefeat()
        {
            GameStateMachine gameStateMachine = AllServices.Instance.Get<GameStateMachine>();
            if (gameStateMachine == null)
            {
                L.LogError("GameStateMachine not found.");
                return;
            }

            gameStateMachine.Fire(GameplayEvent.DEFEAT);
            L.Log("Triggered defeat state transition");
        }
#endif

#if GAMEPUSH_ENABLED
        [Command("sound.mute", "Mutes sound (all/music/sfx)")]
        public static void MuteSound(string soundType = "all")
        {
            SoundType type = ParseSoundType(soundType);
            GP_Sounds.Mute(type);
            L.Log($"Muted {type} sound");
        }

        [Command("sound.unmute", "Unmutes sound (all/music/sfx)")]
        public static void UnmuteSound(string soundType = "all")
        {
            SoundType type = ParseSoundType(soundType);
            GP_Sounds.Unmute(type);
            L.Log($"Unmuted {type} sound");
        }

        private static SoundType ParseSoundType(string soundType)
        {
            return soundType.ToLower() switch
            {
                "all" => SoundType.All,
                "music" => SoundType.Music,
                "sfx" => SoundType.SFX,
                _ => SoundType.All
            };
        }
#endif
    }
}