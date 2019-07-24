using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class Explosion : ComponentSystem
{
    private List<float3> NextExplosionPos = new List<float3>();
    private List<float> NextExplosionRadius = new List<float>();
    private List<int> NextExplosionLevel = new List<int>();

    protected override void OnUpdate()
    {
        NextExplosionPos.Clear();
        NextExplosionRadius.Clear();
        NextExplosionLevel.Clear();

        Entities.WithAll<ExplosionData>().ForEach((Entity explosion, ref Scale scale, ref ExplosionData data, ref Translation translation)=> {

            float radius = scale.Value;
            int level = data.Level;
            float3 pos = translation.Value;

            Entities.WithAll<EnemyIsAlive>().WithAll<EnemyMissileData>().ForEach((Entity enemy, ref Translation enemyTranslation, ref EnemyExplosionData enemyData) => {

                if (math.distance(pos, enemyTranslation.Value) <= radius + 0.01f)
                {
                    NextExplosionPos.Add(enemyTranslation.Value);
                    NextExplosionRadius.Add(enemyData.ExplosionRadius);
                    NextExplosionLevel.Add(level + 1);
                    GameManager.Instance.AddScore(10, false);
                    PostUpdateCommands.DestroyEntity(enemy);
                }

            });

            Entities.WithAll<EnemyIsAlive>().WithAll<EnemyAircraftData>().ForEach((Entity enemy, ref Translation enemyTranslation, ref EnemyExplosionData enemyData) => {

                if (math.distance(pos, enemyTranslation.Value) <= radius + 0.5f)
                {
                    NextExplosionPos.Add(enemyTranslation.Value);
                    NextExplosionRadius.Add(enemyData.ExplosionRadius);
                    NextExplosionLevel.Add(level + 1);
                    GameManager.Instance.AddScore(50, false);
                    PostUpdateCommands.DestroyEntity(enemy);              
                }

            });

            if (scale.Value < data.Radius)
            {
                scale.Value += Time.deltaTime * 2f;
            }
            else if (data.ExtentionTime > 0f)
            {
                data.ExtentionTime -= Time.deltaTime;
            }
            else
            {
                PostUpdateCommands.DestroyEntity(explosion);
            }
        });

        for (int i = 0; i < NextExplosionPos.Count; i++)
        {
            ExplosionManager.Instance.CreateExplosion(NextExplosionPos[i], NextExplosionRadius[i], NextExplosionLevel[i]);
        }
    }
}

public struct ExplosionData : IComponentData
{
    public int Level;
    public float Radius;
    public float ExtentionTime;
}
