using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[CreateAssetMenu(fileName = "MapEventDispatcher", menuName = "ScriptableObjects/MapEventDispatcher", order = 1)]
public class MapEventDispatcher : ScriptableObject
{
    public ReactiveProperty<bool> OnMapLoaded = new ReactiveProperty<bool>(false);

    public void MapLoaded()
    {
        OnMapLoaded.Value = true;
    }

    public void MapUnLoaded()
    {
        OnMapLoaded.Value = false;
    }
}
