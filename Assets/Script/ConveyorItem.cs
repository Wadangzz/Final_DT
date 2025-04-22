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

        // Pack에 닿았을 때
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

                // Gripper에서 떨어뜨리고 Pack으로 붙이기
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

                // Gripper에서 떨어뜨리고 Pack으로 붙이기
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
            //    // 부모가 아직도 Pack이면 => 따로 이동시키지 말기
            //    if (transform.parent != null && transform.parent.CompareTag("Pack"))
            //    {
            //        // Cap은 Pack에 붙어있는 상태니까 무시
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

    private void OnTriggerExit(Collider other) // 스토퍼 올라가면 다시 이동
    {
        if (other.CompareTag("Stopper"))
        {
            isStopped = false;
        }
    }
}
