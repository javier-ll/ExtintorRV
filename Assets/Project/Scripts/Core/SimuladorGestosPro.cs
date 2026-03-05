using UnityEngine;
using UnityEngine.XR.Hands;

public class SimuladorGestosPro : MonoBehaviour
{
    [Header("Referencias de Interacción")]
    [Tooltip("Arrastra aquí el objeto del Rayo de Teletransporte")]
    [SerializeField] private GameObject teleportInteractorObj;

    [Header("Configuración de Mano")]
    [SerializeField] private Handedness manoAUsar = Handedness.Right;

    // --- VARIABLES MATEMÁTICAS HARDCODEADAS ---
    // (Ocultas del Inspector por seguridad de diseño)
    private float distanciaPellizco = 0.025f;
    private float limiteExtendido = 0.09f;
    private float limiteFlexionado = 0.08f; // Actualizado para una mano más relajada
    private float umbralPalmaArriba = 0.0f;

    private XRHandSubsystem handSubsystem;
    private enum EstadoTeletransporte { Reposo, Apuntando, Pellizcando }
    private EstadoTeletransporte estadoActual = EstadoTeletransporte.Reposo;

    private void Start()
    {
        var subsystems = new System.Collections.Generic.List<XRHandSubsystem>();
        SubsystemManager.GetInstances(subsystems);

        if (subsystems.Count > 0)
        {
            handSubsystem = subsystems[0];
        }

        if (teleportInteractorObj != null)
        {
            teleportInteractorObj.SetActive(false);
        }
    }

    private void Update()
    {
        if (handSubsystem != null && handSubsystem.running)
        {
            var targetHand = manoAUsar == Handedness.Left ? handSubsystem.leftHand : handSubsystem.rightHand;

            if (!targetHand.isTracked) return;

            // 1. Obtenemos las posiciones espaciales de los huesos clave
            targetHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wrist);
            targetHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTip);
            targetHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTip);
            targetHand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out Pose middleTip);
            targetHand.GetJoint(XRHandJointID.RingTip).TryGetPose(out Pose ringTip);
            targetHand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out Pose pinkyTip);
            targetHand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palm);

            // 2. Calculamos las distancias de los dedos a la muñeca
            float distIndex = Vector3.Distance(indexTip.position, wrist.position);
            float distMiddle = Vector3.Distance(middleTip.position, wrist.position);
            float distRing = Vector3.Distance(ringTip.position, wrist.position);
            float distPinky = Vector3.Distance(pinkyTip.position, wrist.position);

            // Distancia entre yemas para el pellizco
            float distPinch = Vector3.Distance(thumbTip.position, indexTip.position);

            // 3. Evaluamos booleanos
            bool esPellizco = distPinch < distanciaPellizco;

            bool indiceExtendido = distIndex > limiteExtendido;
            bool otrosFlexionados = distMiddle < limiteFlexionado &&
                                    distRing < limiteFlexionado &&
                                    distPinky < limiteFlexionado;

            bool esPistola = indiceExtendido && otrosFlexionados && !esPellizco;

            // Ángulo de la palma
            float dotPalma = Vector3.Dot(palm.up, Vector3.down);
            bool palmaArriba = dotPalma > umbralPalmaArriba;

            // 4. MÁQUINA DE ESTADOS
            switch (estadoActual)
            {
                case EstadoTeletransporte.Reposo:
                    if (esPistola && palmaArriba)
                    {
                        estadoActual = EstadoTeletransporte.Apuntando;
                        if (teleportInteractorObj != null) teleportInteractorObj.SetActive(true);
                    }
                    break;

                case EstadoTeletransporte.Apuntando:
                    if (esPellizco)
                    {
                        estadoActual = EstadoTeletransporte.Pellizcando;
                    }
                    else if (!palmaArriba || distMiddle > limiteExtendido)
                    {
                        estadoActual = EstadoTeletransporte.Reposo;
                        if (teleportInteractorObj != null) teleportInteractorObj.SetActive(false);
                    }
                    break;

                case EstadoTeletransporte.Pellizcando:
                    if (!esPellizco)
                    {
                        estadoActual = EstadoTeletransporte.Reposo;
                        if (teleportInteractorObj != null) teleportInteractorObj.SetActive(false);
                    }
                    break;
            }
        }
    }
}