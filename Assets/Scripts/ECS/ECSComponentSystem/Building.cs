using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Building : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.WithAll<BuildingData>().WithNone<BuildingIsAlive>().ForEach((Entity entity) =>
        {
            PostUpdateCommands.DestroyEntity(entity);
        });
    }
}

public struct BuildingIsAlive : IComponentData { }
public struct BuildingData : IComponentData
{
    public int Index;
}

