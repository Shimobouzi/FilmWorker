using System.Linq;

public class ReplayInputProvider : IInputProvider
{
    ReplayData data;
    float timer;

    public ReplayInputProvider(ReplayData data)
    {
        this.data = data;
    }

    public void Update(float delta)
    {
        timer += delta * data.speed;
    }

    InputFrame GetCurrentFrame()
    {
        return data.frames
            .LastOrDefault(f => f.time <= timer);
    }

    public float GetHorizontal() => GetCurrentFrame().horizontal;
    public bool GetJumpDown() => GetCurrentFrame().jump;
    public bool GetActionDown() => GetCurrentFrame().action;
}
