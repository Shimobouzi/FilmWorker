using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InputFrame
{
    public float time;
    public float horizontal;
    public bool jump;
    public bool action;
}
public class InputRecorder : MonoBehaviour
{
    public List<InputFrame> frames = new();

    float timer;
    IInputProvider input;
    bool isRecording;

    void Start()
    {
        SetInput(GetComponent<IInputProvider>());
    }

    public void SetInput(IInputProvider provider)
    {
        input = provider;
    }

    void Update()
    {
        if (!isRecording) return;
        if (input == null) return;

        Record(input);
    }

    void Record(IInputProvider current)
    {
        timer += Time.deltaTime;

        frames.Add(new InputFrame
        {
            time = timer,
            horizontal = current.GetHorizontal(),
            jump = current.GetJumpDown(),
            action = false,
        });
    }

    public void BeginRecording()
    {
        frames.Clear();
        timer = 0f;
        isRecording = true;
    }

    public ReplayData EndRecording()
    {
        isRecording = false;

        return new ReplayData
        {
            frames = new List<InputFrame>(frames),
            startTime = 0f,
            endTime = timer,
            speed = 1f,
            loop = false,
        };
    }

    public int EndRecordingAndSave()
    {
        var data = EndRecording();

        var nextId = JsonController.instance.GetFileCount();
        JsonController.instance.SaveFile(data);
        return nextId;
    }

    public bool IsRecording => isRecording;
    public float GetDurationSeconds() => timer;
}

