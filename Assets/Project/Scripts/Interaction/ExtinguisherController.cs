using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using FireSim.Core; // Necesario para ver el BLEManager

namespace FireSim.Interaction
{
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class ExtinguisherController : MonoBehaviour
    {
        [Header("Componentes")]
        [SerializeField] private ParticleSystem foamParticles;
        [SerializeField] private GameObject safetyPin; // Arrastra aquí el objeto visual del seguro
        
        [Header("Configuración")]
        [SerializeField] private float emissionRate = 300f;

        // Estado
        private bool _isHeld = false;
        private bool _isTriggerPressed = false;
        private bool _isSpraying = false;
        
        // NUEVO: Estado del seguro
        private bool _isPinRemoved = false;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
            // Validación de seguridad (Clean Code)
            if (foamParticles == null)
            {
                Debug.LogError($"[Extinguisher] Falta asignar el ParticleSystem en {gameObject.name}");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            // 1. Nos suscribimos a eventos de XR (Agarre)
            // XRIT actualizado usa selectEntered/selectExited
            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);

            // 2. Nos suscribimos al evento del Hardware (BLE)
            // Verificamos que el Manager exista para evitar errores en Edit Mode
            if (BLEManager.Instance != null)
            {
                BLEManager.Instance.OnButtonStateChanged += HandleTriggerState;
                // Sincronizar estado inicial por si ya estaba apretado
                _isTriggerPressed = BLEManager.Instance.IsButtonPressed;
            }
        }

        private void OnDisable()
        {
            // Siempre desuscribir eventos para evitar Memory Leaks
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);

            if (BLEManager.Instance != null)
            {
                BLEManager.Instance.OnButtonStateChanged -= HandleTriggerState;
            }
        }

        // --- Event Handlers (Input) ---

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            _isHeld = true;
            UpdateSprayState();
        }

        private void OnReleased(SelectExitEventArgs args)
        {
            _isHeld = false;
            UpdateSprayState();
        }

        private void HandleTriggerState(bool isPressed)
        {
            _isTriggerPressed = isPressed;
            UpdateSprayState();
        }

        public void RemovePin()
        {
            if (!_isPinRemoved)
            {
                _isPinRemoved = true;
                
                // Feedback visual/sonoro
                if(safetyPin != null) 
                {
                    safetyPin.SetActive(false); // Ocultar el pin (como si lo hubieras sacado)
                    // Aquí podrías reproducir el sonido 'fire-extinguisher-pin-fall'
                }
                
                Debug.Log("[Extinguisher] Seguro retirado. ¡Armado!");
            }
        }

        // Modificamos la lógica de disparo
        private void UpdateSprayState()
        {
            // AHORA: Solo dispara si está Agarrado + Gatillo + SIN SEGURO
            bool shouldSpray = _isHeld && _isTriggerPressed && _isPinRemoved;

            if (shouldSpray != _isSpraying)
            {
                _isSpraying = shouldSpray;
                ToggleParticles(_isSpraying);
            }
        }

        private void ToggleParticles(bool active)
        {
            var emission = foamParticles.emission;
            emission.rateOverTime = active ? emissionRate : 0f;
            
            // Opcional: Feedback háptico en el control VR si quisieras añadirlo aquí
            // (Aunque tu guante ya es háptico físico).
        }
    }
}