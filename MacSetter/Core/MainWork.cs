using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Management;
using System.Net.NetworkInformation;

namespace MacSetter.Core
{
    public class MainWork
    {
        //Chỉ có 2 dạng của Mac:
        //1. Không có ký tự đặc biệt: 08EDB9BB0F9F
        //2. Có ký tự phân cách, chuẩn là sử dụng dấu gạch ngang: 08-ED-B9-BB-0F-9F;

        public string ethernetName = "Ethernet"; //Cái này có thể thay đổi phù hợp với mỗi máy, được load từ file settings
        public ManagementObject ethernetAdapterObject; //Cái này sẽ được gán giá trị mỗi khi khởi tạo class MainWork, 
                                                       //sử dụng biến toàn cục để không phải load lại mỗi khi cần sử dụng
                                                       //ethernetAdapterObject["Name"]: "Realtek PCIe GBE Family Controller", 
                                                       //ethernetAdapterObject["NetConnectionID"]: "Ethernet"
        public string ethernetRegistryPath; //Giá trị này được load lại mỗi khi khởi động, không được ghi vào file settings
                                            //Giá trị này: SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0002

        public const int MACLENGTH = 12;

        private static MainWork instance;
        public static MainWork Instance
        {
            get
            {
                if (instance == null)
                    instance = new MainWork();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private MainWork()
        {
            SetEthernetAdapterObject();
            ethernetRegistryPath = FindEthernetAdapterRegistryPath();
        }

        /// <summary>
        /// Chuyển đổi Mac sang 2 dạng tiêu chuẩn: không có ký tự khác hoặc có dấu gạch ngang phân cách
        /// </summary>
        /// <param name="anyTypeOfMac"></param>
        /// <param name="toSeparate">Chuyển sang dạng nào: True = Có dấu gạch ngang phân cách, False = Loại bỏ mọi ký tự khác</param>
        /// <returns>Chuỗi Mac sau khi đã chuyển đổi, nếu không thể chuyển đổi được về dạng MAC 12 ký tự thì trả về null</returns>
        public string ConvertMac(string anyTypeOfMac, bool toSeparate)
        {
            if (string.IsNullOrEmpty(anyTypeOfMac))
                return null; //Nếu không có chuỗi Mac truyền vào thì trả về null luôn

            Regex rgx = new Regex("[^a-zA-Z0-9]");
            string onlyWordMac = rgx.Replace(anyTypeOfMac, "");
            if (onlyWordMac.Length != MACLENGTH)
                return null;

            string macAfterConvert = null;

            if (toSeparate)
            {
                macAfterConvert = "";
                for (int i = 0; i < onlyWordMac.Length; i++)
                {
                    macAfterConvert += onlyWordMac[i];
                    if ((i % 2 == 1) && (i != onlyWordMac.Length - 1))
                    {
                        macAfterConvert += "-";
                    }
                }
            }
            else
            {
                macAfterConvert = onlyWordMac;
            }
            return macAfterConvert;
        }

        /// <summary>
        /// Tìm đường dẫn đến EthernetAdapter dựa vào tên của EthernetAdapter, dùng Control Panel\Network and Internet\Network Connections để xem tên này
        /// </summary>
        /// <returns>Trả về giá trị cần tìm, hoặc null nếu không tìm thấy</returns>
        public string FindEthernetAdapterRegistryPath()
        {
            string path = null;
            RegistryKey rKey;

            rKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}", true);

            foreach (var v in rKey.GetSubKeyNames())
            {
                try
                {
                    RegistryKey netAdapterKey = rKey.OpenSubKey(v, true);
                    if (netAdapterKey != null)
                    {

                        string driverDesc = netAdapterKey.GetValue("DriverDesc")?.ToString() ?? "";//Lấy DriverDesc của Adapter
                        if (driverDesc.Equals(ethernetAdapterObject["Name"].ToString(), StringComparison.OrdinalIgnoreCase))//So sánh giá trị cần lấy, ví dụ là Realtek PCIe GBE Family Controller
                        {
                            //Đường dẫn registry đến EthernetAdapter,
                            //ví dụ: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0002
                            //Console.WriteLine("\t\Path: " + netAdapterKey.Name); 

                            path = netAdapterKey.Name;
                            //Path ban đầu dạng: HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0002
                            path = path.Substring(path.IndexOf(@"\") + 1);
                            //Sau khi chuyển: SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}\0002
                        }

                    }
                }
                catch (SecurityException)
                {
                    //Thực hiện các lệnh khi không thể truy cập vào SubRegistry bị lỗi SecurityException
                }
            }

            return path;
        }

        /// <summary>
        /// Tìm Adapter của Ethernet, gán vào biến toàn cục ethernetAdapterObject
        /// </summary>
        public void SetEthernetAdapterObject()
        {
            try
            {
                ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapter");
                ManagementObjectCollection mgmtObjectColl = managementClass.GetInstances();

                foreach (ManagementObject mgmtObject in mgmtObjectColl)
                {
                    if (mgmtObject["NetConnectionID"] != null && mgmtObject["NetConnectionID"].Equals(ethernetName)) //NetConnectionID: "Ethernet"
                    {
                        ethernetAdapterObject = mgmtObject;

                        //Console.WriteLine("{0}, {1}", mgmtObject["Name"], mgmtObject["NetConnectionID"]);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Disable và Enable Ethernet Adapter
        /// </summary>
        public void DisableAndEnableNetworkAdapter()
        {
            try
            {
                object result = ethernetAdapterObject.InvokeMethod("Disable", null);
                object result3 = ethernetAdapterObject.InvokeMethod("Enable", null);
                //Console.WriteLine("{0}, {1}", mgmtObject["Name"], mgmtObject["NetConnectionID"]);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Đặt giá trị Mac mới vào registry
        /// </summary>
        /// <param name="newMac">Chuỗi chứa địa chỉ Mac, không phân biệt các dạng của Mac</param>
        public void SetMac(string newMac)
        {
            newMac = ConvertMac(newMac, false); //Chuyển đổi chuỗi Mac sang dạng không có dấu phân cách 
                                                //do giá trị trong registry không được có dấu phân cách
            if (ethernetRegistryPath != null && newMac != null)
            {
                try
                {
                    RegistryKey rKey = Registry.LocalMachine.OpenSubKey(ethernetRegistryPath, true);
                    rKey.SetValue("NetworkAddress", newMac);
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                throw new Exception("Mac Address did not changed.");
            }
        }

        /// <summary>
        /// Lấy giá trị Mac hiện tại từ registry (Nên sử dụng phương thức GetMacViaNetworkInterface)
        /// </summary>
        /// <returns>Giá trị Mac hiện tại ở dạng không có dấu phân cách, null nếu không tìm thấy Value "NetworkAddress" hoặc có lỗi xảy</returns>
        public string GetMacViaRegistry()
        {
            if (ethernetRegistryPath != null)
            {
                RegistryKey rKey = Registry.LocalMachine.OpenSubKey(ethernetRegistryPath, true);
                return (string)rKey.GetValue("NetworkAddress", null);
                //Nếu có Value "NetworkAddress" thì trả về giá trị của nó
                //Nếu không có Value "NetworkAddress" thì trả về null
                //Đáng lẽ trả về giá trị mặc định nhưng chưa biết làm thế nào :v
            }
            return null;
        }

        /// <summary>
        /// Xóa 1 Value từ 1 Registry Key
        /// </summary>
        /// <param name="keyPath">Đường dẫn đến Registry Key</param>
        /// <param name="keyValue">Tên của Value cần xóa, ví dụ: NetworkAddress</param>
        public void DeleteRegistryValue(string keyPath, string keyValue)
        {
            using (RegistryKey rKey = Registry.LocalMachine.OpenSubKey(keyPath, true))
            {
                if (rKey.GetValue(keyValue, null) != null)
                {
                    rKey.DeleteValue(keyValue);
                }
            }
        }

        /// <summary>
        /// Lấy Mac đang sử dụng của EthernetAdapter
        /// </summary>
        /// <returns>Trả về giá trị Mac hiện tại đang sử dụng của EthernetAdapter, trả về giá trị gốc của máy nếu Mac không được thiết lập</returns>
        public string GetMacViaNetworkInterface()
        {
            var macAddr =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.Name == ethernetName
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();

            return macAddr;
        }

        /// <summary>
        /// Kiểm tra xem EthernetAdapter đã được Enabled hay chưa
        /// </summary>
        /// <returns>True nếu Enabled, False nếu Disabled</returns>
        public bool IsEthernetAdapterEnabled()
        {
            return ((
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.Name == ethernetName
                select nic
             ).FirstOrDefault() != null) ? true : false;

        }

        /// <summary>
        /// Sử dụng phương thức DeleteRegistryValue(string keyPath, string keyValue) để xóa "NetAddress" trong Registry Key
        /// </summary>
        public void RestoreDefaultMac()
        {
            if (ethernetRegistryPath != null)
            {
                DeleteRegistryValue(ethernetRegistryPath, "NetworkAddress");
            }
        }
    }
}
