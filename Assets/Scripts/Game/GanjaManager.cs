using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GanjaManager : MonoBehaviour
{
    public Transform EndPoint;
    public GameObject GanjaPlant;
    public int NumPlants = 30;

    public Transform StartPoint;

    private GanjaPlant[] _plants;

    // Use this for initialization

    public int AliveCount
    {
        get { return _plants.Where(p => p.IsAlive).Count(); }
    }

    public int DeadCount
    {
        get { return _plants.Where(p => !p.IsAlive).Count(); }
    }

    public bool IsAllAlive
    {
        get { return _plants.All(p => p.IsAlive); }
    }

    public bool IsAllDead
    {
        get { return _plants.All(p => !p.IsAlive); }
    }

    public IList<GanjaPlant> GanjaPlants
    {
        get { return _plants; }
    }

    private void Start()
    {
        ResetPlants();
    }

    // Use this for initialization
    private void Awake()
    {
        ResetPlants();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public GanjaPlant TryAquireGanjaPlantTarget(int sideBias = 0, bool strict = false)
    {
        int aliveCount = AliveCount;

        int i = Random.Range(0, aliveCount);

        if (i < aliveCount)
        {
            return _plants.Where(p => p.IsAlive).Skip(i).FirstOrDefault();
        }

        //Acquire failed
        return null;
    }

    public void ResetPlants()
    {
        if (_plants == null)
        {
            _plants = new GanjaPlant[NumPlants];
        }

        for (int i = 0; i < _plants.Length; i++)
        {
            if (_plants[i] == null)
            {
                var spawn = Instantiate(GanjaPlant,
                                        StartPoint.position +
                                        ((EndPoint.position - StartPoint.position)*(i/(NumPlants - 1.0f))),
                                        GanjaPlant.transform.rotation) as GameObject;

                _plants[i] = spawn.GetComponent<GanjaPlant>();
                _plants[i].Manager = this;
            }

            _plants[i].Reset();
        }
    }

    public void RestorePlants(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int deadCount = GanjaPlants.Count(
                p =>
                p.CurrentState != global::GanjaPlant.State.Alive && p.CurrentState != global::GanjaPlant.State.Repairing);

            if (deadCount == 0)
                break;

            Debug.Log("Found " + deadCount + " dead plants. ");

            int repairIndex = Random.Range(0, deadCount - 1);

            Debug.Log("Repairing # " + repairIndex);

            GanjaPlants.Where(
                p =>
                p.CurrentState != global::GanjaPlant.State.Alive && p.CurrentState != global::GanjaPlant.State.Repairing)
                .Skip(repairIndex).First().Repair();
        }
    }
}