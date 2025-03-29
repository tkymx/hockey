using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraAngle = 45f; // カメラの見下ろし角度（デフォルト45度）
    [SerializeField] private float zOffset = 0f; // ステージに対するZ方向のオフセット（+で後ろ側、-で前側）
    [SerializeField] private float moveDuration = 0.5f; // カメラ移動時間
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeInOutQuad; // イージングタイプ
    
    private Camera _camera;
    private Vector3 _targetPosition;
    private bool _isMoving = false;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }
    
    public void UpdateCameraPosition(Vector3 stageCenter, Vector3 stageBounds)
    {
        if (_camera == null) return;

        // カメラの角度を設定
        transform.rotation = Quaternion.Euler(cameraAngle, 0f, 0f);
        
        // ステージの寸法を取得（ゾーンの直径を使用）
        float zoneWidth = stageBounds.x;
        float zoneDepth = stageBounds.z;
        
        // カメラ角度（ラジアン）
        float cameraAngleRad = cameraAngle * Mathf.Deg2Rad;
        
        // カメラの現在のFOV（ラジアン）
        float fovRad = _camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        
        // ステージの中心からカメラまでの必要な距離を計算
        // 横幅と奥行きの両方が視界に収まる必要がある
        float distanceForWidth = (zoneWidth * 0.5f) / (Mathf.Tan(fovRad) * _camera.aspect);
        float distanceForDepth = (zoneDepth * 0.5f) / Mathf.Tan(fovRad);
        
        // 大きい方の距離を採用
        float distance = Mathf.Max(distanceForWidth, distanceForDepth);
        
        // カメラの高さと後ろ方向のオフセットを計算
        float cameraHeight = distance * Mathf.Sin(cameraAngleRad);
        float cameraZOffset = distance * Mathf.Cos(cameraAngleRad);
        
        // 目標位置を計算
        Vector3 newPosition = new Vector3(
            stageCenter.x,
            cameraHeight,
            stageCenter.z - cameraZOffset + zOffset
        );

        // 現在の位置と目標位置が十分に離れている場合のみアニメーションを実行
        if (Vector3.Distance(transform.position, newPosition) > 0.01f)
        {
            // 既存のトゥイーンをキャンセル
            LeanTween.cancel(gameObject);
            
            // 新しい位置へスムーズに移動
            LeanTween.move(gameObject, newPosition, moveDuration)
                .setEase(easeType);
        }
    }
}