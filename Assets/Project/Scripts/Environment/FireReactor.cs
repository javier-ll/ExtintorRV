using UnityEngine;
using System.Collections;

namespace FireSim.Environment
{
    public class FireReactor : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Vida total del fuego (0 a 100)")]
        [SerializeField] private float maxHealth = 100f;
        
        [Tooltip("Cuánto daño hace CADA partícula que toca el fuego")]
        [SerializeField] private float damagePerParticle = 0.5f;
        
        [Tooltip("Tiempo antes de regenerarse si dejas de disparar (Opcional)")]
        [SerializeField] private float regenDelay = 2.0f;

        [Header("Visuals")]
        [SerializeField] private GameObject fireVisuals; // El prefab del fuego visual

        private float _currentHealth;
        private Vector3 _initialScale;
        private ParticleSystem _myFireParticles; // Si el asset usa partículas

        private void Start()
        {
            _currentHealth = maxHealth;
            
            // Asumimos que el fuego empieza a escala 1, o guardamos la actual
            if (fireVisuals != null)
                _initialScale = fireVisuals.transform.localScale;
            else
                _initialScale = transform.localScale;

            // Intentamos obtener el sistema de partículas del fuego para apagarlo visualmente
            if(fireVisuals != null)
                _myFireParticles = fireVisuals.GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// Este método es MÁGICO. Unity lo llama automáticamente cuando
        /// una partícula con "Send Collision Messages" toca este collider.
        /// </summary>
        private void OnParticleCollision(GameObject other)
        {
            // Verificación de seguridad: ¿Es el extintor?
            // Podrías usar Tags, pero por ahora asumimos que solo el extintor tira partículas.
            
            TakeDamage(damagePerParticle);
        }

        private void TakeDamage(float amount)
        {
            _currentHealth -= amount;

            // Feedback Visual: Reducir escala
            float healthPercent = Mathf.Clamp01(_currentHealth / maxHealth);
            
            if (fireVisuals != null)
            {
                fireVisuals.transform.localScale = _initialScale * healthPercent;
            }
            else 
            {
                transform.localScale = _initialScale * healthPercent;
            }

            // Lógica de muerte
            if (_currentHealth <= 0)
            {
                Extinguish();
            }
        }

        private void Extinguish()
        {
            Debug.Log("¡Fuego Extinguido!");
            
            // Opción A: Destruir el objeto
            // Destroy(gameObject);

            // Opción B (Mejor): Desactivarlo para poder reiniciar la simulación después
            gameObject.SetActive(false);
            
            // Aquí podrías llamar al GameManger para decir "Misión Cumplida"
        }
    }
}