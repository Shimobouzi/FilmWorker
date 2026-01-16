using System.Linq;
using UnityEngine;

public class ReplayInputProvider : IInputProvider
{
    ReplayData data;
    float timer;

    bool paused;
    bool ended;
    float start;
    float end;

    public ReplayInputProvider(ReplayData data)
    {
        this.data = data;

        var last = (data?.frames != null && data.frames.Count > 0)
            ? data.frames[data.frames.Count - 1].time
            : 0f;

        start = Mathf.Max(0f, data?.startTime ?? 0f);
        end = data?.endTime ?? 0f;
        if (end <= 0f || end < start)
            end = Mathf.Max(start, last);

        timer = start;
    }

    public void Update(float delta)
    {
        if (data == null) return;
        if (paused) return;
        if (ended) return;

        var speed = Mathf.Clamp(data.speed, 0.01f, 100f);
        timer += delta * speed;

        if (timer > end)
        {
            if (data.loop)
            {
                var length = Mathf.Max(0.0001f, end - start);
                timer = start + ((timer - start) % length);
            }
            else
            {
                timer = end;
                ended = true;
            }
        }
    }

    InputFrame GetCurrentFrame()
    {
        if (paused)
            return default;

        if (ended)
            return default;

        if (data?.frames == null || data.frames.Count == 0)
            return default;

        return data.frames.LastOrDefault(f => f.time <= timer);
    }

    public float GetHorizontal() => GetCurrentFrame().horizontal;
    public bool GetJumpDown() => GetCurrentFrame().jump;
    public bool GetActionDown() => GetCurrentFrame().action;

    public void SetPaused(bool value) => paused = value;
    public bool IsPaused => paused;
    public bool IsEnded => ended;

    public float GetSpeed()
    {
        if (data == null) return 1f;
        return Mathf.Clamp(data.speed, 0.01f, 100f);
    }

    public void SetTime(float time)
    {
        timer = Mathf.Clamp(time, start, end);
        ended = false;
    }

    public float GetTime() => timer;
    public float GetStartTime() => start;
    public float GetEndTime() => end;
}
