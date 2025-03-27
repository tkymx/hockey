using UnityEngine;
using UnityEngine.Events;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float gameDuration = 180f; // 3分
    private float remainingTime;
    private bool isRunning;

    public UnityEvent<float> OnTimeChanged = new UnityEvent<float>();
    public UnityEvent OnTimeUp = new UnityEvent();

    private void Start()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (isRunning)
        {
            remainingTime -= Time.deltaTime;
            OnTimeChanged.Invoke(remainingTime);

            if (remainingTime <= 0)
            {
                remainingTime = 0;
                StopTimer();
                OnTimeUp.Invoke();
            }
        }
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        remainingTime = gameDuration;
        isRunning = false;
        OnTimeChanged.Invoke(remainingTime);
    }

    public float GetRemainingTime()
    {
        return remainingTime;
    }

    public bool IsTimeUp()
    {
        return remainingTime <= 0;
    }
}