using UnityEngine;
using NModbus;
using System;
using System.Net.Sockets;
using System.Collections;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class DT : MonoBehaviour
{
    private TcpClient robot;
    private TcpClient client;
    private IModbusMaster modbusMaster;
    private TcpListener robotAxis;
    private NetworkStream stream;



    [SerializeField] private GameObject cylinder1;
    [SerializeField] private GameObject cylinder2;
    [SerializeField] private GameObject cylinder3;
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

    bool isConnected = false;
    bool isRunning = false;
    bool on_cylinder1 = false;
    bool off_cylinder1 = false;
    bool[] coils = new bool[1024];
    byte[] buffer;
    ushort[] registers = new ushort[123];

    Vector3 velocity = Vector3.zero;
    Quaternion angle = Quaternion.Euler(0, 0, 0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start");
        Task.Run(()=>Connect());

        StartCoroutine(ReadInputRegisters());
        StartCoroutine(ReadInputs());
        StartCoroutine(WaitForAcceptClients());
    }

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

                    on_cylinder1 = coils[352];
                    off_cylinder1 = coils[353];
                    Debug.Log($"Coils : {on_cylinder1}, {off_cylinder1}");
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator WaitForAcceptClients()
    {
        while (!isRunning) // isRunning이 true가 될 때까지 대기
        {
            yield return null;
        }
        Debug.Log("isRunning is true. Starting AcceptClients...");
        StartCoroutine(AcceptClients());
    }

    IEnumerator AcceptClients()
    {
        Debug.Log("AcceptClients coroutine started.");
        while (isRunning)
        {

            var acceptTask = robotAxis.AcceptTcpClientAsync();
            while (!acceptTask.IsCompleted)
            {
                yield return null; // 메인 스레드 블로킹 방지
            }
            try
            {
                robot = acceptTask.Result;
                Debug.Log("Client is connected.");
                StartCoroutine(ReceiveData(robot));
            }
            catch (Exception e)
            {
                Debug.LogError($"Error accepting client: {e.Message}");
            }
        }
        Debug.Log("AcceptClients coroutine stopped.");
    }
    

    IEnumerator ReadInputRegisters()
    {
        while (true)
        {
            try
            {
                if (modbusMaster != null)
                {
                    ushort startAddress = 0;
                    ushort numRegisters = 123;//최대 123워드 주고 받기
                    registers = modbusMaster.ReadInputRegisters(1, startAddress, numRegisters);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Exception: {e.Message}\nStack Trace: {e.StackTrace}");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator ReceiveData(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[17];  // 1 byte ID + 4 floats

        while (client.Connected)
        {
            var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
            while (!readTask.IsCompleted)
            {
                yield return null; // 메인 스레드 블로킹 방지
            }
            try
            {   
                int read = readTask.Result;
                //Debug.Log($"Buffer : {string.Join(", ", buffer)}");
                if (read == 17)
                {
                    float[] joint = new float[4];
                    byte id = buffer[0];
                    joint[0] = BitConverter.ToSingle(buffer, 1);
                    joint[1] = BitConverter.ToSingle(buffer, 5);
                    joint[2] = BitConverter.ToSingle(buffer, 9);
                    joint[3] = BitConverter.ToSingle(buffer, 13);

                    Debug.Log($"[로봇{id}] J1={joint[0]}, J2={joint[1]}, J3={joint[2]}, J4={joint[3]}");
                    if (id == 1)
                    {   
                        for (int i = 0; i < 4; i++)
                        {
                            joint1[i] = joint[i];
                        }
                        robot1_1.transform.localRotation = Quaternion.Euler(0, -joint1[0], 0);
                        robot1_2.transform.localRotation = Quaternion.Euler(-joint1[1], 0, 0);
                        robot1_3.transform.localRotation = Quaternion.Euler(-joint1[2] + joint1[1], 0 , 0);
                        robot1_4.transform.rotation = Quaternion.Euler(0, robot1_4.transform.rotation.eulerAngles.y, 0);
                    }

                    if (id == 2)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            joint2[i] = joint[i];
                        }
                        robot2_1.transform.localRotation = Quaternion.Euler(0, -joint2[0], 0);
                        robot2_2.transform.localRotation = Quaternion.Euler(-joint2[1], 0, 0);
                        robot2_3.transform.localRotation = Quaternion.Euler(-joint2[2] + joint2[1], 0, 0);
                        robot2_4.transform.rotation = Quaternion.Euler(0, robot2_4.transform.rotation.eulerAngles.y, 0);
                    }

                    if (id == 3)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            joint3[i] = joint[i];
                        }
                        robot3_1.transform.localRotation = Quaternion.Euler(0, -joint3[0], 0);
                        robot3_2.transform.localRotation = Quaternion.Euler(-joint3[1], 0, 0);
                        robot3_3.transform.localRotation = Quaternion.Euler(-joint3[2] + joint3[1], 0, 0);
                        robot3_4.transform.rotation = Quaternion.Euler(0, robot3_4.transform.rotation.eulerAngles.y, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("데이터 수신 오류: " + ex.Message);
            }
        }
    }
    private async void Connect()
    {
        try
        {
            client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 502);

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

            robotAxis = new TcpListener(IPAddress.Any, 8000);
            robotAxis.Start();
            Debug.Log("Listening for robot connections...");

            isRunning = true;
            //Debug.Log("Receive waiting");
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

            on_cylinder1 = false;
            off_cylinder1 = false;

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
        if (isConnected)
        {
            if (on_cylinder1 == true)
            {
                cylinder1.transform.localPosition = Vector3.SmoothDamp(cylinder1.transform.localPosition, new Vector3(0.5f, 0, 0), ref velocity, 1.0f);
            }
            else if (off_cylinder1 == true)
            {
                cylinder1.transform.localPosition = Vector3.SmoothDamp(cylinder1.transform.localPosition, new Vector3(-0.3f, 0, 0), ref velocity, 0.5f);
            }
        }
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
            stream?.Close();
            robotAxis.Stop();
            isRunning = false;
        }
    }
}

