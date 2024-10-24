using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public SpriteRenderer stump;
    public SpriteRenderer top;
    public BoxCollider2D treeCollider;

    public void Initialize()
    {
        // Ensure proper layer ordering
        if (stump != null) stump.sortingOrder = 0;
        if (top != null) top.sortingOrder = 2;

        // Setup collider if not already set
        if (treeCollider == null)
        {
            treeCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // Set collider size based on the stump sprite only
        if (stump != null)
        {
            Vector2 stumpSize = stump.sprite.bounds.size;

            // EDIT THIS VALUE to change collider size (0.7f = 70% of sprite size)
            // Smaller number = smaller collider
            float colliderWidth = stumpSize.x * 0.1f;
            float colliderHeight = stumpSize.y * 0.25f;

            treeCollider.size = new Vector2(colliderWidth, colliderHeight);

            // EDIT THESE VALUES to adjust collider position
            float offsetY = (-stumpSize.y / 2) + (colliderHeight / 2);
            // Add an additional offset here to move the collider down
            // Negative numbers move down, positive move up
            offsetY += -1f; // Adjust this value to move collider up/down

            treeCollider.offset = new Vector2(0, offsetY);
        }
        else
        {
            Debug.LogWarning("Tree stump is missing! Collider not properly set up.");
        }
    }
}