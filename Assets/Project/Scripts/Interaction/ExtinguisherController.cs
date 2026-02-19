using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using FireSim.Core; 

namespace FireSim.Interaction
{
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
    public class ExtinguisherController : MonoBehaviour
    {
        [Header("Componentes")]
        [SerializeField] private ParticleSystem foamParticles;
        [SerializeField] private GameObject safetyPin; 
        
        [Header("Configuración")]
        [SerializeField] private float emissionRate = 300f;

        // Estado
        private bool _isHeld = false;
        private bool _isTriggerPressed = false;
        private bool _isSpraying = false;
        private bool _isPinRemoved = false;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grabInteractable;

        private void Awake()
        {
            _grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
            if (foamParticles == null)
            {
                Debug.LogError($"[Extinguisher] Falta asignar el ParticleSystem en {gameObject.name}");
                enabled = false;
            }
        }

        private void Start()
        {
            _grabInteractable.selectEntered.AddListener(OnGrabbed);
            _grabInteractable.selectExited.AddListener(OnReleased);

            if (BLEManager.Instance != null)
            {
                BLEManager.Instance.OnButtonStateChanged += HandleTriggerState;
                _isTriggerPressed = BLEManager.Instance.IsButtonPressed;
            }
        }

        private void OnDestroy()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
                _grabInteractable.selectExited.RemoveListener(OnReleased);
            }

            if (BLEManager.Instance != null)
            {
                BLEManager.Instance.OnButtonStateChanged -= HandleTriggerState;
            }
        }

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
            UpdateSprayState(); // Se eliminó el Debug.Log spam
        }

        public void RemovePin()
        {
            if (!_isPinRemoved)
            {
                _isPinRemoved = true;
                if(safetyPin != null) safetyPin.SetActive(false); 
                // Se eliminó el Debug.Log del seguro
            }
        }

        private void UpdateSprayState()
        {
            // ACTUAL: Dispara directo (ideal para probar sin ponerse el casco)
            bool shouldSpray = _isTriggerPressed; 
            
            // PARA EL PRODUCTO FINAL CON VR: Borra la línea de arriba y quítale las barras a la línea de abajo
            // bool shouldSpray = _isHeld && _isTriggerPressed && _isPinRemoved;

            if (shouldSpray != _isSpraying)
            {
                _isSpraying = shouldSpray;
                ToggleParticles(_isSpraying);
            }
        }

        private void ToggleParticles(bool active)
        {
            // Accedemos al módulo de emisión
            var emission = foamParticles.emission;
            
            // Simplemente abrimos o cerramos la "llave de paso" del gas
            emission.enabled = active;

            // Solo necesitamos darle Play la primerísima vez que se usa
            if (active && !foamParticles.isPlaying)
            {
                foamParticles.Play();
            }
        }
    }
}