using System.Collections.Generic;
using Topic_of_Love.Mian.CustomAssets.Traits;

namespace Topic_of_Love.Mian.CustomAssets
{
    public class CommunicationTopics
    {
        public static void Init()
        {
            Add(new CommunicationAsset
            {
                id = "orientation",
                rate = 0.5f,
                check = pActor => Orientations.GetPreferenceFromActor(pActor, false) != Preference.Inapplicable 
                                  && Orientations.GetPreferenceFromActor(pActor, true) != Preference.Inapplicable
                                  && Orientations.GetQueerTraits(pActor, true).Count >= 2
                                  && !pActor.hasCultureTrait("orientationless"),
                pot_fill = (actor, sprites) =>
                {
                    if (Orientations.GetQueerTraits(actor, true).Count < 2) return;
                    var unfitPreferences = new List<Preference>();
                    if (actor.hasCultureTrait("homophobic"))
                    {
                        unfitPreferences.Add(Preference.All);
                        unfitPreferences.Add(Preference.SameSex);
                        unfitPreferences.Add(Preference.SameOrDifferentSex);
                    }
                    if (actor.hasCultureTrait("heterophobic"))
                    {
                        unfitPreferences.Add(Preference.DifferentSex);
                    }

                    var queerTraits = Orientations.GetQueerTraits(actor, true);
                    var sexualPreference = queerTraits[0].preference;
                    var sexualSprite = queerTraits[0].getSprite();
                    var romanticPreference = queerTraits[1].preference;
                    var romanticSprite = queerTraits[1].getSprite();

                    if (sexualSprite == null || romanticSprite == null)
                        return;
                    sprites.Add(sexualSprite);
                    sprites.Add(romanticSprite);

                    if (unfitPreferences.Contains(sexualPreference) || unfitPreferences.Contains(romanticPreference))
                        actor.changeHappiness("orientation_does_not_fit");
                    else
                        actor.changeHappiness("orientation_fits");
                } 
            });
        }

        private static void Add(CommunicationAsset asset)
        {
            AssetManager.communication_topic_library.add(asset);
        }
    }
}