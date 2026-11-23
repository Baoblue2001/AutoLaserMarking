# V8 - VisionMaster model-call / shutdown fix

## Fixed: Assertion Failed when calling a model
The failing stack was inside `VmProcedure.Dispose()` -> `VmModule.CommonDispose()` -> `VmIO.RelateIO` / `LoadModuleRelateIO()`.

Changes:
- `VisionMasterProcess.Dispose()` is now idempotent.
- Before disposing a VM procedure, all procedure/module `RelateIO` links are materialized while the full graph still exists.
- The VisionMaster/.NET assertion dialog is disabled only for the short VM-procedure release section, then the previous listener setting is restored.
- Calling the same model/PRC set again reuses the already loaded runtime procedures instead of deleting and loading identical PRCs again.
- A single runtime lock now prevents `VisionRun()` from running at the same time as model reload/dispose.
- Model calling temporarily switches runtime state to Manual while the model is being changed, then restores the previous machine mode.
- MAIN render controls are detached from `ModuleSource` before old VM procedures are disposed.
- The teaching editor control is disposed/unbound before an editor-owned procedure is disposed.

## Fixed: Assertion/error when closing the application
Changes:
- Camera/PLC worker loops now honor a shutdown flag.
- Camera worker thread is a background thread and is asked to stop before VM/camera resources are destroyed.
- MAIN VisionMaster bindings and MODEL editor bindings are released before procedure disposal.
- Vision procedures are disposed through the new safe-dispose path.
- Camera/image cleanup is exception-safe per camera.
- Removed forced `Environment.Exit(0)` from normal FormClosing cleanup.
- Corrected the existing `PLCDispose()` / `DisconnectPLC()` null checks so PLC cleanup can actually run.

## Files changed
- `SLTtechSoft/Class/VisionMasterProcess.cs`
- `SLTtechSoft/Form1.cs`
- `SLTtechSoft/Model.cs`
- `SLTtechSoft/Main.cs`

## Recommended test on the machine PC
1. Start software and call Model1.
2. Call Model1 again several times.
3. Alternate Model1 -> Model2 -> Model1 at least 10 times.
4. Run a vision cycle, then call another model after the cycle finishes.
5. Close the software while MAIN is open.
6. Open a PRC in MODEL/Teaching, close the editor by calling another model, then close the software.

This package was source-validated in the sandbox. The HIK VisionMaster Windows runtime cannot be executed in this Linux sandbox, so final runtime verification must be done on the VisionMaster machine PC.
