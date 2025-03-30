using System;
using UnityEngine;

public class ZoneController : MonoBehaviour
{
    [SerializeField] private int zoneLevel;
    [SerializeField] private int requiredPlayerLevel;
    [SerializeField] private float width;
    [SerializeField] private float depth;
    [SerializeField] private float forwardOffset;

    private bool isCurrentZone;

    public int ZoneLevel { get => zoneLevel; set => zoneLevel = value; }
    public int RequiredPlayerLevel { get => requiredPlayerLevel; set => requiredPlayerLevel = value; }
    public bool IsCurrentZone { get => isCurrentZone; set => isCurrentZone = value; }
    public float Width { get => width; set => width = value; }
    public float Depth { get => depth; set => depth = value; }
    public float ForwardOffset { get => forwardOffset; set => forwardOffset = value; }

    public void Initialize(int playerLevel)
    {
        // プレイヤーレベルに基づいてゾーンの状態を更新
        UpdateZoneState(playerLevel);
    }

    public void UpdateZoneState(int playerLevel)
    {
        bool isUnlocked = playerLevel >= requiredPlayerLevel;
        
        // 壁の表示/非表示を制御 - 現在のゾーンの場合は必ず表示
        var wall = GetComponentInChildren<ZoneWall>();
        if (wall != null)
        {
            wall.SetWallState(isCurrentZone);
            wall.CurrentLevel = playerLevel;
        }

        // フォグエフェクトの制御
        var fog = transform.Find("ZoneFog")?.gameObject;
        if (fog != null)
        {
            fog.SetActive(!isUnlocked);
        }

        if (isUnlocked && !isCurrentZone)
        {
            PlayUnlockEffect();
        }
    }

    private void PlayUnlockEffect()
    {
        // ゾーン解放時のエフェクト再生
        // パーティクルシステムやアニメーション等を実装
    }
}
