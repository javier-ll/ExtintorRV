using UnityEngine;
using System;

// Namespace para organizar mejor el código y evitar conflictos
namespace FireSim.Core
{
    /// <summary>
    /// Gestiona la entrada del guante háptico vía Bluetooth (BLE).
    /// Actúa como Singleton para ser accesible globalmente.
    /// </summary>
    public class BLEManager : MonoBehaviour
    {
        // Singleton Instance
        public static BLEManager Instance { get; private set; }

        [Header("BLE Configuration")]
        [Tooltip("UUID del Servicio BLE del ESP32")]
        private const string SERVICE_UUID = "4fafc201-1fb5-459e-8fcc-c5c9c331914b";
        [Tooltip("UUID de la Característica (Botón)")]
        private const string CHARACTERISTIC_UUID = "beb5483e-36e1-4688-b7f5-ea07361b26a8";

        [Header("Debug / Simulation")]
        [Tooltip("Tecla para simular el apretar el extintor en el Editor")]
        [SerializeField] private KeyCode debugKey = KeyCode.Space;
        
        // Estado público (Solo lectura desde fuera)
        public bool IsConnected { get; private set; } = false;
        
        // Evento: Se dispara cuando cambia el estado del botón (True = Apretado, False = Soltado)
        public event Action<bool> OnButtonStateChanged;

        // Estado interno del botón
        private bool _isButtonPressed = false;
        public bool IsButtonPressed => _isButtonPressed;

        private void Awake()
        {
            // Implementación del patrón Singleton
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Persiste entre escenas
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeBLE();
        }

        private void Update()
        {
            // Lógica de simulación solo para el Editor de Unity
            #if UNITY_EDITOR
            HandleEditorInput();
            #endif
        }

        /// <summary>
        /// Inicializa la conexión Bluetooth.
        /// Aquí iría la lógica real del plugin de Android.
        /// </summary>
        private void InitializeBLE()
        {
            Debug.Log($"[BLEManager] Inicializando servicio BLE... ESPERANDO UUID: {SERVICE_UUID}");
            
            // TODO: Integrar aquí el plugin nativo (ej: ArduinoBluetoothPlugin)
            // Por ahora, simulamos una conexión exitosa falsa para probar la lógica
            IsConnected = true; 
        }

        /// <summary>
        /// Simula la entrada del guante usando el teclado.
        /// </summary>
        private void HandleEditorInput()
        {
            if (Input.GetKeyDown(debugKey))
            {
                SetButtonState(true);
            }
            else if (Input.GetKeyUp(debugKey))
            {
                SetButtonState(false);
            }
        }

        /// <summary>
        /// Método centralizado para cambiar el estado.
        /// Debe ser llamado por el callback del plugin BLE real.
        /// </summary>
        /// <param name="isPressed">Nuevo estado del botón</param>
        public void SetButtonState(bool isPressed)
        {
            // Evitamos disparar eventos si el estado no cambió realmente
            if (_isButtonPressed != isPressed)
            {
                _isButtonPressed = isPressed;
                
                // Disparamos el evento para quien esté escuchando (El Extintor)
                OnButtonStateChanged?.Invoke(_isButtonPressed);

                Debug.Log($"[BLEManager] Estado Botón: {(_isButtonPressed ? "ACTIVADO (Gas ON)" : "DESACTIVADO (Gas OFF)")}");
            }
        }
        
        // TODO: Añadir método 'OnDataReceived' cuando tengas el plugin real.
    }
}