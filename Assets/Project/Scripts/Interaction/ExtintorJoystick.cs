using UnityEngine;

public class ExtintorJoystick : MonoBehaviour
{
    public ParticleSystem particulasGas;

    void Update()
    {
        // Esto detecta el botón del guante sin importar si usas manos o mandos
        if (Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetButtonDown("Fire1"))
        {
            if (particulasGas != null)
            {
                particulasGas.Play();
                Debug.Log("Gas activado");
            }
        }

        if (Input.GetKeyUp(KeyCode.JoystickButton0) || Input.GetButtonUp("Fire1"))
        {
            if (particulasGas != null)
            {
                particulasGas.Stop();
            }
        }
    }
}