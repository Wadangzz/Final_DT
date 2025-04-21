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

            // SetParent�ϸ鼭 world ��ǥ ����
            transform.SetParent(other.transform, true);

            // �� ��ġ�� ������ ���� (�̰� �ٽ�!)
            transform.position = worldTargetPos;
        }
    }
}
