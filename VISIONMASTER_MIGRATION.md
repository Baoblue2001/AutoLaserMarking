# Chuyển đổi SLTtechSoft: Cognex VisionPro -> HIK VisionMaster 4.3

## Trạng thái chuyển đổi

Project C# đã được chuyển phần runtime vision chính từ Cognex VisionPro sang HIK VisionMaster 4.3.

Các thay đổi chính:

| Cognex cũ | VisionMaster mới |
|---|---|
| `CogToolBlock` / `.vpp` | `VmProcedure` / `.prc` |
| `CogImage8Grey` | `Bitmap` -> `ImageBaseData` |
| `CogRecordDisplay` | `VmRenderControl` |
| `CogToolBlockEditV2` | `VmProcedureConfigControl` |
| `CogImageFileTool` | đọc ảnh bằng `Bitmap` |
| `CogStopwatch` | `System.Diagnostics.Stopwatch` |
| `CogFindLineTool` | VisionMaster `Line Find` (`IMVSLineFindModu`) |

Code adapter mới:
- `Class/VisionMasterProcess.cs`
- `Class/VisionMasterUiHelper.cs`

Bổ sung độ ổn định:
- Tự chuyển Bitmap không tương thích sang `Format24bppRgb` trước khi tạo `ImageBaseData` (HIK V4.3 chỉ hỗ trợ trực tiếp 8bpp indexed và 24bpp RGB ở constructor này).
- Bind `VmRenderControl` với Procedure trước khi chạy để nhận overlay/result.
- Các thao tác với control VisionMaster được marshal về WinForms UI thread.
- Đọc `Result` tránh nhầm các field trạng thái `nValueStatus/nValueNum` thành kết quả inspection.

## Điểm quan trọng về file VPP

`.vpp` của Cognex và `.prc` của VisionMaster là hai định dạng proprietary khác nhau. SDK VisionMaster không có API đọc trực tiếp file Cognex `.vpp`, vì vậy không thể đổi byte-for-byte từ VPP sang PRC chỉ bằng C#.

Trong archive dự án được cung cấp có một file:
- `bin/Debug/CogBlockTool_0.vpp`

Phân tích file này cho thấy có `CogFindLineTool`. Tool tương ứng trong VisionMaster là **Line Find** (`IMVSLineFindModu`). Các terminal tìm được trong VPP gồm `InputImage`, `OutputImage`, `DisplayImage`, `DataResults`, `Result`, `Line`. Bản VPP được giữ tại `LegacyReference/CogBlockTool_0.vpp` chỉ để đối chiếu và không còn được project mới load.

Nếu các model thực tế của máy còn dùng những file `.vpp` nằm ở thư mục ngoài project, từng flow đó cũng cần được dựng lại một lần trong VisionMaster và export thành `.prc`.

## Contract I/O của PRC

Adapter hiện chấp nhận:

### Input image
Ưu tiên:
1. `InputImage`
2. `ImageData`

C# gọi:

```csharp
Procedure.ModuParams.SetInputImage_V2(name, new ImageBaseData(bitmap));
```

### Inspection result
Ưu tiên tìm output:
1. `Result`
2. `result`
3. `OK`
4. `out`

Có thể là output số hoặc chuỗi. Các chuỗi `OK/PASS/TRUE` được hiểu là 1; `NG/FAIL/FALSE` được hiểu là 0.

**Nếu không tìm được output kết quả, adapter trả về NG = 0.** Đây là cố ý để tránh pass nhầm sản phẩm.

### Output image
Ưu tiên:
1. `OutputImage`
2. `ImageData`

Nếu không có output image, chương trình dùng ảnh input để lưu/hiển thị fallback.

## Cách tạo PRC tương đương VPP hiện có

Trong VisionMaster 4.3:

1. Tạo một Procedure mới.
2. Tạo/expose input image của Procedure với tên `InputImage` hoặc `ImageData`.
3. Thêm module **Line Find**.
4. Nối ảnh input vào Line Find.
5. Chuyển ROI, polarity, threshold, caliper/edge settings từ Cognex sang Line Find.
6. Tạo output số tên `Result`: `1 = OK`, `0 = NG`.
7. Nếu muốn lưu ảnh result, expose ảnh cuối với tên `OutputImage`.
8. Export Procedure thành `.prc`.
9. Trong phần Model của phần mềm, chọn file `.prc` thay cho `.vpp`.
10. Khi dùng nhiều PRC, nên đặt **tên Procedure bên trong mỗi PRC là duy nhất** (ví dụ `Cam1_Index0`, `Cam1_Index1`, ...) để tránh xung đột tên khi VisionMaster import nhiều Procedure cùng lúc.

Để hỗ trợ model cũ, nếu table vẫn chứa đường dẫn `xxx.vpp`, code sẽ tự thử tìm file `xxx.prc` cùng thư mục.

