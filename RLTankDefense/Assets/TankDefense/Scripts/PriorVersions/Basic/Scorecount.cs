using UnityEngine;
using TMPro;

public class Scorecount : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;  
    private TankAgent agent;  
    void Start()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: 0";  //default score
        }
        agent = FindObjectOfType<TankAgent>();  //find agent
    }

    void Update()
    {
        // update score
        if (scoreText != null && agent != null)
        {
            scoreText.text = "Score: " + agent.GetScore();
        }
    }
}
