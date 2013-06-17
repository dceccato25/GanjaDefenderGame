using System;
using UnityEngine;

[Serializable]
public class GameStage
{
    public Texture2D Image;
    public bool IsEnabled;
    public bool IsUnlocked;
    public Level[] Levels;
    public string SceneName;
    public string Title;
}

[Serializable]
public class Level
{
    public LevelSequence[] Sequences;
}

[Serializable]
public class LevelSequence
{
    public float Duration;
    public LevelSequenceLayer[] Layers;
}

[Serializable]
public class LevelSequenceLayer
{
    public int ActorIndex;
    public float Bias1Max = 1.0f;
    public float Bias1Min = 1.0f;
    public float Bias2Max = 1.0f;
    public float Bias2Min = 1.0f;
    public int InnerSpawnCountMax;
    public int InnerSpawnCountMin;
    public int SpawnCountMax;
    public int SpawnCountMin;
    public float SpeedMax = 5.0f;
    public float SpeedMin = 1.0f;
}