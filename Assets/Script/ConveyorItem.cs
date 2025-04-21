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
        if (other.CompareTag("Pack"))
        {
            Debug.Log("collide");
            Vector3 targetLocalPos = Vector3.zero;
            int index = other.transform.childCount;

            if (index == 2)
            {
                Debug.Log("collide2");
                targetLocalPos = new Vector3(0, -19.0f, 7.6f);
            }
            else if (index == 3)
            {
                Debug.Log("collide3");
                targetLocalPos = new Vector3(0, -19.0f, -13.7f);
            }

            Vector3 worldTargetPos = other.transform.TransformPoint(targetLocalPos);

            // SetParent하면서 world 좌표 유지
            transform.SetParent(other.transform, true);

            // 그 위치로 강제로 고정 (이게 핵심!)
            transform.position = worldTargetPos;
        }
    }
}
