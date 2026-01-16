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

    void Start()
    {
        // HumanInputProvider ‚ðŽ©“®‚ÅƒZƒbƒg
        SetInput(GetComponent<IInputProvider>());
    }

    public void SetInput(IInputProvider provider)
    {
        input = provider;
    }
    void Update()
    {
        Record(input);
    }

    public void Record(IInputProvider input)
    {
        timer += Time.deltaTime;

        frames.Add(new InputFrame
        {
            time = timer,
            horizontal = input.GetHorizontal(),
            jump = input.GetJumpDown(),
            action = input.GetActionDown()
        });
    }

    public ReplayData CreateReplay()
    {
        return new ReplayData
        {
            frames = new List<InputFrame>(frames),
            startTime = 0,
            endTime = timer,
            speed = 1f
        };
    }
    public void SaveCurrentReplay()
    {
        ReplayData data = CreateReplay();
        JsonController.instance.SaveFile(data);
    }
}

