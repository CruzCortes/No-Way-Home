using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DayNightController : MonoBehaviour
{
    [System.Serializable]
    public class TimePhase
    {
        public string name;
        public float startTime; // 0-1 range
        public float duration;  // in seconds
        public Color skyColor;
        public float lightIntensity;
    }

    [Header("Time Settings")]
    public float dayDuration = 240f;
    [Range(0, 1)]
    public float timeOfDay = 0.5f;
    public bool isPaused = false;

    [Header("Time Phases")]
    public TimePhase[] timePhases;

    [Header("Visual Settings")]
    public float transitionDuration = 1f; // Duration of transition between phases in seconds

    private Material postProcessingMaterial;
    private Camera cam;
    private TimePhase currentPhase;
    private TimePhase nextPhase;
    private float transitionProgress = 1f; // 1 means no transition is happening

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("DayNightController needs to be attached to a Camera!");
            return;
        }

        // Initialize default time phases if none are set
        if (timePhases == null || timePhases.Length == 0)
        {
            InitializeDefaultPhases();
        }

        // Create material from shader
        Shader shader = Shader.Find("Custom/DayNightShader");
        if (shader != null)
        {
            postProcessingMaterial = new Material(shader);
            UpdateShaderProperties();
        }
        else
        {
            Debug.LogError("DayNightShader not found!");
        }
    }

    void InitializeDefaultPhases()
    {
        timePhases = new TimePhase[]
        {
            new TimePhase
            {
                name = "Dawn",
                startTime = 0.0f,
                duration = dayDuration * 0.1f,
                skyColor = new Color(0.7f, 0.7f, 0.9f, 0.3f),
                lightIntensity = 0.3f
            },
            new TimePhase
            {
                name = "Morning",
                startTime = 0.2f,
                duration = dayDuration * 0.2f,
                skyColor = new Color(1f, 1f, 1f, 0f),
                lightIntensity = 0f
            },
            new TimePhase
            {
                name = "Day",
                startTime = 0.3f,
                duration = dayDuration * 0.4f,
                skyColor = new Color(1f, 1f, 1f, 0f),
                lightIntensity = 0f
            },
            new TimePhase
            {
                name = "Sunset",
                startTime = 0.7f,
                duration = dayDuration * 0.1f,
                skyColor = new Color(0.9f, 0.6f, 0.3f, 0.4f),
                lightIntensity = 0.4f
            },
            new TimePhase
            {
                name = "Night",
                startTime = 0.8f,
                duration = dayDuration * 0.2f,
                skyColor = new Color(0.1f, 0.1f, 0.2f, 1f),
                lightIntensity = 1f
            }
        };
    }

    void Update()
    {
        if (!isPaused)
        {
            // Update time of day
            timeOfDay += Time.deltaTime / dayDuration;
            if (timeOfDay >= 1f)
                timeOfDay = 0f;

            UpdateCurrentPhase();
            UpdateTransition();
            UpdateShaderProperties();
        }
    }

    void UpdateCurrentPhase()
    {
        // Find current and next phase
        TimePhase newCurrentPhase = null;
        TimePhase newNextPhase = null;

        for (int i = 0; i < timePhases.Length; i++)
        {
            if (timeOfDay >= timePhases[i].startTime &&
                (i == timePhases.Length - 1 || timeOfDay < timePhases[i + 1].startTime))
            {
                newCurrentPhase = timePhases[i];
                newNextPhase = timePhases[(i + 1) % timePhases.Length];
                break;
            }
        }

        // If we're entering a new phase, start transition
        if (newCurrentPhase != currentPhase)
        {
            currentPhase = newCurrentPhase;
            nextPhase = newNextPhase;
            transitionProgress = 0f;
        }
    }

    void UpdateTransition()
    {
        if (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionDuration;
            transitionProgress = Mathf.Min(transitionProgress, 1f);
        }
    }

    void UpdateShaderProperties()
    {
        if (postProcessingMaterial != null && currentPhase != null)
        {
            Color currentColor = currentPhase.skyColor;
            float currentIntensity = currentPhase.lightIntensity;

            // If in transition, lerp between current and next phase
            if (transitionProgress < 1f && nextPhase != null)
            {
                currentColor = Color.Lerp(currentPhase.skyColor, nextPhase.skyColor, transitionProgress);
                currentIntensity = Mathf.Lerp(currentPhase.lightIntensity, nextPhase.lightIntensity, transitionProgress);
            }

            postProcessingMaterial.SetColor("_NightColor", currentColor);
            postProcessingMaterial.SetFloat("_NightIntensity", currentIntensity);
            postProcessingMaterial.SetFloat("_TimeOfDay", timeOfDay);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (postProcessingMaterial != null)
        {
            Graphics.Blit(source, destination, postProcessingMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

    public void SetTimeOfDay(float time)
    {
        timeOfDay = Mathf.Clamp01(time);
        UpdateCurrentPhase();
        UpdateShaderProperties();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    // Helper method to get current phase name
    public string GetCurrentPhaseName()
    {
        return currentPhase?.name ?? "Unknown";
    }
}