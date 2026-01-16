using System;
using System.Collections.Generic;

[Serializable]
public class ReplayData
{
    public List<InputFrame> frames;

    public float startTime;
    public float endTime;
    public float speed = 1f;

    public bool loop;
}
