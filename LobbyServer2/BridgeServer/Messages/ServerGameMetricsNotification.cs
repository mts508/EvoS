using System;
using System.Collections.Generic;
using System.Text;


[Serializable]
public class ServerGameMetricsNotification : AllianceMessageBase
{
    public int CurrentTurn;

    public int TeamAPoints;

    public int TeamBPoints;

    public float AverageFrameTime;
}

