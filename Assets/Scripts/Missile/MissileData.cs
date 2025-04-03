using UnityEngine;

[CreateAssetMenu(fileName = "MissileData", menuName = "Hockey/MissileData")]
public class MissileData : ScriptableObject
{
    [Header("ミサイル基本設定")]
    [Tooltip("ミサイルの発射間隔（秒）")]
    public float firingInterval = 5f;
    
    [Tooltip("ミサイルの移動速度")]
    public float speed = 10f;
    
    [Tooltip("ミサイルの与えるダメージ量")]
    public float damage = 50f;
    
    [Tooltip("ミサイルの存在時間（秒）")]
    public float lifetime = 3f;
    
    [Tooltip("ミサイルのプレハブ")]
    public GameObject missilePrefab;
}