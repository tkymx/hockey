using UnityEngine;
using System.Collections;

public class MissileSkill : MonoBehaviour
{
    [SerializeField] private MissileData missileData;
    
    private Player owner;
    private bool isActive = false;
    private float nextFireTime;
    private Puck targetPuck;
    
    public void Initialize(Player player, Puck puck)
    {
        owner = player;
        targetPuck = puck;
        
        // ミサイルデータがない場合はスキルを有効化しない
        if (missileData == null)
        {
            Debug.LogError("MissileData is not assigned for MissileSkill");
            return;
        }
        
        // パックの参照チェック
        if (targetPuck == null)
        {
            Debug.LogError("Puck reference is null. Missile skill will not work correctly.");
            return;
        }
        
        // スキルを有効化
        isActive = true;
        nextFireTime = Time.time + missileData.firingInterval;
        
        // 発射コルーチンを開始
        StartCoroutine(FireMissileRoutine());
    }
    
    // 下位互換性のため、古いInitializeメソッドも残す
    public void Initialize(Player player)
    {
        owner = player;
        
        // ミサイルデータがない場合はスキルを有効化しない
        if (missileData == null)
        {
            Debug.LogError("MissileData is not assigned for MissileSkill");
            return;
        }
        
        // パックを探す
        targetPuck = FindObjectOfType<Puck>();
        if (targetPuck == null)
        {
            Debug.LogError("Puck not found in the scene. Missile skill will not work correctly.");
            return;
        }
        
        // スキルを有効化
        isActive = true;
        nextFireTime = Time.time + missileData.firingInterval;
        
        // 発射コルーチンを開始
        StartCoroutine(FireMissileRoutine());
    }
    
    private IEnumerator FireMissileRoutine()
    {
        // スキルがアクティブである限り実行し続ける
        while (isActive)
        {
            // 発射間隔を待つ
            yield return new WaitForSeconds(missileData.firingInterval);
            
            // ミサイルを発射
            FireMissile();
        }
    }
    
    private void FireMissile()
    {
        if (!isActive || missileData == null || missileData.missilePrefab == null) return;
        if (targetPuck == null) return;
        
        // パックの中心位置を取得
        Vector3 puckPosition = targetPuck.GetPosition();
        
        // 向きはランダムにする（または適切な方向を計算）
        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        
        // プレハブからミサイルを生成
        GameObject missileObj = Instantiate(
            missileData.missilePrefab, 
            puckPosition, 
            randomRotation
        );
        
        // ミサイルを初期化
        Missile missile = missileObj.GetComponent<Missile>();
        if (missile != null)
        {
            missile.Initialize(missileData, owner);
        }
        else
        {
            Debug.LogError("Missile component not found on missile prefab");
            Destroy(missileObj);
        }
    }
    
    public void SetActive(bool active)
    {
        if (isActive == active) return;
        
        isActive = active;
        
        // アクティブになったときは発射ルーチンを開始
        if (isActive && !IsInvoking("FireMissileRoutine"))
        {
            StartCoroutine(FireMissileRoutine());
        }
    }
    
    private void OnDestroy()
    {
        // コルーチンを停止
        StopAllCoroutines();
    }
}