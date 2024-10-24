using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // This ensures the camera component exists
public class DayNightController : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 240f; // Full day-night cycle duration in seconds
    [Range(0, 1)]
    public float timeOfDay = 0.5f; // 0-1 represents the full day cycle
    public bool isPaused = false;

    [Header("Visual Settings")]
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f, 0.5f);
    [Range(0, 5)]
    public float nightIntensity = 1f;

    private Material postProcessingMaterial;
    private Camera cam;

    void Start()
    {
        // Get the camera component
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("DayNightController needs to be attached to a Camera!");
            return;
        }

        // Create material from shader
        Shader shader = Shader.Find("Custom/DayNightShader");
        if (shader != null)
        {
            postProcessingMaterial = new Material(shader);
            // Ensure the initial properties are set
            UpdateShaderProperties();
        }
        else
        {
            Debug.LogError("DayNightShader not found! Make sure the shader is in your project.");
        }
    }

    void Update()
    {
        if (!isPaused)
        {
            // Update time of day
            timeOfDay += Time.deltaTime / dayDuration;
            if (timeOfDay >= 1f)
                timeOfDay = 0f;

            // Update shader properties whenever time changes
            UpdateShaderProperties();
        }
    }

    void UpdateShaderProperties()
    {
        if (postProcessingMaterial != null)
        {
            postProcessingMaterial.SetFloat("_TimeOfDay", timeOfDay);
            postProcessingMaterial.SetColor("_NightColor", nightColor);
            postProcessingMaterial.SetFloat("_NightIntensity", nightIntensity);
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

    // Optional: Add methods to control the time of day
    public void SetTimeOfDay(float time)
    {
        timeOfDay = Mathf.Clamp01(time);
        UpdateShaderProperties();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }
}