using UnityEngine;
using UnityEngine.InputSystem;

public class ExtintorJoystick : MonoBehaviour
{
    public ParticleSystem particulasGas;
    public bool estaAgarrado = false;

    [Tooltip("Nos dice si ya se retiró la traba de seguridad")]
    public bool precintoRetirado = false;

    void Start()
    {
        if (particulasGas != null) particulasGas.Stop();
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            // CONDICIÓN TRIPLE: Tecla G + Agarrado + Precinto Sacado
            if (Keyboard.current.gKey.wasPressedThisFrame && estaAgarrado && precintoRetirado)
            {
                if (particulasGas != null && !particulasGas.isPlaying)
                {
                    particulasGas.Play();
                    Debug.Log("💨 Gas liberado (Condiciones de seguridad cumplidas)");
                }
            }

            // DETENER: Si suelta el botón, o suelta el extintor
            if (Keyboard.current.gKey.wasReleasedThisFrame || (!estaAgarrado && particulasGas.isPlaying))
            {
                if (particulasGas != null && particulasGas.isPlaying)
                {
                    particulasGas.Stop();
                }
            }
        }
    }

    public void ExtintorAgarrado() { estaAgarrado = true; }
    public void ExtintorSoltado() { estaAgarrado = false; }

    // NUEVO MÉTODO: Esto lo va a llamar el precinto cuando lo arranquemos
    public void QuitarPrecinto()
    {
        precintoRetirado = true;
        Debug.Log("🔓 Precinto retirado. ¡Extintor armado y listo!");
    }
}