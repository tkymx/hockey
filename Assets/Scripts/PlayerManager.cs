using UnityEngine;
using Hockey.Data;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Player playerPrefab;
    
    private Player currentPlayer;
    private PlayerData playerData;
    
    public void Initialize(GameConfigRepository gameConfigRepository)
    {
        this.playerData = gameConfigRepository.PlayerConfig;
        
        if (currentPlayer == null)
        {
            // Default spawn position if not specified elsewhere
            SpawnPlayer(new Vector3(0, 0.5f, -10f));
        }
        else
        {
            // 既存のプレイヤーを初期化
            currentPlayer.Initialize(this.playerData);
        }
    }
    
    public void SpawnPlayer(Vector3 position)
    {
        currentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        currentPlayer.Initialize(this.playerData);
        currentPlayer.name = "Player";
    }
    
    public Player GetPlayer()
    {
        return currentPlayer;
    }
    
    public void ResetPlayer()
    {
        if (currentPlayer != null)
        {
            currentPlayer.ResetPosition();
        }
    }
    
    // 現在のゾーンをプレイヤーに設定
    public void UpdatePlayerZone(ZoneController zone)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetCurrentZone(zone);
        }
    }
}
