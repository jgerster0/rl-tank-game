using UnityEngine;
public class FinalVTankNewSpawn : MonoBehaviour
{
    public GameObject FinalVEnemyTankNew;
    public float delay1 = 4f;
    public GameObject friendlyTank;
    public float delay2 = 7.99f;
    public float spawnRange = 30f;
    public float yRangeHigh = 20f;
    public float yRangeLow = 4f;
    public int EnemyCount = 0;
    public bool EnableMissiles = true;

    // Reference to the agent within the Tank object
    public FinalVTankAgent agent;
    private Vector3 tankPosition;

    private float timeStart1;
    private float timeStart2;

    void Start()
    {
        timeStart1 = 0f;
        timeStart2 = 0f;
    }

    void Update()
    {
        timeStart1 += Time.deltaTime;
        timeStart2 += Time.deltaTime;

        if (timeStart1 > delay1)
        {
            SpawnTank(true);
            timeStart1 -= delay1;
        }

        if (timeStart2 > delay2)
        {
            SpawnTank(false);
            timeStart2 -= delay2;
        }
    }

    public void SpawnTank(bool isEnemy)
    {
        float YRand = 0f;
        if (isEnemy){
        EnemyCount += 1;
        if (EnemyCount % 3 == 0) YRand = Random.Range(yRangeLow, yRangeHigh);}

        

        Vector3 spawnPosition = new Vector3(
            Random.Range(-spawnRange, spawnRange) + transform.position.x,
            YRand + transform.position.y,
            30f + transform.position.z
        );

        Quaternion spawnRotation = Quaternion.AngleAxis(180f, Vector3.up);
        GameObject tankToSpawn = isEnemy ? FinalVEnemyTankNew : friendlyTank;

        GameObject spawnedTank = Instantiate(tankToSpawn, spawnPosition, spawnRotation, transform);
        Rigidbody tankRigidbody = spawnedTank.GetComponent<Rigidbody>();
        if (isEnemy && EnemyCount % 3 == 0){
            tankRigidbody.useGravity = false;
        }

        var tankScript = spawnedTank.GetComponent<FinalVEnemyTankNew>();
        if (isEnemy){
        if (((EnemyCount +1) % 3 == 0) && EnableMissiles) tankScript.shootMissile = true;
        }

    }

    public void DestroyAllTanks()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("EnemyAI") || child.CompareTag("Friendly"))
            {
                Destroy(child.gameObject);
            }
        }

        timeStart1 = 0f;
        timeStart2 = 0f;
    }
}

