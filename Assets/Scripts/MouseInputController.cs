using UnityEngine;

public class MouseInputController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private Plane groundPlane;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        // XZ平面を作成（Y=0の平面）
        groundPlane = new Plane(Vector3.up, Vector3.zero);
    }

    public Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        float enter;

        if (groundPlane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter);
        }

        return Vector3.zero;
    }
}