using UnityEngine;
using UnityEngine.EventSystems; // Necesario para detectar el Hover

[RequireComponent(typeof(AudioSource))]
public class ButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip hoverSound; // Sonido al pasar el rayo "woosh"
    [SerializeField] private AudioClip clickSound; // Sonido al pulsar "click"

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // Se ejecuta automágicamente cuando el rayo entra al botón
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverSound != null)
            audioSource.PlayOneShot(hoverSound, 0.5f); // 0.5f es volumen
    }

    // Se ejecuta al pulsar
    public void OnPointerDown(PointerEventData eventData)
    {
        if (clickSound != null)
            audioSource.PlayOneShot(clickSound, 1f);
    }
}
