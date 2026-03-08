using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Android.BLE;
using Android.BLE.Commands;

public class ExtintorJoystick : MonoBehaviour
{
    [Header("Componentes Visuales")]
    public ParticleSystem particulasGas;
    public XRGrabInteractable interactableExtintor;
    public PrecintoSeguridad scriptPrecinto;
    public AudioSource audioGas;

    [Header("Configuración BLE")]
    public string macDelESP32 = "88:56:A6:6E:E0:3E"; // ¡Recuerda poner tu MAC!
    public int tiempoEscaneoMs = 10000;

    private string serviceUUID = "12345678-1234-5678-1234-56789abcdef0";
    private string txUUID = "12345678-1234-5678-1234-56789abcdef1";
    private string rxUUID = "12345678-1234-5678-1234-56789abcdef2";

    private bool motorEncendido = false;
    private bool gatilloPresionadoFisicamente = false;
    private bool bleConectado = false;

    void Start()
    {
        if (particulasGas != null) particulasGas.Stop();
        if (audioGas != null) audioGas.Stop();

        // El BleManager de esta escena tiene "Initialize On Awake",
        // le damos 1.5 segundos a Java para arrancar antes de pedirle que escanee.
        Invoke("EmpezarEscaneo", 1.5f);
    }

    public void EmpezarEscaneo()
    {
        if (BleManager.Instance == null)
        {
            Debug.LogError("BLE: ¡CRÍTICO! Falta el objeto BLE_Manager en la escena Fábrica.");
            return;
        }

        Debug.LogError("BLE: Iniciando Escaneo de dispositivos (DiscoverDevices)...");
        BleManager.Instance.QueueCommand(new DiscoverDevices(OnDispositivoDescubierto, tiempoEscaneoMs));
    }

    private void OnDispositivoDescubierto(string deviceAddress, string deviceName)
    {
        Debug.Log($"BLE: Encontrado -> {deviceName} [{deviceAddress}]");

        if (deviceAddress == macDelESP32)
        {
            Debug.LogError("BLE: ¡ESP32 ENCONTRADO! Ordenando conexión...");
            BleManager.Instance.QueueCommand(new ConnectToDevice(macDelESP32, OnConexionExitosa, OnDesconexion));
        }
    }

    private void OnConexionExitosa(string deviceAddress)
    {
        bleConectado = true;
        Debug.LogError("BLE: ¡EXITO! Extintor conectado. Suscribiéndose al botón...");
        BleManager.Instance.QueueCommand(new SubscribeToCharacteristic(macDelESP32, serviceUUID, txUUID, OnBotonRecibido, true));
    }

    private void OnDesconexion(string deviceAddress)
    {
        bleConectado = false;
        gatilloPresionadoFisicamente = false;
        Debug.LogError("BLE: ERROR - Se desconectó el ESP32.");
        ApagarMotor();
    }

    private void OnBotonRecibido(byte[] data)
    {
        string mensaje = System.Text.Encoding.ASCII.GetString(data);

        if (mensaje == "1") {
            gatilloPresionadoFisicamente = true;
            Debug.LogError("¡ATENCIÓN! -> ESP32 DICE: BOTON PRESIONADO (1)");
        } else if (mensaje == "0") {
            gatilloPresionadoFisicamente = false;
            Debug.LogError("¡ATENCIÓN! -> ESP32 DICE: BOTON SOLTADO (0)");
        }
    }

    void Update()
    {
        bool estaAgarrado = interactableExtintor != null && interactableExtintor.isSelected;
        bool precintoRetirado = scriptPrecinto != null && scriptPrecinto.precintoQuitado;

        bool puedeDisparar = gatilloPresionadoFisicamente && estaAgarrado && precintoRetirado;

        if (puedeDisparar)
        {
            if (particulasGas != null && !particulasGas.isPlaying)
            {
                particulasGas.Play();
                if (audioGas != null && !audioGas.isPlaying) audioGas.Play();
                EncenderMotor();
            }
        }
        else
        {
            if (particulasGas != null && particulasGas.isPlaying)
            {
                particulasGas.Stop();
                if (audioGas != null && audioGas.isPlaying) audioGas.Stop();
                ApagarMotor();
            }
        }
    }

    private void EncenderMotor()
    {
        if (motorEncendido || !bleConectado) return;
        motorEncendido = true;
        byte[] dataOn = System.Text.Encoding.ASCII.GetBytes("1");
        BleManager.Instance.QueueCommand(new WriteToCharacteristic(macDelESP32, serviceUUID, rxUUID, dataOn, true));
    }

    private void ApagarMotor()
    {
        if (!motorEncendido || !bleConectado) return;
        motorEncendido = false;
        byte[] dataOff = System.Text.Encoding.ASCII.GetBytes("0");
        BleManager.Instance.QueueCommand(new WriteToCharacteristic(macDelESP32, serviceUUID, rxUUID, dataOff, true));
    }
}