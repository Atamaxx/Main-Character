using UnityEngine;

public class Ticker : MonoBehaviour
{
    public static float TickTime;
    public static float TickTime_075;

    [RuntimeInitializeOnLoadMethod]
    private static void Initialize()
    {
        TickTime = 0.2f;
        TickTime_075 = 0.75f;
    }

    private float _tickerTimer;
    private float _tickerTimer_075;
    public delegate void TickAction();
    public static event TickAction OnTickAction;

    public delegate void Tick075Action();
    public static event Tick075Action OnTick075Action;

    private void Update()
    {
        _tickerTimer += Time.deltaTime;
        if (_tickerTimer >= TickTime)
        {
            _tickerTimer = 0f;
            TickEvent();
        }

        if (_tickerTimer_075 >= TickTime)
        {
            _tickerTimer_075 = 0f;
            Tick075Event();
        }
    }

    private void TickEvent()
    {
        OnTickAction?.Invoke();
    }

    private void Tick075Event()
    {
        OnTick075Action?.Invoke();
    }
}
