using System;

[Serializable]
public class FocusSnapshot
{
    public int roundIndex;
    public float successRate;
    public float avgReactionTime;
    public float targetLifeTime;
    public int streakSuccess;
    public int streakFail;
    public bool lastSuccess;
}
