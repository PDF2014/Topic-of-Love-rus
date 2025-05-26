using ai.behaviours;

namespace Topic_of_Love.Mian.CustomAssets.AI.CustomBehaviors.other;

public class BehTryToEatCityFoodIfCan : BehCityActor
{
    public override BehResult execute(Actor pActor)
    {
        City city = pActor.city;
        if (!city.hasSuitableFood(pActor.subspecies))
            return BehResult.Continue;
        ResourceAsset foodItem1 = city.getFoodItem(pActor.subspecies, pActor.data.favorite_food);
        bool pNeedToPay = !pActor.isFoodFreeForThisPerson();
        if (foodItem1 != null)
        {
            if (pNeedToPay && !pActor.hasEnoughMoney(foodItem1.money_cost))
                return BehResult.Continue;
            this.eatFood(pActor, city, foodItem1, pNeedToPay);
            if (pActor.hasTrait("gluttonous"))
            {
                ResourceAsset foodItem2 = city.getFoodItem(pActor.subspecies, pActor.data.favorite_food);
                if (foodItem2 != null && pNeedToPay && pActor.hasEnoughMoney(foodItem2.money_cost))
                    this.eatFood(pActor, city, foodItem2, true);
            }
        }
        return BehResult.Continue;
    }

    public void eatFood(Actor pActor, City pCity, ResourceAsset pFoodItem, bool pNeedToPay)
    {
        if (pNeedToPay)
            pActor.spendMoney(pFoodItem.money_cost);
        pCity.eatFoodItem(pFoodItem.id);
        pActor.consumeFoodResource(pFoodItem);
    }
}