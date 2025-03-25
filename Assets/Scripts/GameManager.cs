using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private StageManager stageManager;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private MouseInputController mouseInputController;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (stageManager == null || playerManager == null || mouseInputController == null)
        {
            Debug.LogError("Required components are not assigned to GameManager!");
            return;
        }

        stageManager.Initialize();
        stageManager.LoadStage();
        
        playerManager.Initialize();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0)) // 左クリック中
        {
            Vector3 mousePosition = mouseInputController.GetMouseWorldPosition();
            Player player = playerManager.GetPlayer();
            if (player != null)
            {
                player.MoveTo(mousePosition);
            }
        }
    }
}