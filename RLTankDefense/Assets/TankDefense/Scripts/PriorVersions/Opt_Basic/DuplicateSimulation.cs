using UnityEngine;

public class SimulationDuplicator : MonoBehaviour
{
    public string originalSimulation = "EnemySpawnPointEnv";  // original simulation
    public int totalSimulations = 4; 
    public Vector3 spacing = new Vector3(30f, 0f, 0f);  //spacing

    void Start()
    {
        // check if simulation is original
        if (gameObject.name != originalSimulation)
        {
            return;
        }

        // Duplicate simulations
        for (int i = 1; i < totalSimulations; i++)
        {
            Vector3 newPosition = transform.position + spacing * i;

            // create copy 
            GameObject newSimulation = Instantiate(gameObject, newPosition, Quaternion.identity);

            //rename 
            int copy = i +1;
            newSimulation.name = originalSimulation + copy;
        }

        Debug.Log("Created simulations");
    }
}

