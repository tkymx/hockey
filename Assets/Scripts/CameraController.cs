using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float cameraAngleX = 45f; // 垂直方向の角度（見下ろし角度）
    [SerializeField] private float cameraAngleY = 0f;  // 水平方向の角度（回転）
    [SerializeField] private float viewMargin = 1.1f;  // 視界に余裕を持たせる係数（10%のマージン）
    [SerializeField] private float moveSpeed = 5.0f;   // カメラ移動の速度
    [SerializeField] private float rotationSpeed = 3.0f; // カメラ回転の速度

    private Vector3 targetPosition;    // 目標位置
    private Quaternion targetRotation; // 目標回転

    private void Start()
    {
        // 初期化時にターゲットカメラがなければMainCameraを取得
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // 初期位置と回転を現在の値に設定
        if (targetCamera != null)
        {
            targetPosition = targetCamera.transform.position;
            targetRotation = targetCamera.transform.rotation;
        }
    }

    private void LateUpdate()
    {
        // カメラがない場合は何もしない
        if (targetCamera == null) return;

        // 現在の位置から目標位置へスムーズに移動
        targetCamera.transform.position = Vector3.Lerp(
            targetCamera.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 現在の回転から目標回転へスムーズに回転
        targetCamera.transform.rotation = Quaternion.Slerp(
            targetCamera.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void UpdateCameraPosition(Vector3 stageCenter, Vector3 stageBounds)
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("カメラが見つかりません。インスペクタで設定するか、Main Cameraタグを設定してください。");
                return;
            }
        }

        // カメラの回転を計算（X軸とY軸の回転を適用）
        targetRotation = Quaternion.Euler(cameraAngleX, cameraAngleY, 0);

        // カメラの視野角とアスペクト比を取得
        float verticalFOV = targetCamera.fieldOfView;
        float horizontalFOV = Camera.VerticalToHorizontalFieldOfView(verticalFOV, targetCamera.aspect);

        // ステージの寸法を考慮して必要な距離を計算
        // X, Y, Z各方向の最大値を考慮
        float distanceForWidth = stageBounds.x * viewMargin / (2 * Mathf.Tan(horizontalFOV * 0.5f * Mathf.Deg2Rad));
        float distanceForHeight = stageBounds.y * viewMargin / (2 * Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad));
        float distanceForDepth = stageBounds.z * viewMargin / (2 * Mathf.Tan(verticalFOV * 0.5f * Mathf.Deg2Rad));

        // 必要な最大距離を計算
        float distance = Mathf.Max(distanceForWidth, distanceForHeight, distanceForDepth);

        // 角度を考慮してカメラ位置を計算
        float radAngleX = cameraAngleX * Mathf.Deg2Rad;
        float radAngleY = cameraAngleY * Mathf.Deg2Rad;

        // カメラの位置を計算
        Vector3 cameraOffset = new Vector3(
            distance * Mathf.Sin(radAngleY), 
            distance * Mathf.Sin(radAngleX), 
            -distance * Mathf.Cos(radAngleX) * Mathf.Cos(radAngleY)
        );

        // ステージの中心からオフセットを適用して目標位置を設定
        targetPosition = stageCenter + cameraOffset;

        // LookAtの代わりに目標回転を計算
        Vector3 direction = stageCenter - targetPosition;
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }

        // デバッグログ
        Debug.Log($"Target Position: {targetPosition}, Target Rotation: {targetRotation.eulerAngles}");
        Debug.Log($"Stage Center: {stageCenter}, Stage Bounds: {stageBounds}, Distance: {distance}");
    }
}