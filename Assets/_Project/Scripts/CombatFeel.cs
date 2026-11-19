using System.Collections;
using UnityEngine;

public class CombatFeel : MonoBehaviour
{
    public static CombatFeel Instance { get; private set; }

    [Header("Hit-Stop Settings")]
    [SerializeField] private float defaultHitStop = 0.035f;

    [Header("Smooth Camera Shake Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float shakeFrequency = 38f; 

    private Coroutine hitStopRoutine;
    private Coroutine shakeRoutine;
    private Vector3 currentShakeOffset = Vector3.zero;
    private Vector3 lastAppliedOffset = Vector3.zero;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

     
        cameraTransform.localPosition -= lastAppliedOffset;
        cameraTransform.localPosition += currentShakeOffset;
        lastAppliedOffset = currentShakeOffset;
    }

    public void TriggerImpact(float hitStopDuration = -1f, float shakeIntensity = 0.09f, float shakeDuration = 0.14f, float hitDirectionX = 0f)
    {
        float stopTime = hitStopDuration < 0f ? defaultHitStop : hitStopDuration;

        if (stopTime > 0f)
        {
            if (hitStopRoutine != null) StopCoroutine(hitStopRoutine);
            hitStopRoutine = StartCoroutine(HitStopCoroutine(stopTime));
        }

        if (shakeIntensity > 0f && cameraTransform != null)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(SmoothShakeCoroutine(shakeIntensity, shakeDuration, hitDirectionX));
        }
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        Time.timeScale = 0.08f; 
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        hitStopRoutine = null;
    }

    private IEnumerator SmoothShakeCoroutine(float intensity, float duration, float hitDirectionX)
    {
        float elapsed = 0f;
        float dir = hitDirectionX == 0f ? (Random.value > 0.5f ? 1f : -1f) : Mathf.Sign(hitDirectionX);

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
       
            float damper = Mathf.Pow(1f - progress, 2.2f);
            float wave = Mathf.Cos(elapsed * shakeFrequency);

            float offsetX = dir * wave * intensity * damper;
            float offsetY = Mathf.Sin(elapsed * shakeFrequency * 0.8f) * (intensity * 0.45f) * damper;

            currentShakeOffset = new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        currentShakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}