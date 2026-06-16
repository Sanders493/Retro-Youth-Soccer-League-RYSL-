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
    /// Gets the playable field bounds in world space.
    /// </summary> 
    public Bounds Bounds
    {
        get
        {
            Vector3 center = new Vector3(
                (minimumX + maximumX) * 0.5f,
                (minimumY + maximumY) * 0.5f,
                0f);

            Vector3 size = new Vector3(
                maximumX - minimumX,
                maximumY - minimumY,
                0f);

            return new Bounds(center, size);
        }
    }
    
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
