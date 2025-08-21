using UnityEngine;

public class Missile : MonoBehaviour
{
    private Vector3 targetPos;
    private Rigidbody rb;
    public bool Homing = true; 
    public float homingStrength = 20f;
    public float homingTime = 2f;
    private FinalVTankAgent agent;      
    private float timeSinceLaunch = 0f; 
    private bool isHoming = false;
    public Transform env;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = transform.parent.GetComponentInChildren<FinalVTankAgent>();
        targetPos = new Vector3(agent.transform.position.x, agent.transform.position.y, agent.transform.position.z - 1.5f);
        rb.useGravity = true;
    }

    public static void LaunchMissile(GameObject missilePrefab, Vector3 startPosition, Vector3 target, Transform env)
    {
        // Calc direction to target
        Vector3 direction = target - startPosition;
        Quaternion rotation = Quaternion.LookRotation(direction);

        //rotation offset
        Quaternion rotationOffset = Quaternion.Euler(90, 0, 0); 
        rotation *= rotationOffset;

        //Instantiate missile 
        GameObject missileInstance = Instantiate(missilePrefab, startPosition, rotation, env);

        
        Missile missileScript = missileInstance.GetComponent<Missile>();
        missileScript.env = env; 

        missileScript.rb = missileInstance.GetComponent<Rigidbody>();
        missileScript.rb.velocity = CalculateVelocity(startPosition, target);
    }

    private static Vector3 CalculateVelocity(Vector3 origin, Vector3 target)
    {
        float gravity = Physics.gravity.magnitude;
        float angle = 45f * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(target.x, 0, target.z);
        Vector3 planarOrigin = new Vector3(origin.x, 0, origin.z);
        float distance = Vector3.Distance(planarTarget, planarOrigin);

        Vector3 direction = (planarTarget - planarOrigin).normalized;
        float heightDifference = origin.y - target.y;

        float cosAngle = Mathf.Cos(angle);
        float sinAngle = Mathf.Sin(angle);

        float speedSquared = (gravity * distance * distance) /
                             (2 * cosAngle * cosAngle * (distance * Mathf.Tan(angle) + heightDifference));

        if (speedSquared <= 0)
        {
            return Vector3.zero;
        }

        float speed = Mathf.Sqrt(speedSquared);

        Vector3 velocity = direction * speed * cosAngle;
        velocity.y = speed * sinAngle;

        return velocity;
    }

    void FixedUpdate()
    {
        timeSinceLaunch += Time.deltaTime;

        if (Homing && timeSinceLaunch >= homingTime)
        {
            isHoming = true;
            rb.useGravity = false;
        }
        //if homing
        if (isHoming && agent != null)
        {
            targetPos = new Vector3(agent.transform.position.x, agent.transform.position.y, agent.transform.position.z - 1.5f);
            //Calc direction towards tank
            Vector3 direction = (targetPos - transform.position).normalized;

            //Calc steering force 
            Vector3 desiredVelocity = direction * rb.velocity.magnitude;
            Vector3 steeringForce = desiredVelocity - rb.velocity;

            //Apply force
            rb.AddForce(steeringForce.normalized * homingStrength);

            // Rotate missile to face direction
            if (rb.velocity != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(rb.velocity);
                Quaternion rotationOffset = Quaternion.Euler(90, 0, 0); 
                targetRotation *= rotationOffset;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);
            }


        }
        //if not homing
        else if (rb.velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.velocity);
            Quaternion rotationOffset = Quaternion.Euler(90, 0, 0); 
            targetRotation *= rotationOffset;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Tank")
        {
            //Debug.Log("Missile hit Tank");
            //Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Terrain") && timeSinceLaunch > 2f || transform.position.y < 0f) {
            //Debug.Log("Missile hit Terrain");
            Destroy(gameObject);
        }
        if (transform.localPosition.z < -50f)
        {
            Destroy(gameObject);
        }
    }
}
