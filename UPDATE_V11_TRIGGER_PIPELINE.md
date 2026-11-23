# V11 - Trigger pipeline performance fix

This revision targets the actual hot path seen in the user's second video.

## Main changes
- Changed VM 4.3 `SetImageData(..., true)` to the non-blocking copy mode `SetImageData(..., false)` while keeping the image data alive through `Procedure.Run()`.
- Removed 2-3 full-resolution Bitmap copies from every camera trigger.
- VisionMasterProcess no longer caches the input image unless FilmStrip saving actually needs it.
- Source image is passed directly to FilmStrip saving instead of cloning it into VisionSystem first.
- Successful VisionMaster triggers are no longer written to ListView + CSV on every shot. Errors are still logged.
- Result lookup is cached after the first successful run, avoiding repeated module enumeration/reflection on every trigger.
- Runtime model loading no longer calls `VmSolution.EnableModulesCallback()` globally; only the procedures/modules actually used by the render are enabled.
- Log file writes are moved to ThreadPool so disk/antivirus/network latency never blocks the UI thread.
- Fixed duplicate Click event subscriptions in CameraVisionDisplay.RefreshDisplay().

## Why V10 did not visibly improve
V10 optimized ModuleSource/InitView work, but the dominant cost remained in the application layer: repeated Bitmap clones and synchronous per-trigger logging/repaint.

## Architecture note
LocateDemoCs binds VmRenderControl once to the procedure and keeps the runtime path small. V11 keeps that same principle while preserving the application's existing multi-PRC model-management design.
