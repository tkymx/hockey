public class GamePresenter
{
    private IGameUseCase gameUseCase;
    private GameViewModel gameViewModel;
    private IGameView gameView;

    public GamePresenter(IGameUseCase useCase, GameViewModel viewModel, IGameView view)
    {
        gameUseCase = useCase;
        gameViewModel = viewModel;
        gameView = view;
    }

    public void HandleGameStart()
    {
        gameUseCase.StartGame();
        UpdateGameState();
    }

    public void HandleGamePause()
    {
        gameUseCase.PauseGame();
        UpdateGameState();
    }

    public void UpdateGameState()
    {
        gameViewModel.UpdateDisplayData();
        gameView.UpdateUI();
    }
}