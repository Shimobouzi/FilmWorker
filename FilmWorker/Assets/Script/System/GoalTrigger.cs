using UnityEngine;

public sealed class GoalTrigger : MonoBehaviour
{
    TurnController turnController;

    void Awake()
    {
        turnController = FindFirstObjectByType<TurnController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player だけ反応させる（ReplayPlayer には InputRecorder が付かない想定）
        var inputRecorder = other.GetComponentInParent<InputRecorder>();
        if (inputRecorder == null) return;

        if (turnController == null)
            turnController = FindFirstObjectByType<TurnController>();

        turnController?.NotifyGoalReached(inputRecorder);
    }
}
