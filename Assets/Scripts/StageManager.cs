using UnityEngine;
using System;
using Hockey.Data;

public class StageManager : MonoBehaviour
{
    [SerializeField] private StageGenerator stageGenerator; // JSONベースの動的ステージ生成器
    [SerializeField] private string defaultStageId = "stage_default"; // デフォルトステージのID
    
    private GameObject currentStage;
    private Bounds stageBounds;
    
    public event Action<GameObject> OnStageLoaded;

    public void Initialize()
    {
        if (stageGenerator == null)
        {
            // StageGeneratorが指定されていない場合は子オブジェクトとして作成
            GameObject generatorObj = new GameObject("StageGenerator");
            generatorObj.transform.SetParent(transform);
            stageGenerator = generatorObj.AddComponent<StageGenerator>();
        }
        
        // StageGenerator初期化
        stageGenerator.Initialize();
    }

    public void LoadStage(string stageId = null)
    {
        if (currentStage != null)
        {
            Destroy(currentStage);
        }

        // 動的生成方式を使用
        currentStage = stageGenerator.GenerateStageById(stageId ?? defaultStageId);

        if (currentStage != null)
        {
            // ステージの境界を計算
            CalculateStageBounds();
            
            // ステージロードイベントを発行（外部クラスに通知）
            OnStageLoaded?.Invoke(currentStage);
        }
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