namespace Hockey.Core
{
    // ゲームの状態を表す列挙型
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    // オブジェクトの種類を表す列挙型
    public enum ObjectType
    {
        Rock,
        Wood,
        Plant,
        Building,
        Metal
    }

    // オブジェクトのサイズを表す列挙型
    public enum Size
    {
        Small,
        Medium,
        Large
    }

    // エフェクトの種類を表す列挙型
    public enum EffectType
    {
        Collision,
        Destruction,
        Growth,
        Trail,
        Score
    }

    // サウンドの種類を表す列挙型
    public enum SoundType
    {
        PuckHit,
        ObjectDestroy,
        Growth,
        BGM,
        UIEffect
    }

    // 破壊可能オブジェクトのインターフェース
    public interface IDestructible
    {
        float Durability { get; }
        int PointValue { get; }
        void TakeDamage(float damage);
        void OnDestroy();
    }

    // スコア計算のインターフェース
    public interface IScoreCalculator
    {
        int CalculateScore(ObjectType type, Size size, float impactForce);
        int CalculateComboBonus(int comboCount);
    }

    // 成長システムのインターフェース
    public interface IGrowthSystem
    {
        int CurrentLevel { get; }
        float CurrentGrowthMultiplier { get; }
        bool CheckGrowthCondition(int currentScore);
        void ApplyGrowth(GameObject target);
    }

    // エフェクト再生のインターフェース
    public interface IEffectPlayer
    {
        void PlayEffect(EffectType type, Vector3 position, float scale = 1.0f);
        void StopEffect(EffectType type);
        void SetEffectScale(EffectType type, float scale);
    }

    // サウンド再生のインターフェース
    public interface ISoundPlayer
    {
        void PlaySound(SoundType type, Vector3 position = default);
        void StopSound(SoundType type);
        void SetVolume(SoundType type, float volume);
    }

    // 入力処理のインターフェース
    public interface IInputHandler
    {
        Vector2 GetTouchPosition();
        Vector2 GetDragDirection();
        float GetDragMagnitude();
        bool IsUITouched();
        event System.Action<Vector2> OnDragStart;
        event System.Action<Vector2> OnDragEnd;
    }
}