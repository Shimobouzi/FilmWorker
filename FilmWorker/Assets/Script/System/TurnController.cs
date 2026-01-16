using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TurnController : MonoBehaviour
{
    public enum Phase
    {
        ActionIdle,
        ActionRecording,
        Edit,
    }

    [Header("References")]
    [SerializeField] PlayerController player;
    [SerializeField] InputRecorder recorder;
    [SerializeField] ReplayManager replayManager;

    [Header("Input (Player ActionMap)")]
    [SerializeField] InputActionReference actionAction;
    [SerializeField] InputActionReference cutAction;
    [SerializeField] InputActionReference stopAction;

    InputAction resolvedAction;
    InputAction resolvedCut;
    InputAction resolvedStop;

    [Header("Stop Targeting (Temporary Spec)")]
    [SerializeField] float stopRadius = 3f;

    [Header("Goal (Temporary)")]
    [SerializeField] Transform goal;
    [SerializeField] Collider2D goalCollider;
    [SerializeField] float goalRadius = 0.5f;

    BoxCollider2D playerCollider;
    Vector3 playerStartPosition;
    Quaternion playerStartRotation;

    [Header("Edit Defaults")]
    [SerializeField] float editSpeed = 1f;
    [SerializeField] bool editLoop;

    [Header("Runtime")]
    [SerializeField] Phase phase = Phase.ActionIdle;
    [SerializeField] int actionTurnCount = 1;
    [SerializeField] bool goalReached;
    [SerializeField] bool cleared;

    ReplayData pendingReplay;
    ReplayCharacter currentTarget;

    public Phase CurrentPhase => phase;
    public bool HasPendingReplay => pendingReplay != null;
    public float PendingReplayDurationSeconds => pendingReplay != null ? Mathf.Max(0f, pendingReplay.endTime) : 0f;
    public float PendingReplayStartSeconds => pendingReplay != null ? Mathf.Max(0f, pendingReplay.startTime) : 0f;
    public float PendingReplayEndSeconds => pendingReplay != null ? Mathf.Max(0f, pendingReplay.endTime) : 0f;
    public float EditSpeed => editSpeed;
    public bool EditLoop => editLoop;

    void Awake()
    {
        if (player == null) player = FindFirstObjectByType<PlayerController>();
        if (recorder == null) recorder = FindFirstObjectByType<InputRecorder>();
        if (replayManager == null) replayManager = FindFirstObjectByType<ReplayManager>();

        if (player != null)
        {
            playerCollider = player.GetComponent<BoxCollider2D>();
            playerStartPosition = player.transform.position;
            playerStartRotation = player.transform.rotation;
        }

        if (goal == null)
        {
            var goalObject = GameObject.Find("Goal");
            if (goalObject != null) goal = goalObject.transform;
        }

        if (goalCollider == null && goal != null)
            goalCollider = goal.GetComponent<Collider2D>();

        ResolveInputActions();
    }

    void Start()
    {
        // Stage 開始時に保存リプレイを全消去し、ゴーストも初期化する
        if (JsonController.instance != null)
            JsonController.instance.DeleteAllReplays();

        replayManager?.ClearSpawnedReplays();
        pendingReplay = null;
        actionTurnCount = 1;
        goalReached = false;
        cleared = false;
    }

    void OnEnable()
    {
        ResolveInputActions();
        ApplyPhase(phase);
    }

    void OnDisable()
    {
        resolvedAction?.Disable();
        resolvedCut?.Disable();
        resolvedStop?.Disable();
    }

    void Update()
    {
        if (cleared) return;

        if (resolvedAction == null || resolvedCut == null)
            return;

        switch (phase)
        {
            case Phase.ActionIdle:
                if (resolvedAction.WasPressedThisFrame())
                    StartActionRecording();
                break;

            case Phase.ActionRecording:
                if (UpdateGoalReached())
                    return;

                UpdateStopTarget();

                if (actionTurnCount >= 2 && resolvedStop != null && resolvedStop.WasPressedThisFrame())
                    ToggleStopOnTarget();

                if (resolvedCut.WasPressedThisFrame())
                    EndActionRecording();
                break;

            case Phase.Edit:
                // UI実装前の暫定：Action を押すと次の行動ターンへ（録画開始）
                if (resolvedAction.WasPressedThisFrame())
                    StartActionRecording();
                break;
        }
    }

    bool UpdateGoalReached()
    {
        if (goalReached) return false;
        if (player == null || goal == null) return false;

        if (goalCollider != null && playerCollider != null)
        {
            if (!playerCollider.Distance(goalCollider).isOverlapped) return false;

            goalReached = true;
            EndActionRecording();
            return true;
        }

        var distance = Vector2.Distance(player.transform.position, goal.position);
        if (distance > goalRadius) return false;

        goalReached = true;
        EndActionRecording();
        return true;
    }

    void ApplyPhase(Phase next)
    {
        phase = next;

        // Action/Cut は同じキー運用のため、状態によって片方だけ有効化する
        if (resolvedAction != null)
        {
            if (phase != Phase.ActionRecording) resolvedAction.Enable();
            else resolvedAction.Disable();
        }

        if (resolvedCut != null)
        {
            if (phase == Phase.ActionRecording) resolvedCut.Enable();
            else resolvedCut.Disable();
        }

        if (resolvedStop != null)
        {
            // Stop は 2度目以降の行動ターンだけ扱うが、入力自体は行動中のみ有効にしておく
            if (phase == Phase.ActionRecording) resolvedStop.Enable();
            else resolvedStop.Disable();
        }

        if (player != null)
        {
            player.enabled = (phase == Phase.ActionRecording);
        }

        if (phase != Phase.ActionRecording)
            ClearTarget();
    }

    void StartActionRecording()
    {
        if (cleared) return;

        // 行動→編集→行動：次の行動ターン開始時にスタート位置へ戻す
        if (phase == Phase.Edit)
            ResetPlayerToStart();

        // Edit → Action へ遷移するタイミングで、編集済みリプレイをゴーストとして生成
        if (phase == Phase.Edit && pendingReplay != null)
        {
            pendingReplay.speed = editSpeed;
            pendingReplay.loop = editLoop;
            replayManager?.SpawnReplay(pendingReplay, playerStartPosition);
            pendingReplay = null;
        }

        // 行動ターン開始時に、既存の全ゴーストを「最初から」再生し直す
        replayManager?.RestartAllReplays(playerStartPosition);

        recorder?.BeginRecording();
        ApplyPhase(Phase.ActionRecording);
    }

    void ResetPlayerToStart()
    {
        if (player == null) return;

        player.transform.SetPositionAndRotation(playerStartPosition, playerStartRotation);
        player.ResetMotion();
        Physics2D.SyncTransforms();
    }

    public void NotifyGoalReached(InputRecorder reachedBy)
    {
        if (cleared) return;
        if (reachedBy == null) return;
        if (recorder != null && reachedBy != recorder) return;

        goalReached = true;
    }

    public void SetEditSpeed(float speed)
    {
        editSpeed = Mathf.Clamp(speed, 0.5f, 2f);
    }

    public void SetEditLoop(bool loop)
    {
        editLoop = loop;
    }

    public void SetEditRange(float startSeconds, float endSeconds)
    {
        if (pendingReplay == null) return;

        var duration = Mathf.Max(0f, pendingReplay.endTime);
        var start = Mathf.Clamp(startSeconds, 0f, duration);
        var end = Mathf.Clamp(endSeconds, start, duration);
        pendingReplay.startTime = start;
        pendingReplay.endTime = end;
    }

    void ResolveInputActions()
    {
        // 1) 明示参照（Inspector）
        InputAction baseAction = actionAction != null ? actionAction.action : null;

        // 2) Player の HumanInputProvider から参照
        if (baseAction == null && player != null)
        {
            var human = player.GetComponent<HumanInputProvider>();
            if (human != null && human.actionAction != null)
                baseAction = human.actionAction.action;
        }

        // 3) どうしても無ければ cutAction/stopAction を起点にする
        if (baseAction == null && cutAction != null)
            baseAction = cutAction.action;
        if (baseAction == null && stopAction != null)
            baseAction = stopAction.action;

        if (baseAction == null)
        {
            resolvedAction = null;
            resolvedCut = null;
            resolvedStop = null;
            return;
        }

        var map = baseAction.actionMap;
        if (map == null)
        {
            resolvedAction = baseAction;
            resolvedCut = baseAction;
            resolvedStop = stopAction != null ? stopAction.action : null;
            return;
        }

        // 名前で解決（InputActions Editor 上の Action 名）
        resolvedAction = map.FindAction("Action", throwIfNotFound: false) ?? baseAction;
        resolvedCut = map.FindAction("Cut", throwIfNotFound: false) ?? resolvedAction;
        resolvedStop = map.FindAction("Stop", throwIfNotFound: false) ?? (stopAction != null ? stopAction.action : null);
    }

    void EndActionRecording()
    {
        if (recorder == null) return;

        var data = recorder.EndRecording();
        JsonController.instance.SaveFile(data);

        if (goalReached)
        {
            pendingReplay = null;
            ApplyPhase(Phase.ActionIdle);
            EnterClearedState();
            return;
        }

        // 編集ターン用に保持
        pendingReplay = data;
        editSpeed = Mathf.Clamp(editSpeed, 0.5f, 2f);

        // 次から「2度目の行動ターン」扱い
        actionTurnCount = Mathf.Max(2, actionTurnCount + 1);

        ApplyPhase(Phase.Edit);
    }

    void EnterClearedState()
    {
        cleared = true;

        resolvedAction?.Disable();
        resolvedCut?.Disable();
        resolvedStop?.Disable();

        if (player != null)
            player.enabled = false;

        ClearTarget();
        Debug.Log("Stage cleared (goal reached).", this);
    }

    void UpdateStopTarget()
    {
        if (player == null || replayManager == null) return;
        if (actionTurnCount < 2) return;

        var nearest = replayManager.FindNearestReplay(player.transform.position, stopRadius);
        if (nearest == currentTarget) return;

        if (currentTarget != null) currentTarget.SetHighlighted(false);
        currentTarget = nearest;
        if (currentTarget != null) currentTarget.SetHighlighted(true);
    }

    void ToggleStopOnTarget()
    {
        if (currentTarget == null) return;
        currentTarget.TogglePaused();
    }

    void ClearTarget()
    {
        if (currentTarget != null) currentTarget.SetHighlighted(false);
        currentTarget = null;
    }
}
