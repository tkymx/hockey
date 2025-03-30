using UnityEngine;
using System;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GameObject stagePrefab;
    private GameObject currentStage;
    private Bounds stageBounds;
    
    public event Action<GameObject> OnStageLoaded;

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
        CalculateStageBounds();
        
        // ステージロードイベントを発行
        OnStageLoaded?.Invoke(currentStage);
    }

    private void CalculateStageBounds()
    {
        if (currentStage == null) return;

        // ステージの全てのColliderを取得してBoundsを計算
        Collider[] colliders = currentStage.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found in stage!");
            return;
        }

        stageBounds = colliders[0].bounds;
        foreach (Collider collider in colliders)
        {
            stageBounds.Encapsulate(collider.bounds);
        }
    }

    public GameObject GetCurrentStage()
    {
        return currentStage;
    }

    public Vector3 GetStageCenter()
    {
        return currentStage != null ? currentStage.transform.position : Vector3.zero;
    }

    public Vector3 GetStageBounds()
    {
        return stageBounds.size;
    }

    public Transform GetStageTransform()
    {
        return currentStage != null ? currentStage.transform : null;
    }
}