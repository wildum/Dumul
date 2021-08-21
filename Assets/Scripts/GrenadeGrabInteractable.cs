using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrenadeGrabInteractable : XRGrabInteractable
{
    public Transform attachPointRight;
    public Transform attachPointLeft;

    protected override void OnSelectEntering(XRBaseInteractor interactor)
    {
        attachTransform = interactor.gameObject.GetComponentInChildren<HandPresence>().side == HandSideEnum.Right ? attachPointRight : attachPointLeft;
        base.OnSelectEntering(interactor);
    }
}