using UnityEngine;

public class FinalVEnemyTankNew : MonoBehaviour
{
    public float speed;
    public GameObject AI;
    private FinalVTankAgent agent;
    private Vector3 tankPosition;
    public GameObject missilePrefab; 
    private Rigidbody rbody;
    private float fireTimer = 2f;
    private const float posThreshold = -37f;
    private bool Flag = true;
    public bool shootMissile = false;
    public Transform turret;

    void Start()
    {
        rbody = GetComponent<Rigidbody>();
            
    }

    void Update()
    {
        // Move the enemy tank forward
        Vector3 moveVect = transform.forward * speed * Time.deltaTime;
        rbody.MovePosition(rbody.position + moveVect);

        // Destroy the tank if it moves past the front line
        if (transform.localPosition.z < posThreshold)
        {
            Destroy(gameObject);
        }
        // Update the fire timer 
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f && Flag && shootMissile)
        {
            Flag = false;
            turret.localRotation = Quaternion.Euler(-45f, turret.localEulerAngles.y, turret.localEulerAngles.z);
            FireMissile();
        }
    }

    private void FireMissile()
    {
        Vector3 missileStartPos = new Vector3(transform.position.x, transform.position.y + 3f, transform.position.z - 5f);
        //Debug.Log("Rocket Start Pos: " + missileStartPos);
        agent = transform.parent.GetComponentInChildren<FinalVTankAgent>();
        tankPosition = new Vector3(agent.transform.position.x, agent.transform.position.y, agent.transform.position.z - 1.5f);
        //Debug.Log("Target Pos: " + tankPosition);
        Transform simulation = transform.parent;
        Missile.LaunchMissile(missilePrefab, missileStartPos, tankPosition, simulation);
    }

    public void Hit()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Tank")
        {
            Destroy(gameObject);
        }
    }
}
