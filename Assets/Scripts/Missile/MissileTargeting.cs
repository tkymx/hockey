using UnityEngine;
using System.Collections.Generic;

// ミサイルターゲット判定のインターフェース
public interface IMissileTargeting
{
    Transform GetTarget(Vector3 missilePosition);
}

// 通常の最近接ターゲット判定（全オブジェクトから探索）
public class DefaultMissileTargeting : IMissileTargeting
{
    public Transform GetTarget(Vector3 missilePosition)
    {
        // 全ての破壊可能オブジェクトから探索
        DestructibleObject[] targets = Object.FindObjectsByType<DestructibleObject>(FindObjectsSortMode.None);
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (DestructibleObject destructible in targets)
        {
            if (destructible.IsDestroyed())
                continue;
            
            float distance = Vector3.Distance(missilePosition, destructible.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = destructible.transform;
            }
        }
        
        return closestTarget;
    }
}

// 特定のゾーンに限定したターゲット判定
public class ZoneRestrictedTargeting : IMissileTargeting
{
    private ZoneController targetZone;
    
    public ZoneRestrictedTargeting(ZoneController zoneController)
    {
        this.targetZone = zoneController;
    }
    
    public Transform GetTarget(Vector3 missilePosition)
    {
        if (targetZone == null)
            return null;
            
        // ゾーンから破壊可能オブジェクトのリストを取得
        List<DestructibleObject> zoneTargets = targetZone.GetTargetableDestructibles();
        float closestDistance = float.MaxValue;
        Transform closestTarget = null;
        
        foreach (DestructibleObject destructible in zoneTargets)
        {
            if (destructible == null || destructible.IsDestroyed())
                continue;
            
            float distance = Vector3.Distance(missilePosition, destructible.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = destructible.transform;
            }
        }
        
        return closestTarget;
    }
}
