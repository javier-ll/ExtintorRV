using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireReactor : MonoBehaviour
{
    [Header("Configuración Vida")]
    [SerializeField] private float startHealth = 100f;
    [SerializeField] private float damagePerSecond = 20f;
    
    [Header("Audio Dinámico (Arrastra los clips aquí)")]
    [SerializeField] private AudioClip fireBigClip;    // SFX_FireBig_L
    [SerializeField] private AudioClip fireMediumClip; // SFX_FireMedium_L
    [SerializeField] private AudioClip fireSmallClip;  // SFX_FireSmall_L
    [SerializeField] private AudioClip hissClip;       // cig_extinguish (Sonido de vapor)

    private float currentHealth;
    private Vector3 initialScale;
    private AudioSource audioSource;
    private float lastHissTime = 0f; // Para no saturar el sonido de 'pshh'

    private void Start()
    {
        currentHealth = startHealth;
        initialScale = transform.localScale;
        audioSource = GetComponent<AudioSource>();
        
        // Empezar con el sonido fuerte
        if (fireBigClip != null)
        {
            audioSource.clip = fireBigClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        // Actualizar visuales
        UpdateVisuals();
        
        // Actualizar Audio (Lógica de transición)
        UpdateAudioState();
    }

    private void OnParticleCollision(GameObject other)
    {
        float damage = damagePerSecond * 0.02f;
        currentHealth -= damage;

        // Reproducir sonido de "pshhh" (Vapor) al recibir impacto
        // Usamos un pequeño delay (0.2s) para que no suene como ametralladora
        if (hissClip != null && Time.time > lastHissTime + 0.2f)
        {
            audioSource.PlayOneShot(hissClip, 0.5f); // 0.5f es el volumen
            lastHissTime = Time.time;
        }

        if (currentHealth <= 0)
        {
            Extinguish();
        }
    }

    private void UpdateVisuals()
    {
        float healthPercentage = Mathf.Clamp01(currentHealth / startHealth);
        transform.localScale = initialScale * healthPercentage;
    }

    private void UpdateAudioState()
    {
        float healthPercentage = currentHealth / startHealth;
        AudioClip targetClip = fireBigClip;

        // Decidir qué sonido corresponde según la vida
        if (healthPercentage > 0.6f)       targetClip = fireBigClip;
        else if (healthPercentage > 0.3f)  targetClip = fireMediumClip;
        else                               targetClip = fireSmallClip;

        // Solo cambiamos si el clip es diferente al actual (para no reiniciar el audio a cada frame)
        if (audioSource.clip != targetClip && targetClip != null)
        {
            float currentTime = audioSource.time; // Guardamos por donde iba la canción
            audioSource.clip = targetClip;
            audioSource.time = currentTime; // Continuamos por el mismo punto (si duran lo mismo)
            audioSource.Play();
        }
    }

    private void Extinguish()
    {
        Debug.Log("¡FUEGO APAGADO!");
        audioSource.Stop(); // Silencio total
        gameObject.SetActive(false); 
        // Aquí podrías enviar señal al ESP32 (Vibración de victoria)
    }
}