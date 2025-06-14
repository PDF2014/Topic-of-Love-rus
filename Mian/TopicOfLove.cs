using System.IO;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.AI;
using Topic_of_Love.Mian.CustomAssets.Traits;
using NeoModLoader.api;
using HarmonyLib;
using NeoModLoader.General;
using Topic_of_Love.Mian.CustomAssets.Custom;

namespace Topic_of_Love.Mian
{
    public class TopicOfLove : BasicMod<TopicOfLove>
    {
        public static BasicMod<TopicOfLove> Mod;
        public void Reload()
        {
            var localeDir = GetLocaleFilesDirectory(GetDeclaration());
            foreach (var file in Directory.GetFiles(localeDir))
            {
                if (file.EndsWith(".json"))
                {
                    LM.LoadLocale(Path.GetFileNameWithoutExtension(file), file);
                }
                else if (file.EndsWith(".csv"))
                {
                    LM.LoadLocales(file);
                }
            }

            LM.ApplyLocale();
        }
        protected override void OnModLoad()
        {
            Mod = this;
            // Initialize your mod.
            // Methods are called in the order: OnLoad -> Awake -> OnEnable -> Start -> Update
            TolUtil.LogInfo("Making people more loveable!");

            Orientations.Init();
            Preferences.Init();
            new ActorTraits().Init();
            new CultureTraits().Init();
            new SubspeciesTraits().Init();
            StatusEffects.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            ActorBehaviorTasks.Init();
            Decisions.Init();
            Opinions.Init();
            BaseStatAssets.Init();
            WorldLawAssets.Init();
            GodPowers.Init();
            TabsAndButtons.Init();
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.topicofloving");
            harmony.PatchAll();
        }
    }
}