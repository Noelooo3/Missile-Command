using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance;

    public ParticleSystem ExplosionPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void DoExplosion(float3 pos, float size)
    {
        size += 0.15f;
        ParticleSystem ps = Instantiate(ExplosionPrefab, (Vector3)pos, Quaternion.identity, this.transform);
        ps.transform.localScale = new Vector3(size, size, size);
        ps.Play();

        Destroy(ps, 5f);
    }
}
