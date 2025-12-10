using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        // Playerに触れたら
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゴールに到達しました！クリア！");
        }
    }
}
