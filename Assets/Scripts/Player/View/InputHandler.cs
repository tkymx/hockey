using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Vector2 touchPosition;
    private Vector2 touchStartPosition;
    private bool isTouching = false;
    
    private PuckPresenter puckPresenter;
    
    public void Initialize(PuckPresenter presenter)
    {
        puckPresenter = presenter;
    }
    
    public void OnPointerDown(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            touchStartPosition = GetTouchPosition(context);
            touchPosition = touchStartPosition;
            isTouching = true;
        }
    }
    
    public void OnPointerMove(InputAction.CallbackContext context)
    {
        if (isTouching && context.performed)
        {
            touchPosition = GetTouchPosition(context);
        }
    }
    
    public void OnPointerUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isTouching = false;
        }
    }
    
    private void Update()
    {
        if (isTouching && puckPresenter != null)
        {
            // 入力方向と強さを計算
            Vector2 direction = (touchPosition - touchStartPosition).normalized;
            float strength = Mathf.Clamp((touchPosition - touchStartPosition).magnitude / 100f, 0f, 1f);
            
            // 入力データを作成してPresenterに渡す
            InputData inputData = new InputData(direction, strength, isTouching);
            puckPresenter.HandleInput(inputData);
        }
    }
    
    private Vector2 GetTouchPosition(InputAction.CallbackContext context)
    {
        Vector2 screenPosition = context.ReadValue<Vector2>();
        return Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
    }
}