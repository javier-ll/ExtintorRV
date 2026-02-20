using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace FireSim.Core
{
    public class BLEManager : MonoBehaviour
    {
        public static BLEManager Instance { get; private set; }

        [Header("Red / Python Bridge")]
        [SerializeField] private int udpPort = 5005;
        
        [Header("Hardware SDK (.exe)")]
        [Tooltip("Ruta completa a tu EJECUTABLE (.exe)")]
        [SerializeField] private string sdkPath = @"C:\Users\fhlla\ExtintorRV\Hardware SDK\driver_guante.exe"; 
        
        private Process pythonProcess;

        public bool IsConnected { get; private set; } = false;
        public event Action<bool> OnButtonStateChanged;

        private bool _isButtonPressed = false;
        public bool IsButtonPressed => _isButtonPressed;

        private UdpClient udpClient;
        private Thread receiveThread;
        
        private bool _threadIsButtonPressed = false;
        private bool _threadStateChanged = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            StartPythonBridge();
            InitializeUDPListener();
        }

        private void Update()
        {
            // Ya no hay Input de teclado aquí. 
            // Solo procesamos lo que llega por red desde el hardware real.
            if (_threadStateChanged)
            {
                _threadStateChanged = false;
                SetButtonState(_threadIsButtonPressed);
            }
        }

        private void InitializeUDPListener()
        {
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            
            IsConnected = true;
            Debug.Log($"[BLEManager] 🎧 Escuchando a Python en el puerto UDP: {udpPort}");
        }

        private void ReceiveData()
        {
            try
            {
                udpClient = new UdpClient(udpPort);
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

                while (true)
                {
                    byte[] data = udpClient.Receive(ref anyIP);
                    string text = Encoding.UTF8.GetString(data);
                    
                    if (text == "1")
                    {
                        _threadIsButtonPressed = true;
                        _threadStateChanged = true;
                    }
                    else if (text == "0")
                    {
                        _threadIsButtonPressed = false;
                        _threadStateChanged = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"[BLEManager] Error en conexión UDP: {e.Message}");
            }
        }

        public void SetButtonState(bool isPressed)
        {
            if (_isButtonPressed != isPressed)
            {
                _isButtonPressed = isPressed;
                OnButtonStateChanged?.Invoke(_isButtonPressed);
                // Hemos eliminado el Debug.Log spam del Gas ON/OFF
            }
        }

        private void OnApplicationQuit()
        {
            if (pythonProcess != null && !pythonProcess.HasExited)
            {
                pythonProcess.Kill();
                pythonProcess.Dispose();
                Debug.Log("[BLEManager] 🛑 SDK de Python cerrado.");
            }

            if (receiveThread != null) receiveThread.Abort();
            if (udpClient != null) udpClient.Close();
        }

        private void StartPythonBridge()
        {
            try
            {
                Debug.Log("[BLEManager] 🚀 Iniciando SDK de Hardware Independiente (.exe)...");
                
                pythonProcess = new Process();
                
                // 1. AHORA EJECUTAMOS EL .EXE DIRECTAMENTE
                pythonProcess.StartInfo.FileName = sdkPath; 
                
                // 2. YA NO HAY ARGUMENTOS
                pythonProcess.StartInfo.Arguments = ""; 
                
                pythonProcess.StartInfo.UseShellExecute = false;
                pythonProcess.StartInfo.CreateNoWindow = true; 
                
                pythonProcess.StartInfo.RedirectStandardOutput = true;
                pythonProcess.StartInfo.RedirectStandardError = true;
                
                pythonProcess.OutputDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) 
                        Debug.Log($"<color=cyan>[Hardware SDK]</color> {args.Data}");
                };
                pythonProcess.ErrorDataReceived += (sender, args) => {
                    if (!string.IsNullOrEmpty(args.Data)) 
                        Debug.LogError($"<color=red>[Hardware SDK ERROR]</color> {args.Data}");
                };

                pythonProcess.Start();
                pythonProcess.BeginOutputReadLine();
                pythonProcess.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                Debug.LogError($"[BLEManager] Error CRÍTICO al lanzar el SDK: {e.Message}");
            }
        }
    }
}