using UnityEngine;

public interface IDestructibleObjectView
{
    void Initialize(DestructibleObjectViewModel viewModel);
    void UpdateVisualState(DestructibleObjectState state);
    void PlayDestroyAnimation();
    void SetObjectColor(Color color);
}