using UnityEngine;
using NModbus;
using System;
using System.Net.Sockets;
using System.Collections;
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

    [Header("Servo & Object")]
    [SerializeField] private GameObject servoTower;
    [SerializeField] private GameObject pack;
    [SerializeField] private GameObject cell1;
    [SerializeField] private GameObject cell2;
    [SerializeField] private GameObject cell3;
    [SerializeField] private GameObject packagingCap;
    [SerializeField] private Transform tower;

    [Header("Conveyors")]
    [SerializeField] private Transform[] conveyors = new Transform[3];

    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float conveyorSpeed = 1.0f;


    private float[][] joints = new float[3][] { new float[4], new float[4], new float[4] };
    private float servoPosition = 0f;

    private bool[] cylinderON = new bool[6];
    private bool[] cylinderOFF = new bool[6];

    private bool isConnected = false;
    private bool isConnecting = false;
    private bool[] objectSpawned = new bool[] { false, false, false, false, false };
    private Vector3[] onpos = new Vector3[] {
                                                new Vector3(1.0f, 0, 0), 
                                                new Vector3(0.45f, 0, 0), 
                                                new Vector3(0.45f, 0, 0), 
                                                new Vector3(0.45f, 0, 0) 
                                            };
    private Vector3[] offpos = new Vector3[] { 
                                                Vector3.zero, 
                                                new Vector3(-0.3f, 0, 0), 
                                                new Vector3(-0.3f, 0, 0), 
                                                new Vector3(-0.3f, 0, 0) 
                                            };
    private Vector3[] spawnPosition = new Vector3[] { 
                                                        new Vector3(0.25f, 4.63f, -0.81f), 
                                                        new Vector3(0.180f, 2.59f, -0.983f), 
                                                        new Vector3(0.180f, 1.77f, -0.983f),
                                                        new Vector3(0.180f, 0.95f, -0.983f), 
                                                        new Vector3(-59.5f, 256.7f, -0.6f) 
                                                    };
    private bool[] coils = new bool[1024];
    private ushort[] registers = new ushort[125];


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

    private void MoveConveyor(Transform conveyor, bool condition)
    {
        if (condition)
        {
            for (int i = 0; i < conveyor.childCount; i++)
            {
                Transform child = conveyor.GetChild(i);
                ConveyorItem item = child.GetComponent<ConveyorItem>();
                if (item != null && item.isStopped) continue; // 멈춘 애는 스킵
                child.localPosition += Vector3.down * conveyorSpeed * Time.deltaTime;
            }
        }
    }
    // 다른 컨베이어랑 좌표가 반대라 따로 선언 
    private void MoveConveyor2(Transform conveyor, bool condition)
    {
        if (condition)
        {
            for (int i = 0; i < conveyor.childCount; i++)
            {
                Transform child = conveyor.GetChild(i);
                ConveyorItem item = child.GetComponent<ConveyorItem>();
                if (item != null && item.isStopped) continue; // 멈춘 애는 스킵
                child.localPosition += Vector3.up * conveyorSpeed * Time.deltaTime;
            }
        }
    }

    private void ApplyActuatorMotions()
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

        MoveConveyor(conveyors[0], coils[349]);
        MoveConveyor(conveyors[1], coils[350]);
        MoveConveyor2(conveyors[2], coils[351]);
    }

    // 메서드 진짜 거지같이 만들었다.......매개변수 7개 실화냐........
    private void ObjectSpawn(ref bool isSpawned, Transform parent, GameObject cylinder, GameObject obj, Vector3 onpos, Vector3 offpos, Vector3 spawnPosition)
    {
        if (coils[504])
        {
            if (!isSpawned && Vector3.Distance(cylinder.transform.localPosition, onpos) < 0.001f)
            {
                GameObject newObj = Instantiate(obj);
                newObj.transform.SetParent(parent);
                //newObj.AddComponent<ConveyorItem>();
                newObj.transform.localPosition = spawnPosition; // 부모 기준 위치
                isSpawned = true;
            }
            if (isSpawned && Vector3.Distance(cylinder.transform.localPosition, offpos) < 0.001f)
            {
                isSpawned = false;
            }
        }
    }

    private void ObjectSpawn2(ref bool isSpawned, Transform parent, GameObject obj, Vector3 spawnPosition)
    {
        if (coils[504])
        {
            if (!isSpawned && coils[121] && registers[80] == 0)
            {
                GameObject newObj = Instantiate(obj);
                newObj.transform.SetParent(parent);
                //newObj.AddComponent<ConveyorItem>();
                newObj.transform.localPosition = spawnPosition; // 부모 기준 위치
                isSpawned = true;
            }
            if (isSpawned && !coils[121])
            {
                isSpawned = false;
            }
        }
    }

    IEnumerator ReadInputRegisters()
    {
        while (true)
        {
            if (modbusMaster == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            else if (modbusMaster != null)
            {
                try
                {
                    registers = modbusMaster.ReadInputRegisters(1, 0, 125);
                    servoPosition = RegistersToInt32(registers[71], registers[72]);

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
                catch (Exception e)
                {
                    Debug.LogError($"Exception: {e.Message}\n{e.StackTrace}");
                    DisConnect();
                    if (!isConnecting)
                        Task.Run(() => Connect());
                }
            }
            yield return new WaitForSeconds(0.001f);
        } 
    }

    IEnumerator ReadInputs()
    {
        while (true)
        {
            if (modbusMaster == null)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }
            else if (modbusMaster != null)
            {
                try
                {
                    coils = modbusMaster.ReadInputs(1, 0, 1024);
                    UpdateDoubleStates();
                    ApplyActuatorMotions();

                    ObjectSpawn(ref objectSpawned[0], conveyors[1], cylinders[0], pack, onpos[0], offpos[0], spawnPosition[0]);
                    ObjectSpawn(ref objectSpawned[1], conveyors[0], cylinders[1], cell1, onpos[1], offpos[1], spawnPosition[1]);
                    ObjectSpawn(ref objectSpawned[2], conveyors[0], cylinders[2], cell2, onpos[2], offpos[2], spawnPosition[2]);
                    ObjectSpawn(ref objectSpawned[3], conveyors[0], cylinders[3], cell3, onpos[3], offpos[3], spawnPosition[3]);
                    ObjectSpawn2(ref objectSpawned[4], tower, packagingCap, spawnPosition[4]);


                    //Debug.Log($"IsSpawned : {String.Join(", ", objectSpawned)}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Exception: {e.Message}\n{e.StackTrace}");
                    DisConnect();
                    if (!isConnecting)
                        Task.Run(() => Connect());
                }
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    private async void Connect()
    {
        isConnecting = true;
        while (!isConnected)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("10.10.24.179", 1502);

                client.ReceiveTimeout = 2000;
                client.SendTimeout = 2000;

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
            await Task.Delay(1000); // 매 초 마다 재시도
        }
        isConnecting = false;
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
