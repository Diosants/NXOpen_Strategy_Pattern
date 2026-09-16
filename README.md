# strategy-pattern-cam

**The Strategy pattern in C#, taken from how a CAM panel picks the right machining routine for each feature.**


## Why this example

PATHNC has ~25 machining routines selectable from combo boxes: M6…M20 taps, socket heads, center drill, peck drill, pockets, planar faces. The "Run" button does not know any of them. It asks a context to execute whatever the user selected:

```csharp
if (cmbMetricTap.SelectedItem is IMachiningStrategy strategy)
    context.Execute(strategy.FeatureName);
```

That is Strategy: a family of interchangeable algorithms behind one interface, chosen at runtime, with the caller unchanged when a new one is added.

This kata rebuilds the idea without NX and adds two things the original panel does not have yet:

- **`CanHandle(feature)`** — a strategy decides whether it applies to a feature, instead of the user picking from a list.
- **`Priority`** — when several strategies apply, the most specific wins: an *M8 Tap* strategy beats a *Generic Tap*; a *Deep Hole* strategy (L/D > 8) beats the standard peck drill.

## What's inside

```
Program.cs
├── Feature / FeatureKind          the input (hole, thread, pocket, planar face…)
├── IMachiningStrategy             FeatureName · Priority · CanHandle · Execute
├── M8TapStrategy                  specific: only M8 threads (priority 20)
├── GenericTapStrategy             fallback for any thread (priority 5)
├── ThroughPeckDrillStrategy       any through hole (priority 10)
├── DeepHoleStrategy               through holes with L/D > 8 (priority 15)
├── OpenPocketStrategy             pockets
└── MachiningContext               Register · Execute(byName) · Resolve(feature)
```

`MachiningContext` never references a concrete strategy. `Resolve` filters by `CanHandle` and takes the highest `Priority`; if nothing applies it returns `null` instead of throwing — the caller decides.

## Run

1. Visual Studio → New Project → **Console App (.NET Framework)**.
2. Replace `Program.cs` with the one in this repository.
3. F5.

Expected output (abridged):

```
H1 (Thread, Ø8.0 x 16.0)
   -> M8 Tap (priority 20)
      CENTER_DRILL H1
      DRILL_6.8 H1
      TAP_M8 H1

H2 (Thread, Ø12.0 x 24.0)
   -> Generic Tap (priority 5)
      ...

H4 (ThroughHole, Ø6.0 x 90.0)
   -> Deep Hole (gun drill) (priority 15)
      PILOT_6 H4
      GUN_DRILL H4

F1 (PlanarFace, Ø0.0 x 0.0)
   -> no strategy
```

## Exercises

- Add a `PlanarFaceStrategy` so `F1` is handled. The only lines that change are the new class and one `Register` call — `MachiningContext` and the loop in `Main` stay untouched. That is the whole point.
- Lower `M8TapStrategy.Priority` to 1 and watch the generic strategy take over — the context does not change.
- Make `Execute` return a `MachiningPlan` object (list of operations + estimated minutes) instead of strings, and let the context sum the minutes for a whole part.

## When to use it — and when not to

| Use Strategy when | Don't when |
|---|---|
| There are several *ways* to do the same action and the choice happens at runtime | There are two fixed cases: an `if` is clearer |
| New variants will keep arriving (new taps, new hole types) | The set is closed and small |
| The caller must not change when a variant is added | The "variants" differ in *order of steps* — that is Template Method |

## Related

- *PATHNC Katas* (PDF) — kata 01 explains this example; kata 04 (Template Method) is the natural next step.
- [`builder-pattern-nxopen`](https://github.com/[SEU-USUARIO]/builder-pattern-nxopen) — the previous kata in the series.

## License

MIT
