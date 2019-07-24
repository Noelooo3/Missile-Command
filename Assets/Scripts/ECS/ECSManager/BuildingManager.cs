using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;

public class BuildingManager : MonoBehaviour
{
    public static EntityManager EM;
    public static BuildingManager Instance;

    public List<float3> PresetPosition;
    public List<Mesh> BuildingMeshs;
    public Material BuildingMaterial;

    private List<bool> BuildingAlive = new List<bool>();
    private List<Entity> Buildings = new List<Entity>();

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        EM = World.Active.EntityManager;
    }

    public void SpawnBuilding(bool restart)
    {
        if (restart) ResetBuilding();
        if (PresetPosition.Count < 6) return;

        Buildings.Clear();

        for (int i = 0; i < BuildingAlive.Count; i++)
        {
            if (BuildingAlive[i])
            {
                SpawnBuilding(i, PresetPosition[i], BuildingMeshs[UnityEngine.Random.Range(0, BuildingMeshs.Count)]);
            }
        }
    }

    private void ResetBuilding()
    {
        BuildingAlive.Clear();

        for (int i = 0; i < 6; i++)
        {
            BuildingAlive.Add(true);
        }
    }

    private void SpawnBuilding(int i, float3 pos, Mesh mesh)
    {
        Entity building = EM.CreateEntity(
            typeof(Translation),
            typeof(Scale),
            typeof(Rotation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(BuildingData),
            typeof(BuildingIsAlive)
            );

        EM.SetComponentData(building, new Translation { Value = pos });
        EM.SetComponentData(building, new Scale { Value = 0.03f });
        EM.SetComponentData(building, new Rotation { Value = Quaternion.Euler(0f, -180f, 0f) });
        EM.SetSharedComponentData(building, new RenderMesh
        {
            mesh = mesh,
            material = BuildingMaterial
        });

        EM.SetComponentData(building, new BuildingData
        {
            Index = i
        });

        Buildings.Add(building);
    }

    public IEnumerator RemoveBuildings()
    {
        for (int i = 0; i < Buildings.Count; i++)
        {
            EM.RemoveComponent<BuildingIsAlive>(Buildings[i]);
            GameManager.Instance.AddScore(500, true);

            yield return new WaitForSeconds(0.75f);
        }

        Buildings.Clear();
    }

    public void DestroyBuilding(Entity entity)
    {
        for (int i = 0; i < Buildings.Count; i++)
        {
            if(entity == Buildings[i])
            {
                VfxManager.Instance.DoExplosion(EM.GetComponentData<Translation>(entity).Value, 1.25f);
                AudioManager.Instance.DoBombExplosion();

                BuildingAlive[EM.GetComponentData<BuildingData>(entity).Index] = false;                
                Buildings.RemoveAt(i);
                EM.RemoveComponent<BuildingIsAlive>(entity);
                CheckGameOver();
                return;
            }
        }
    }

    private void CheckGameOver()
    {
        if(Buildings.Count == 0)
        {
            GameManager.Instance.GameOverWithNoBuilding();
        }
    }
}




