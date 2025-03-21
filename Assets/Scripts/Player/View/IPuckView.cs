using UnityEngine;

public interface IPuckView
{
    void Initialize(PuckViewModel viewModel);
    void UpdatePosition();
    void UpdateSize();
    void PlayGrowthEffect();
}