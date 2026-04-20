using System.Diagnostics;

namespace Rabota;

class DelayedAction
{
    public Action Action { get; set; }
    public double ExecTime { get; set; }

    public DelayedAction(Action _action, double _execTime)
    {
        Action = _action;
        ExecTime = _execTime;
    }
}

static class TimerManager
{
    static List<DelayedAction> _actions = new List<DelayedAction>();
    static Stopwatch _sw = Stopwatch.StartNew();

    public static void Schedule(Action action, double delaySeconds)
    {
        _actions.Add(
            new DelayedAction(
                action,
                _sw.Elapsed.TotalSeconds + delaySeconds
            )
        );
    }

    public static void Update()
    {
        double now = _sw.Elapsed.TotalSeconds;
        for(int i = _actions.Count - 1; i >= 0; i--)
        {
            if(now >= _actions[i].ExecTime)
            {
                _actions[i].Action?.Invoke();
                _actions.RemoveAt(i);
            }
        }
    }
}