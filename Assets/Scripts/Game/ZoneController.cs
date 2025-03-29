using System;
using UnityEngine;

public class ZoneController : MonoBehaviour
{
    [SerializeField] private int zoneLevel;
    [SerializeField] private int requiredPlayerLevel;
    [SerializeField] private float radius;

    public int ZoneLevel { get => zoneLevel; set => zoneLevel = value; }
    public int RequiredPlayerLevel { get => requiredPlayerLevel; set => requiredPlayerLevel = value; }
    public float Radius { get => radius; set => radius = value; }

    public void Initialize(int playerLevel)
    {
        // プレイヤーレベルに基づいてゾーンの状態を更新
        UpdateZoneState(playerLevel);
    }


    public void UpdateZoneState(int playerLevel)
    {
        bool isUnlocked = playerLevel >= requiredPlayerLevel;
        
        // 壁の表示/非表示を制御
        var wall = GetComponentInChildren<ZoneWall>();
        if (wall != null)
        {
            wall.SetWallState(!isUnlocked);
            wall.CurrentLevel = playerLevel;
        }

        // フォグエフェクトの制御
        var fog = transform.Find("ZoneFog")?.gameObject;
        if (fog != null)
        {
            fog.SetActive(!isUnlocked);
        }

        if (isUnlocked)
        {
            PlayUnlockEffect();
        }
    }

    internal void Initialize(object playerLevel)
    {
        throw new NotImplementedException();
    }

    private void PlayUnlockEffect()
    {
        // ゾーン解放時のエフェクト再生
        // パーティクルシステムやアニメーション等を実装
    }
}
