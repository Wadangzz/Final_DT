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

        // Pack�� ����� ��
        if (other.CompareTag("Pack"))
        {

            if (transform.parent != null && transform.parent.CompareTag("Picker"))
            {

                Vector3 targetLocalPos = Vector3.zero;
                int index = other.transform.childCount;

                if (index == 1)
                    targetLocalPos = new Vector3(0, -19.0f, 8.6f);
                else if (index == 2)
                    targetLocalPos = new Vector3(0, -19.0f, -12.7f);
                //else if (index == 3)
                //    targetLocalPos = new Vector3(60.0f, 54.0f, 3.8f);

                Vector3 worldTargetPos = other.transform.TransformPoint(targetLocalPos);

                // Gripper���� ����߸��� Pack���� ���̱�
                transform.SetParent(other.transform, true);
                transform.position = worldTargetPos;
                transform.rotation = Quaternion.Euler(0, 180.0f, 0);
            }

            else if (transform.parent != null && transform.parent.CompareTag("Gripper"))
            {

                Vector3 targetLocalPos = Vector3.zero;
                int index = other.transform.childCount;

                if (index == 3)
                    targetLocalPos = new Vector3(0.8f, 51.3f, 59.0f);

                Vector3 worldTargetPos = other.transform.TransformPoint(targetLocalPos);

                // Gripper���� ����߸��� Pack���� ���̱�
                transform.SetParent(other.transform, true);
                transform.position = worldTargetPos;
                transform.rotation = Quaternion.Euler(0, -90.0f, 0);
            }
        }

        if (other.CompareTag("Picker"))
        {
            Vector3 worldTargetPos = other.transform.TransformPoint(new Vector3(-2.4f, -70.0f, -50.5f));

            transform.SetParent(other.transform, true);
            transform.position = worldTargetPos;
        }

        if (other.CompareTag("Picker2"))
        {
            if (CompareTag("Pack"))
            {
                Vector3 worldTargetPos = other.transform.TransformPoint(new Vector3(-8.9f, -88.6f, -48.9f));

                transform.SetParent(other.transform, true);
                transform.position = worldTargetPos;
            }

            //if (CompareTag("Cap"))
            //{
            //    // �θ� ������ Pack�̸� => ���� �̵���Ű�� ����
            //    if (transform.parent != null && transform.parent.CompareTag("Pack"))
            //    {
            //        // Cap�� Pack�� �پ��ִ� ���´ϱ� ����
            //        return;
            //    }
            //}
        }

        if (other.CompareTag("Belt"))
        {
            transform.SetParent(other.transform, true);
        }

        if (other.CompareTag("Gripper"))
        {
            Vector3 worldTargetPos;
            if (CompareTag("Cap"))
            {
                worldTargetPos = other.transform.TransformPoint(new Vector3(1.51f, 0.315f, -1.17f));

                transform.SetParent(other.transform, true);
                transform.position = worldTargetPos;
            }
            else if (CompareTag("Pack"))
            {
                worldTargetPos = other.transform.TransformPoint(new Vector3(0.96f, 0.31f, -0.6f));

                transform.SetParent(other.transform, true);
                transform.position = worldTargetPos;
            }
        }

        if (other.CompareTag("Endpoint") || other.CompareTag("NG"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit(Collider other) // ������ �ö󰡸� �ٽ� �̵�
    {
        if (other.CompareTag("Stopper"))
        {
            isStopped = false;
        }
    }
}
