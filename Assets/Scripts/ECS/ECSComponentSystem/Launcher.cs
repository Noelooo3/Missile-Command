using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Launcher : ComponentSystem
{
    private Camera Camera
    {
        get
        {
            if(_Camera == null)
            {
                _Camera = Camera.main;
            }
            return _Camera;
        }
    }
    private Camera _Camera;

    protected override void OnUpdate()
    {
        float3 mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);

        AimAt(mousePos);
        CheckForReset();
        CheckDestroy();
    }

    private void AimAt(float3 mousePos)
    {
        Entities.WithAll<MissileLauncherHasAmmo>().WithAll<MissileLauncherAlive>().WithAll<MissileLauncherBody>().ForEach((ref Rotation rotation, ref Translation translation) =>
        {
            rotation.Value = Quaternion.Euler(0f, Mathf.Max(Mathf.Min(((translation.Value.x - mousePos.x) * -10f), 30f), -30f), 0f);
        });

        Entities.WithAll<MissileLauncherHasAmmo>().WithAll<MissileLauncherAlive>().WithAll<MissileLauncherGuns>().ForEach((ref Rotation rotation, ref Translation translation) =>
        {
            rotation.Value = Quaternion.Euler(Mathf.Max(Mathf.Min(((translation.Value.y - mousePos.y) * 10f), 0f), -70f), Mathf.Max(Mathf.Min(((translation.Value.x - mousePos.x) * -10f), 30f), -30f), 0f);
        });
    }


    private void CheckForReset()
    {
        Entities.WithNone<MissileLauncherHasAmmo>().WithAll<MissileLauncherAlive>().WithAll<MissileLauncherBody>().ForEach((ref Rotation rotation) =>
        {
            if(((Quaternion)rotation.Value).eulerAngles.y < 1f || ((Quaternion)rotation.Value).eulerAngles.y > 359f)
            {
                rotation.Value = quaternion.identity;
            }
            else if (((Quaternion)rotation.Value).eulerAngles.y > 180f)
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles + new Vector3(0f, 1f, 0f));
            }
            else if (((Quaternion)rotation.Value).eulerAngles.y < 180f)
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles + new Vector3(0f, -1f, 0f));
            }
        });

        Entities.WithNone<MissileLauncherHasAmmo>().WithAll<MissileLauncherAlive>().WithAll<MissileLauncherGuns>().ForEach((ref Rotation rotation) =>
        {
            if (((Quaternion)rotation.Value).eulerAngles.y < 1f || ((Quaternion)rotation.Value).eulerAngles.y > 359f)
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles.x, 0f, 0f);
            }
            if (((Quaternion)rotation.Value).eulerAngles.y > 180f)
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles + new Vector3(0f, 1f, 0f));
            }
            else if (((Quaternion)rotation.Value).eulerAngles.y < 180f)
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles + new Vector3(0f, -1f, 0f));
            }

            if(((Quaternion)rotation.Value).eulerAngles.x > 359f || ((Quaternion)rotation.Value).eulerAngles.x < 1f)
            {
                rotation.Value = Quaternion.Euler(0f, ((Quaternion)rotation.Value).eulerAngles.y, 0f);
            }
            else
            {
                rotation.Value = Quaternion.Euler(((Quaternion)rotation.Value).eulerAngles + new Vector3(1f, 0f, 0f));
            }
        });
    }

    private void CheckDestroy()
    {
        Entities.WithAll<MissileLauncherGuns>().WithNone<MissileLauncherAlive>().ForEach((Entity entity) => { PostUpdateCommands.DestroyEntity(entity); });
        Entities.WithAll<MissileLauncherBody>().WithNone<MissileLauncherAlive>().ForEach((Entity entity) => { PostUpdateCommands.DestroyEntity(entity); });
        Entities.WithAll<MissileLauncherPlatform>().WithNone<MissileLauncherAlive>().ForEach((Entity entity) => { PostUpdateCommands.DestroyEntity(entity); });
    }
}

//Tag
public struct MissileLauncherPlatform : IComponentData { }
public struct MissileLauncherBody : IComponentData { }
public struct MissileLauncherGuns : IComponentData { }
public struct MissileLauncherAlive : IComponentData { }
public struct MissileLauncherHasAmmo : IComponentData { }