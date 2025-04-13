using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全てのHPゲージを管理するクラス
/// </summary>
public class HPBarManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hpBarPrefab;
    
    // 管理対象のHPバーコントローラー
    private Dictionary<DestructibleObject, HPBarController> hpBars = new Dictionary<DestructibleObject, HPBarController>();
    
    private static HPBarManager instance;
    public static HPBarManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<HPBarManager>();
                
                if (instance == null)
                {
                    Debug.LogError("HPBarManagerのインスタンスが見つかりません。シーンに追加してください。");
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    /// <summary>
    /// 指定した破壊可能オブジェクト用のHPゲージを作成
    /// </summary>
    /// <param name="destructibleObject">対象の破壊可能オブジェクト</param>
    public void CreateHPBar(DestructibleObject destructibleObject)
    {
        if (destructibleObject == null || hpBarPrefab == null)
        {
            Debug.LogError("HPBarの作成に失敗しました。必要なオブジェクトが設定されていません。");
            return;
        }
        
        // すでに作成済みなら何もしない
        if (hpBars.ContainsKey(destructibleObject))
        {
            return;
        }
        
        // HPバーのインスタンス化
        GameObject hpBarInstance = Instantiate(hpBarPrefab, this.transform, false);
        HPBarController hpBarController = hpBarInstance.GetComponent<HPBarController>();
        
        if (hpBarController == null)
        {
            Debug.LogError("HPBarPrefabにHPBarControllerコンポーネントがアタッチされていません。");
            Destroy(hpBarInstance);
            return;
        }
        
        // HPバーの初期化
        hpBarController.Initialize(destructibleObject.transform);
        
        // 初期のHP値を設定
        float healthPercentage = destructibleObject.GetHealthPercentage();
        hpBarController.UpdateHealthBar(healthPercentage);
        
        // HP変更イベントを購読
        destructibleObject.OnHealthChanged += (float health) => {
            if (hpBarController != null)
            {
                hpBarController.UpdateHealthBar(health);
            }
        };
        
        // オブジェクト破壊イベントを購読
        destructibleObject.OnObjectDestroyed += (obj, points) => {
            RemoveHPBar(destructibleObject);
        };
        
        // 管理リストに追加
        hpBars.Add(destructibleObject, hpBarController);
    }
    
    /// <summary>
    /// 指定した破壊可能オブジェクトのHPゲージを削除
    /// </summary>
    /// <param name="destructibleObject">対象の破壊可能オブジェクト</param>
    public void RemoveHPBar(DestructibleObject destructibleObject)
    {
        if (destructibleObject != null && hpBars.TryGetValue(destructibleObject, out HPBarController hpBar))
        {
            if (hpBar != null)
            {
                Destroy(hpBar.gameObject);
            }
            
            hpBars.Remove(destructibleObject);
        }
    }
    
    /// <summary>
    /// 全てのHPゲージを削除
    /// </summary>
    public void ClearAllHPBars()
    {
        foreach (var hpBar in hpBars.Values)
        {
            if (hpBar != null)
            {
                Destroy(hpBar.gameObject);
            }
        }
        
        hpBars.Clear();
    }
    
    private void OnDestroy()
    {
        ClearAllHPBars();
        
        if (instance == this)
        {
            instance = null;
        }
    }
}