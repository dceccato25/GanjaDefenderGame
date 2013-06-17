using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets.Scripts.Shared;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    #region private members

    private const float SEPARATION_BIAS = 0.4f;
    private const float WAVE_END_DELAY = 8.0f;
    private const float LEVEL_END_DELAY = 8.0f;

    private const int SMALL_CRAFT_HEALTH = 4;
    private const int MEDIUM_CRAFT_HEALTH = 7;
    private const int LARGE_CRAFT_HEALTH = 10;
    private const int ARMORED_CRAFT_HEALTH = 16;

    private const int MIN_INNER_SPAWNS = 1;

    private static readonly IDictionary<string, GameStage> _loadedStages = new Dictionary<string, GameStage>();
    private readonly IList<LevelTrigger> _levelTriggerCache = new List<LevelTrigger>();
    private int _currentLevelIndex;
    public GameStage _currentStage;

    private bool _isRunningLevel;
    private float _levelTimeElapsed;
    private int _nextExecutingTrigger;

    private float _nextTriggerTime;

    #endregion

    #region Properties

    public GameObject[] Actors;
    public Transform LeftSpawner;
    public Transform RightSpawner;
    public string[] Stages;
    public Transform TopSpawner;

    #endregion

    private bool isWaitingForLevelToClear;

    public Level CurrentLevel
    {
        get
        {
            if (CurrentLevelNumber < 0)
                return null;

            if (_currentStage.Levels.Length <= CurrentLevelNumber)
                return _currentStage.Levels.Last();

            return _currentStage.Levels[CurrentLevelNumber];
        }
    }

    public int CurrentLevelNumber
    {
        get
        {
            return _currentLevelIndex;
        }
        private set
        {
            _currentLevelIndex = value;
            GameManager.Instance.UIManager.RefreshLevel();
        }
    }

    public float LevelTimeElapsed
    {
        get { return _levelTimeElapsed; }
    }

    public bool IsRunningLevel
    {
        get { return _isRunningLevel; }
    }

    public void SelectStage(int stage)
    {
        if (Stages == null || Stages.Length <= stage)
            return;

        LoadStage(Stages[stage]);
    }

    public void SelectLevel(int level)
    {
        if (_currentStage == null || _currentStage.Levels.Length <= level)
        {
            Debug.LogWarning("Current stage is null or no levels defined: " + _currentStage.Levels.Length);
            return;
        }

        CurrentLevelNumber = level;

        if (CurrentLevel == null)
        {
            Debug.LogWarning("Current level not set successfully: " + _currentStage.Levels.Length);
        }
    }

    public void ResetCurrentLevel()
    {
        _isRunningLevel = false;
        ClearLevelObjects();
        _levelTimeElapsed = 0.0f;
        _nextExecutingTrigger = 0;
    }

    public void PauseCurrentLevel()
    {
        _isRunningLevel = false;
    }

    public void ResumeCurrentLevel()
    {
        _isRunningLevel = true;
    }

    public void StartCurrentLevel()
    {
        _levelTimeElapsed = 0.0f;
        InitLevelTriggers();
        _isRunningLevel = true;
    }

    public void StartNextLevel()
    {
        isWaitingForLevelToClear = false;
        CurrentLevelNumber++;

        GameManager.Instance.GanjaManager.RestorePlants(10);

        ResetCurrentLevel();
        StartCurrentLevel();
    }

    private void InitLevelTriggers()
    {
        if (CurrentLevel == null)
        {
            Debug.LogWarning("Current level not set, unable to init triggers: " + _currentStage.Levels.Length);
            return;
        }

        int triggerIndex = 0;
        int currentTriggerCeiling = _levelTriggerCache.Count;
        float _levelTimeOffset = 0;

        Debug.Log(string.Format("Creating triggers for level {0}. {1} sequences found.", CurrentLevelNumber,
                                CurrentLevel.Sequences.Length));

        foreach (LevelSequence sequence in CurrentLevel.Sequences)
        {
            foreach (LevelSequenceLayer layer in sequence.Layers)
            {
                int spawnCount = Random.Range(layer.SpawnCountMin, layer.SpawnCountMax);
                int innerSpawnCount = Random.Range(layer.InnerSpawnCountMin, layer.InnerSpawnCountMax);
                float sequenceSpawnInterval = sequence.Duration / spawnCount;
                int innerSpawnsPerSpawn = innerSpawnCount / spawnCount;
                int layerSpawnCount = 0;

                for (int i = 0; i < spawnCount; i++)
                {
                    int innerSpawnItems = spawnCount == (i - 1)
                                              ? innerSpawnCount - layerSpawnCount
                                              : ((innerSpawnCount - layerSpawnCount) / (spawnCount - i)) +
                                                Random.Range(innerSpawnsPerSpawn * -1, innerSpawnsPerSpawn);

                    float speed = Random.Range(layer.SpeedMin, layer.SpeedMax);
                    float bias1 = Random.Range(layer.Bias1Min, layer.Bias1Max);
                    float bias2 = Random.Range(layer.Bias2Min, layer.Bias2Max);

                    layerSpawnCount += innerSpawnItems;

                    var trigger = new LevelTrigger
                                      {
                                          ActorIndex = layer.ActorIndex,
                                          Bias1 = bias1,
                                          Bias2 = bias2,
                                          IsActive = true,
                                          IsTriggered = false,
                                          Duration = 40.0f / speed,
                                          StartTime =
                                              _levelTimeOffset + (sequenceSpawnInterval * i) +
                                              (sequenceSpawnInterval * Random.Range(-1f * SEPARATION_BIAS, SEPARATION_BIAS)) +
                                              0.5f,
                                          InnerSpawnCount = innerSpawnItems,
                                          Speed = speed
                                      };

                    if (currentTriggerCeiling <= triggerIndex)
                    {
                        _levelTriggerCache.Add(trigger);
                    }
                    else
                    {
                        _levelTriggerCache[triggerIndex] = trigger;
                    }

                    triggerIndex++;
                }
            }

            _levelTimeOffset += (sequence.Duration + WAVE_END_DELAY);
        }

        Debug.Log("Finished creating level triggers: " + _levelTriggerCache.Count);
    }

    private void ClearLevelObjects()
    {
        //TODO

        foreach (LevelTrigger trigger in _levelTriggerCache)
        {
            trigger.IsActive = false;
        }
    }

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isRunningLevel)
        {
            _levelTimeElapsed += Time.deltaTime;

            if (isWaitingForLevelToClear)
            {
                if (GameObject.FindGameObjectWithTag("Enemy") == null)
                    StartNextLevel();

                return;
            }

            LevelTrigger nextTrigger = _levelTriggerCache.FirstOrDefault(t => !t.IsTriggered);

            if (nextTrigger != null)
                _nextTriggerTime = nextTrigger.StartTime;

            if (_levelTriggerCache.Count <= _nextExecutingTrigger)
            {
                if (_levelTriggerCache.Count > 0 && _nextExecutingTrigger > 0 && _levelTimeElapsed > 1.0f)
                    WaitForEndOfLevel();

                return;
            }

            while (_levelTriggerCache[_nextExecutingTrigger].StartTime < _levelTimeElapsed)
            {
                LevelTrigger aquiredTrigger = _levelTriggerCache[_nextExecutingTrigger];

                _nextExecutingTrigger++;

                if (aquiredTrigger.IsActive && !aquiredTrigger.IsTriggered)
                {
                    aquiredTrigger.IsTriggered = true;
                    SpawnObject(aquiredTrigger);
                }

                if (_levelTriggerCache.Count <= _nextExecutingTrigger)
                    break;
            }
        }
    }

    private void WaitForEndOfLevel()
    {
        isWaitingForLevelToClear = true;
    }

    private void SpawnObject(LevelTrigger spawnTrigger)
    {
        if (CurrentLevel == null)
            return;

        if (Actors.Length <= spawnTrigger.ActorIndex)
            return;

        GameObject actorModel = Actors[spawnTrigger.ActorIndex];
        int direction = Random.Range(0, 1.0f) >= 0.5f ? 1 : 2;

        GameObject newActor = null;
        Vector3 spawnPosition;

        switch (direction)
        {
            case 1:
                spawnPosition = new Vector3(LeftSpawner.position.x,
                                            Random.Range(LeftSpawner.collider.bounds.min.y,
                                                         LeftSpawner.collider.bounds.max.y),
                                            Random.Range(LeftSpawner.collider.bounds.min.z,
                                                         LeftSpawner.collider.bounds.max.z));

                newActor = Instantiate(actorModel, spawnPosition, LeftSpawner.rotation) as GameObject;

                if (newActor != null)
                    newActor.transform.localScale = new Vector3(newActor.transform.localScale.x * -1,
                                                                newActor.transform.localScale.y,
                                                                newActor.transform.localScale.z);

                break;

            case 2:
                spawnPosition = new Vector3(RightSpawner.position.x,
                                            Random.Range(RightSpawner.collider.bounds.min.y,
                                                         RightSpawner.collider.bounds.max.y),
                                            Random.Range(RightSpawner.collider.bounds.min.z,
                                                         RightSpawner.collider.bounds.max.z));

                newActor = Instantiate(actorModel, spawnPosition, RightSpawner.rotation) as GameObject;

                break;
        }

        if (newActor != null)
        {
            var actor = newActor.GetComponent<SpawnableEnemyBase>();

            if (actor != null)
            {
                actor.Bias1 = spawnTrigger.Bias1;
                actor.Bias2 = spawnTrigger.Bias2;
                actor.Speed = spawnTrigger.Speed;
                actor.Direction = direction == 1 ? Direction.Right : Direction.Left;
                actor.SpawnCount = spawnTrigger.InnerSpawnCount;
            }
        }
    }

    private void LoadStage(string stageName)
    {
        if (_loadedStages.ContainsKey(stageName))
        {
            _currentStage = _loadedStages[stageName];
            return;
        }

        var newStage = new GameStage();
        var xmlBody = Resources.Load(stageName) as TextAsset;

        var stageDefinition = new XmlDocument();
        stageDefinition.LoadXml(xmlBody.text);

        XmlElement stage = stageDefinition.DocumentElement;

        newStage.Title = stage.ReadValue("Name", "");
        newStage.IsEnabled = stage.ReadBoolValue("IsEnambled", true);
        newStage.IsUnlocked = stage.ReadBoolValue("IsUnlocked", true);

        newStage.Levels = LoadLevels(stage);

        _loadedStages.Add(stageName, newStage);
        _currentStage = newStage;
    }

    private Level[] LoadLevels(XmlNode stage)
    {
        return stage.ChildNodes.OfType<XmlNode>().Where(n => n.Name == "level")
            .Select(l =>
                        {
                            var level = new Level
                                            {
                                                Sequences = LoadLevelSequences(l)
                                            };
                            return level;
                        }).ToArray();
    }

    private LevelSequence[] LoadLevelSequences(XmlNode level)
    {
        return level.ChildNodes.OfType<XmlNode>().Where(n => n.Name == "sequence")
            .Select(s =>
                        {
                            var sequence = new LevelSequence
                                               {
                                                   Duration = s.ReadDecimalValue("Duration", 10f),
                                                   Layers = LoadLevelSequenceLayers(s)
                                               };
                            return sequence;
                        }).ToArray();
    }

    private LevelSequenceLayer[] LoadLevelSequenceLayers(XmlNode sequence)
    {
        return sequence.ChildNodes.OfType<XmlNode>().Where(n => n.Name == "layer")
            .Select(l =>
                        {
                            var layer = new LevelSequenceLayer
                                            {
                                                ActorIndex = l.ReadIntValue("ActorIndex", 0),
                                                SpawnCountMin = l.ReadIntValue("SpawnCountMin", 0),
                                                SpawnCountMax = l.ReadIntValue("SpawnCountMax", 0),
                                                InnerSpawnCountMin = l.ReadIntValue("InnerSpawnCountMin", 0),
                                                InnerSpawnCountMax = l.ReadIntValue("InnerSpawnCountMax", 0),
                                                SpeedMin = l.ReadDecimalValue("SpeedMin", 1),
                                                SpeedMax = l.ReadDecimalValue("SpeedMax", 5),
                                                Bias1Min = l.ReadDecimalValue("Bias1Min", 0.5f),
                                                Bias1Max = l.ReadDecimalValue("Bias1Max", 0.5f),
                                                Bias2Min = l.ReadDecimalValue("Bias2Min", 0.5f),
                                                Bias2Max = l.ReadDecimalValue("Bias2Max", 0.5f)
                                            };
                            return layer;
                        }).ToArray();
    }
}