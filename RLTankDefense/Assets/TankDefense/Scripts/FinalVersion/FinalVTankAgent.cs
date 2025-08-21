using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class FinalVTankAgent : Agent
{
    public string outcome = "";
    public int outcomeScore = 0;
    [SerializeField] private FinalVTankNewSpawn tankSpawner;
    private float episodeDuration = 120f; 
    private float elapsedTime = 0f;
    public int seed = 20241009;
    public Transform shootPoint; //obj to fire from
    public LineRenderer lineRenderer;

    private Rigidbody rBody;
    private System.Random systemRandom;
    private FinalVScorecount scoreDisplay;
    private int score = 0;  
    private int episode = 0;  
    private FinalVEnemyTankNew[] localEnemies;
    private FriendlyTankNew[] localFriendlies;

    private const float resetYPosition = 0.5f;
    private const float maxRange = 30f; 
    private const float posThreshold = -36.9f;
    [SerializeField] private Transform turret;
    private float xRotation;
    [SerializeField] private float rotationSpeed = 300f;

    protected override void Awake()
    {
        base.Awake();
        rBody = GetComponent<Rigidbody>();
        systemRandom = new System.Random(seed);
    }

    protected void Start()
{
    rBody = GetComponent<Rigidbody>();  
    scoreDisplay = FindObjectOfType<FinalVScorecount>();
    InitializeRandomState();

    if (lineRenderer == null)
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
    }
    lineRenderer.enabled = false;
}


    public int GetScore()
    {
        return score;
    }

    private void InitializeRandomState()
    {
        //Init state with dynamic seed
        Random.InitState(seed + episode);
    }

    public override void OnEpisodeBegin()
    {
        InitializeRandomState();
        ResetAgent();
        tankSpawner.DestroyAllTanks();
        DestroyMissiles();
        score = 0;
        episode += 1;
        elapsedTime = 0f;
        //Debug.Log("Episode " + episode);
    }



    private void ResetAgent()
{
    // Reset velocities
    rBody.angularVelocity = Vector3.zero;
    rBody.velocity = Vector3.zero;

    // Reset the position
    transform.localPosition = new Vector3(0f, 0f, -35f);
    // Reset Rotation
    transform.rotation = Quaternion.Euler(0f, 0f, 0f);

    //Reset turret
    turret.Rotate(0, 0, 0);
}
    private void DestroyMissiles()
{
    Transform parentTransform = transform.parent;
    foreach (Transform child in parentTransform)
    {
        if (child.CompareTag("Missile"))
        {
            Destroy(child.gameObject);
        }
    }
}



    public override void CollectObservations(VectorSensor sensor)
    {
        Transform Simulation = transform.parent;
        FinalVEnemyTankNew[] localEnemies = Simulation.GetComponentsInChildren<FinalVEnemyTankNew>();
        FriendlyTankNew[] localFriendlies = Simulation.GetComponentsInChildren<FriendlyTankNew>();
        //Debug.Log("Length of friendlies:" +localFriendlies.Length);

        // observations of agents tank
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rBody.velocity);
        sensor.AddObservation(transform.forward);

        //Find enemy tanks by component
        foreach (var enemy in localEnemies)
        {
            Vector3 relativeEnemyPos = enemy.transform.position - transform.position;
            sensor.AddObservation(relativeEnemyPos / 30f);  // normalized by 30 

            //Add enemy distance as observation 
            float enemyDistance = Vector3.Distance(enemy.transform.position, transform.position);
            sensor.AddObservation(enemyDistance / 30f);

            if (enemy.transform.localPosition.z < posThreshold)
            {
                score -= 1;
                AddReward(-1.0f);
            }
        }

        //Find friendly tanks by component
        foreach (var friendly in localFriendlies)
        {
            Vector3 relativeFriendlyPos = friendly.transform.position - transform.position;
            sensor.AddObservation(relativeFriendlyPos / 30f);  // normalized by 30 

            
        }

        //Find missiles by component
        Transform parentTransform = transform.parent;
        foreach (Transform child in parentTransform)
        {
            if (child.CompareTag("Missile"))
            {
                Vector3 relativeMissilePos = child.position - transform.position;
        
                sensor.AddObservation(relativeMissilePos / 30f);
                float missileDistance = Vector3.Distance(child.transform.position, transform.position);
                sensor.AddObservation(missileDistance / 30f);
            }
}

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // time based penalty
        elapsedTime += Time.deltaTime;
        AddReward(-0.1f * Time.deltaTime);



        //end episode after 2 min
        if (elapsedTime >= episodeDuration)
        {
            //Debug.Log("Episode ends");
            AddReward(-5.0f); 
            EndEpisode();
            return;
        }

        float TankSpeed = 20f;
        // x movement
        float moveX = actionBuffers.ContinuousActions[0];
        // Calc move offset
        Vector3 moveVect = new Vector3(moveX * TankSpeed * Time.deltaTime, 0f, 0f); 
        // Calc new position
        Vector3 newPosition = rBody.position + moveVect;
        // Move tank
        rBody.MovePosition(newPosition);

        //shoot
        float shoot = actionBuffers.ContinuousActions[1];
        if (shoot > 0)
        {Shoot();}


        // Turret rotation horizontal
        //float hturretRotationInput = actionBuffers.ContinuousActions[3];
        //hRotateTurret(hturretRotationInput);

        // Turret rotation vertical
        float vturretRotation = actionBuffers.ContinuousActions[2];
        vRotateTurret(vturretRotation);

        // end episode if tank falls off
        if (transform.localPosition.y < -0.5f || Mathf.Abs(transform.localPosition.x) > 37.5f)
        {
            AddReward(-50.0f);
            outcome = "Lose";
            outcomeScore = score;
            EndEpisode();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
        Vector3 shootDirection = shootPoint.forward;
        lineRenderer.enabled = true;

        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, maxRange))
        {
            //draw ray to hit point
            DrawRay(shootPoint.position, hit.point);

            if (hit.collider.CompareTag("EnemyAI"))
            {
                AddReward(2.0f);
                score += 2;
            }
            else if (hit.collider.CompareTag("Friendly"))
            {
                AddReward(-1.0f);
                score -= 1;
            }
            else if (hit.collider.CompareTag("Missile"))
            {
                AddReward(3.0f);
                score += 1;
            }

            Destroy(hit.collider.gameObject);
        }
        else
        {
            // draw ray to max range
            DrawRay(shootPoint.position, shootPoint.position + shootDirection * maxRange);
        }

        CancelInvoke("DisableLine");
        Invoke("DisableLine", 0.1f);
        //winning cond
        if (score >= 20)
        {
            outcome = "Win";
            outcomeScore = score;
            AddReward(5.0f);
            EndEpisode();
        }
    }

    //private void hRotateTurret(float rotationInput)
    //{
    //float rotationSpeed = 100f;
    //turret.Rotate(0, rotationInput * rotationSpeed * Time.deltaTime, 0);
    //}

    private void vRotateTurret(float rotationInput)
{
    //apply the input
    xRotation += rotationInput * rotationSpeed * Time.deltaTime;

    // Clamp rotation between -90 to 0
    xRotation = Mathf.Clamp(xRotation, -90f, 0f);

    //set rotation
    turret.localRotation = Quaternion.Euler(xRotation, turret.localEulerAngles.y, turret.localEulerAngles.z);
}


    //draw ray with LineRenderer
    private void DrawRay(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start); //start point
        lineRenderer.SetPosition(1, end); //end point
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    private void DisableLine()
    {
        lineRenderer.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // collide with friendly
        if (collision.gameObject.CompareTag("Friendly"))
        { 
            AddReward(3.0f);
            score += 2;
        }
        // collide with enemy
        if (collision.gameObject.CompareTag("EnemyAI"))
        {
            outcome = "Lose";
            outcomeScore = score;
            AddReward(-3.0f);
            EndEpisode();
        }
        // collide with missile
        if (collision.gameObject.CompareTag("Missile"))
        {
            outcome = "Lose";
            outcomeScore = score;
            AddReward(-10.0f);
            EndEpisode();
        }
        //winning cond
        if (score >= 20)
        {
            outcome = "Win";
            outcomeScore = score;
            AddReward(5.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        if (continuousActions.Length > 0)
        {
            //Move
            continuousActions[0] = Input.GetKey(KeyCode.D) ? 1.0f : (Input.GetKey(KeyCode.A) ? -1.0f : 0.0f); // left/right movement
            //Shoot
            continuousActions[1] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
            //Rotate turret up/down
            continuousActions[2] = Input.GetKey(KeyCode.DownArrow) ? 1.0f : (Input.GetKey(KeyCode.UpArrow) ? -1.0f : 0.0f);
            //Rotate turret left/right
            //continuousActions[3] = Input.GetKey(KeyCode.RightArrow) ? 1.0f : (Input.GetKey(KeyCode.LeftArrow) ? -1.0f : 0.0f);
        }
    }
}