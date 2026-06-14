# Soccer AI Setup

## 1. Add the AI scripts

Place the AI scripts in your Unity project, for example:

```text
Assets/Scripts/AI/
```

## 2. Implement the required interfaces

Connect the AI system to the existing player, ball, and match systems by implementing:

- `IAIActor`
- `IGameState`
- `IAIActionOutput`

`IAIActor` provides player information such as position, team, role, formation position, and ball possession.

`IGameState` provides the ball position, players, goals, formations, possession, and active-pass information.

`IAIActionOutput` converts AI decisions into gameplay actions such as:

```csharp
RequestMove(...)
RequestStop(...)
RequestPass(...)
RequestShoot(...)
RequestTakeBall(...)
```

## 3. Assign formation positions

Create a formation dictionary for each team:

```csharp
Dictionary<string, EFormationPosition> enemyFormation =
    new Dictionary<string, EFormationPosition>
    {
        { "Enemy_GK", EFormationPosition.Goalkeeper },
        { "Enemy_LD", EFormationPosition.LeftDefender },
        { "Enemy_RD", EFormationPosition.RightDefender },
        { "Enemy_LM", EFormationPosition.LeftMidfielder },
        { "Enemy_RF", EFormationPosition.RightForward }
    };
```

Each actor ID must match the ID returned by `IAIActor.ActorId`.

## 4. Create a controller for each team

```csharp
private TeamAIController playerTeamAI;
private TeamAIController enemyTeamAI;

private void Start()
{
    playerTeamAI = new TeamAIController(
        ETeamId.PlayerTeam,
        gameState,
        actionOutput,
        playerFormation);

    enemyTeamAI = new TeamAIController(
        ETeamId.EnemyTeam,
        gameState,
        actionOutput,
        enemyFormation);
}
```

Human-controlled players should return `false` from `IsAIControlled`.

## 5. Update the AI

Call both team controllers during gameplay:

```csharp
private void FixedUpdate()
{
    if (!gameState.IsMatchActive)
        return;

    playerTeamAI.UpdateTeam();
    enemyTeamAI.UpdateTeam();
}
```

## 6. Connect all behaviors

Make sure `TeamAIController` creates and evaluates:

- `ShootBehavior`
- `ReceivePassBehavior`
- `GoalkeeperBehavior`
- `ChaseBallBehavior`
- `DefendBehavior`
- `OpenSpaceBehavior`
- `FormationBehavior`

Without this step, open-space movement, pass receiving, and goalkeeper behavior will not run.

## 7. Active pass data

When a pass starts, update the game state:

```csharp
HasActivePass = true;
IntendedPassReceiverId = receiverId;
IntendedPassTargetPosition = targetPosition;
```

Clear the active-pass state when the pass ends, is intercepted, or leaves play.
