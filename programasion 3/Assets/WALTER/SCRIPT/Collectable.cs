using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] private int points;      
    [HideInInspector] public int poolIndex;   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(points);
            CollectablesSpawn.Instance.CollectObj(gameObject, poolIndex);
        }
    }
}
