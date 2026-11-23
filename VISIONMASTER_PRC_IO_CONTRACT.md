# VisionMaster PRC contract used by this project

## 1. Image input

The C# application grabs the image from the camera and sends it into the VisionMaster procedure through an **Image Source** module.

In VisionMaster, set the first Image Source module to:

- **Image Source = SDK**
- Pixel format as required by your tool flow (MONO8 is recommended for the current project).

The C# adapter automatically searches common module names such as:

- `InputImage`
- `Image Source`
- `Image Source1`
- `ImageSource1`
- indexed variants such as `0 Image Source`

For the most predictable setup, rename the Image Source module to `InputImage`.

The previous Procedure Input method (`ModuParams.SetInputImage_V2("InputImage", ...)`) is no longer used.

## 2. Inspection result

Expose a Procedure output named one of:

- `Result` (recommended)
- `result`
- `OK`
- `out`

Recommended value convention:

- `0` = NG
- non-zero = OK

String results `OK`, `PASS`, `TRUE`, `NG`, `FAIL`, `FALSE` are also supported.

If no readable result exists, the application forces the result to NG (`0`).

## 3. Output image

An output image is optional. If the PRC exposes `OutputImage` or `ImageData`, the adapter reads it. Otherwise the original camera image remains available as the last image.

## 4. Required SDK assembly

The project now references `ImageSourceModuleCs.dll` from VisionMaster 4.3 because `ImageSourceModuleTool.SetImageData(...)` is used to inject the camera image.
