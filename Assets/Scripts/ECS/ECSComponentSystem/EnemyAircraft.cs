using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class EnemyAircraft : ComponentSystem
{
    private List<float3> LaunchPos = new List<float3>();

    protected override void OnUpdate()
    {
        LaunchPos.Clear();

        Entities.WithAll<EnemyAircraftData>().WithNone<EnemyIsAlive>().ForEach((Entity aircraft) =>
        {
            PostUpdateCommands.DestroyEntity(aircraft);
        });

        Entities.WithAll<EnemyAircraftData>().WithAll<EnemyIsAlive>().ForEach((Entity aircraft, ref Translation translation, ref EnemyAircraftData data) =>
        {
            translation.Value += (float3)Vector3.right * data.Speed * Time.deltaTime;
            data.TimeToLaunchMissile -= Time.deltaTime;

            if (data.TimeToLaunchMissile <= 0)
            {
                LaunchPos.Add(translation.Value);
                data.TimeToLaunchMissile = UnityEngine.Random.Range(1f, 3f);
            }

            if (translation.Value.x <= -7.5f || translation.Value.x > 7.5f)
            {
                PostUpdateCommands.DestroyEntity(aircraft);
            }
        });

        for (int i = 0; i < LaunchPos.Count; i++)
        {
            EnemyManager.Instance.SpawnEnemyMissile(LaunchPos[i], new float3(UnityEngine.Random.Range(-6f, 6f), -3f, 0f), false);
        }
    }
}

public struct EnemyAircraftData : IComponentData
{
    public float Speed;
    public float TimeToLaunchMissile;
}
