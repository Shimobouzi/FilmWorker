using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 編集ターン(Edit)用の簡易UIをランタイム生成する。
/// - Speed: 0.5〜2.0
/// - Loop: on/off
/// - Range: Start/End 秒
/// </summary>
public sealed class EditUIRuntime : MonoBehaviour
{
    TurnController turn;

    GameObject root;
    Slider speedSlider;
    Text speedLabel;

    Toggle loopToggle;

    Slider startSlider;
    Text startLabel;

    Slider endSlider;
    Text endLabel;

    Text hintLabel;

    bool wasActive;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindFirstObjectByType<EditUIRuntime>() != null) return;

        var go = new GameObject("EditUIRuntime");
        DontDestroyOnLoad(go);
        go.AddComponent<EditUIRuntime>();
    }

    void Awake()
    {
        turn = FindFirstObjectByType<TurnController>();
        BuildUI();
    }

    void Update()
    {
        if (turn == null)
            turn = FindFirstObjectByType<TurnController>();

        bool active = turn != null && turn.CurrentPhase == TurnController.Phase.Edit && turn.HasPendingReplay;
        if (root != null && root.activeSelf != active)
            root.SetActive(active);

        if (!active)
        {
            wasActive = false;
            return;
        }

        if (!wasActive)
        {
            SyncFromTurnController();
            wasActive = true;
        }

        var duration = Mathf.Max(0f, turn.PendingReplayDurationSeconds);
        if (duration <= 0f) duration = 0.0001f;

        // Slider range
        startSlider.maxValue = duration;
        endSlider.maxValue = duration;

        // Clamp relationship
        if (startSlider.value > endSlider.value)
            endSlider.value = startSlider.value;

        HandleHotkeys(duration);

        // Labels
        speedLabel.text = $"Speed: {speedSlider.value:0.00}x";
        startLabel.text = $"Start: {startSlider.value:0.00}s";
        endLabel.text = $"End: {endSlider.value:0.00}s";
        hintLabel.text = "Edit: W/S=Speed  A/D=Start  J/L=End  Space=Loop  Action=次へ";

        // Apply to controller
        turn.SetEditSpeed(speedSlider.value);
        turn.SetEditLoop(loopToggle.isOn);
        turn.SetEditRange(startSlider.value, endSlider.value);
    }

    void BuildUI()
    {
        if (root != null) return;

        var canvasGo = new GameObject("EditUI_Canvas");
        canvasGo.transform.SetParent(transform, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        root = new GameObject("EditUI_Root");
        root.transform.SetParent(canvasGo.transform, false);
        root.SetActive(false);

        var panel = new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        var image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);

        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(10f, -10f);
        rt.sizeDelta = new Vector2(360f, 220f);

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 6f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        speedLabel = CreateLabel(panel.transform, "Speed: 1.00x");
        speedSlider = CreateSlider(panel.transform, 0.5f, 2f, 1f);
        speedSlider.interactable = false;

        loopToggle = CreateToggle(panel.transform, "Loop", false);
        loopToggle.interactable = false;

        startLabel = CreateLabel(panel.transform, "Start: 0.00s");
        startSlider = CreateSlider(panel.transform, 0f, 1f, 0f);
        startSlider.interactable = false;

        endLabel = CreateLabel(panel.transform, "End: 0.00s");
        endSlider = CreateSlider(panel.transform, 0f, 1f, 0f);
        endSlider.interactable = false;

        hintLabel = CreateLabel(panel.transform, "");
        hintLabel.color = new Color(1f, 1f, 1f, 0.9f);

        // Defaults
        speedSlider.onValueChanged.AddListener(_ => { });
        loopToggle.onValueChanged.AddListener(_ => { });
        startSlider.onValueChanged.AddListener(_ => { });
        endSlider.onValueChanged.AddListener(_ => { });
    }

    void SyncFromTurnController()
    {
        if (turn == null) return;

        var duration = Mathf.Max(0.0001f, turn.PendingReplayDurationSeconds);

        speedSlider.value = Mathf.Clamp(turn.EditSpeed, speedSlider.minValue, speedSlider.maxValue);
        loopToggle.isOn = turn.EditLoop;

        startSlider.maxValue = duration;
        endSlider.maxValue = duration;

        startSlider.value = Mathf.Clamp(turn.PendingReplayStartSeconds, 0f, duration);
        var end = turn.PendingReplayEndSeconds;
        if (end <= 0f) end = duration;
        endSlider.value = Mathf.Clamp(end, startSlider.value, duration);
    }

    void HandleHotkeys(float duration)
    {
        float dt = Time.unscaledDeltaTime;

        float speedStep = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ? 0.5f : 0.05f;
        float timeStep = (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ? 1.0f : 0.1f;

        // Speed: W/S
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame) speedSlider.value += speedStep;
            if (Keyboard.current.sKey.wasPressedThisFrame) speedSlider.value -= speedStep;

            // Start: A/D
            if (Keyboard.current.aKey.wasPressedThisFrame) startSlider.value -= timeStep;
            if (Keyboard.current.dKey.wasPressedThisFrame) startSlider.value += timeStep;

            // End: J/L
            if (Keyboard.current.jKey.wasPressedThisFrame) endSlider.value -= timeStep;
            if (Keyboard.current.lKey.wasPressedThisFrame) endSlider.value += timeStep;

            // Loop: Space
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                loopToggle.isOn = !loopToggle.isOn;
        }

        // Gamepad fallback
        if (Gamepad.current != null)
        {
            // Speed: dpad up/down
            if (Gamepad.current.dpad.up.wasPressedThisFrame) speedSlider.value += speedStep;
            if (Gamepad.current.dpad.down.wasPressedThisFrame) speedSlider.value -= speedStep;

            // Start: dpad left/right
            if (Gamepad.current.dpad.left.wasPressedThisFrame) startSlider.value -= timeStep;
            if (Gamepad.current.dpad.right.wasPressedThisFrame) startSlider.value += timeStep;

            // End: shoulder left/right
            if (Gamepad.current.leftShoulder.wasPressedThisFrame) endSlider.value -= timeStep;
            if (Gamepad.current.rightShoulder.wasPressedThisFrame) endSlider.value += timeStep;

            // Loop: south button
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                loopToggle.isOn = !loopToggle.isOn;
        }

        // Clamp
        speedSlider.value = Mathf.Clamp(speedSlider.value, speedSlider.minValue, speedSlider.maxValue);
        startSlider.value = Mathf.Clamp(startSlider.value, 0f, duration);
        endSlider.value = Mathf.Clamp(endSlider.value, startSlider.value, duration);
    }

    static Text CreateLabel(Transform parent, string text)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);

        var t = go.AddComponent<Text>();
        t.text = text;
        t.font = GetBuiltinFontSafe();
        t.fontSize = 18;
        t.color = Color.white;

        var rt = t.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 24f);

        return t;
    }

    static Font GetBuiltinFontSafe()
    {
        // Unity 6: Arial.ttf は無効。LegacyRuntime.ttf を使う。
        Font font = null;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font != null) return font;

        // 古い環境向けフォールバック
        try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        return font;
    }

    static Slider CreateSlider(Transform parent, float min, float max, float value)
    {
        var go = new GameObject("Slider");
        go.transform.SetParent(parent, false);

        var slider = go.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        // Background
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bg = bgGo.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.15f);

        // Fill
        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(fillArea.transform, false);
        var fill = fillGo.AddComponent<Image>();
        fill.color = new Color(1f, 0.9f, 0.2f, 0.9f);

        // Handle
        var handleGo = new GameObject("Handle");
        handleGo.transform.SetParent(go.transform, false);
        var handle = handleGo.AddComponent<Image>();
        handle.color = new Color(1f, 1f, 1f, 0.9f);

        slider.targetGraphic = handle;
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();

        // RectTransforms
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 20f);

        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;

        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRt.offsetMin = new Vector2(10f, 0f);
        fillAreaRt.offsetMax = new Vector2(-10f, 0f);

        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(1f, 1f);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;

        var handleRt = handle.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(12f, 24f);

        return slider;
    }

    static Toggle CreateToggle(Transform parent, string labelText, bool initial)
    {
        var go = new GameObject("Toggle");
        go.transform.SetParent(parent, false);

        var toggle = go.AddComponent<Toggle>();

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(go.transform, false);
        var bg = bgGo.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0.15f);

        var checkGo = new GameObject("Checkmark");
        checkGo.transform.SetParent(bgGo.transform, false);
        var check = checkGo.AddComponent<Image>();
        check.color = new Color(1f, 0.9f, 0.2f, 0.9f);

        var label = CreateLabel(go.transform, labelText);
        label.alignment = TextAnchor.MiddleLeft;

        toggle.targetGraphic = bg;
        toggle.graphic = check;
        toggle.isOn = initial;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0f, 24f);

        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.1f);
        bgRt.anchorMax = new Vector2(0f, 0.9f);
        bgRt.pivot = new Vector2(0f, 0.5f);
        bgRt.sizeDelta = new Vector2(20f, 20f);
        bgRt.anchoredPosition = new Vector2(0f, 0f);

        var checkRt = check.GetComponent<RectTransform>();
        checkRt.anchorMin = new Vector2(0.5f, 0.5f);
        checkRt.anchorMax = new Vector2(0.5f, 0.5f);
        checkRt.pivot = new Vector2(0.5f, 0.5f);
        checkRt.sizeDelta = new Vector2(12f, 12f);
        checkRt.anchoredPosition = Vector2.zero;

        var labelRt = label.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.offsetMin = new Vector2(26f, 0f);
        labelRt.offsetMax = new Vector2(0f, 0f);

        return toggle;
    }
}
