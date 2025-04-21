using UnityEngine;

public class ConveyorItem : MonoBehaviour
{
    private void Start()
    {
        
    }

    void Update()
    {

    }
    public bool isStopped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Stopper"))
        {
            isStopped = true;
        }
    }
}
