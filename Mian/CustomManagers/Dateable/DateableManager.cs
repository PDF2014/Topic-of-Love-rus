using NeoModLoader.services;

namespace Topic_of_Love.Mian.CustomManagers.Dateable;

using System.Collections.Generic;

public class DateableManager : MetaSystemManager<Dateable, DateableData>
{
    public static DateableManager Manager;

    public DateableManager()
    {
        type_id = "dateable";
    }
    
    public override void updateDirtyUnits()
    {
    }
    
    public override void checkDeadObjects()
    {
        foreach (var dateable in this)
        {
            if (!dateable.Actor.isAlive())
                removeObject(dateable);
            else
            {
                foreach (var actor in dateable.GetUndateables())
                {
                    if(!actor.isAlive())
                        dateable.AddOrRemoveUndateable(actor);
                }
            }
        }
    }
    
    public void AddOrRemoveUndateable(Actor actor, Actor undateable)
    {
        foreach (var dateable in this)
        {
            if (dateable.Actor == actor)
            {
                dateable.AddOrRemoveUndateable(undateable);
                return;
            }
        }
        
        var dateableObject = newObject();
        dateableObject.newDateable(actor);
        dateableObject.AddOrRemoveUndateable(undateable);
    }

    public List<Actor> GetUndateablesFor(Actor actor)
    {
        foreach (var dateable in this)
        {
            if (dateable.Actor == actor)
                return dateable.GetUndateables();
        }

        return null;
    }
    
    public bool IsActorUndateable(Actor actor, Actor undateable)
    {
        foreach (var dateable in this)
        {
            if (dateable.Actor == actor)
            {
                return dateable.IsUndateable(undateable);
            }
        }

        return false;
    }

    public static void Init()
    {
        TolUtil.LogWithId("Adding Dateable Manager");

        MapBox.instance._list_meta_main_managers.Add(Manager = new DateableManager());
    }
}