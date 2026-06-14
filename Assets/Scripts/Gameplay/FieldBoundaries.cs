using UnityEngine;

/// <summary>
/// Prevents a player or object from leaving the playable soccer field area.
/// </summary>
public class FieldBounds : MonoBehaviour
{
    [SerializeField] private float minimumX = -8f;
    [SerializeField] private float maximumX = 8f;
    [SerializeField] private float minimumY = -4.5f;
    [SerializeField] private float maximumY = 4.5f;

    /// <summary>
    /// Clamps the object position inside the field boundaries.
    /// </summary>
    private void LateUpdate()
    {
        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minimumX, maximumX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minimumY, maximumY);

        transform.position = clampedPosition;
    }
}
