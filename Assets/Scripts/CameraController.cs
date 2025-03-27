using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float cameraAngle = 45f; // カメラの見下ろし角度（デフォルト45度）
    [SerializeField] private float zOffset = 0f; // ステージに対するZ方向のオフセット（+で後ろ側、-で前側）
    
    private Camera _camera;
    
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
        
        // ステージの寸法を取得
        float stageWidth = stageBounds.x;
        float stageDepth = stageBounds.z;
        
        // 画面のアスペクト比
        float screenAspect = (float)Screen.width / Screen.height;
        
        // カメラ角度（ラジアン）
        float cameraAngleRad = cameraAngle * Mathf.Deg2Rad;
        
        // カメラ位置とサイズの計算
        float cameraHeight, cameraZ;
        
        if (_camera.orthographic)
        {
            // 正投影カメラの場合
            _camera.orthographicSize = stageWidth * 0.5f / screenAspect;
            
            // 単純に高さと位置を計算
            cameraHeight = stageDepth;
            cameraZ = stageCenter.z - stageDepth * 0.5f + zOffset;
        }
        else
        {
            // 透視投影カメラの場合
            // ステージ全体が視野に入るための最小距離を計算
            float fovRad = _camera.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float distanceForDepth = stageDepth * 0.5f / Mathf.Tan(fovRad);
            
            // 角度を考慮した位置を計算
            cameraHeight = distanceForDepth * Mathf.Sin(cameraAngleRad);
            cameraZ = stageCenter.z - distanceForDepth * Mathf.Cos(cameraAngleRad) + zOffset;
            
            // 横幅も視野に入ることを確認
            float distanceToCenter = Vector3.Distance(
                new Vector3(stageCenter.x, 0, stageCenter.z),
                new Vector3(stageCenter.x, cameraHeight, cameraZ)
            );
            
            float requiredFOV = 2.0f * Mathf.Atan(stageWidth * 0.5f / (distanceToCenter * screenAspect)) * Mathf.Rad2Deg;
            _camera.fieldOfView = Mathf.Max(_camera.fieldOfView, requiredFOV);
        }
        
        // カメラ位置の設定
        transform.position = new Vector3(
            stageCenter.x,
            cameraHeight,
            cameraZ
        );
    }
}