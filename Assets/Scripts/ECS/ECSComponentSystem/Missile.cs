using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class Missile : ComponentSystem
{
    public List<float3> ExplosionPos = new List<float3>();
    public List<float> ExplosionRadius = new List<float>();

    protected override void OnUpdate()
    {
        ExplosionPos.Clear();
        ExplosionRadius.Clear();

        Entities.WithAll<MissileData>().ForEach((Entity missile, ref Translation translation, ref MissileData data) =>
        {

            float3 direction = math.normalize(data.TargetPos - translation.Value);
            data.CurrentSpeed = data.CurrentSpeed >= data.MaxSpeed ? data.MaxSpeed : data.CurrentSpeed + data.Acceleration * Time.deltaTime;
            translation.Value += direction * data.CurrentSpeed * Time.deltaTime;

            if (math.distance(translation.Value, data.TargetPos) < 0.1f)
            {
                ExplosionPos.Add(translation.Value);
                ExplosionRadius.Add(data.ExplosionRadius);

                PostUpdateCommands.DestroyEntity(missile);
            }
        });

        for (int i = 0; i < ExplosionPos.Count; i++)
        {
            ExplosionManager.Instance.CreateExplosion(ExplosionPos[i], ExplosionRadius[i], 1);
        }
    }
}

public struct MissileData : IComponentData
{
    public float3 TargetPos;
    public float MaxSpeed;
    public float Acceleration;
    public float CurrentSpeed;
    public float ExplosionRadius;
}
