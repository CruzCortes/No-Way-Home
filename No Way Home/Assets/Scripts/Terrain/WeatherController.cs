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

    [Header("Temperature Settings")]
    public float baseTemperature = 20f; // Base environment temperature
    public float temperatureVariation = 5f; // How much temperature can vary
    public float windChill = 0.5f; // How much wind affects temperature

    [Header("Weather State")]
    public float temperature = 0f;
    public bool isSnowing = false;

    private float timeOffset = 0f;
    private float temperatureOffset = 0f;
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
        temperatureOffset = Random.Range(0f, 1000f); // Random offset for temperature variation
    }

    private void Update()
    {
        UpdateWind();
        UpdateTemperature();
    }

    private void UpdateWind()
    {
        timeOffset += Time.deltaTime * windChangeSpeed;
        float noiseX = Mathf.PerlinNoise(timeOffset, 0) * 2 - 1;
        float noiseY = Mathf.PerlinNoise(0, timeOffset) * 2 - 1;
        Vector2 windVariation = new Vector2(noiseX, noiseY) * turbulence;
        Vector2 targetDirection = (windDirection + windVariation).normalized;
        currentWindDirection = Vector2.Lerp(currentWindDirection, targetDirection, Time.deltaTime);
    }

    private void UpdateTemperature()
    {
        temperatureOffset += Time.deltaTime * 0.1f;

        // Use Perlin noise for smoother temperature variations
        float tempNoise = Mathf.PerlinNoise(temperatureOffset, 0);

        // Base temperature varies more slowly
        float targetTemp = baseTemperature + (tempNoise * temperatureVariation * 2 - temperatureVariation);
        temperature = Mathf.Lerp(temperature, targetTemp, Time.deltaTime * 0.5f);

        // Wind chill is more severe in colder temperatures
        float windChillEffect = windSpeed * windChill;
        if (temperature < baseTemperature - 5f)
        {
            windChillEffect *= 1.5f;
        }

        // Apply wind chill
        temperature -= windChillEffect * Time.deltaTime;

        // Clamp temperature to realistic range
        temperature = Mathf.Clamp(temperature, baseTemperature - 20f, baseTemperature + 20f);
    }

    public Vector2 GetCurrentWindDirection()
    {
        return currentWindDirection;
    }

    public float GetWindSpeed()
    {
        return windSpeed;
    }

    public float GetCurrentTemperature()
    {
        return temperature;
    }
}