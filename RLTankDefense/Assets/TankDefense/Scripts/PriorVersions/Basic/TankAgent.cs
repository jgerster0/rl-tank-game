using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class TankAgent : Agent
{
    [SerializeField] private TankNewSpawn tankSpawner;
    private float episodeDuration = 120f; 
    private float elapsedTime = 0f;
    public int seed = 20241007;
    public Transform shootPoint; //obj to fire from
     public LineRenderer lineRenderer;


    private Rigidbody rBody;
    private System.Random systemRandom;
    private Scorecount scoreDisplay;
    private int score = 0;
    private int episode = 0;  

    private const float resetYPosition = 0.5f;
    private const float maxRange = 30f;
    private const float posThreshold = -36.9f;

    protected override void Awake()
    {
        base.Awake();
        systemRandom = new System.Random(seed);
    }

    protected void Start()
    {
        scoreDisplay = FindObjectOfType<Scorecount>();
        rBody = GetComponent<Rigidbody>();
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
        Random.InitState(seed);
    }

    public override void OnEpisodeBegin()
    {
        InitializeRandomState();
        ResetAgent();
        tankSpawner.DestroyAllTanks();
        score = 0;
        episode += 1;
        elapsedTime = 0f;
        //Debug.Log("Episode "+ episode);
    }

    private void ResetAgent()
    {

            // Reset velocities
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;

            // Reset position 
            transform.localPosition = new Vector3(0f, 0f, -35f);
            //Reset Rotation
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
{

    // observations of agents tank
    sensor.AddObservation(transform.localPosition);
    sensor.AddObservation(rBody.velocity);
    sensor.AddObservation(transform.forward);

    //Find enemy tanks by component
    EnemyTankNew[] enemies = Object.FindObjectsOfType<EnemyTankNew>();
    foreach (var enemy in enemies)
    {
        Vector3 relativeEnemyPos = enemy.transform.position - transform.position;
        sensor.AddObservation(relativeEnemyPos / 30f);  // normalized by 30 

        if (enemy.transform.localPosition.z < posThreshold)
        {
            score -= 1;
            AddReward(-1.0f);}
    }

    //Find friendly tanks by component
    FriendlyTankNew[] friendlies = Object.FindObjectsOfType<FriendlyTankNew>();
    foreach (var friendly in friendlies)
    {
        Vector3 relativeFriendlyPos = friendly.transform.position - transform.position;
        sensor.AddObservation(relativeFriendlyPos / 30f);  // normalized by 30 
    }

}





    public override void OnActionReceived(ActionBuffers actionBuffers)
{

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


    //track time
    elapsedTime += Time.deltaTime;
    //end episode after 2 min
    if (elapsedTime >= episodeDuration)
    {
        Debug.Log("Episode ends");
        EndEpisode();
    }    

    // end episode if tank falls off
    if (transform.localPosition.y < -0.5f || Mathf.Abs(transform.localPosition.x) > 37.5f)
    {
        AddReward(-10.0f);  
        score -= 10;  
        //Debug.Log("Score: " + score);
        EndEpisode();
    }

     
    //if(score < 0){
    //    EndEpisode();
    //}
}



    //Shoot function
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
                //Debug.Log("Hit Enemy");
                AddReward(2.0f); 
                score += 2;  
            }
            else if (hit.collider.CompareTag("Friendly"))
            {
                //Debug.Log("Hit Friendly");
                AddReward(-1.0f);
                score -= 1;
            }

            Destroy(hit.collider.gameObject);
        }
        else
        {
            //Debug.Log("missed");
            // draw ray to max range
            DrawRay(shootPoint.position, shootPoint.position + shootDirection * maxRange);
            
        }
       
        CancelInvoke("DisableLine"); 
        Invoke("DisableLine", 0.1f);
        //winning cond
        if (score >= 20)
                {
                    AddReward(5.0f); 
                    EndEpisode();
                }
    }

    //draw ray with LineRenderer
    private void DrawRay(Vector3 start, Vector3 end)
    {
        lineRenderer.enabled = true; 
        lineRenderer.SetPosition(0, start); //start point
        lineRenderer.SetPosition(1, end); //end point
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));//default material
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }

    private void DisableLine()
    {
        lineRenderer.enabled = false;
    }
    
    private void OnCollisionEnter(Collision collision){
    // collide with friendly
    if (collision.gameObject.CompareTag("Friendly"))
    {
        AddReward(2.0f);
        score += 2;
        //Debug.Log("friendly touched; score" + score);}

    // collide with enemy
    if (collision.gameObject.CompareTag("EnemyAI"))
    {
        AddReward(-3.0f);
        score -= 3;
        //Debug.Log("enemy touched; score" + score);
        EndEpisode();
    }

    //winning cond
    if (score >= 20)
        {
            AddReward(5.0f); 
            EndEpisode(); 
        }
}
}



    public override void Heuristic(in ActionBuffers actionsOut)
{
    var continuousActions = actionsOut.ContinuousActions;
    if (continuousActions.Length > 0)
    {
        continuousActions[0] = Input.GetAxis("Horizontal"); // left/right movement 
        continuousActions[1] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f; //shoot
        }
}


}

