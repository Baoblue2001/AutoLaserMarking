# V10 - Trigger Smooth / LocateDemoCs pattern

Main changes:

1. VmRenderControl is now bound to each VmProcedure once when the model/procedure is loaded, instead of assigning ModuleSource on every trigger.
2. VisionMaster result callbacks are enabled when a procedure is loaded/bound, not repeatedly on every trigger.
3. Procedure trigger path is reduced to image injection -> Procedure.Run() -> result read. No WinForms control manipulation in VisionRunCore.
4. Removed InitView() on every trigger. The old auto-fit caused visible image jumping/flicker.
5. Restored `if (!Enable) return;` in CreatFilmStripOne. Manual trigger no longer creates folders, captures render image, writes JPEGs, or invokes UI synchronously.
6. Auto filmstrip UI update uses BeginInvoke instead of two blocking Invoke calls.
7. Test/disconnected PLC thread no longer busy-spins at 100% CPU.
8. Manual camera worker sleeps 1 ms per loop instead of consuming a full CPU core; Auto mode still uses Thread.Yield to preserve response behavior.
9. Output image copy is skipped when SaveGraphic is disabled.

Why this differs from LocateDemoCs:
- LocateDemoCs generally loads a VM solution (.sol) then gets VmProcedure objects from VmSolution.Instance.
- This machine software stores multiple independent .prc files per camera/model. V10 keeps that model architecture, but adopts the same important runtime principle: load/bind once, trigger only Run, and let the bound renderer update from VisionMaster callbacks.

Recommended test:
- Manual mode, click Trigger repeatedly: image should update without re-fit jump and without filmstrip disk I/O.
- Test Mode (No PLC/Camera): CPU usage should be much lower than V9.
- Auto mode: verify PLC FinishTrigger/Result timing and image saving.
- Call Model1 -> Model2 -> Model1, then trigger each camera.
