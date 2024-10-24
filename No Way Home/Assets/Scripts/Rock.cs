using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    public int rockType; // 0 for rock1, 1 for rock2
    private bool canBePickedUp = false;
    private PlayerController player;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canBePickedUp = true;
            player = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canBePickedUp = false;
            player = null;
        }
    }

    private void Update()
    {
        if (canBePickedUp && Input.GetKeyDown(KeyCode.E) && player != null)
        {
            player.CollectRock(rockType);
            Destroy(gameObject);
        }
    }
}
