## Soccer AI System

The soccer AI uses a centralized `TeamAIController` to evaluate the current match state and assign actions to each AI-controlled actor.

Each actor executes an `ActorAssignment`, which may include:

- Moving toward a position or actor
- Holding a formation or defensive position
- Chasing a loose ball
- Passing
- Shooting
- Receiving a pass
- Taking possession
- Clearing the ball

Behaviors are selected using role-specific behavior trees on the `TeamAIController`. Field players evaluate attacking, defending, formation, support, and ball-chasing behaviors.

## How Designers Create a Behavior Tree

Behavior trees control how an AI player chooses what to do.

Designers do not need to write code to create a tree. The tree is built in the Unity Inspector using conditions, selectors, sequences, and actions.

### Basic Node Types

#### Selector

A selector checks its children from top to bottom.

It chooses the first child that can successfully run.

Use a selector when the AI has several possible behaviors.

Example:

```text
Goalkeeper Root Selector
├── Dive
├── Clear Ball
├── Collect Ball
└── Guard Goal
```

The goalkeeper first checks whether it should dive. If not, it checks whether it should clear the ball, then collect it, and finally guard the goal.

lower indexed nodes have higher priority.

#### Sequence

A sequence checks its children from top to bottom.

Every child must succeed for the sequence to continue.

Use a sequence when several conditions must all be true before an action occurs.

Example:

```text
Collect Ball Sequence
├── Ball Is Loose
├── Ball Is Inside Penalty Box
├── Goalkeeper Can Reach Ball
└── Move To Ball
```

If any condition fails, the sequence stops.

## Priority Order

Behavior priority is determined by tree order.

The AI checks behaviors from top to bottom.

For example:

```text
Player Root Selector
├── Shoot Sequence
├── Pass Sequence
├── Chase Ball Sequence
├── Defend Sequence
├── Move Into Space Sequence
└── Formation Sequence
```

In this example, shooting has the highest priority and formation movement has the lowest priority.

Do not place a general behavior above an emergency or possession behavior.

For example, `Guard Goal` should not be placed above `Dive`, because the goalkeeper would continue guarding instead of reacting to a dangerous shot.

#### Condition

A condition checks whether something is true.

Examples include:

- Actor has the ball
- Ball is loose
- Ball is inside the penalty box
- Opponent has possession
- Actor is the primary ball chaser
- Actor is close enough to shoot
- Teammate is available for a pass

Conditions do not directly move the player or perform an action.

#### Action

An action tells the AI what to do.

Examples include:

- Move
- Hold position
- Pass
- Shoot
- Take ball
- Clear ball
- Dive

Actions should normally appear at the end of a sequence.

---

## Creating a Tree in Unity

1. Richt click on empty space in the project window(aka the Unity file system).
2. click Create -> Soccer AI -> Behaviour Tree.
3. Add a root selector by click Create -> Soccer AI -> Nodes -> Composite -> Selector.
4. Put the selector in the tree's 'Root Node' and on the selector check isRoot to true(use to tell the system that this is what should be debugged).
5. Add one sequence or selector for each behavior the actor should perform.
6. Add the required conditions under each sequence.
7. Add the final action at the bottom of each sequence.
8. Arrange the sequences from highest priority to lowest priority.
9. Enter Play Mode and watch the AI debug logs.

---

## Example: Goalkeeper Tree

```text
Goalkeeper Root Selector
├── Dive Sequence
│   ├── Ball Is Near Goal
│   └── Dive
│
├── Clear Ball Sequence
│   ├── Goalkeeper Has Ball
│   └── Clear Ball
│
├── Collect Ball Sequence
│   ├── Ball Is Loose
│   ├── Ball Is Inside Penalty Box
│   └── Move To Or Take Ball
│
└── Guard Goal Sequence
    └── Move To Or Hold Guard Position
```

This means the goalkeeper will:

1. Dive when the ball is dangerous.
2. Clear the ball when already in possession.
3. Collect a loose ball inside the penalty box.
4. Guard the goal when no higher-priority behavior is needed.

---

## Example: Defender Tree

```text
Defender Root Selector
├── Clear Ball Sequence
│   ├── Defender Has Ball Near Own Goal
│   └── Clear Ball
│
├── Chase Ball Sequence
│   ├── Ball Is Loose
│   ├── Defender Is Primary Chaser
│   └── Move To Ball
│
├── Mark Opponent Sequence
│   ├── Opponent Has Ball
│   ├── Opponent Is In Defensive Area
│   └── Mark Opponent
│
└── Formation Sequence
    └── Move To Formation Position
```

---

## Important Design Rules

- Put urgent behaviors above general behaviors.
- Place actions at the end of sequences.
- Avoid placing `Hold Position` too high in the tree.
- Use `Move To Position` before `Hold Position` when the actor may not already be at the target.
- Use the primary chaser condition so every player does not chase the ball.
- Keep trees simple so that other designers can understand why a behavior was selected.

---

## Testing a Tree

During Play Mode, the AI will automatically show what they are evaluating too using gizmo's.
This shows which sequence was selected and which action was assigned.

You can also turn on each debug prints for the AI actor on on their AIActorController and setting the logs to true
Example:

```text
TreeSelection=node-Chase Ball Sequence → Move To Ball
Action=Move
```

When testing, check:

- Does the correct sequence run?
- Is a higher-priority behavior blocking another behavior?
- Are conditions becoming true when expected?
- Is the actor moving before being told to hold position?
- Are several players trying to chase the same ball?
- Does the actor return to formation after the behavior ends?

Change one part of the tree at a time, then test again.
I REPEAT change ONE part at a time, it'll save you a lot of headache.

You can see the full structure of the tree by clicking the 'print full tree structure' button on the tree's ScriptableObject

---

## Common Problems

### Crowing always holds position

Collision avoidance may require further adjustment when several actors converge on the ball.

### The AI rapidly switches behaviors

The conditions may change every update.

Increase the behavior duration, add a release distance, or allow the AI to remain committed to the current behavior briefly.

This can result in things like two opposing players kicking and taking the ball from each other over and over, the ball has some settings to make this unlikely.
However, the ai could be changed later to allow for delays in actions of the AI.
