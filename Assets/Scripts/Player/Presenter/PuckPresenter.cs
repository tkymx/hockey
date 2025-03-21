using UnityEngine;

public class PuckPresenter
{
    private IPuckUseCase puckUseCase;
    private PuckViewModel puckViewModel;
    private IPuckView puckView;
    private int lastGrowthLevel;

    public PuckPresenter(IPuckUseCase useCase, PuckViewModel viewModel, IPuckView view)
    {
        puckUseCase = useCase;
        puckViewModel = viewModel;
        puckView = view;
        lastGrowthLevel = 1;
    }

    public void HandleInput(InputData input)
    {
        if (input.isPressing)
        {
            // 入力方向と強さをUseCaseに渡してコマを移動
            puckUseCase.Move(input.direction, input.strength * 10f);
            UpdatePuckState();
        }
    }

    public void HandleCollision(Collision collision)
    {
        // 衝突をUseCaseに通知
        puckUseCase.HandleCollision(collision);
        UpdatePuckState();
        
        // 成長レベルが変化した場合はエフェクトを再生
        if (puckViewModel.growthLevel > lastGrowthLevel)
        {
            puckView.PlayGrowthEffect();
            lastGrowthLevel = puckViewModel.growthLevel;
        }
    }

    public void UpdatePuckState()
    {
        puckViewModel.UpdateDisplayData();
        puckView.UpdatePosition();
        puckView.UpdateSize();
    }
}