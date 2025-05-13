using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class XRTerrainTilt : MonoBehaviour
{
    public float tiltAngle = 10f;           //Auto tilt desired angle in degrees
    public float tiltDuration = 3f;         //Time to reach max set automated tilt value
    public float manualTiltSpeed = 20f;     //Speed of Manual Tilt in Seconds
    public float autoTiltDelay = 1f;       //Time in seconds until auto tilt starts

    private Quaternion originalRotation;
    private bool isTilting = false;

    void Start()
    {
        originalRotation = transform.rotation;
        if (autoTiltDelay >= 0) {
            StartCoroutine(AutoTiltSequence());
        }
    }

    void Update()
    {
        HandleManualTilt();
    }

    void HandleManualTilt()
    {
        float input = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
            input = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed)
            input = 1f;

        if (input != 0f)
        {
            transform.Rotate(0f, 0f, input * manualTiltSpeed * Time.deltaTime, Space.Self);
        }
    }

    IEnumerator AutoTiltSequence()
    {
        yield return new WaitForSeconds(autoTiltDelay);

        if (!isTilting)
        {
            yield return TiltRoutine(tiltAngle);
            yield return TiltRoutine(-tiltAngle);
        }
    }

    IEnumerator TiltRoutine(float targetZ)
    {
        isTilting = true;
        Quaternion targetRotation = originalRotation * Quaternion.Euler(0f, 0f, targetZ);
        float elapsedTime = 0f;

        while (elapsedTime < tiltDuration)
        {
            transform.rotation = Quaternion.Slerp(originalRotation, targetRotation, elapsedTime / tiltDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        yield return new WaitForSeconds(tiltDuration);

        elapsedTime = 0f;
        while (elapsedTime < tiltDuration)
        {
            transform.rotation = Quaternion.Slerp(targetRotation, originalRotation, elapsedTime / tiltDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = originalRotation;
        isTilting = false;
    }
}

