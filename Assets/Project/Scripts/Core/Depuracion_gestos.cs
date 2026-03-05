using UnityEngine;
using TMPro;
using UnityEngine.XR.Hands;

public class Depuracion_gestos : MonoBehaviour
{
    [SerializeField] private TMP_Text debugText;
    private XRHandSubsystem handSubsystem;
    private enum EstadoTeletransporte { Reposo, Apuntando, Pellizcando }
    private EstadoTeletransporte estadoActual = EstadoTeletransporte.Reposo;

    [Header("Configuración Matemática (Ajustable)")]
    [SerializeField] private float distanciaPellizco = 0.025f; // 2.5 cm de distancia entre yemas
    [SerializeField] private float limiteExtendido = 0.09f;    // Distancia mínima punta-muñeca para estar "estirado"
    [SerializeField] private float limiteFlexionado = 0.07f;   // Distancia máxima punta-muñeca para estar "cerrado"

    [Range(-0.5f, 1f)]
    [SerializeField] private float umbralPalmaArriba = 0.0f;

    private void Start()
    {
        var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
        SubsystemManager.GetInstances(subsystems);

        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
            debugText.text = "SISTEMA OK. Esperando gesto...";
            debugText.color = Color.yellow;
        }
    }

    private void Update()
    {
        if (handSubsystem != null && handSubsystem.running)
        {
            var rightHand = handSubsystem.rightHand;
            if (!rightHand.isTracked) return;

            // 1. Obtenemos las posiciones espaciales de los huesos clave
            rightHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wrist);
            rightHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTip);
            rightHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTip);
            rightHand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out Pose middleTip);
            rightHand.GetJoint(XRHandJointID.RingTip).TryGetPose(out Pose ringTip);
            rightHand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out Pose pinkyTip);
            rightHand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palm);

            // 2. Calculamos las distancias de los dedos a la muñeca
            float distIndex = Vector3.Distance(indexTip.position, wrist.position);
            float distMiddle = Vector3.Distance(middleTip.position, wrist.position);
            float distRing = Vector3.Distance(ringTip.position, wrist.position);
            float distPinky = Vector3.Distance(pinkyTip.position, wrist.position);

            // Distancia entre yemas para el pellizco
            float distPinch = Vector3.Distance(thumbTip.position, indexTip.position);

            // 3. Evaluamos el Gesto: Pellizco
            bool esPellizco = distPinch < distanciaPellizco;

            // 4. Evaluamos el Gesto: Pistola
            bool indiceExtendido = distIndex > limiteExtendido;
            bool otrosFlexionados = distMiddle < limiteFlexionado &&
                                    distRing < limiteFlexionado &&
                                    distPinky < limiteFlexionado;

            bool esPistola = indiceExtendido && otrosFlexionados && !esPellizco;

            // 5. Evaluamos la orientación: "Palma hacia arriba" (AHORA ERGONÓMICO)
            // Cambiamos el 0.4f fijo por nuestra nueva variable ajustable
            bool palmaArriba = Vector3.Dot(palm.up, Vector3.down) > umbralPalmaArriba;

            // 6. MÁQUINA DE ESTADOS (Adiós a los milisegundos en blanco)
            switch (estadoActual)
            {
                case EstadoTeletransporte.Reposo:
                    // Solo salimos del reposo si hacemos la pistola perfecta
                    if (esPistola && palmaArriba)
                    {
                        estadoActual = EstadoTeletransporte.Apuntando;
                    }
                    else
                    {
                        debugText.text = "Descanso...\n(Haz pistola arriba)";
                        debugText.color = Color.white;
                    }
                    break;

                case EstadoTeletransporte.Apuntando:
                    // Si estamos apuntando y detecta el pellizco, avanzamos
                    if (esPellizco)
                    {
                        estadoActual = EstadoTeletransporte.Pellizcando;
                    }
                    // CONDICIÓN DE CANCELACIÓN: Solo volvemos a reposo si giras la palma hacia abajo
                    // o si abres los demás dedos (cancelación intencional del usuario).
                    // El viaje del índice ya no cancela el estado.
                    else if (!palmaArriba || distMiddle > limiteExtendido)
                    {
                        estadoActual = EstadoTeletransporte.Reposo;
                    }
                    else
                    {
                        debugText.text = "¡APUNTANDO!\n(Pellizca para saltar)";
                        debugText.color = Color.green;
                    }
                    break;

                case EstadoTeletransporte.Pellizcando:
                    // Mantenemos el estado de pellizco hasta que separes los dedos
                    if (!esPellizco)
                    {
                        // Al soltar, volvemos a reposo limpio
                        estadoActual = EstadoTeletransporte.Reposo;
                    }
                    else
                    {
                        debugText.text = "¡PELLIZCO!\n(Teletransporte ejecutado)";
                        debugText.color = Color.cyan;
                    }
                    break;
            }
        }
    }
}