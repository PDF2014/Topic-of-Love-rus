using System.IO;
using Topic_of_Love.Mian.CustomAssets;
using Topic_of_Love.Mian.CustomAssets.AI;
using Topic_of_Love.Mian.CustomAssets.Traits;
using NeoModLoader.api;
using HarmonyLib;
using NeoModLoader.General;

/*


- custom find lover decision (combination of romantic tasks that infleunce romantic opinion, lovers may randomly fall in love even when fighting)
- cupid who can force lovers together
- other specices no reproduce :(((
- CanStopBeingLover may not be working properly?
- error with initDecisionsCHildren at line 2150

- refactor how preferences work. Should be based on specifically which sex/gender a person is attracted to
- ^^ not necessarily same/different sex
- determine general label based off preferences

- combination of kiss, date and picnic will be for custom find lover task. All in one task. If they go successful, the two become lovers.
Another way ppl can become lovers is by being from enemy kingdoms while being in close proximity. Very low chance and is rare
- Make far-distance relationships possible somehow

- new visual bar for sexual happiness (done)
- new BehFindAPartner task which will look for lover and non-lovers. Can be used for cheating and will be a common task for initating romantic/sex tasks
(combination of BehGetLoverForRomanceAction and BehFindMatchingPreference etc. This will be the ultimate final task) (done)

- sexual ivf doesnt work for opposite sex sometimes for some reason?

- queer traits arent 100% being added
- sprites are offseted upwards for our speech bubbles idk why :(

*/
namespace Topic_of_Love.Mian
{
    public class TopicOfLove : BasicMod<TopicOfLove>
    {
        public static BasicMod<TopicOfLove> Mod;
        
        protected override void OnModLoad()
        {
            Mod = this;
            // Initialize your mod.
            // Methods are called in the order: OnLoad -> Awake -> OnEnable -> Start -> Update
            Util.LogWithId("Making people more loveable!");
            
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
            
            QueerTraits.Init();
            new ActorTraits().Init();
            new CultureTraits().Init();
            new SubspeciesTraits().Init();
            StatusEffects.Init();
            Happiness.Init();
            CommunicationTopics.Init();
            ActorBehaviorTasks.Init();
            Decisions.Init();
            Opinions.Init();
            GodPowers.Init();
            TabsAndButtons.Init();
            
            // Managers
            // DateableManager.Init();
        }
        private void Awake()
        {
            var harmony = new Harmony("netdot.mian.topicofloving");
            harmony.PatchAll();
        }
    }
}