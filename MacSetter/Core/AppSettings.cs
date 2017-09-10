using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace MacSetter.Core
{
    public class AppSettings
    {

        //Settings sẽ được đọc từ file config của file .exe
        //Lớp này chỉ cần tạo 1 lần nên có thể sử dụng mô hình Singleton
        private static AppSettings instance;
        public static AppSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new AppSettings();
                return instance;
            }
            set
            {
                instance = value;
            }
        }


        private string pathListFile;
        private int lastPosition;
        private string ethernetName;

        public string PathListFile { get => pathListFile; set => pathListFile = value; }
        public int LastPosition { get => lastPosition; set => lastPosition = value; }
        public string EthernetName { get => ethernetName; set => ethernetName = value; }

        private AppSettings()
        {
            ReadConfig();
        }

        /// <summary>
        /// Đọc settings từ file config
        /// </summary>
        public void ReadConfig()
        {
            PathListFile = ConfigurationManager.AppSettings["pathListFile"].ToString();
            LastPosition = int.Parse(ConfigurationManager.AppSettings["lastPosition"]);
            EthernetName = ConfigurationManager.AppSettings["ethernetName"].ToString();
        }

        /// <summary>
        /// Lưu settings vào file config
        /// </summary>
        public void WriteConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["pathListFile"].Value = PathListFile;
            config.AppSettings.Settings["lastPosition"].Value = LastPosition.ToString();
            //config.AppSettings.Settings["ethernetName"].Value = EthernetName; 
            //Người dùng sẽ tự thay đổi EthernetName nếu cần thiết, hiện tại chỉ có thể thay đổi thông qua VS

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
