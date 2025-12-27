using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WaypointNavigator : MonoBehaviour
{
    [Header("AI Character")]
    CharacterNavigating character;
    public Waypoint currentWaypoint;

    private void Awake()
    {
        character = GetComponent<CharacterNavigating>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //direction = Mathf.RoundToInt(Random.Range(0f, 1f));
        character.LocationDestination(currentWaypoint.GetPosition());
    }

    // Update is called once per frame
    void Update()
    {
        if (character.destinationReached)
        {
            currentWaypoint = currentWaypoint.nextWaypoint;
            character.LocationDestination(currentWaypoint.GetPosition());
        }
    }
}
