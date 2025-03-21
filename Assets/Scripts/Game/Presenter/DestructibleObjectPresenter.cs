using UnityEngine;

public class DestructibleObjectPresenter
{
    private DestructibleObjectUseCase objectUseCase;
    private DestructibleObjectViewModel objectViewModel;
    private IDestructibleObjectView objectView;

    public DestructibleObjectPresenter(DestructibleObjectUseCase useCase, DestructibleObjectViewModel viewModel, IDestructibleObjectView view)
    {
        objectUseCase = useCase;
        objectViewModel = viewModel;
        objectView = view;
    }

    public void Initialize(Vector2 position, ObjectType type)
    {
        // ViewModelにモデル参照を設定
        objectViewModel.SetModel(objectUseCase.GetModel());
        
        // オブジェクトの初期位置を設定
        objectUseCase.Move(position);
        
        // 初期の表示状態を更新
        UpdateObjectState();
        
        // 設定から色を取得してビューに設定
        Color objectColor = objectUseCase.GetObjectTypeConfig().color;
        objectView.SetObjectColor(objectColor);
    }

    public void HandleCollision(Collision collision)
    {
        // 衝突処理のロジック
        objectUseCase.HandleCollision(collision);
        UpdateObjectState();
    }

    public void UpdateObjectState()
    {
        objectViewModel.UpdateDisplayData();
        
        // オブジェクトの状態に応じてビューを更新
        objectView.UpdateVisualState(objectViewModel.state);
        
        // 破壊されている場合はエフェクト再生
        if (objectViewModel.state == DestructibleObjectState.Destroyed)
        {
            objectView.PlayDestroyAnimation();
        }
    }
}