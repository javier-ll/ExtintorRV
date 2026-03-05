using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using TMPro; // Necesario para el texto UI

public class AgarrePorGestos : MonoBehaviour
{
    [Header("Referencias XRI")]
    [Tooltip("Arrastra aquí el Direct Interactor de esta mano")]
    [SerializeField] private XRDirectInteractor interactorMano;

    [Header("Configuración de Mano")]
    [SerializeField] private Handedness manoAUsar = Handedness.Right;

    [Header("Calibración del Puño (Ajustable)")]
    [Tooltip("Distancia en metros para considerar el dedo cerrado")]
    [SerializeField] private float limiteIndexFlexionado = 0.11f;
    [SerializeField] private float limiteMiddleFlexionado = 0.075f;
    [SerializeField] private float limiteRingLittleFlexionado = 0.07f;

    [Header("Telemetría / Debug")]
    [SerializeField] private TextMeshProUGUI textoDebug; // Panel de datos

    private XRHandSubsystem handSubsystem;
    private XRInteractionManager interactionManager;
    private bool estaAgarrando = false;
    private IXRSelectInteractable objetoAgarrado = null;

    private void Start()
    {
        var subsystems = new List<XRHandSubsystem>();
        SubsystemManager.GetInstances(subsystems);
        if (subsystems.Count > 0) handSubsystem = subsystems[0];

        interactionManager = FindObjectOfType<XRInteractionManager>();
    }

    private void Update()
    {
        if (handSubsystem == null || !handSubsystem.running || interactorMano == null) return;

        var targetHand = manoAUsar == Handedness.Left ? handSubsystem.leftHand : handSubsystem.rightHand;
        if (!targetHand.isTracked) return;

        // 1. Obtenemos posiciones
        targetHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wrist);
        targetHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTip);
        targetHand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out Pose middleTip);
        targetHand.GetJoint(XRHandJointID.RingTip).TryGetPose(out Pose ringTip);
        targetHand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out Pose pinkyTip);

        // 2. Medimos las distancias en metros
        float distIndex = Vector3.Distance(indexTip.position, wrist.position);
        float distMiddle = Vector3.Distance(middleTip.position, wrist.position);
        float distRing = Vector3.Distance(ringTip.position, wrist.position);
        float distPinky = Vector3.Distance(pinkyTip.position, wrist.position);

        // 3. Evaluamos si es puño
        bool esPuno = distIndex < limiteIndexFlexionado &&
                      distMiddle < limiteMiddleFlexionado &&
                      distRing < limiteRingLittleFlexionado &&
                      distPinky < limiteRingLittleFlexionado;

        // --- PANEL DE TELEMETRÍA ---
        if (textoDebug != null)
        {
            textoDebug.text = $"Umbral Índice: {limiteIndexFlexionado}\n" +
                              $"Umbral Medio: {limiteMiddleFlexionado}\n" +
                              $"Umbral Anular y Meñique: {limiteRingLittleFlexionado}\n" +
                              $"Indice: {distIndex:F3}\n" +
                              $"Medio:  {distMiddle:F3}\n" +
                              $"Anular: {distRing:F3}\n" +
                              $"Meñique:{distPinky:F3}\n" +
                              $"¿Es Puño?: {esPuno}\n" +
                              $"¿Tocando Objeto?: {interactorMano.interactablesHovered.Count > 0}";
        }

        // 4. Lógica de Agarre
        if (esPuno && !estaAgarrando)
        {
            if (interactorMano.interactablesHovered.Count > 0)
            {
                IXRSelectInteractable interactableToGrab = interactorMano.interactablesHovered[0] as IXRSelectInteractable;

                if (interactableToGrab != null)
                {
                    interactionManager.SelectEnter(interactorMano, interactableToGrab);
                    objetoAgarrado = interactableToGrab;
                    estaAgarrando = true;
                }
            }
        }
        else if (!esPuno && estaAgarrando)
        {
            if (objetoAgarrado != null)
            {
                interactionManager.SelectExit(interactorMano, objetoAgarrado);
            }
            estaAgarrando = false;
            objetoAgarrado = null;
        }
    }
}