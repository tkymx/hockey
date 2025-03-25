using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 initialPosition = Vector3.zero;
    private Player currentPlayer;

    public void Initialize()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned!");
            return;
        }
        CreatePlayer();
    }

    public void CreatePlayer()
    {
        if (currentPlayer != null)
        {
            Destroy(currentPlayer.gameObject);
        }

        GameObject playerObject = Instantiate(playerPrefab);
        currentPlayer = playerObject.GetComponent<Player>();
        SetInitialPlayerPosition(initialPosition);
    }

    public void SetInitialPlayerPosition(Vector3 position)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetPosition(position);
        }
    }

    public Player GetPlayer()
    {
        return currentPlayer;
    }
    
    // プレイヤーを初期位置にリセット
    public void ResetPlayer()
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetPosition(initialPosition);
        }
        else
        {
            CreatePlayer();
        }
    }
}