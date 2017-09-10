using MacSetter.Essential;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MacSetter.Core;
using System.Threading;

namespace MacSetter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    handled = true;
                    break;
            }
            return (IntPtr)0;
        }

        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            int MONITOR_DEFAULTTONEAREST = 0x00000002;
            IntPtr monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                MONITORINFO monitorInfo = new MONITORINFO();
                GetMonitorInfo(monitor, monitorInfo);
                RECT rcWorkArea = monitorInfo.rcWork;
                RECT rcMonitorArea = monitorInfo.rcMonitor;
                mmi.ptMaxPosition.x = Math.Abs(rcWorkArea.left - rcMonitorArea.left);
                mmi.ptMaxPosition.y = Math.Abs(rcWorkArea.top - rcMonitorArea.top);
                mmi.ptMaxSize.x = Math.Abs(rcWorkArea.right - rcWorkArea.left);
                mmi.ptMaxSize.y = Math.Abs(rcWorkArea.bottom - rcWorkArea.top);
            }
            Marshal.StructureToPtr(mmi, lParam, true);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            /// <summary>x coordinate of point.</summary>
            public int x;
            /// <summary>y coordinate of point.</summary>
            public int y;
            /// <summary>Construct a point of coordinates (x,y).</summary>
            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MONITORINFO
        {
            public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
            public RECT rcMonitor = new RECT();
            public RECT rcWork = new RECT();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
            public static readonly RECT Empty = new RECT();
            public int Width { get { return Math.Abs(right - left); } }
            public int Height { get { return bottom - top; } }
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
            public RECT(RECT rcSrc)
            {
                left = rcSrc.left;
                top = rcSrc.top;
                right = rcSrc.right;
                bottom = rcSrc.bottom;
            }
            public bool IsEmpty { get { return left >= right || top >= bottom; } }
            public override string ToString()
            {
                if (this == Empty) { return "RECT {Empty}"; }
                return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
            }
            public override bool Equals(object obj)
            {
                if (!(obj is Rect)) { return false; }
                return (this == (RECT)obj);
            }
            /// <summary>Return the HashCode for this struct (not garanteed to be unique)</summary>
            public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();
            /// <summary> Determine if 2 RECT are equal (deep compare)</summary>
            public static bool operator ==(RECT rect1, RECT rect2) { return (rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom); }
            /// <summary> Determine if 2 RECT are different(deep compare)</summary>
            public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
        }

        [DllImport("user32")]
        internal static extern bool GetMonitorInfo(IntPtr hMonitor, MONITORINFO lpmi);

        [DllImport("User32")]
        internal static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);


        public MainWindow()
        {
            InitializeComponent();

            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));
            };

            MinimizeButton.Click += (sender, e) => WindowState = WindowState.Minimized;
            MaximizeButton.Click += (sender, e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (sender, e) => Close();

            DataContext = this;

            RefreshCurrentMac(true); //Load Mac hiện tại
            LoadFromFileBefore(); //Lấy đường dẫn file list Mac đã dùng trước đó
        }

        //private void SettingsButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SettingsWindow set = new SettingsWindow();
        //    set.ShowInTaskbar = false;
        //    set.ShowDialog();
        //}

        //Xóa text trong TextBox có nút Clear
        private void ClearTextBox_Click(object sender, RoutedEventArgs e)
        {
            txtFilePath.Clear();
        }


        //Sử dụng để chuyển Tab
        private void btnLeftShow_Click(object sender, RoutedEventArgs e)
        {
            tabVis.LefTabVisibility = true;
        }

        private void btnRightShow_Click(object sender, RoutedEventArgs e)
        {
            tabVis.LefTabVisibility = false;
        }

        private TabVisibility tabVis = new TabVisibility();
        public TabVisibility TabVis { get => tabVis; set => tabVis = value; }



        //Dùng cho thay đổi trạng thái của nút Change
        private ButtonChangeState state = new ButtonChangeState();
        public ButtonChangeState State { get => state; set => state = value; }







        /// <summary>
        /// Check xem được sử dụng địa chỉ Mac từ đâu
        /// </summary>
        /// <returns></returns>
        public InputMacType ChooseWhichMacInputType()
        {
            InputMacType type = InputMacType.None;

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!string.IsNullOrEmpty(txtNewMac.Text))
                    type = InputMacType.Direct;
                else
                {
                    if (!string.IsNullOrEmpty(txtFilePath.Text))
                        type = InputMacType.FromFile;
                    else
                    {
                        type = InputMacType.None;
                    }
                }
            }));

            return type;
        }

        /// <summary>
        /// Lấy chuỗi Mac mới
        /// </summary>
        /// <returns>Chuỗi Mac trả về có thể đã định dạng hay chưa định dạng đều được, có thể trả về null nếu muốn khôi phục Mac mặc định</returns>
        public string GetNewMacToReplace()
        {
            string resultMac = null; //Chuỗi Mac trả về có thể đã định dạng hay chưa định dạng đều được
            InputMacType inputType = ChooseWhichMacInputType();
            this.Dispatcher.Invoke(new Action(() =>
            {
                switch (inputType)
                {
                    case InputMacType.Direct:
                        resultMac = txtNewMac.Text; //Mac ở bất kỳ dạng nào 
                        break;
                    case InputMacType.FromFile:
                        string path = txtFilePath.Text;
                        resultMac = ReadListFile.Instance.GetNextMacFromFile(path); //Mac theo định dạng chung của list lưu trong file
                        break;
                    case InputMacType.None:
                        resultMac = null; //Không có địa chỉ Mac mới
                        break;
                }
            }));

            return resultMac;
        }


        /// <summary>
        /// Cài đặt Mac mới cho máy, hoặc khôi phục về mặc định
        /// </summary>
        /// <returns>True: Nếu thay đổi thành công, False: Nếu không thể thay đổi Mac được</returns>
        public Task<bool> SetNewMac()
        {
            return Task.Run(() =>
            {
                string newMac =  GetNewMacToReplace(); //Mac này không cần định dạng lại vì phương thức SetMac đã định dạng ngay trước khi thay rồi
                try
                {
                    if (newMac != null)
                    {
                        MainWork.Instance.SetMac(newMac);
                        //Trong phương thức SetMac(newMac) có chuyển đổi newMac sang dạng không có dấu phân cách,
                        //do đó các trường hợp không thỏa mãn sẽ được trả về null, đồng thời, trước khi thay đổi có 
                        //kiểm trả newMac != null mới thay đổi, do đó nếu newMac không thỏa mãn thì sẽ không đổi
                    }
                    else
                    {
                        MainWork.Instance.RestoreDefaultMac();
                    }

                    MainWork.Instance.DisableAndEnableNetworkAdapter(); //Disable rồi Enable EthernetAdapter

                    return true;
                }
                catch (Exception)
                {
                    //MessageBox.Show(e.Message); //Cái này cho mục đích thử lỗi
                    return false;
                }
            });
        }

        /// <summary>
        /// Thực hiện thay địa chỉ Mac, thay đổi trạng thái của Rectangle_Change dựa trên kết quả từ SetNewMac()
        /// </summary>
        public async void ChangeMacAndShowResult()
        {
            bool result = await SetNewMac();
            if (result == true) //Nếu thay đổi thành công
            {
                State.ButtonChangingState = ChangingState.Success;
            }
            else
            {
                State.ButtonChangingState = ChangingState.Failure;
            }
            RefreshCurrentMac(false);
        }

        /// <summary>
        /// Đưa lastPosition trở về 0 khi sử dụng file list Mac mới, thay đổi LastPosition trong lúc Runtime - ReadListFile
        /// </summary>
        public void ResetPositionWhenNewFileLoaded()
        {
            ReadListFile.Instance.LastPosition = 0;
        }


        #region Tải lại các thông tin cần thiết khi Window mở lên

        /// <summary>
        /// Lấy Mac hiện tại đang sử dụng đưa vào lblCurrentValue
        /// </summary>
        /// <param name="canInDisableCase">False nếu Refresh Mac ngay sau khi đổi Mac băng phương thức DisableAndEnableNetworkAdapter()</param>
        public void RefreshCurrentMac(bool canInDisableCase)
        {
            if (!canInDisableCase) //TH này xảy ra ngay sau khi Disable rồi Enable khi đổi Mac

            {
                while (!MainWork.Instance.IsEthernetAdapterEnabled())
                {
                    Thread.Sleep(500);
                }
            }

            //Đến đây thì trạng thái của EthernetAdapter đã ổn định, hoặc là Disabled: trả về null khi gán lblCurrentValue.Content,
            //hoặc giá trị đang dùng của Mac nếu Enabled
            string macFromRegistry = MainWork.Instance.GetMacViaNetworkInterface(); //Đây là dạng không có dấu phân cách
            tbCurrentMac.Text = MainWork.Instance.ConvertMac(macFromRegistry, true); //Đã được định dạng để hiển thị lên Label
        }

        /// <summary>
        /// Tải đường dẫn của file List Mac đã dùng ở lần trước
        /// </summary>
        public void LoadFromFileBefore()
        {
            txtFilePath.Text = AppSettings.Instance.PathListFile;
        }

        /// <summary>
        /// Lưu đường dẫn hiện tại vào config
        /// </summary>
        public void SaveCurrentFromFile()
        {
            AppSettings.Instance.PathListFile = txtFilePath.Text;
        }

        /// <summary>
        /// Lưu vị trí dòng Mac cuối cùng đã đọc, lấy từ ReadListFile
        /// </summary>
        public void SaveLastPosition()
        {
            AppSettings.Instance.LastPosition = ReadListFile.Instance.LastPosition;
        }


        #endregion

        private void btnRefreshMac_Click(object sender, RoutedEventArgs e)
        {
            RefreshCurrentMac(true);
        }

        private void btnChangeMac_Click(object sender, RoutedEventArgs e)
        {
            switch (State.ButtonChangingState)
            {
                case ChangingState.Default:
                    State.ButtonChangingState = ChangingState.Changing;
                    ChangeMacAndShowResult();
                    break;
                case ChangingState.Changing:
                    //Click lúc đang ở trạng thái đang thay đổi thì không làm gì hết
                    break;
                case ChangingState.Success:
                case ChangingState.Failure:
                    State.ButtonChangingState = ChangingState.Default;
                    break;
            }
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string fileName = dlg.FileName;
                txtFilePath.Text = fileName;
            }
        }

        private void MacSetterWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveCurrentFromFile(); //Lưu đường dẫn file list Mac vào config
            SaveLastPosition(); //Lưu dòng đã đọc lần trước
            //Không cần lưu EthernetName vì cái này chỉ đọc, người dùng sẽ tự thay đổi nếu cần thiết

            AppSettings.Instance.WriteConfig(); //Lưu lại config trước khi đóng file
        }

        private void txtFilePath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Sử dụng sự kiện này để reset LastPosition vì: sự kiện chỉ xả ra khi text trong txtFile thay đổi, 
            //nếu có giá trị mới được gán vào nhưng giống hết giá trị cũ thì sự kiện vẫn không xảy ra

            if (this.IsLoaded) //Không cho sự kiện xảy ra ở lần load dữ liệu từ config
            {
                if (string.IsNullOrEmpty(txtFilePath.Text))
                {

                }
                else
                {

                    ResetPositionWhenNewFileLoaded();
                }
            }
        }
    }

    /// <summary>
    /// Enum dùng để lựa chọn Mac được lấy từ đâu
    /// </summary>
    public enum InputMacType
    {
        Direct,
        FromFile,
        None
    }
}
