using UnityEngine;
using NModbus;
using System;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DT : MonoBehaviour
{
    private TcpClient client;
    private IModbusMaster modbusMaster;

    [Header("Cylinders")]
    [SerializeField] private GameObject[] cylinders = new GameObject[6];
    [SerializeField] private GameObject gripper;

    [Header("Robots")]
    [SerializeField] private GameObject[] robot1 = new GameObject[4];
    [SerializeField] private GameObject[] robot2 = new GameObject[4];
    [SerializeField] private GameObject[] robot3 = new GameObject[4];

    [Header("Servo & Prefabs")]
    [SerializeField] private GameObject servoTower;
    [SerializeField] private GameObject objectPrefab;

    [Header("Conveyors")]
    [SerializeField] private Transform[] conveyors = new Transform[3];

    private float[][] joints = new float[3][] { new float[4], new float[4], new float[4] };
    private float servoPosition = 0f;

    private bool[] cylinderON = new bool[6];
    private bool[] cylinderOFF = new bool[6];

    private bool isConnected = false;
    private bool objectSpawned = false;
    private bool[] coils = new bool[1024];
    private ushort[] registers = new ushort[125];

    private float moveSpeed = 3.0f;
    private float conveyorSpeed = 50.0f;

    private Queue<GameObject> Cell = new Queue<GameObject>();
    private List<GameObject> batteryCase = new List<GameObject>();

    void Start()
    {
        Debug.Log("Start");
        Task.Run(() => Connect());
        StartCoroutine(ReadInputRegisters());
        StartCoroutine(ReadInputs());
    }

    float RegistersToFloat(ushort low, ushort high)
    {
        byte[] bytes = new byte[4];
        BitConverter.GetBytes(low).CopyTo(bytes, 0);
        BitConverter.GetBytes(high).CopyTo(bytes, 2);
        return BitConverter.ToSingle(bytes, 0);
    }

    int RegistersToInt32(ushort low, ushort high)
    {
        uint raw = ((uint)high << 16) | low;
        return unchecked((int)raw);
    }
    private void ApplyRobotRotation(GameObject[] robot, float[] joint)
    {
        robot[0].transform.localRotation = Quaternion.Euler(0, -joint[0], 0);
        robot[1].transform.localRotation = Quaternion.Euler(-joint[1], 0, 0);
        robot[2].transform.localRotation = Quaternion.Euler(-joint[2] + joint[1], 0, 0);
        robot[3].transform.rotation = Quaternion.Euler(0, robot[3].transform.rotation.eulerAngles.y, 0);
    }


    private void UpdateDoubleStates()
    {
        for (int i = 0; i < 4; i++)
        {
            if (coils[340 + i * 2])
            {
                cylinderON[i] = true;
                cylinderOFF[i] = false;
            }
            else if (coils[341 + i * 2])
            {
                cylinderOFF[i] = true;
                cylinderON[i] = false;
            }
        }
    }
    private void MoveDouble(int index, Vector3 onPos, Vector3 offPos)
    {
        if (cylinderON[index] && !cylinderOFF[index])
            cylinders[index].transform.localPosition = Vector3.MoveTowards(cylinders[index].transform.localPosition, onPos, moveSpeed * Time.deltaTime);
        else if (cylinderOFF[index] && !cylinderON[index])
            cylinders[index].transform.localPosition = Vector3.MoveTowards(cylinders[index].transform.localPosition, offPos, 2 * moveSpeed * Time.deltaTime);
    }

    private void MoveSingle(GameObject obj, Vector3 onPos, Vector3 offPos, bool condition, float speedMultiplier = 1f)
    {
        Vector3 target = condition ? onPos : offPos;
        obj.transform.localPosition = Vector3.MoveTowards(obj.transform.localPosition, target, moveSpeed * speedMultiplier * Time.deltaTime);
    }


    private void ApplyCylinderMotions()
    {
        // 양측 실린더
        MoveDouble(0, new Vector3(1.0f, 0, 0), Vector3.zero);
        MoveDouble(1, new Vector3(0.45f, 0, 0), new Vector3(-0.3f, 0, 0));
        MoveDouble(2, new Vector3(0.45f, 0, 0), new Vector3(-0.3f, 0, 0));
        MoveDouble(3, new Vector3(0.45f, 0, 0), new Vector3(-0.3f, 0, 0));

        // 편측 실린더
        MoveSingle(cylinders[4], new Vector3(0, 0, -0.55f), Vector3.zero, coils[348]);
        MoveSingle(cylinders[5], new Vector3(0, 0, -0.55f), Vector3.zero, coils[352]);
        MoveSingle(gripper, new Vector3(-18.5f, 0, 0), Vector3.zero, coils[353], 10f);
    }


    IEnumerator ReadInputRegisters()
    {
        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    registers = modbusMaster.ReadInputRegisters(1, 0, 125);
                    servoPosition = RegistersToInt32(registers[71], registers[72]) / 2f;

                    for (int i = 0; i < 4; i++)
                    {
                        joints[0][i] = RegistersToFloat(registers[100 + i * 2], registers[101 + i * 2]);
                        joints[1][i] = RegistersToFloat(registers[108 + i * 2], registers[109 + i * 2]);
                        joints[2][i] = RegistersToFloat(registers[116 + i * 2], registers[117 + i * 2]);
                    }

                    ApplyRobotRotation(robot1, joints[0]);
                    ApplyRobotRotation(robot2, joints[1]);
                    ApplyRobotRotation(robot3, joints[2]);

                    servoTower.transform.localPosition = new Vector3(0, servoPosition / 10000f, 0);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}\n{e.StackTrace}");
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    IEnumerator ReadInputs()
    {
        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    coils = modbusMaster.ReadInputs(1, 0, 1024);
                    UpdateDoubleStates();
                    ApplyCylinderMotions();

                    //if (coils[560])
                    //{
                    //    objectSpawned = false; // 오브젝트 재생성 가능
                    //    cylinder1_OFF = false; // 초기화해서 재생성 방지
                    //}
                    // 추후에 오브젝트 스폰 및 병합 삭제 로직 추가 필요
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception: {e.Message}\n{e.StackTrace}");
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    private async void Connect()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 1502);

            isConnected = client.Connected;
            if (isConnected)
            {
                ModbusFactory factory = new ModbusFactory();
                modbusMaster = factory.CreateMaster(client);
                Debug.Log("Connected to Modbus TCP server.");
            }
            else
            {
                Debug.LogWarning("Failed to connect to Modbus TCP server.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Exception during connection: {e.Message}\n{e.StackTrace}");
        }
    }

    private void DisConnect()
    {
        modbusMaster?.Dispose();
        modbusMaster = null;
        client?.Close();
        isConnected = false;

        Array.Clear(coils, 0, coils.Length);
        Array.Clear(registers, 0, registers.Length);
    }

    void LateUpdate()
    {
        Vector3 forward = transform.forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    private void OnApplicationQuit()
    {
        if (isConnected)
        {
            DisConnect();
        }
    }
}
