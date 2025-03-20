using UnityEngine;
using Hockey.Core;

namespace Hockey.Player
{
    public class PuckController : MonoBehaviour, IInputHandler
    {
        [Header("Input Settings")]
        [SerializeField] private float minDragDistance = 0.1f;
        [SerializeField] private float maxDragDistance = 2.0f;
        [SerializeField] private float dragForceMultiplier = 1.0f;

        [Header("Visual Feedback")]
        [SerializeField] private LineRenderer dragIndicator;
        [SerializeField] private GameObject selectionIndicator;

        private PlayerPuck puck;
        private Camera mainCamera;
        private Vector2 dragStartPosition;
        private bool isDragging = false;

        // イベント
        public event System.Action<Vector2> OnDragStart;
        public event System.Action<Vector2> OnDragEnd;

        private void Awake()
        {
            puck = GetComponent<PlayerPuck>();
            mainCamera = Camera.main;
            
            // 視覚的フィードバックの初期設定
            if (dragIndicator != null)
            {
                dragIndicator.positionCount = 2;
                dragIndicator.enabled = false;
            }
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            // タッチ入力の取得
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchBegan(touch);
                        break;
                    case TouchPhase.Moved:
                        HandleTouchMoved(touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandleTouchEnded(touch);
                        break;
                }
            }
            // マウス入力（デバッグ用）
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    HandleMouseDown();
                }
                else if (Input.GetMouseButton(0))
                {
                    HandleMouseDrag();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    HandleMouseUp();
                }
            }
        }

        private void HandleTouchBegan(Touch touch)
        {
            if (IsUITouched()) return;

            dragStartPosition = touch.position;
            StartDrag(dragStartPosition);
        }

        private void HandleTouchMoved(Touch touch)
        {
            if (!isDragging) return;
            UpdateDrag(touch.position);
        }

        private void HandleTouchEnded(Touch touch)
        {
            if (!isDragging) return;
            EndDrag(touch.position);
        }

        private void HandleMouseDown()
        {
            if (IsUITouched()) return;

            dragStartPosition = Input.mousePosition;
            StartDrag(dragStartPosition);
        }

        private void HandleMouseDrag()
        {
            if (!isDragging) return;
            UpdateDrag(Input.mousePosition);
        }

        private void HandleMouseUp()
        {
            if (!isDragging) return;
            EndDrag(Input.mousePosition);
        }

        private void StartDrag(Vector2 position)
        {
            isDragging = true;
            dragStartPosition = position;
            OnDragStart?.Invoke(position);

            // 視覚的フィードバックの表示
            if (dragIndicator != null)
            {
                dragIndicator.enabled = true;
                Vector3 worldPos = GetWorldPosition(position);
                dragIndicator.SetPosition(0, worldPos);
                dragIndicator.SetPosition(1, worldPos);
            }
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(true);
                selectionIndicator.transform.position = transform.position;
            }
        }

        private void UpdateDrag(Vector2 currentPosition)
        {
            if (!isDragging) return;

            // ドラッグ方向の表示更新
            if (dragIndicator != null)
            {
                Vector3 startWorld = GetWorldPosition(dragStartPosition);
                Vector3 currentWorld = GetWorldPosition(currentPosition);
                dragIndicator.SetPosition(0, startWorld);
                dragIndicator.SetPosition(1, currentWorld);
            }
        }

        private void EndDrag(Vector2 endPosition)
        {
            if (!isDragging) return;

            // ドラッグ距離と方向の計算
            Vector2 dragVector = endPosition - dragStartPosition;
            float dragDistance = Mathf.Clamp(dragVector.magnitude, 0, maxDragDistance);

            if (dragDistance > minDragDistance)
            {
                // 正規化された方向と力の計算
                Vector2 direction = dragVector.normalized;
                float force = (dragDistance / maxDragDistance) * dragForceMultiplier;

                // コマに力を適用
                puck.Move(direction, force);
            }

            // 視覚的フィードバックの非表示
            if (dragIndicator != null)
            {
                dragIndicator.enabled = false;
            }
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(false);
            }

            isDragging = false;
            OnDragEnd?.Invoke(endPosition);
        }

        private Vector3 GetWorldPosition(Vector2 screenPosition)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        #region IInputHandler Implementation
        public Vector2 GetTouchPosition()
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).position;
            }
            return Input.mousePosition;
        }

        public Vector2 GetDragDirection()
        {
            if (!isDragging) return Vector2.zero;
            return (GetTouchPosition() - dragStartPosition).normalized;
        }

        public float GetDragMagnitude()
        {
            if (!isDragging) return 0f;
            return Mathf.Clamp(Vector2.Distance(GetTouchPosition(), dragStartPosition) / maxDragDistance, 0f, 1f);
        }

        public bool IsUITouched()
        {
            // UI上のタッチを検出（実際のUI実装に応じて調整）
            if (UnityEngine.EventSystems.EventSystem.current == null) return false;
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
        }
        #endregion
    }
}