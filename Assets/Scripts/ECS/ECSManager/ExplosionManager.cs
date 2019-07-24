using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

public class ExplosionManager : MonoBehaviour
{
    public static EntityManager EM;
    public static ExplosionManager Instance;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        EM = World.Active.EntityManager;
    }

    public void CreateExplosion(float3 pos, float radius, int explosionLevel)
    {
        Entity explosion = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(LocalToWorld),
            typeof(ExplosionData)
            );

        EM.SetComponentData(explosion, new Translation { Value = pos });
        EM.SetComponentData(explosion, new Scale { Value = 0f });
        EM.SetComponentData(explosion, new ExplosionData
        {
            Level = explosionLevel,
            Radius = radius,
            ExtentionTime = 0.5f
        });

        VfxManager.Instance.DoExplosion(pos, radius);
        GameManager.Instance.DoSlowMotion(explosionLevel);

        if (explosionLevel == 1)
        {
            AudioManager.Instance.DoMissileExplosion();
        }
    }
}
