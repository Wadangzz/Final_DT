using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    [SerializeField] public float moveSpeed;
    [SerializeField] public float rotationSpeed;
    [SerializeField] public float zoomSpeed;
    [SerializeField] public float panSpeed;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        // 마우스 우클릭 시 회전
        if (Input.GetMouseButton(1))
        {
            yaw += rotationSpeed * Input.GetAxis("Mouse X");
            pitch -= rotationSpeed * Input.GetAxis("Mouse Y");
            pitch = Mathf.Clamp(pitch, 0, 2000f); // 위아래 제한

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        // WASD 이동
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        transform.Translate(move * moveSpeed * Time.deltaTime);

        // 휠 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        transform.Translate(Vector3.forward * scroll * zoomSpeed * 10000 * Time.deltaTime, Space.Self);

        // middle mouse drag for panning
        if (Input.GetMouseButton(2)) // 2 = middle mouse button
        {
            float h = -Input.GetAxis("Mouse X") * panSpeed * 100 * Time.deltaTime;
            float v = -Input.GetAxis("Mouse Y") * panSpeed * 100 * Time.deltaTime;
            // 카메라의 로컬 축 기준으로 이동
            transform.Translate(new Vector3(h, v, 0));
        }

    }
}
