using UnityEngine;

/// <summary>
/// Represents an invisible field wall boundary that blocks players and the ball.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class FieldWallBoundary : MonoBehaviour
{
    [SerializeField] private bool isTriggerBoundary = false;

    private BoxCollider2D wallCollider;

    /// <summary>
    /// Gets the wall collider and applies boundary settings.
    /// </summary>
    private void Awake()
    {
        wallCollider = GetComponent<BoxCollider2D>();
        wallCollider.isTrigger = isTriggerBoundary;
    }
}
