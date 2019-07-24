using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EntityManager EM;
    public static EnemyManager Instance;

    private bool HasLivedMissile;

    [Header("Enemy missile")]
    public Mesh MeshEnemyMissile;
    public Material MatEnemyMissile;

    public int MissileRemind;
    public float MaxMissileSpawnTime;
    public float MinMissileSpawnTime;

    [Header("Enemy aircraft")]
    public Mesh MeshEnemyAircraft;
    public Material MatEnemyAircraft;

    public int AircraftRemind;
    public float MaxAircraftSpawnTime;
    public float MinAircraftSpawnTime;

    [Header("Enemy data")]
    public float EnemyMissileSpeed = 0.75f;
    public float EnemyMissileExplosionRadius = 0.5f;
    public float EnemyAircraftSpeed = 1f;
    public float EnemyAircraftExplosionRadius = 0.75f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        EM = World.Active.EntityManager;      
    }

    public void StartGame(int missile, float minMis, float maxMis, int aircraft, float minAir, float maxAir)
    {
        MissileRemind = missile;
        MinMissileSpawnTime = minMis;
        MaxMissileSpawnTime = maxMis;

        AircraftRemind = aircraft;
        MinAircraftSpawnTime = minAir;
        MaxAircraftSpawnTime = maxAir;

        StartCoroutine(SpawnMissile());
        StartCoroutine(SpawnAircraft());
    }

    public void StopGame()
    {
        StopAllCoroutines();
        NativeArray<Entity> allEntities = EM.GetAllEntities();

        for (int i = 0; i < allEntities.Length; i++)
        {
            EM.RemoveComponent<EnemyIsAlive>(allEntities[i]);
        }
    }
    
    private IEnumerator SpawnMissile()
    {
        while (MissileRemind > 0)
        {
            SpawnEnemyMissile(new float3(UnityEngine.Random.Range(-6f, 6f), 6f, 0f), new float3(UnityEngine.Random.Range(-6f, 6f), -3f, 0f), UnityEngine.Random.Range(0, 2) == 1);
            MissileRemind--;
            yield return new WaitForSeconds(UnityEngine.Random.Range(MinMissileSpawnTime, MaxMissileSpawnTime));
        }   
    }

    private IEnumerator SpawnAircraft()
    {
        while (AircraftRemind > 0)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(MinAircraftSpawnTime, MaxAircraftSpawnTime));
            SpawnAircraft(UnityEngine.Random.Range(3f, 4f));
            AircraftRemind--;
        }
    }

    public void SpawnEnemyMissile(float3 from, float3 to, bool respawnable)
    {
        HasLivedMissile = true;

        Entity missile = EM.CreateEntity(

            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(EnemyIsAlive),
            typeof(EnemyMissileData),
            typeof(EnemyExplosionData)
            );

        EM.SetComponentData(missile, new Translation { Value = from });
        EM.SetComponentData(missile, new Scale { Value = 0.105f });
        EM.SetComponentData(missile, new Rotation { Value = Quaternion.Euler(90f - Mathf.Rad2Deg * Mathf.Atan((to.x - from.x) / (to.y - from.y)), -90f, -90f) });
        EM.SetComponentData(missile, new EnemyExplosionData { ExplosionRadius = EnemyMissileExplosionRadius });
        EM.SetSharedComponentData(missile, new RenderMesh
        {
            mesh = MeshEnemyMissile,
            material = MatEnemyMissile,
        });
        EM.SetComponentData(missile, new EnemyMissileData
        {
            TargetPos = to,
            Speed = EnemyMissileSpeed,
            Respawnable = respawnable,
            LifeSpan = UnityEngine.Random.Range(3f, 7f)
        });
    }

    public void SpawnAircraft(float atHeight)
    {
        Entity aircraft = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(EnemyIsAlive),
            typeof(EnemyAircraftData),
            typeof(EnemyExplosionData)
            );

        bool leftToRight = UnityEngine.Random.Range(0, 2) == 1;

        EM.SetComponentData(aircraft, new Translation { Value = new float3(leftToRight ? -7f : 7f, atHeight, 0f) });
        EM.SetComponentData(aircraft, new Scale { Value = 0.05f });
        EM.SetComponentData(aircraft, new Rotation { Value = Quaternion.Euler(0f, leftToRight ? 90f : -90f, 0f) });
        EM.SetComponentData(aircraft, new EnemyExplosionData { ExplosionRadius = EnemyAircraftExplosionRadius });
        EM.SetSharedComponentData(aircraft, new RenderMesh
        {
            mesh = MeshEnemyAircraft,
            material = MatEnemyAircraft,
        });
        EM.SetComponentData(aircraft, new EnemyAircraftData
        {
            Speed = leftToRight ? 1f * EnemyAircraftSpeed : -1f * EnemyAircraftSpeed,
            TimeToLaunchMissile = UnityEngine.Random.Range(1f, 3f)
        });
    }

    public void NoLivedMissile()
    {
        if (HasLivedMissile)
        {
            HasLivedMissile = false;
            CheckGameFinish();
        }
    }

    private void CheckGameFinish()
    {
        if (!HasLivedMissile && MissileRemind == 0 && AircraftRemind == 0)
        {
            GameManager.Instance.GameFinishedWithNoEnemy();
        }
    }
}

public struct EnemyIsAlive : IComponentData { }
public struct EnemyExplosionData : IComponentData
{
    public float ExplosionRadius;
}