## Các phần đã đổi trong chương trình

- `Form1`
  - Load và chạy `VisionMasterProcess[]`.
  - Camera callback dùng `Bitmap`.
  - Kết quả trả từ PRC điều khiển bit PLC.
  - `VmRenderControl` hiển thị procedure/result.
  - Save Origin/Graphic không còn dùng Cognex.
- `Model`
  - Load/Save `.prc`.
  - Run test bằng `VmProcedure`.
  - Panel editor đổi sang `VmProcedureConfigControl`.
- `FrmVisionFileSetup`
  - Dialog chọn file chỉ nhận `.prc`.
- `PartOfCam`
  - `CogRecordDisplay` -> `VmRenderControl`.
- `FormOption`
  - Test image không còn dùng `CogImageFileTool`.
  - Camera mặc định chuyển sang HIK.
- `Class/Vision.cs`
  - Loại bỏ Cognex types khỏi runtime state.

## Một lỗi trong code cũ đã được sửa

Trong `VisionRun()` cũ:

```csharp
Result = (int)cogToolBlock.Outputs["Result"].Value;
...
Result = 1;
```

Do đó dù ToolBlock trả NG, chương trình vẫn ép `Result = 1`. Bản VisionMaster không còn dòng ép OK này.

## Dependency

Project target:
- .NET Framework 4.8
- x64

Đã thêm reference:
- `VM.Core`
- `VM.PlatformSDKCS`
- `VMControls.BaseInterface`
- `VMControls.Interface`
- `VMControls.Winform.Release`

Các DLL managed lấy từ bộ VisionMaster 4.3 người dùng cung cấp được đặt tại:
- `ThirdParty/VisionMasterSDK`

Một số DLL riêng của project gốc cũng được gom vào:
- `ThirdParty/ProjectDependencies`

Máy chạy vẫn cần cài đúng **VisionMaster 4.3 x64 runtime/SDK** và driver/runtime camera HIK tương ứng, vì VisionMaster còn sử dụng native DLL/module ngoài các managed assembly trên.

## Build/test

Môi trường chuyển đổi hiện tại không phải Windows và không có MSBuild/.NET Framework + VisionMaster runtime, vì vậy không thể chạy build/runtime VisionMaster thực tế tại đây.

Đã thực hiện static validation:
- `.csproj` parse XML hợp lệ.
- Toàn bộ file được khai báo `<Compile>` đều tồn tại.
- Không còn Cognex reference trong `.csproj`.
- Không còn Cognex types trong các file đang được compile.
- Kiểm tra cân bằng `{}`, `()`, `[]` trên toàn bộ C# compile files.
- API VisionMaster dùng trong adapter được đối chiếu với sample V4.x đi kèm archive: `VmProcedure.Load`, `SaveAs`, `Run`, `ModuParams.SetInputImage_V2`, `ModuResult.GetOutputImageV2`, `VmRenderControl.ModuleSource`.

## Bước kiểm tra đầu tiên trên máy Windows

1. Cài/kiểm tra VisionMaster 4.3 x64.
2. Mở solution bằng Visual Studio.
3. Chọn `x64`.
4. Build.
5. Tạo một PRC rất đơn giản chỉ nhận ảnh và xuất `Result = 1`.
6. Chọn PRC đó trong Model, chạy ảnh file trước.
7. Sau khi pipeline C# chạy ổn, dựng lại Line Find/Match/Fixture thật trong PRC.
8. Sau cùng mới test camera + PLC Auto.



## Lưu ý khi dùng nhiều PRC

- Nên đặt tên Procedure duy nhất cho từng Cam/Index, ví dụ `Cam1_Index0`, `Cam1_Index1`, `Cam2_Index0`.
- `VmProcedureConfigControl.BindSingleProcedure(...)` bind theo **tên Procedure**, không bind trực tiếp object.
- `VmRenderControl` được bind với `VmProcedure` trước khi Run để nhận ảnh kết quả và graphics/overlay.
- Live camera sử dụng `VmRenderControl.ImageSource`; inspection mode sử dụng `ModuleSource = VmProcedure`.

## V6 - MAIN native graphics + automatic Format result

- MAIN `VmRenderControl` is now bound to the inspection module rather than the SDK Image Source. This follows the HIK CircleFind SDK sample (`vmRenderControl.ModuleSource = circleFindModule`) and allows native module result graphics to render.
- The display module is auto-selected. A module containing `Display` or `Graphic` in its name is preferred; otherwise the last inspection module such as Blob/Match/Line/Circle/Measure is selected. Image Source, Image Output and Format are skipped.
- Result reading first uses Procedure `Result/result/OK/out`. If absent, the last Format module is inspected and `format_out` (or another meaningful output) is used automatically.
- `LastResultSource` records the actual source, e.g. `Format1.format_out`.
