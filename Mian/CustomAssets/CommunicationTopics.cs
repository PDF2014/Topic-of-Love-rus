using System.Collections.Generic;
using System.Linq;
using Topic_of_Love.Mian.CustomAssets.Custom;
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
                check = pActor => !pActor.hasCultureTrait("orientationless") && (pActor.hasCultureTrait("homophobic") || pActor.hasCultureTrait("heterophobic")),
                pot_fill = (actor, sprites) =>
                {
                    var unfitPreferences = Orientation.Orientations.Where(orientation =>
                    {
                        if (actor.hasCultureTrait("homophobic"))
                            return orientation.IsHomo;
                        if (actor.hasCultureTrait("heterophobic"))
                            return orientation.IsHetero;
                        return false;
                    });

                    var sexualPreference = Orientation.GetOrientation(actor, true);
                    var romanticPreference = Orientation.GetOrientation(actor, false);

                    sprites.Add(sexualPreference.GetSprite(true));
                    sprites.Add(romanticPreference.GetSprite(false));

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