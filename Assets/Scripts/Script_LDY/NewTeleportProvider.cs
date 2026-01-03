using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;

public class UniversalTeleport : TeleportationProvider
{
    protected override void Update()
    {
        if (!validRequest)
        {
            return;
        }

        Debug.Log("Teleport request received. Moving...");

        XROrigin xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("XROrigin not found in scene. Please check if XROrigin is loaded.");
            return;
        }

        Vector3 targetPos = currentRequest.destinationPosition;

        Vector3 camPos = xrOrigin.Camera.transform.position;
        Vector3 originPos = xrOrigin.Origin.transform.position;
        Vector3 camOffset = camPos - originPos;
        camOffset.y = 0;

        Vector3 finalOriginPos = targetPos - camOffset;
        finalOriginPos.y = targetPos.y;

        xrOrigin.Origin.transform.position = finalOriginPos;

        if (currentRequest.matchOrientation == MatchOrientation.TargetUpAndForward)
        {
            float currentRotY = xrOrigin.Camera.transform.eulerAngles.y;
            float targetRotY = currentRequest.destinationRotation.eulerAngles.y;
            float angleDiff = targetRotY - currentRotY;

            xrOrigin.Origin.transform.Rotate(0, angleDiff, 0);
        }

        validRequest = false;
    }
}