using AYellowpaper.SerializedCollections;
using UnityEngine;

/// <summary>
/// Selects a team-relative shape position using a role-configured field depth
/// and the lateral coordinate of the actor's formation position.
/// </summary>
[CreateAssetMenu(
    fileName = "Select Role Shape Position",
    menuName =
        "Soccer AI/Nodes/Selection/Select Role Shape Position")]
public sealed class SelectRoleShapePositionNode :
    AIBehaviorTreeNode
{
    [Header("Role Depths")]
    [Tooltip(
        "The team-relative field depth used by each player role. " +
        "Zero is the defending goal and one is the attacking goal.")]
    [SerializeField, SerializedDictionary(
        "Player Role",
        "Team-Relative Depth")]
    private SerializedDictionary<EPlayerRole, float>
        roleDepths = new();

    /// <summary>
    /// Adds default depth entries when the node asset is created.
    /// </summary>
    private void Reset()
    {
        EnsureRoleDepths();
    }

    /// <summary>
    /// Selects a shape position using the actor's role depth and formation
    /// side.
    /// </summary>
    /// <param name="context">
    /// The actor-specific behavior-tree context.
    /// </param>
    /// <returns>
    /// Success when a role depth and formation position are available.
    /// </returns>
    public override EBehaviorTreeResult Evaluate(
        AIBehaviorContext context)
    {
        if (context == null
            || context.Actor == null
            || context.GameState == null)
        {
            return EBehaviorTreeResult.Failure;
        }

        EPlayerRole playerRole =
            context.Actor.PlayerRole;

        if (!roleDepths.TryGetValue(
                playerRole,
                out float selectedDepth))
        {
            return EBehaviorTreeResult.Failure;
        }

        Vector2 formationWorldPosition =
            context.GameState.GetFormationWorldPosition(
                context.Actor.TeamId,
                context.FormationPosition);

        Vector2 formationFieldPosition =
            context.GameState.GetTeamRelativeFieldPosition(
                context.Actor.TeamId,
                formationWorldPosition);

        Vector2 selectedFieldPosition =
            new Vector2(
                Mathf.Clamp01(
                    selectedDepth),
                formationFieldPosition.y);

        Vector2 selectedWorldPosition =
            context.GameState.GetWorldPositionFromTeamRelative(
                context.Actor.TeamId,
                selectedFieldPosition);

        context.ClearSelectedActor();

        context.SetSelectedPosition(
            selectedWorldPosition);

        return EBehaviorTreeResult.Success;
    }

    /// <summary>
    /// Ensures every supported role has a configurable depth.
    /// </summary>
    private void EnsureRoleDepths()
    {
        if (roleDepths == null)
        {
            roleDepths =
                new SerializedDictionary<
                    EPlayerRole,
                    float>();
        }

        AddRoleDepthIfMissing(
            EPlayerRole.Goalkeeper,
            0.1f);

        AddRoleDepthIfMissing(
            EPlayerRole.Defender,
            0.5f);

        AddRoleDepthIfMissing(
            EPlayerRole.Midfielder,
            0.65f);

        AddRoleDepthIfMissing(
            EPlayerRole.Forward,
            0.8f);
    }

    /// <summary>
    /// Adds a default role depth when no entry exists.
    /// </summary>
    private void AddRoleDepthIfMissing(
        EPlayerRole playerRole,
        float depth)
    {
        if (roleDepths.ContainsKey(
                playerRole))
        {
            return;
        }

        roleDepths.Add(
            playerRole,
            Mathf.Clamp01(
                depth));
    }

#if UNITY_EDITOR
    /// <summary>
    /// Ensures all role entries exist and their depths remain normalized.
    /// </summary>
    private void OnValidate()
    {
        EnsureRoleDepths();

        EPlayerRole[] roles =
        {
            EPlayerRole.Goalkeeper,
            EPlayerRole.Defender,
            EPlayerRole.Midfielder,
            EPlayerRole.Forward
        };

        foreach (EPlayerRole role in roles)
        {
            roleDepths[role] =
                Mathf.Clamp01(
                    roleDepths[role]);
        }
    }
#endif
}