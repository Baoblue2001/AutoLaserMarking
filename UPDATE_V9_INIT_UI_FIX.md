# V9 - Fix startup NullReference before Main.InitializeUI

## Symptom
At startup, `Model.InitializeUI()` calls `LoadAModel()` before `Main.InitializeUI()` has completed. `formMain` already exists, but its `_form1` field and camera-display lists are not initialized yet. `LoadAModel()` then called `formMain.UpdateControlStatus()`, causing a `NullReferenceException` in `Main.UpdateControlStatus()`.

## Changes
- `Main.UpdateControlStatus()` now returns until `IsUiInitialized == true` and `_form1` is assigned.
- Added null/length guards for camera display lists, `IsCamRunContinous`, and `IsLiveCam`.
- Added safe UI-thread marshaling with `BeginInvoke` when needed.
- `Main.InitializeUI()` calls `UpdateControlStatus()` once only after initialization has completed.
- `Model.LoadAModel()` only refreshes MAIN controls when `formMain.IsUiInitialized` is true.

This keeps the existing initialization order because `Main.InitializeUI()` depends on vision rows prepared by `Model.InitializeUI()`.
