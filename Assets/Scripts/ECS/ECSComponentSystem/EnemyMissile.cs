using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class EnemyMissile : ComponentSystem
{
    private List<float3> RespawnPos = new List<float3>();
    private List<float3> TargetPos = new List<float3>();

    private List<Entity> Buildings = new List<Entity>();
    private List<Entity> Launchers = new List<Entity>();

    private bool MissileRemind = false;

    protected override void OnUpdate()
    {
        MissileRemind = false;

        for (int i = 0; i < RespawnPos.Count; i++)
        {
            for (int j = 0; j < UnityEngine.Random.Range(1f, 4f); j++)
            {
                EnemyManager.Instance.SpawnEnemyMissile(RespawnPos[i], TargetPos[i] + new float3(0.5f * j, 0f, 0f), false);
            }
        }

        for (int i = 0; i < Launchers.Count; i++)
        {
            LaunchManager.Instance.DestroyLauncher(Launchers[i]);
            AudioManager.Instance.DoBombExplosion();
        }

        for (int i = 0; i < Buildings.Count; i++)
        {
            BuildingManager.Instance.DestroyBuilding(Buildings[i]);
            AudioManager.Instance.DoBombExplosion();
        }

        RespawnPos.Clear();
        TargetPos.Clear();
        Launchers.Clear();
        Buildings.Clear();

        Entities.WithAll<EnemyMissileData>().WithNone<EnemyIsAlive>().ForEach((Entity missile) =>
        {
            MissileRemind = true;
            PostUpdateCommands.DestroyEntity(missile);
        });

        Entities.WithAll<EnemyMissileData>().WithAll<EnemyIsAlive>().ForEach((Entity missile, ref Translation translation, ref EnemyMissileData data) =>
        {
            MissileRemind = true;

            float3 direction = math.normalize(data.TargetPos - translation.Value);
            translation.Value = translation.Value += direction * data.Speed * Time.deltaTime;
            float3 missilePos = translation.Value;

            if (math.distance(translation.Value, data.TargetPos) < 0.1f)
            {
                PostUpdateCommands.DestroyEntity(missile);
                return;
            }

            Entities.WithAll<MissileLauncherAlive>().ForEach((Entity launcher, ref Translation launcherTranslation) =>
            {
                if (math.distance(launcherTranslation.Value, missilePos) < 0.35f)
                {
                    PostUpdateCommands.DestroyEntity(missile);
                    Launchers.Add(launcher);
                    return;
                }
            });

            Entities.WithAll<BuildingIsAlive>().ForEach((Entity building, ref Translation launcherTranslation) =>
            {
                if (math.distance(launcherTranslation.Value, missilePos) < 1f)
                {
                    PostUpdateCommands.DestroyEntity(missile);
                    Buildings.Add(building);
                    return;
                }
            });

            if (data.Respawnable)
            {
                data.LifeSpan -= Time.deltaTime;

                if (data.LifeSpan <= 0)
                {
                    PostUpdateCommands.DestroyEntity(missile);
                    RespawnPos.Add(translation.Value);
                    TargetPos.Add(data.TargetPos);
                }
            }
        });
     
        if (!MissileRemind)
        {
            EnemyManager.Instance.NoLivedMissile();
        }
    }
}

public struct EnemyMissileData : IComponentData
{
    public float3 TargetPos;
    public float Speed;
    public bool Respawnable;
    public float LifeSpan;
}

