using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class AgarrePorGestos : MonoBehaviour
{
    public enum GestoDeAgarre { Puno, Pellizco }

    [Header("Referencias XRI")]
    [SerializeField] private XRDirectInteractor interactorMano;

    [Header("Configuración de Mano")]
    [SerializeField] private Handedness manoAUsar = Handedness.Right;
    [Tooltip("Elige qué gesto activa el agarre en esta mano")]
    [SerializeField] private GestoDeAgarre gestoRequerido = GestoDeAgarre.Puno;

    [Header("Calibración Matemática")]
    [SerializeField] private float limiteIndexFlexionado = 0.11f;
    [SerializeField] private float limiteMiddleFlexionado = 0.08f;
    [SerializeField] private float limiteRingLittleFlexionado = 0.07f;
    [Tooltip("Distancia en metros para el pellizco")]
    [SerializeField] private float distanciaPellizco = 0.02f;

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

        targetHand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wrist);
        targetHand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTip);
        targetHand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTip);
        targetHand.GetJoint(XRHandJointID.MiddleTip).TryGetPose(out Pose middleTip);
        targetHand.GetJoint(XRHandJointID.RingTip).TryGetPose(out Pose ringTip);
        targetHand.GetJoint(XRHandJointID.LittleTip).TryGetPose(out Pose pinkyTip);

        // Medir distancias
        float distIndex = Vector3.Distance(indexTip.position, wrist.position);
        float distMiddle = Vector3.Distance(middleTip.position, wrist.position);
        float distRing = Vector3.Distance(ringTip.position, wrist.position);
        float distPinky = Vector3.Distance(pinkyTip.position, wrist.position);
        float distPinch = Vector3.Distance(thumbTip.position, indexTip.position);

        // Evaluar qué gesto se está pidiendo
        bool gestoActivado = false;

        if (gestoRequerido == GestoDeAgarre.Puno)
        {
            gestoActivado = distIndex < limiteIndexFlexionado &&
                            distMiddle < limiteMiddleFlexionado &&
                            distRing < limiteRingLittleFlexionado &&
                            distPinky < limiteRingLittleFlexionado;
        }
        else if (gestoRequerido == GestoDeAgarre.Pellizco)
        {
            gestoActivado = distPinch < distanciaPellizco;
        }

        // Lógica de Agarre
        if (gestoActivado && !estaAgarrando)
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
        else if (!gestoActivado && estaAgarrando)
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