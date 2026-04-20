namespace Rabota;

class TweenEase
{
    public static float Linear(float t)
    {
        return t;
    }

    public static float SmoothStep(float t)
    {
        return t * t * (3 - 2*t);
    }

    public static float Quad(float t)
    {
        return t < 0.5f 
            ? 2*t*t 
            : 1f-2f*(1f-t)*(1f-t);
    }
}

class Tween
{
    public float StartValue { get; set; }
    public float EndValue { get; set; }
    public float Duration { get; set; }
    public float Elapsed { get; private set; }
    public bool IsPlaying { get; private set; }
    public Action<float> OnUpdate { get; set; }
    public Action OnComplete { get; set; }
    public Func<float, float> Easing { get; set; }

    private float _curValue;

    public Tween(float start, float end, float duration, Action<float> onUpdate = null)
    {
        StartValue = start;
        EndValue = end;
        Duration = duration;

        OnUpdate = onUpdate;
        
        IsPlaying = true;
        Elapsed = 0f;
        _curValue = start;
    }

    public void Update()
    {
        if (!IsPlaying) return;

        Elapsed += Jarvis.GetFrameTime();
        float t = Math.Clamp(Elapsed / Duration, 0f, 1f);

        float easedT = Easing != null ? Easing(t) : t;

        _curValue = StartValue + (EndValue - StartValue) * easedT;
        OnUpdate?.Invoke(_curValue);

        if(t >= 1f)
        {
            IsPlaying = false;
            OnComplete?.Invoke();
        }
    }

    public void Stop()
    {
        IsPlaying = false;
    }

    public void Start()
    {
        IsPlaying = true;
    }
}

class TweenManager
{
    private static List<Tween> _activeTweens = new List<Tween>();

    public static void Update()
    {
        for(int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            var tween = _activeTweens[i];

            tween.Update();

            if(!tween.IsPlaying)
            {
                _activeTweens.RemoveAt(i);
            }
        }
    }

    public static Tween FromTo(float from, float to, float duration, Action<float> onUpdate, Func<float, float> easing = null)
    {
        var tween = new Tween(from, to, duration, onUpdate);
        tween.Easing = easing ?? TweenEase.Linear;
        _activeTweens.Add(tween);
        return tween;
    }

    public static void StopAll()
    {
        _activeTweens.Clear();
    }
}