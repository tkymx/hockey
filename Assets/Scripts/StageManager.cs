using UnityEngine;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GameObject stagePrefab;
    private GameObject currentStage;

    public void Initialize()
    {
        if (stagePrefab == null)
        {
            Debug.LogError("Stage prefab is not assigned!");
            return;
        }
    }

    public void LoadStage()
    {
        if (currentStage != null)
        {
            Destroy(currentStage);
        }

        currentStage = Instantiate(stagePrefab);
    }

    public Transform GetStageTransform()
    {
        return currentStage != null ? currentStage.transform : null;
    }
}