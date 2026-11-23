# V7 - Model / Procedure load-save redesign

## Model Save
- Saves model JSON and model list information only.
- Does **not** call `InitialAllCameras()`.
- Does **not** rebuild MAIN displays.
- Does **not** load/reload PRC files.
- Does **not** create or rebind `VmMainViewConfigControl`.
- If a new model file path is selected, the path in the Model table is updated before `ModelInfomation.txt` is saved.

## Model Call / Load
- Closes the teaching editor first so no old procedure remains bound.
- Reads the selected model and refreshes all camera vision tables.
- Rebuilds MAIN display only when a model is actually called.
- Reuses cameras that are already open; only reconnects missing/closed cameras.
- Reloads runtime PRC arrays once for the called model.
- If any PRC for a camera cannot be loaded, that camera's vision runtime is marked uninitialized so the old/wrong algorithm is not executed.

## Procedure Load
- `VmMainViewConfigControl` is created only when the user presses a camera row's **Load** button.
- If that PRC is already loaded in runtime, the editor uses the same `VisionMasterProcess` instance instead of loading a duplicate PRC.
- The editor control is recreated when switching edited procedures, preventing old bindings from accumulating.

## Procedure Save
- Saves the exact procedure instance that is currently open in the editor.
- Does not reload all procedures and does not rebind the editor after save.
- If the editor had to load a private procedure instance, that same instance is transferred into the existing runtime slot after save where possible, instead of loading the PRC again.
- Saving is blocked if the user changes row/path after Load; the user is asked to Load the selected row first to avoid saving the wrong PRC.

## Other model fixes
- Fixed Model Table last-row handling when `AllowUserToAddRows = false`.
- Fixed manual model-file Load so the selected file is actually loaded instead of always calling model index 0.
- Camera exposure edits on MAIN are synchronized back to the model vision table before model save.
- Removed the unnecessary 200 ms sleep after model save.
