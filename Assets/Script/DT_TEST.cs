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

    bool isConnected = false;
    bool[] coils = new bool[1024];
    bool[] outPut = new bool[14];
    ushort[] registers = new ushort[125];

    Vector3 velocity = Vector3.zero;
    public float moveSpeed = 3.0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        Task.Run(()=>Connect());

        StartCoroutine(ReadInputRegisters());
        StartCoroutine(ReadInputs());
    }
    // 레지스터를 float로 변환하는 메서드
    float RegistersToFloat(ushort low, ushort high)
    {
        byte[] bytes = new byte[4];
        BitConverter.GetBytes(low).CopyTo(bytes, 0);
        BitConverter.GetBytes(high).CopyTo(bytes, 2);
        return BitConverter.ToSingle(bytes, 0);
    }
    // 레지스터를 읽어오고 float 변환 후 로봇에 적용
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
    // 코일을 읽어오고 실린더, 컨베이어, 오브젝트에 적용
    IEnumerator ReadInputs()
    {

        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    ushort startAddress = 0;
                    ushort numCoils = 1024;//코일 갯수
                    coils = modbusMaster.ReadInputs(1, startAddress, numCoils);

                    for (int i = 0 ; i < 14; i++)
                    {
                       outPut[i] = coils[i+340];
                    }
                    Debug.Log($"Coils : {string.Join(", ",outPut)}");
                }
                if (outPut[0] == true)
                {
                    cylinder1_ON = true;
                    cylinder1_OFF = false;
                }
                else if (outPut[1] == true)
                {
                    cylinder1_OFF = true;
                    cylinder1_ON = false;

                }
                if (outPut[2] == true)
                {
                    cylinder2_ON = true;
                    cylinder2_OFF = false;
                }
                else if (outPut[3] == true)
                {
                    cylinder2_OFF = true;
                    cylinder2_ON = false;       
                }
                if (outPut[4] == true)
                {
                    cylinder3_ON = true;
                    cylinder3_OFF = false;
                }
                else if (outPut[5] == true)
                {
                    cylinder3_OFF = true;
                    cylinder3_ON = false;
                }
                if (outPut[6] == true)
                {
                    cylinder4_ON = true;
                    cylinder4_OFF = false;
                }
                else if (outPut[7] == true)
                {
                    cylinder4_OFF = true;
                    cylinder4_ON = false;
                }
                if (cylinder1_ON && cylinder1_OFF == false)
                {
                    cylinder1.transform.localPosition = Vector3.MoveTowards(cylinder1.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder1_OFF && cylinder1_ON == false)
                {
                    cylinder1.transform.localPosition = Vector3.MoveTowards(cylinder1.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder2_ON && cylinder2_OFF == false)
                {
                    cylinder2.transform.localPosition = Vector3.MoveTowards(cylinder2.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder2_OFF && cylinder2_ON == false)
                {
                    cylinder2.transform.localPosition = Vector3.MoveTowards(cylinder2.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder3_ON && cylinder3_OFF == false)
                {
                    cylinder3.transform.localPosition = Vector3.MoveTowards(cylinder3.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder3_OFF && cylinder3_ON == false)
                {
                    cylinder3.transform.localPosition = Vector3.MoveTowards(cylinder3.transform.localPosition, new Vector3(-0.3f, 0, 0), 2 * moveSpeed * Time.deltaTime);
                }
                if (cylinder4_ON && cylinder4_OFF == false)
                {
                    cylinder4.transform.localPosition = Vector3.MoveTowards(cylinder4.transform.localPosition, new Vector3(0.45f, 0, 0), moveSpeed * Time.deltaTime);
                }
                else if (cylinder4_OFF && cylinder4_ON == false)
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

            // 레지스터와 코일 초기화

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
        // 자식 오브젝트가 바라보는 방향은 유지하면서, 위쪽 방향만 World Up으로 고정
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

