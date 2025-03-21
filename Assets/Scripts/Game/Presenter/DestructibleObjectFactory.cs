using UnityEngine;
using System.Collections.Generic;

public class DestructibleObjectFactory
{
    private GameConfig config;
    private GameObject destructibleObjectPrefab;
    private Transform objectParent;
    
    public DestructibleObjectFactory(GameObject prefab, Transform parent)
    {
        destructibleObjectPrefab = prefab;
        objectParent = parent;
        config = ConfigManager.Instance.GetConfig();
    }
    
    public DestructibleObjectPresenter CreateObject()
    {
        // 画面内のランダムな位置を計算
        float screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 0.8f;
        float screenHeight = Camera.main.orthographicSize * 0.8f;
        Vector2 randomPosition = new Vector2(
            Random.Range(-screenWidth, screenWidth),
            Random.Range(-screenHeight, screenHeight)
        );
        
        return CreateObjectAtPosition(randomPosition);
    }
    
    public DestructibleObjectPresenter CreateObjectAtPosition(Vector2 position)
    {
        // オブジェクトタイプをランダムに選択
        ObjectType objectType = (ObjectType)Random.Range(0, System.Enum.GetValues(typeof(ObjectType)).Length);
        
        // 設定から値を取得
        float objectSize = Random.Range(config.minObjectSize, config.maxObjectSize);
        float durability;
        int pointValue;
        
        // オブジェクトタイプに応じた値を設定
        switch (objectType)
        {
            case ObjectType.Bonus:
                durability = config.bonusObject.durability;
                pointValue = config.bonusObject.pointValue;
                break;
            case ObjectType.Obstacle:
                durability = config.obstacleObject.durability;
                pointValue = config.obstacleObject.pointValue;
                break;
            default:
                durability = config.normalObject.durability;
                pointValue = config.normalObject.pointValue;
                break;
        }
        
        // オブジェクトモデルの作成
        DestructibleObjectModel objectModel = new DestructibleObjectModel(objectType, objectSize, durability, pointValue);
        objectModel.Move(position);
        
        // ビューモデルの作成
        DestructibleObjectViewModel objectViewModel = new DestructibleObjectViewModel();
        objectViewModel.SetModel(objectModel);
        
        // オブジェクトの生成
        GameObject objectGO = Object.Instantiate(destructibleObjectPrefab, position, Quaternion.identity, objectParent);
        DestructibleObjectView objectView = objectGO.GetComponent<DestructibleObjectView>();
        
        // UseCaseとPresenterの作成
        DestructibleObjectUseCase objectUseCase = new DestructibleObjectUseCase(objectModel);
        DestructibleObjectPresenter objectPresenter = new DestructibleObjectPresenter(objectUseCase, objectViewModel, objectView);
        
        // 初期化
        objectView.Initialize(objectViewModel);
        objectPresenter.Initialize(position, objectType);
        
        return objectPresenter;
    }
    
    public List<DestructibleObjectPresenter> CreateInitialObjects()
    {
        List<DestructibleObjectPresenter> objects = new List<DestructibleObjectPresenter>();
        
        // 設定から初期オブジェクト数を取得
        for (int i = 0; i < config.initialObjectCount; i++)
        {
            objects.Add(CreateObject());
        }
        
        return objects;
    }
}