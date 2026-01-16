using UnityEngine;

public class ReplayCharacter : MonoBehaviour
{
    PlayerController controller;
    ReplayInputProvider replayInput;

    Animator[] animators;
    float[] animatorBaseSpeeds;
    float cachedPlaybackSpeed = 1f;

    MeshOutlineHighlighter outlineHighlighter;

    public int SpawnIndex { get; private set; }

    public bool IsPaused => replayInput != null && replayInput.IsPaused;

    public void ResetToStart()
    {
        if (replayInput != null)
            replayInput.SetTime(replayInput.GetStartTime());

        if (controller != null)
            controller.ResetMotion();
    }

    public void Initialize(ReplayData data)
    {
        Initialize(data, 0);
    }

    public void Initialize(ReplayData data, int spawnIndex)
    {
        SpawnIndex = spawnIndex;

        controller = GetComponent<PlayerController>();
        replayInput = new ReplayInputProvider(data);
        animators = GetComponentsInChildren<Animator>(true);
        CacheAnimatorBaseSpeeds();
        cachedPlaybackSpeed = replayInput != null ? replayInput.GetSpeed() : 1f;

        outlineHighlighter = GetComponent<MeshOutlineHighlighter>();
        if (outlineHighlighter == null)
            outlineHighlighter = gameObject.AddComponent<MeshOutlineHighlighter>();

        if (controller != null)
            controller.SetInput(replayInput);

        SetHighlighted(false);
        SetPaused(false);
    }

    void Update()
    {
        replayInput?.Update(Time.deltaTime);

        if (replayInput != null && replayInput.IsEnded)
        {
            Destroy(gameObject);
            return;
        }

        if (!IsPaused)
            UpdateAnimatorSpeedIfNeeded();
    }

    public void TogglePaused()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool value)
    {
        replayInput?.SetPaused(value);

        if (controller != null)
        {
            controller.enabled = !value;
            if (value)
                controller.ResetMotion();
        }

        ApplyAnimatorSpeed(value);
    }

    public void SetHighlighted(bool value)
    {
        outlineHighlighter?.SetHighlighted(value);
    }

    void CacheAnimatorBaseSpeeds()
    {
        if (animators == null || animators.Length == 0)
        {
            animatorBaseSpeeds = null;
            return;
        }

        animatorBaseSpeeds = new float[animators.Length];
        for (int i = 0; i < animators.Length; i++)
        {
            var animator = animators[i];
            animatorBaseSpeeds[i] = animator != null ? animator.speed : 1f;
        }
    }

    void UpdateAnimatorSpeedIfNeeded()
    {
        if (replayInput == null) return;

        var speed = replayInput.GetSpeed();
        if (Mathf.Approximately(speed, cachedPlaybackSpeed)) return;

        cachedPlaybackSpeed = speed;
        ApplyAnimatorSpeed(paused: false);
    }

    void ApplyAnimatorSpeed(bool paused)
    {
        if (animators == null || animators.Length == 0) return;

        if (paused)
        {
            foreach (var animator in animators)
            {
                if (animator == null) continue;
                animator.speed = 0f;
            }

            return;
        }

        var playbackSpeed = replayInput != null ? replayInput.GetSpeed() : 1f;
        cachedPlaybackSpeed = playbackSpeed;

        for (int i = 0; i < animators.Length; i++)
        {
            var animator = animators[i];
            if (animator == null) continue;

            var baseSpeed = (animatorBaseSpeeds != null && i < animatorBaseSpeeds.Length)
                ? animatorBaseSpeeds[i]
                : 1f;

            animator.speed = baseSpeed * playbackSpeed;
        }
    }
}
