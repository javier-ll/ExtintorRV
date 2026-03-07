using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PrecintoSeguridad : MonoBehaviour
{
    public bool precintoQuitado = false;
    private XRBaseInteractable interactable;
    private AudioSource audioSource;
    private bool yaSono = false;
    private Rigidbody rb;

    [Header("Físicas")]
    [Tooltip("Arrastra aquí el Box Collider del Precinto que NO tiene marcado Is Trigger")]
    [SerializeField] private Collider colisionadorSolido;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        if(audioSource != null) audioSource.playOnAwake = false;

        // Apagamos el colisionador sólido al inicio para que no pelee con el extintor
        if (colisionadorSolido != null) colisionadorSolido.enabled = false;
    }

    public void ArrancarPrecinto()
    {
        if (precintoQuitado) return;

        precintoQuitado = true;
        Debug.Log("¡Precinto arrancado! El extintor está listo para usarse.");

        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // ¡Encendemos el colisionador para que pueda chocar contra el piso!
        if (colisionadorSolido != null) colisionadorSolido.enabled = true;

        if (interactable != null) interactable.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (precintoQuitado && audioSource != null && !yaSono)
        {
            audioSource.Play();
            yaSono = true;
        }
    }
}