# Ballast — Project Knowledge

Unity 6.3 / URP demo. Diver descends constantly; player steers laterally and manages weight. Target build: **WebGL**, playable on desktop + mobile browsers.

## Stack
- Unity **6.3** (URP 17.3, Input System 1.18). New Input System only — never `UnityEngine.Input`.
- C# 9, namespace **`Ballast.Gameplay`** for all gameplay scripts.
- Rigidbody-based movement. Unity 6 renames: `rb.drag` → `rb.linearDamping`, `rb.angularDrag` → `rb.angularDamping`, `rb.velocity` → `rb.linearVelocity`. Use the new names.

## Layout
```
Assets/
├── Scripts/Gameplay/    # all gameplay C# lives here
├── Scenes/              # GameScene.unity is the live scene
├── Settings/            # URP renderers + build profiles (Web Desktop / Mobile)
├── InputSystem_Actions.inputactions   # Player/Move (Vector2) is the canonical move action
├── Art/, Audio/         # placeholder asset folders
Packages/, ProjectSettings/, .gitignore, CLAUDE.md
```

`Library/`, `Logs/`, `Temp/`, `UserSettings/`, `*.csproj`, `*.sln`, `Ballast.slnx`, `.vscode/`, `.claude/settings.local.json` are gitignored.

## Gameplay scripts (`Assets/Scripts/Gameplay/`)
- **DiverController** — Rigidbody descent + lateral force + weight-coupled multiplier (AnimationCurve) + wall-knockback `OnCollisionEnter`. Requires `Rigidbody` + `InputReader`.
- **InputReader** — wraps an `InputActionReference` (drag `Player/Move` onto it). Optional accelerometer tilt gated by `useTilt` (off by default; iOS Safari needs HTTPS + user gesture).
- **WeightSystem** — singleton (`WeightSystem.Instance`). Holds `currentWeight`, `maxWeight = 10`, `WeightZone` enum (Light/Loaded/Critical via `loadedThreshold` / `criticalThreshold`). `AddWeight` / `RemoveWeight`. Events: `OnWeightChanged`, `OnZoneChanged`.
- **CameraFollow** — Y-axis only, SmoothDamp with `verticalSmoothTime` (default 0.8s). No LookAt, no X/Z follow — camera X/Z stays where placed in the scene.
- **DebugWeightControl** — `#if UNITY_EDITOR` only. `[` removes 1 weight, `]` adds 1. Reads `Keyboard.current` from the new Input System.

## Conventions / decisions
- **Input:** Input System with `InputActionReference` serialized fields, not embedded `InputAction`. Bind to `Player/Move`.
- **Movement:** `AddForce` only, except a single direct write to `rb.linearVelocity.y` for the constant descent (intentional). Lateral velocity is **capped** by overwriting `.x` when over the cap — that's not "movement by velocity write," it's a clamp.
- **Weight coupling:** read `WeightSystem.Instance.CurrentWeight` each `FixedUpdate`, evaluate the curve, scale `lateralForce` and `maxLateralSpeed` by the multiplier.
- **Walls:** use the `Wall` user layer. `DiverController.wallLayer` mask must include it. Knockback is along the contact normal with Y zeroed.
- **Camera:** position only, Y only, no rotation control. If the user wants rotation following, ask first — current design is deliberate.
- **WebGL/mobile:** keep URP-Mobile asset clean, no heavy post FX. Tilt input is unreliable on mobile web; ship with on-screen stick (Input System's `OnScreenStick`).

## Editor setup (not committed — done inside Unity)
1. Add `Wall` user layer in `ProjectSettings → Tags and Layers`.
2. Build Diver prefab in `Assets/Prefabs/`: Capsule + Rigidbody (no gravity, freeze rotation, linearDamping ~4, Continuous Dynamic, Interpolate) + DiverController + InputReader + WeightSystem + DebugWeightControl. Drag `Player/Move` into InputReader's `Move Action`.
3. In `GameScene`: place Diver, Main Camera with `CameraFollow` (drag Diver into Target).
4. For mobile: Canvas with `OnScreenStick` bound to `Player/Move`.

Prefabs and scene wiring are NOT created from outside Unity — script GUID refs would break. Always do prefab work in the editor, then commit `Assets/Prefabs/*` and `Assets/Scenes/*`.

## Git
- Remote: `https://github.com/maurigalvez/Ballast.git`, branch `main`.
- Commit per logical change. Existing history uses `Task N: <description>` for the initial 8 movement tasks; subsequent commits use plain conventional descriptions.
- LF→CRLF warnings on commit are normal on Windows; ignore.

## Verification
Press Play in `GameScene`:
- Diver descends; A/D steers with momentum and a max lateral speed.
- `[` / `]` change weight — Critical zone makes movement noticeably heavier.
- Steering into a `Wall`-layer collider produces a clean bounce, no sticking.
- Camera drifts down on Y, X/Z stay put.
