using UnityEngine;

public class Footprint : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float fadeDuration = 3f;
    private float timer = 0f;

    [SerializeField] private Vector2 footprintSize = new Vector2(1f, 1f);

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = footprintSize;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}
