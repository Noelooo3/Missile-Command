using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

public class LaunchManager : MonoBehaviour
{
    public static EntityManager EM;
    public static LaunchManager Instance;

    private Camera Camera
    {
        get
        {
            if (_Camera == null)
            {
                _Camera = Camera.main;
            }
            return _Camera;
        }
    }
    private Camera _Camera;


    private bool GameOn;

    [Header("Launcher")]
    public Mesh MeshLauncherPlatform;
    public Mesh MeshLauncherBody, MeshLauncherGuns;
    public Material MatLauncherPlatform, MatLauncherBody, MatLauncherGuns;

    private List<LauncherObj> Launchers = new List<LauncherObj>();


    [Header("Missile")]
    public Mesh MeshMissile1;
    public Material MatMissile1;

    public Mesh MeshMissile2;
    public Material MatMissile2;

    [Header("Missile data")]
    public float Missile1Acceleration = 3f;
    public float Missile1MaxSpeed = 8f;
    public float Missile1ExplosionRadius = 0.35f;

    public float Missile2Acceleration = 1f;
    public float Missile2MaxSpeed = 4f;
    public float Missile2ExplosionRadius = 1f;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        EM = World.Active.EntityManager;
    }

    private void OnMouseOver()
    {
        if (GameOn && Time.timeScale != 0)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LaunchMissile1(Camera.ScreenToWorldPoint(Input.mousePosition));
            }

            if (Input.GetMouseButtonDown(1))
            {
                LaunchMissile2(Camera.ScreenToWorldPoint(Input.mousePosition));
            }
        }
    }

    public void StartGame(int m1, int m2)
    {
        Launchers.Clear();
        GameOn = true;
        SpawnMissileLauncher(new float3(-5f, -3f, 0f), m1, m2);
        SpawnMissileLauncher(new float3(0f, -3f, 0f), m1, m2);
        SpawnMissileLauncher(new float3(5f, -3f, 0f), m1, m2);

        UIManager.Instance.UpdateLauncher1(m1, m2);
        UIManager.Instance.UpdateLauncher2(m1, m2);
        UIManager.Instance.UpdateLauncher3(m1, m2);
    }

    public void StopGame()
    {
        GameOn = false;
    }

    #region Spawn/remove missile launcher
    private void SpawnMissileLauncher(float3 pos, int missile1, int missile2)
    {
        Entity platform = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MissileLauncherPlatform)
            );

        EM.SetComponentData(platform, new Translation { Value = pos});
        EM.SetComponentData(platform, new Scale { Value = 0.001f });
        EM.SetComponentData(platform, new Rotation { Value = Quaternion.identity });
        EM.SetSharedComponentData(platform, new RenderMesh
        {
            mesh = MeshLauncherPlatform,
            material = MatLauncherPlatform,
        });

        Entity body = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MissileLauncherBody),
            typeof(MissileLauncherAlive),
            typeof(MissileLauncherHasAmmo)
            );

        EM.SetComponentData(body, new Translation { Value = new float3(0f, 0.103f, 0f) + pos});
        EM.SetComponentData(body, new Scale { Value = 0.001f });
        EM.SetComponentData(body, new Rotation { Value = Quaternion.identity });
        EM.SetSharedComponentData(body, new RenderMesh
        {
            mesh = MeshLauncherBody,
            material = MatLauncherBody,
        });

        Entity guns = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MissileLauncherGuns),
            typeof(MissileLauncherAlive),
            typeof(MissileLauncherHasAmmo)
            );

        EM.SetComponentData(guns, new Translation { Value = new float3(0f, 0.359f, 0f) + pos});
        EM.SetComponentData(guns, new Scale { Value = 0.001f });
        EM.SetComponentData(guns, new Rotation { Value = Quaternion.identity });
        EM.SetSharedComponentData(guns, new RenderMesh
        {
            mesh = MeshLauncherGuns,
            material = MatLauncherGuns,
        });

        LauncherObj launcher = new LauncherObj()
        {
            Platform = platform,
            Body = body,
            Guns = guns,
            Missile1 = missile1,
            Missile2 = missile2,
            Index = Launchers.Count + 1
        };

        Launchers.Add(launcher);
        
    }

    public void DestroyLauncher(Entity part)
    {
        for (int i = 0; i < Launchers.Count; i++)
        {
            if (part == Launchers[i].Platform || part == Launchers[i].Body || part == Launchers[i].Guns)
            {
                VfxManager.Instance.DoExplosion(EM.GetComponentData<Translation>(part).Value, 0.85f);

                EM.RemoveComponent(Launchers[i].Platform, typeof(MissileLauncherAlive));
                EM.RemoveComponent(Launchers[i].Body, typeof(MissileLauncherAlive));
                EM.RemoveComponent(Launchers[i].Guns, typeof(MissileLauncherAlive));

                UIUpdate(Launchers[i].Index, 0, 0);
                Launchers.RemoveAt(i);
                return;
            }
        }
    }

    public IEnumerator RemoveLaunchers()
    {
        for (int i = 0; i < Launchers.Count; i++)
        {
            EM.RemoveComponent(Launchers[i].Platform, typeof(MissileLauncherAlive));
            EM.RemoveComponent(Launchers[i].Body, typeof(MissileLauncherAlive));
            EM.RemoveComponent(Launchers[i].Guns, typeof(MissileLauncherAlive));

            GameManager.Instance.AddScore(Launchers[i].Missile1 * 50 + Launchers[i].Missile2 * 200, true);
            UIUpdate(Launchers[i].Index, 0, 0);

            yield return new WaitForSeconds(0.75f);
        }

        Launchers.Clear();
    }

    #endregion

    #region Launch missile
    private void LaunchMissile1(float3 targetPos)
    {
        targetPos.z = 0f;

        LauncherObj launcher = null;
        float distance = float.MaxValue;

        for (int i = 0; i < Launchers.Count; i++)
        {
            if (Launchers[i].Missile1 > 0)
            {
                if (distance > math.distance(targetPos,EM.GetComponentData<Translation>(Launchers[i].Guns).Value))
                {
                    launcher = Launchers[i];
                    distance = math.distance(targetPos, EM.GetComponentData<Translation>(Launchers[i].Guns).Value);
                }
            }
        }

        if(launcher != null)
        {
            SpawnMissile1(EM.GetComponentData<Translation>(launcher.Guns).Value + new float3(0f, 0f, 0.1f), targetPos);
            launcher.Missile1--;
            UIUpdate(launcher.Index, launcher.Missile1, launcher.Missile2);
            LauncherAmmoCheck(launcher);
        }
    }

    private void LaunchMissile2(float3 targetPos)
    {
        targetPos.z = 0f;

        LauncherObj launcher = null;
        float distance = float.MaxValue;

        for (int i = 0; i < Launchers.Count; i++)
        {
            if (Launchers[i].Missile2 > 0)
            {
                if (distance > math.distance(targetPos, EM.GetComponentData<Translation>(Launchers[i].Guns).Value))
                {
                    launcher = Launchers[i];
                    distance = math.distance(targetPos, EM.GetComponentData<Translation>(Launchers[i].Guns).Value);
                }
            }
        }

        if (launcher != null)
        {
            SpawnMissile2(EM.GetComponentData<Translation>(launcher.Guns).Value + new float3(0f, 0f, 0.1f), targetPos);
            launcher.Missile2--;
            UIUpdate(launcher.Index, launcher.Missile1, launcher.Missile2);
            LauncherAmmoCheck(launcher);
        }
    }

    private void LauncherAmmoCheck(LauncherObj launcher)
    {
        if (launcher.Missile1 == 0 && launcher.Missile2 == 0)
        {
            EM.RemoveComponent(launcher.Body, typeof(MissileLauncherHasAmmo));
            EM.RemoveComponent(launcher.Guns, typeof(MissileLauncherHasAmmo));
            EM.RemoveComponent(launcher.Platform, typeof(MissileLauncherHasAmmo));
        }
    }

    private void SpawnMissile1(float3 from, float3 to)
    {
        Entity missile1 = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MissileData)
            );

        EM.SetComponentData(missile1, new Translation { Value = from });
        EM.SetComponentData(missile1, new Scale { Value = 0.105f });
        EM.SetComponentData(missile1, new Rotation { Value = Quaternion.Euler(-90f - Mathf.Rad2Deg * Mathf.Atan((to.x - from.x) / (to.y - from.y)), -90f, -90f) });
        EM.SetSharedComponentData(missile1, new RenderMesh
        {
            mesh = MeshMissile1,
            material = MatMissile1,
        });
        EM.SetComponentData(missile1, new MissileData { Acceleration = Missile1Acceleration, CurrentSpeed = 0f, MaxSpeed = Missile1MaxSpeed, TargetPos = to, ExplosionRadius = Missile1ExplosionRadius });

        AudioManager.Instance.DoMissileLaunch();
    }

    private void SpawnMissile2(float3 from, float3 to)
    {
        Entity missile2 = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MissileData)
            );

        EM.SetComponentData(missile2, new Translation { Value = from });
        EM.SetComponentData(missile2, new Scale { Value = 0.125f });
        EM.SetComponentData(missile2, new Rotation { Value = Quaternion.Euler(-90f - Mathf.Rad2Deg * Mathf.Atan((to.x - from.x) / (to.y - from.y)), -90f, -90f) });
        EM.SetSharedComponentData(missile2, new RenderMesh
        {
            mesh = MeshMissile2,
            material = MatMissile2,
        });
        EM.SetComponentData(missile2, new MissileData { Acceleration = Missile2Acceleration, CurrentSpeed = 0f, MaxSpeed = Missile2MaxSpeed, TargetPos = to, ExplosionRadius = Missile2ExplosionRadius });

        AudioManager.Instance.DoMissileLaunch();
    }
    #endregion

    private void UIUpdate(int i, int m1, int m2)
    {
        if(i == 1)
        {
            UIManager.Instance.UpdateLauncher1(m1, m2);
        }
        else if(i == 2)
        {
            UIManager.Instance.UpdateLauncher2(m1, m2);
        }
        else if(i == 3)
        {
            UIManager.Instance.UpdateLauncher3(m1, m2);
        }
    }
}



public class LauncherObj
{
    public Entity Platform;
    public Entity Body;
    public Entity Guns;
    public int Missile1;
    public int Missile2;
    public int Index;
}


