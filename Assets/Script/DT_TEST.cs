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

    [SerializeField] private GameObject cylinder1;
    [SerializeField] private GameObject cylinder2;
    [SerializeField] private GameObject cylinder3;

    [SerializeField] private GameObject cylinder4;
    [SerializeField] private GameObject cylinder5;
    [SerializeField] private GameObject cylinder6;

    [SerializeField] private GameObject robot1_1;
    [SerializeField] private GameObject robot1_2;
    [SerializeField] private GameObject robot1_3;
    [SerializeField] private GameObject robot1_4;
    [SerializeField] private GameObject robot2_1;
    [SerializeField] private GameObject robot2_2;
    [SerializeField] private GameObject robot2_3;
    [SerializeField] private GameObject robot2_4;
    [SerializeField] private GameObject robot3_1;
    [SerializeField] private GameObject robot3_2;
    [SerializeField] private GameObject robot3_3;
    [SerializeField] private GameObject robot3_4;
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private Transform conveyorTransform;

    float[] joint1 = new float[4];
    float[] joint2 = new float[4];
    float[] joint3 = new float[4];

    bool cylinder1_ON = false;
    bool cylinder1_OFF = false;
    bool cylinder2_ON = false;
    bool cylinder2_OFF = false;
    bool cylinder3_ON = false;
    bool cylinder3_OFF = false;
    bool cylinder4_ON = false;
    bool cylinder4_OFF = false;


    bool objectSpawned = false;

    bool isConnected = false;
    bool[] coils = new bool[1024];
    bool[] outPut = new bool[14];
    ushort[] registers = new ushort[125];

    Vector3 velocity = Vector3.zero;
    private float moveSpeed = 3.0f;
    private float conveyorSpeed = 50.0f;

    private List<GameObject> batteryCase = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        Task.Run(()=>Connect());

        StartCoroutine(ReadInputRegisters());
        StartCoroutine(ReadInputs());
    }
    // �������͸� float�� ��ȯ�ϴ� �޼���
    float RegistersToFloat(ushort low, ushort high)
    {
        byte[] bytes = new byte[4];
        BitConverter.GetBytes(low).CopyTo(bytes, 0);
        BitConverter.GetBytes(high).CopyTo(bytes, 2);
        return BitConverter.ToSingle(bytes, 0);
    }
    // �������͸� �о���� float ��ȯ �� �κ��� ����
    IEnumerator ReadInputRegisters()
    {
        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    ushort startAddress = 0;
                    ushort numRegisters = 125;//
                    registers = modbusMaster.ReadInputRegisters(1, startAddress, numRegisters);
                }

                for (int i = 0; i < 4; i++)
                {
                    joint1[i] = RegistersToFloat(registers[i * 2 + 100], registers[i * 2 + 101]);
                    joint2[i] = RegistersToFloat(registers[i * 2 + 108], registers[i * 2 + 109]);
                    joint3[i] = RegistersToFloat(registers[i * 2 + 116], registers[i * 2 + 117]);
                }

                robot1_1.transform.localRotation = Quaternion.Euler(0, -joint1[0], 0);
                robot1_2.transform.localRotation = Quaternion.Euler(-joint1[1], 0, 0);
                robot1_3.transform.localRotation = Quaternion.Euler(-joint1[2] + joint1[1], 0, 0);
                robot1_4.transform.rotation = Quaternion.Euler(0, robot1_4.transform.rotation.eulerAngles.y, 0);

                robot2_1.transform.localRotation = Quaternion.Euler(0, -joint2[0], 0);
                robot2_2.transform.localRotation = Quaternion.Euler(-joint2[1], 0, 0);
                robot2_3.transform.localRotation = Quaternion.Euler(-joint2[2] + joint2[1], 0, 0);
                robot2_4.transform.rotation = Quaternion.Euler(0, robot2_4.transform.rotation.eulerAngles.y, 0);

                robot3_1.transform.localRotation = Quaternion.Euler(0, -joint3[0], 0);
                robot3_2.transform.localRotation = Quaternion.Euler(-joint3[1], 0, 0);
                robot3_3.transform.localRotation = Quaternion.Euler(-joint3[2] + joint3[1], 0, 0);
                robot3_4.transform.rotation = Quaternion.Euler(0, robot3_4.transform.rotation.eulerAngles.y, 0);
            }
            catch (Exception e)
            {
                Debug.Log($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
            }
            yield return new WaitForSeconds(0.001f);
        }
    }
    // ������ �о���� �Ǹ���, �����̾�, ������Ʈ�� ����
    IEnumerator ReadInputs()
    {

        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    ushort startAddress = 0;
                    ushort numCoils = 1024;//���� ����
                    coils = modbusMaster.ReadInputs(1, startAddress, numCoils);
                    //Debug.Log($"Coils : {string.Join(", ",outPut)}");
                }
                if (coils[340] == true)
                {
                    cylinder1_ON = true;
                    cylinder1_OFF = false;
                }
                else if (coils[341] == true)
                {
                    cylinder1_OFF = true;
                    cylinder1_ON = false;
                }
                if (coils[342] == true)
                {
                    cylinder2_ON = true;
                    cylinder2_OFF = false;
                }
                else if (coils[343] == true)
                {
                    cylinder2_OFF = true;
                    cylinder2_ON = false;       
                }
                if (coils[344] == true)
                {
                    cylinder3_ON = true;
                    cylinder3_OFF = false;
                }
                else if (coils[345] == true)
                {
                    cylinder3_OFF = true;
                    cylinder3_ON = false;
                }
                if (coils[346] == true)
                {
                    cylinder4_ON = true;
                    cylinder4_OFF = false;
                }
                else if (coils[347] == true)
                {
                    cylinder4_OFF = true;
                    cylinder4_ON = false;
                }
                //if (coils[348] == true)
                //{
                //    cylinder5.transform.localPosition = Vector3.MoveTowards(cylinder5.transform.localPosition, new Vector3(0.25f, 0, 0), moveSpeed * Time.deltaTime);
                    
                //} 
                //else if (coils[348] == false)
                //{
                //    cylinder5.transform.localPosition = Vector3.MoveTowards(cylinder5.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                //}
                if (coils[350] == true)
                {
                    foreach (GameObject obj in batteryCase)
                    {
                        obj.transform.localPosition += Vector3.right * conveyorSpeed * Time.deltaTime;
                    }
                }
                //else if (coils[350] == false)
                //{
                //    cylinder5_ON = false;
                //}

                if (coils[560])
                {
                    objectSpawned = false; // ������Ʈ ����� ����
                    cylinder1_OFF = false; // �ʱ�ȭ�ؼ� ����� ����
                }

                if (cylinder1_ON && !cylinder1_OFF)
                {
                    cylinder1.transform.localPosition = Vector3.MoveTowards(cylinder1.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                    if (cylinder1.transform.localPosition.x >= 0.4f && !objectSpawned)
                    {
                        Vector3 spawnPosition = new Vector3(-808, 130, 318);
                        Quaternion spawnRotation = Quaternion.identity;
                        GameObject newObj = Instantiate(objectPrefab, spawnPosition, spawnRotation);
                        newObj.transform.SetParent(conveyorTransform);
                        batteryCase.Add(newObj);
                        objectSpawned = true; // ������Ʈ�� �����Ǿ����� ǥ��
                    }
                }
                else if (cylinder1_OFF && !cylinder1_ON)
                {
                    cylinder1.transform.localPosition = Vector3.MoveTowards(cylinder1.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder2_ON && !cylinder2_OFF)
                {
                    cylinder2.transform.localPosition = Vector3.MoveTowards(cylinder2.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder2_OFF && !cylinder2_ON)
                {
                    cylinder2.transform.localPosition = Vector3.MoveTowards(cylinder2.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder3_ON && !cylinder3_OFF)
                {
                    cylinder3.transform.localPosition = Vector3.MoveTowards(cylinder3.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder3_OFF && !cylinder3_ON)
                {
                    cylinder3.transform.localPosition = Vector3.MoveTowards(cylinder3.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder4_ON && !cylinder4_OFF)
                {
                    cylinder4.transform.localPosition = Vector3.MoveTowards(cylinder4.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder4_OFF && !cylinder4_ON)
                {
                    cylinder4.transform.localPosition = Vector3.MoveTowards(cylinder4.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
            }
            yield return new WaitForSeconds(0.001f);
        }
    }

    private async void Connect()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync("10.10.24.179", 1502);

            isConnected = client.Connected;

            if (isConnected)
            {
                ModbusFactory factory = new ModbusFactory();
                modbusMaster = factory.CreateMaster(client);
                Debug.Log("Connected to Modbus TCP server.");
            }
            else
            {
                Debug.Log("Failed to connect to Modbus TCP server.");
            }

        }
        catch (Exception e)
        {
            Debug.Log($"Exception during connection: {e.Message}\nStack Trace: {e.StackTrace}");
        }
    }

    private void DisConnect()
    {
        if (modbusMaster != null)
        {
            modbusMaster.Dispose();
            modbusMaster = null;
            client?.Close();
            isConnected = false;

            for (int i = 0; i < 14; i++)
            {
                outPut[i] = false;
            }

            // �������Ϳ� ���� �ʱ�ȭ

            for (int i = 0; i < coils.Length; i++)
            {
                coils[i] = false;
            }
            for (int i = 0; i < registers.Length; i++)
            {
                registers[i] = 0;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        // �ڽ� ������Ʈ�� �ٶ󺸴� ������ �����ϸ鼭, ���� ���⸸ World Up���� ����
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

