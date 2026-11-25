using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SLTtechSoft
{
    public enum AppLanguage
    {
        Vietnamese,
        English,
        Korean
    }

    /// <summary>
    /// Runtime UI translation for the main application and embedded WinForms.
    /// Text is translated by exact caption, so switching language does not alter
    /// control layout, values, PLC data, model data, or user-entered content.
    /// </summary>
    public static class LanguageManager
    {
        private static readonly Dictionary<string, string[]> _translations =
            new Dictionary<string, string[]>(StringComparer.Ordinal);

        private static readonly HashSet<Control> _registered = new HashSet<Control>();
        private static AppLanguage _language = AppLanguage.Vietnamese;
        private static bool _isApplying;

        static LanguageManager()
        {
            Add("LASERMARKING MACHINE", "HỆ THỐNG LAZER CHẤP", "레이저 마킹 시스템");
            Add("MAIN", "CHÍNH", "메인");
            Add("MODEL", "MODEL", "모델");
            Add("I/O", "I/O", "I/O");
            Add("OPTION", "CÀI ĐẶT", "설정");
            Add("SCANNER", "MÁY QUÉT", "스캐너");
            Add("Printer", "Máy in", "프린터");
            Add("Process Step:", "Bước quy trình:", "공정 단계:");
            Add("Alarm:", "Cảnh báo:", "알람:");
            Add("T Write:", "Thời gian ghi:", "쓰기 시간:");
            Add("T Read:", "Thời gian đọc:", "읽기 시간:");
            Add("VisionTime:", "Thời gian xử lý:", "비전 처리 시간:");
            Add("AcqTime:", "Thời gian chụp:", "획득 시간:");
            Add("PLC 1", "PLC 1", "PLC 1");
            Add("PLC 2", "PLC 2", "PLC 2");
            Add("CAM 1", "CAM 1", "카메라 1");
            Add("CAM 2", "CAM 2", "카메라 2");
            Add("CAM 3", "CAM 3", "카메라 3");
            Add("CAM 4", "CAM 4", "카메라 4");
            Add("PC - PLC", "PC - PLC", "PC → PLC");
            Add("PLC-PC", "PLC - PC", "PLC → PC");
            Add("Main", "Màn hình chính", "메인");
            Add("Model", "Model", "모델");
            Add("FormIO", "I/O", "I/O");
            Add("FormOption", "Cài đặt", "설정");

            Add("LOT:", "LOT:", "LOT:");
            Add("Quality ALL", "Tổng chất lượng", "전체 품질");
            Add("Total ALL:", "Tổng sản phẩm:", "총 생산:");
            Add("Extend", "Mở rộng", "확장");
            Add("NG ALL:", "Tổng NG:", "전체 NG:");
            Add("OK ALL:", "Tổng OK:", "전체 OK:");
            Add("TEACH", "DẠY", "티칭");
            Add("STOP", "DỪNG", "정지");
            Add("AUTO", "TỰ ĐỘNG", "자동");
            Add("Run Program", "Chạy chương trình", "프로그램 실행");
            Add("Model Name", "Tên Model", "모델명");
            Add("Time", "Thời gian", "시간");
            Add("Code", "Mã", "코드");
            Add("Content", "Nội dung", "내용");
            Add("Main Log", "Nhật ký chính", "메인 로그");
            Add("Vision Log", "Nhật ký Vision", "비전 로그");
            Add("MODEL", "MODEL", "모델");
            Add("LOT NO", "SỐ LOT", "LOT 번호");
            Add("1'ST BARCODE", "BARCODE ĐẦU TIÊN", "첫 번째 바코드");
            Add("YMMDDN", "YMMDDN", "YMMDDN");
            Add("SERIAL NO", "SỐ SERIAL", "시리얼 번호");
            Add("RANK", "HẠNG", "등급");
            Add("ARRAY COUNT", "SỐ LƯỢNG ARRAY", "배열 수량");
            Add("MES RESULT", "KẾT QUẢ MES", "MES 결과");
            Add("1'ST SCAN", "LẦN QUÉT ĐẦU", "첫 번째 스캔");
            Add("COUNT", "ĐẾM", "수량");
            Add("CAM 1 - RESULT", "KẾT QUẢ CAM 1", "CAM 1 결과");

            Add("Run\nOnce", "Chạy một lần", "한 번 실행");
            Add("Use Image Setting", "Dùng cài đặt ảnh", "이미지 설정 사용");
            Add("Use File", "Dùng file", "파일 사용");
            Add("Use Folder", "Dùng thư mục", "폴더 사용");
            Add("Use Cam", "Dùng camera", "카메라 사용");
            Add("Vision File:", "File Vision:", "비전 파일:");
            Add("Run Test", "Chạy thử", "테스트 실행");
            Add("Vision", "Vision", "비전");
            Add("Marking Setting", "Cài đặt khắc", "마킹 설정");
            Add("TOOL", "CÔNG CỤ", "도구");
            Add("Create New Model", "Tạo Model mới", "새 모델 만들기");
            Add("Model name:", "Tên Model:", "모델 이름:");
            Add("Create", "Tạo", "만들기");
            Add("Cancel", "Hủy", "취소");

            Add("Call Model Manual", "Gọi Model thủ công", "모델 수동 호출");
            Add("File Offset Table 2", "Bảng Offset File 2", "파일 오프셋 테이블 2");
            Add("File Offset Table1", "Bảng Offset File 1", "파일 오프셋 테이블 1");
            Add("Path save file", "Đường dẫn lưu file", "파일 저장 경로");
            Add("Common Path", "Đường dẫn chung", "공통 경로");
            Add("PASSWORD", "MẬT KHẨU", "비밀번호");
            Add("MC TITLE", "TÊN MÁY", "장비 이름");
            Add("MACHINE SETTING", "CÀI ĐẶT MÁY", "장비 설정");
            Add("Save Parameter", "Lưu thông số", "매개변수 저장");
            Add("Cam 1", "Camera 1", "카메라 1");
            Add("Cam 2", "Camera 2", "카메라 2");
            Add("Cam 3", "Camera 3", "카메라 3");
            Add("Cam 4", "Camera 4", "카메라 4");
            Add("Other", "Khác", "기타");
            Add("Cam", "Camera", "카메라");
            Add("PLC 1", "PLC 1", "PLC 1");
            Add("PLC 2", "PLC 2", "PLC 2");
            Add("PLC", "PLC", "PLC");
            Add("Refresh", "Làm mới", "새로 고침");
            Add("HOST IP", "ĐỊA CHỈ IP", "호스트 IP");
            Add("Port Name", "Tên cổng", "포트 이름");
            Add("PORT", "CỔNG", "포트");
            Add("One", "Một", "1");
            Add("Baudrate", "Tốc độ baud", "보드레이트");
            Add("Stop bits", "Bit dừng", "정지 비트");
            Add("Disconnect", "Ngắt kết nối", "연결 해제");
            Add("Data bits", "Bit dữ liệu", "데이터 비트");
            Add("Connect", "Kết nối", "연결");
            Add("None", "Không", "없음");
            Add("Parity", "Parity", "패리티");
            Add("Scanner", "Máy quét", "스캐너");
            Add("Printer", "Máy in", "프린터");
            Add("MES Enable", "Bật MES", "MES 사용");
            Add("Test", "Kiểm tra", "테스트");
            Add("MC CODE", "MÃ MÁY", "장비 코드");
            Add("MES Check Count", "Số lần kiểm tra MES", "MES 확인 횟수");
            Add("Mes", "MES", "MES");
            Add("Display", "Hiển thị", "표시");
            Add("Column", "Cột", "열");
            Add("Row", "Hàng", "행");

            Add("CODE", "MÃ", "코드");
            Add("RIGHT TEXT", "VĂN BẢN PHẢI", "오른쪽 텍스트");
            Add("LEFT TEXT", "VĂN BẢN TRÁI", "왼쪽 텍스트");
            Add("TOP TEXT", "VĂN BẢN TRÊN", "위쪽 텍스트");
            Add("BOTTOM TEXT", "VĂN BẢN DƯỚI", "아래쪽 텍스트");
            Add("Select", "Chọn", "선택");
            Add("Align Left", "Căn trái", "왼쪽 정렬");
            Add("Align Center", "Căn giữa", "가운데 정렬");
            Add("Align Right", "Căn phải", "오른쪽 정렬");
            Add("Align Top", "Căn trên", "위쪽 정렬");
            Add("Align Middle", "Căn giữa", "가운데 정렬");
            Add("Align Bottom", "Căn dưới", "아래쪽 정렬");
            Add("Distribute Horizontally", "Phân bố đều theo chiều ngang", "가로 간격 동일하게");
            Add("Distribute Vertically", "Phân bố đều theo chiều dọc", "세로 간격 동일하게");
            Add("Picture ", "Ảnh ", "이미지 ");
            Add("Picture - TOP", "Ảnh - TRÊN", "이미지 - 상단");
            Add("Picture - BOT", "Ảnh - DƯỚI", "이미지 - 하단");
            Add("Add", "Thêm", "추가");
            Add("Delete", "Xóa", "삭제");
            Add("Delete All", "Xóa tất cả", "전체 삭제");
            Add("Code set", "Cài đặt mã", "코드 설정");
            Add("Position X:", "Vị trí X:", "X 위치:");
            Add("Position Y:", "Vị trí Y:", "Y 위치:");
            Add("Print Type:", "Kiểu in:", "인쇄 유형:");
            Add("Content:", "Nội dung:", "내용:");
            Add("Details...", "Chi tiết...", "세부 정보...");
            Add("Fiducial Point Set", "Thiết lập điểm chuẩn", "기준점 설정");
            Add("Marking List", "Danh sách khắc", "마킹 목록");
            Add("Enable Marking", "Bật khắc", "마킹 사용");
            Add("Up", "Lên", "위로");
            Add("Down", "Xuống", "아래로");
            Add("Marking Setting Image", "Ảnh cài đặt khắc", "마킹 설정 이미지");
            Add("BOT", "DƯỚI", "하단");
            Add("TOP", "TRÊN", "상단");
            Add("TOP/BOT SELECTION", "CHỌN MẶT TRÊN/DƯỚI", "상/하단 선택");
            Add("Load Image", "Tải ảnh", "이미지 불러오기");
            Add("Product Size", "Kích thước sản phẩm", "제품 크기");
            Add("Width (X):", "Chiều rộng (X):", "너비 (X):");
            Add("Height (Y):", "Chiều cao (Y):", "높이 (Y):");
            Add("ImagePixelToMM:", "Tỷ lệ Pixel sang MM:", "픽셀-MM 변환:");
            Add("Robot Resolution:", "Độ phân giải Robot:", "로봇 분해능:");
            Add("Scan Image", "Quét ảnh", "이미지 스캔");
            Add("Save All", "Lưu tất cả", "전체 저장");
            Add("Save One", "Lưu một", "하나 저장");
            Add("Discard", "Hủy thay đổi", "변경 취소");
            Add("Fit", "Vừa khung", "화면 맞춤");
            Add("Click Image", "Chọn ảnh", "이미지 선택");
            Add("Enable", "Bật", "사용");
            Add("Swap X/Y", "Đổi X/Y", "X/Y 교환");
            Add("Flip X", "Lật X", "X 반전");
            Add("Flip Y", "Lật Y", "Y 반전");

            // Vietnamese captions used by existing .aa resources and runtime-created controls.
            Add("TỰ ĐỘNG", "TỰ ĐỘNG", "자동");
            Add("DỪNG", "DỪNG", "정지");
            Add("CHẠY", "CHẠY", "실행");
            Add("Send Pattern to PLC", "Gửi Pattern\\n đến PLC", "PLC로 패턴 전송");
            Add("Save Position", "Lưu vị trí", "위치 저장");
            Add("Move to Position", "Di chuyển đến Vị trí", "위치로 이동");
            Add("Print Type", "Kiểu IN", "인쇄 유형");
            Add("Standby Position", "Vị trí chờ", "대기 위치");
            Add("First Print Position", "Vị trí In đầu tiên", "첫 인쇄 위치");
            Add("Print Spacing (mm)", "Khoảng cách In(mm)", "인쇄 간격(mm)");
            Add("Forward Start Point", "Điểm bắt đầu Phía trước", "전면 시작점");
            Add("Backward Start Point", "Điểm bắt đầu Phía sau", "후면 시작점");
            Add("Print Count", "Số lần In", "인쇄 횟수");
            Add("Print Speed", "Tốc độ In", "인쇄 속도");
            Add("Offset Distance", "Khoảng cách", "오프셋 거리");
            Add("First Scan Position", "Điểm Scan đầu tiên", "첫 스캔 위치");
            Add("Print Direction", "Hướng in", "인쇄 방향");
            Add("Use Round-Trip Print", "Sử dụng in hai chiều", "왕복 인쇄 사용");
            Add("Print Time", "Thời gian In", "인쇄 시간");
            Add("Scan Count", "Số lần Quét", "스캔 횟수");
            Add("MES Connection", "Kết nối Mes", "MES 연결");
            Add("Barcode Type", "Loại Barcode", "바코드 유형");
            Add("Font Type", "Kiểu phông chữ", "글꼴 유형");
            Add("Save Pattern", "Lưu Pattern", "패턴 저장");
            Add("Machine No.", "Máy Số", "장비 번호");
            Add("Delete Pattern", "Xóa Pattern", "패턴 삭제");
            Add("Add Pattern", "Thêm Pattern", "패턴 추가");
            Add("Select Pattern", "Chọn Pattern", "패턴 선택");
            Add("Total Patterns", "Tổng số Pattern", "전체 패턴 수");
            Add("Model Name", "Tên Model", "모델명");
            Add("Create Model", "Tạo Model", "모델 생성");
            Add("Save MODEL", "Lưu MODEL", "모델 저장");
            Add("Use MODEL", "Sử dụng MODEL", "모델 사용");
            Add("Save All", "Lưu tất cả", "전체 저장");
            Add("Fast Speed", "Tốc độ Nhanh", "고속");
            Add("Medium Speed", "Tốc độ Trung", "중속");
            Add("Slow Speed", "Tốc độ chậm", "저속");
            Add("Manual Scan", "Scan Thủ công", "수동 스캔");
            Add("Horizontal Print Head Position", "Vị trí đầu In nằm ngang", "수평 인쇄 헤드 위치");
            Add("Vertical Print Head Position", "Vị trí đầu In nằm dọc", "수직 인쇄 헤드 위치");
            Add("Edit Position", "Sửa Vị trí", "위치 수정");
            Add("Move Speed", "Tốc độ di chuyển", "이동 속도");
            Add("Array Count", "Số Array", "배열 수");
            Add("Enable Scan", "Bật Scan", "스캔 활성화");
            Add("Conveyor Width", "Độ rộng Conveyor", "컨베이어 폭");
            Add("Sử dụng MES", "Sử dụng MES", "MES 사용");
            Add("Sử dụng chế độ Bypass", "Sử dụng chế độ Bypass", "바이패스 모드 사용");
            Add("Sử dụng vị trí Chờ", "Sử dụng vị trí Chờ", "대기 위치 사용");
            Add("Sử dụng Fix Scanner", "Sử dụng Fix Scanner", "고정 스캐너 사용");
            Add("Sử dụng Scanner Cầm tay", "Sử dụng Scanner Cầm tay", "핸드 스캐너 사용");
            Add("Sử dụng Áp suất khí kiểm tra", "Sử dụng Áp suất khí kiểm tra", "공기압 검사 사용");
            Add("Thời gian tự động xóa bộ đếm", "Thời gian tự động xóa bộ đếm", "카운터 자동 초기화 시간");
            Add("Kiểm tra tất cả Scanner", "Kiểm tra tất cả Scanner", "모든 스캐너 검사");
            Add("Bật xoay đầu In", "Bật xoay đầu In", "인쇄 헤드 회전 사용");
            Add("Kiểm tra cửa mở", "Kiểm tra cửa mở", "도어 열림 검사");
            Add("Sử dụng chế độ In thủ công", "Sử dụng chế độ In thủ công", "수동 인쇄 모드 사용");
            Add("Lưu Thông số Cài đặt", "Lưu Thông số Cài đặt", "설정 매개변수 저장");
        }

        private static void Add(string english, string vietnamese, string korean)
        {
            string[] values = { english, vietnamese, korean };
            _translations[english] = values;
            _translations[vietnamese] = values;
            _translations[korean] = values;
        }

        public static string GetLanguageName(AppLanguage language)
        {
            switch (language)
            {
                case AppLanguage.English: return "English";
                case AppLanguage.Korean: return "한국어";
                default: return "Tiếng Việt";
            }
        }

        public static AppLanguage Parse(string value)
        {
            AppLanguage result;
            return Enum.TryParse(value, true, out result) ? result : AppLanguage.Vietnamese;
        }

        public static void Apply(Control root, AppLanguage language)
        {
            if (root == null || root.IsDisposed) return;
            _language = language;
            RegisterTree(root);
            ApplyTree(root);
        }

        private static void RegisterTree(Control control)
        {
            if (control == null || !_registered.Add(control)) return;

            control.ControlAdded += (sender, e) =>
            {
                RegisterTree(e.Control);
                ApplyTree(e.Control);
            };

            control.TextChanged += (sender, e) =>
            {
                if (_isApplying || IsEditableInput(control)) return;
                string translated = Translate(control.Text, _language);
                if (translated == null || translated == control.Text) return;

                try
                {
                    _isApplying = true;
                    control.Text = translated;
                }
                finally { _isApplying = false; }
            };

            DataGridView grid = control as DataGridView;
            if (grid != null)
            {
                grid.ColumnAdded += (sender, e) =>
                {
                    string translated = Translate(e.Column.HeaderText, _language);
                    if (translated != null) e.Column.HeaderText = translated;
                };
            }

            foreach (Control child in control.Controls)
                RegisterTree(child);
        }

        private static void ApplyTree(Control control)
        {
            if (control == null) return;

            string translated = IsEditableInput(control) ? null : Translate(control.Text, _language);
            if (translated != null && translated != control.Text)
            {
                try
                {
                    _isApplying = true;
                    control.Text = translated;
                }
                finally { _isApplying = false; }
            }

            DataGridView grid = control as DataGridView;
            if (grid != null)
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    string header = Translate(column.HeaderText, _language);
                    if (header != null) column.HeaderText = header;
                }
            }

            ListView list = control as ListView;
            if (list != null)
            {
                foreach (ColumnHeader column in list.Columns)
                {
                    string header = Translate(column.Text, _language);
                    if (header != null) column.Text = header;
                }
            }

            ToolStrip strip = control as ToolStrip;
            if (strip != null) ApplyToolStripItems(strip.Items);

            foreach (Control child in control.Controls)
                ApplyTree(child);
        }

        private static void ApplyToolStripItems(ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                string translated = Translate(item.Text, _language);
                if (translated != null) item.Text = translated;

                ToolStripDropDownItem dropDown = item as ToolStripDropDownItem;
                if (dropDown != null) ApplyToolStripItems(dropDown.DropDownItems);
            }
        }

        private static bool IsEditableInput(Control control)
        {
            return control is TextBoxBase || control is ComboBox || control is NumericUpDown;
        }

        private static string Translate(string text, AppLanguage language)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string[] values;
            if (!_translations.TryGetValue(text, out values)) return null;
            switch (language)
            {
                case AppLanguage.English: return values[0];
                case AppLanguage.Korean: return values[2];
                default: return values[1];
            }
        }
    }
}