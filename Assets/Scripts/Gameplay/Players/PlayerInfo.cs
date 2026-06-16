using UnityEngine;

/// <summary>
/// Stores player information used by the AI system.
/// </summary>
public sealed class PlayerInfo : MonoBehaviour, IAIActor
{
    [Header("Actor")]
    [SerializeField] private string actorId;
    [SerializeField] private ETeamId teamId;
    [SerializeField] private bool isAIControlled = true;

    [Header("Role")]
    public EFormationPosition formationPosition;

    [Header("Gameplay Output")]
    [Tooltip("Assign a component that implements IAIActionOutput.")]
    [SerializeField] private MonoBehaviour actionOutputSource;

    private IAIActionOutput actionOutput;

    public string ActorId => actorId;

    public ETeamId TeamId => teamId;

    public Vector2 Position => transform.position;

    public bool IsActive => gameObject.activeInHierarchy;

    public bool IsAIControlled => isAIControlled;

    public bool IsGoalkeeper =>
        PlayerRole == EPlayerRole.Goalkeeper;

    public bool HasBall { get; private set; }

    public EPlayerRole PlayerRole =>
        GetPlayerRole(formationPosition);

    public EFormationPosition FormationPosition =>
        formationPosition;

    public IAIActionOutput ActionOutput => actionOutput;

    /// <summary>
    /// Retrieves the assigned gameplay action output.
    /// </summary>
    private void Awake()
    {
        actionOutput = actionOutputSource as IAIActionOutput;

        if (actionOutputSource != null && actionOutput == null)
        {
            Debug.LogError(
                $"{name}: Action Output Source must implement IAIActionOutput.",
                this);
        }
    }

    /// <summary>
    /// Determines a player's role from their formation position.
    /// </summary>
    /// <param name="position">The player's formation position.</param>
    /// <returns>The corresponding player role.</returns>
    private EPlayerRole GetPlayerRole(
        EFormationPosition position)
    {
        switch (position)
        {
            case EFormationPosition.Goalkeeper:
                return EPlayerRole.Goalkeeper;

            case EFormationPosition.LeftDefender:
            case EFormationPosition.CenterDefender:
            case EFormationPosition.RightDefender:
                return EPlayerRole.Defender;

            case EFormationPosition.LeftMidfielder:
            case EFormationPosition.CenterMidfielder:
            case EFormationPosition.RightMidfielder:
                return EPlayerRole.Midfielder;

            case EFormationPosition.LeftForward:
            case EFormationPosition.CenterForward:
            case EFormationPosition.RightForward:
                return EPlayerRole.Forward;

            default:
                return EPlayerRole.Midfielder;
        }
    }
}