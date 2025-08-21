using UnityEngine;
using TMPro;

public class OptScorecount : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    private OptTankAgent agent;
    private float displayDuration = 2f;  
    private float timer = 0f; 

    void Start()
    {
        Transform parentSimulation = transform.parent; 

        if (parentSimulation != null)
        {
            agent = parentSimulation.GetComponentInChildren<OptTankAgent>();
        }

        if (scoreText != null)
        {
            scoreText.text = "Score: 0";  // default score
        }
    }

    void Update()
    {
        if (agent != null)
        {
            if (agent.outcome == "")
            {
                scoreText.text = "Score: " + agent.GetScore();
            }
            else if (agent.outcome == "Win")
            {
                // Show last outcome
                scoreText.text = "Score: " + agent.GetScore() + "\n<color=green>You won!\nScore: " + agent.outcomeScore + "</color>";
                timer += Time.deltaTime;
                //Reset
                if (timer >= displayDuration)
                {
                    agent.outcome = "";
                    agent.outcomeScore = 0;
                    timer = 0f;
                }
            }
            else if (agent.outcome == "Lose")
            {
                // Show last outcome
                scoreText.text = "Score: " + agent.GetScore() + "\n<color=red>You lost!\nScore: " + agent.outcomeScore + "</color>";
                timer += Time.deltaTime;
                //Reset
                if (timer >= displayDuration)
                {
                    agent.outcome = ""; 
                    agent.outcomeScore = 0; 
                    timer = 0f;
                }
            }
        }
    }
}