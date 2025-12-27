using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [Header("AI Prefabs")]
    public GameObject[] aiPrefabs;

    [Header("Spawn Control")]
    public int targetPopulation = 10;
    public float spawnInterval = 2f;

    [Tooltip("If false, no new NPCs will be spawned.")]
    public bool spawningEnabled = true;

    [Header("Night Behavior")]
    [Tooltip("Maximum NPCs allowed to remain at night.")]
    public int nightMaxPopulation = 2;

    [Tooltip("Seconds between removing one NPC at night.")]
    public float nightDespawnInterval = 6f;

    private readonly List<GameObject> spawnedAgents = new List<GameObject>();
    private Coroutine nightDespawnRoutine;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    public void SetTargetPopulation(int target)
    {
        targetPopulation = Mathf.Max(0, target);
        CleanupDestroyedAgents();
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
    }

    public void EnterNightMode()
    {
        spawningEnabled = false;

        if (nightDespawnRoutine == null)
            nightDespawnRoutine = StartCoroutine(NightDespawnLoop());
    }

    public void ExitNightMode()
    {
        spawningEnabled = true;

        if (nightDespawnRoutine != null)
        {
            StopCoroutine(nightDespawnRoutine);
            nightDespawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            CleanupDestroyedAgents();

            if (spawningEnabled && spawnedAgents.Count < targetPopulation)
                SpawnOneAgent();

            yield return new WaitForSeconds(Mathf.Max(0.2f, spawnInterval));
        }
    }

    IEnumerator NightDespawnLoop()
    {
        while (true)
        {
            CleanupDestroyedAgents();

            if (spawnedAgents.Count > nightMaxPopulation)
            {
                int index = spawnedAgents.Count - 1;
                GameObject agent = spawnedAgents[index];
                spawnedAgents.RemoveAt(index);

                if (agent != null)
                    Destroy(agent);
            }

            yield return new WaitForSeconds(Mathf.Max(1f, nightDespawnInterval));
        }
    }

    void SpawnOneAgent()
    {
        if (aiPrefabs == null || aiPrefabs.Length == 0) return;
        if (transform.childCount == 0) return;

        int prefabIndex = Random.Range(0, aiPrefabs.Length);
        GameObject agent = Instantiate(aiPrefabs[prefabIndex]);

        Transform spawnPoint = transform.GetChild(Random.Range(0, transform.childCount));
        agent.transform.position = spawnPoint.position;

        WaypointNavigator navigator = agent.GetComponent<WaypointNavigator>();
        Waypoint waypoint = spawnPoint.GetComponent<Waypoint>();
        if (navigator != null && waypoint != null)
            navigator.currentWaypoint = waypoint;

        spawnedAgents.Add(agent);
    }

    void CleanupDestroyedAgents()
    {
        for (int i = spawnedAgents.Count - 1; i >= 0; i--)
        {
            if (spawnedAgents[i] == null)
                spawnedAgents.RemoveAt(i);
        }
    }
}
