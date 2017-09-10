using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MacSetter.Core
{
    public class ReadListFile
    {
        //File được sử dụng chỉ để đọc, file này được tự tạo bằng tay, 
        //gồm các địa chỉ Mac, mỗi dòng một địa chỉ Mac, dạng không phân cách
        //Vị trí của file được chỉ rõ trong TextBox
        //Các dòng trong file sử dụng vị trí bắt đầu từ 1, 2, 3,..., n

        //Có thể sử dụng 1 instance Singleton
        private static ReadListFile instance;
        public static ReadListFile Instance
        {
            get
            {
                if (instance == null)
                    instance = new ReadListFile();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private int lastPosition; //Vị trí của dòng Mac lần đọc cuối, được đọc mỗi khi tạo mới class

        public int LastPosition { get => lastPosition; set => lastPosition = value; }

        private ReadListFile()
        {
            LastPosition = AppSettings.Instance.LastPosition;
        }

        /// <summary>
        /// Đọc dòng tiếp theo trong file, đồng thời thiết lập lastPosition
        /// </summary>
        /// <param name="path">Đường dẫn đến file chứa danh sách Mac</param>
        /// <returns>Trả về Mac tiếp theo, hoặc null nếu không tìm thấy file</returns>
        public string GetNextMacFromFile(string path)
        {
            if (File.Exists(path))
            {
                string line = null;
                int i = 0;
                using (StreamReader reader = new StreamReader(path))
                {
                    do
                    {
                        line = reader.ReadLine();
                        i++;
                        if (line == null)
                        {
                            reader.DiscardBufferedData();
                            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                            i = 0;

                            line = reader.ReadLine();
                            i++;
                            break;
                        }
                    }
                    while (i <= LastPosition);
                    //TH1: dòng tiếp theo vẫn nằm ở giữa: kết thúc hàm này ta được dòng tiếp theo vào số thứ tự của dòng đó
                    //TH2: lastPosition là dòng cuối cùng trước khi thực hiện hàm này: nó sẽ đi vào hàm if(line==null), sau đó
                    //     đưa reader và i về ban đầu, đọc dòng tiếp theo và break.
                    //Cả 2 trường hợp đều cho về kết quả là line là dòng tiếp theo cần đọc, i là chỉ số của dòng tiếp theo
                }

                LastPosition = i; //
                return line;
            }
            return null;
        }
    }
}
