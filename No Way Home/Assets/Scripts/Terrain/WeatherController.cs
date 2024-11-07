using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    [Header("Wind Settings")]
    public float windSpeed = 1f;
    public float windChangeSpeed = 0.2f;
    public Vector2 windDirection = Vector2.right;
    public float turbulence = 0.5f;

    [Header("Weather State")]
    public float temperature = 0f;
    public bool isSnowing = false;

    private float timeOffset = 0f;
    private Vector2 currentWindDirection;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        currentWindDirection = windDirection.normalized;
    }

    private void Update()
    {
        UpdateWind();
    }

    private void UpdateWind()
    {
        timeOffset += Time.deltaTime * windChangeSpeed;

        // Use Perlin noise for smooth wind changes
        float noiseX = Mathf.PerlinNoise(timeOffset, 0) * 2 - 1;
        float noiseY = Mathf.PerlinNoise(0, timeOffset) * 2 - 1;

        Vector2 windVariation = new Vector2(noiseX, noiseY) * turbulence;
        Vector2 targetDirection = (windDirection + windVariation).normalized;

        currentWindDirection = Vector2.Lerp(currentWindDirection, targetDirection, Time.deltaTime);
    }

    public Vector2 GetCurrentWindDirection()
    {
        return currentWindDirection;
    }

    public float GetWindSpeed()
    {
        return windSpeed;
    }
}