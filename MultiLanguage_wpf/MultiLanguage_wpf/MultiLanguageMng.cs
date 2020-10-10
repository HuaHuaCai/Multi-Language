using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;

namespace MultiLanguage
{
    class MultiLanguageMng
    {
        //bin文件存放路径
        private const string _outputPath = "../../../Output/";

        //Excel表路径
        string _inputPath = string.Empty;

        /// <summary>
        /// 生成bin文件
        /// </summary>
        /// <param name="inputPath"></param>
        public void CreateBinaryData(string inputPath)
        {
            if (_inputPath != inputPath) _inputPath = inputPath;

            var styleDatas = ReadStyle();
            WriteStyle(styleDatas);
            ReadTextConfig();
        }

        /// <summary>
        /// 从Excel表中读取多语言文本数据
        /// </summary>
        void ReadTextConfig()
        {
            List<TextData> datas = new List<TextData>();
            using (var stream = File.Open(_inputPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet dataSet = reader.AsDataSet();
                    DataTable textTable = dataSet.Tables[(int)Sheet.TEXT_CONFIG];

                    for (int i = 1; i < textTable.Columns.Count; i++)
                    {
                        for (int j = 1; j < textTable.Rows.Count; j++)
                        {
                            TextData data = new TextData();
                            data.ID = j + 1;
                            data.Style = textTable.Rows[j][0].ToString();
                            data.Content = textTable.Rows[j][i].ToString();
                            datas.Add(data);
                        }

                        //保存数据
                        WriteLanuage(textTable.Rows[0][i].ToString(), datas);
                        datas.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// 将读取到的多语言数据写成二进制文件
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="datas"></param>
        void WriteLanuage(string tableName, List<TextData> datas)
        {
            using (var steam = new FileStream(_outputPath + tableName, FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(steam))
                {
                    //先写人Count,读取的使用才知道需要几次才能读完全部数据
                    writer.Write(datas.Count);
                    foreach (var data in datas)
                    {
                        writer.Write(data.ID);
                        writer.Write(data.Style);
                        writer.Write(data.Content);
                    }
                }
            }
        }

        /// <summary>
        /// 从Excel表中读取字体样式数据
        /// </summary>
        /// <returns></returns>
        List<string> ReadStyle()
        {
            List<string> datas = new List<string>();
            using (var stream = File.Open(_inputPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet dataSet = reader.AsDataSet();
                    DataTable table = dataSet.Tables[(int)Sheet.STYLE_CONFIG];
                    datas.Add((table.Rows.Count - 1).ToString());
                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        for (int j = 0; j < table.Columns.Count; j++)
                        {
                            datas.Add(table.Rows[i][j].ToString());
                        }
                    }
                }
            }

            return datas;
        }

        /// <summary>
        /// 将字体样式数据写成二进制文件
        /// </summary>
        /// <param name="datas"></param>
        void WriteStyle(List<string> datas)
        {
            using (var steam = new FileStream(_outputPath + "Style", FileMode.Create, FileAccess.Write))
            {
                using (var writer = new BinaryWriter(steam))
                {
                    foreach (var data in datas)
                    {
                        writer.Write(data);
                    }
                }
            }
        }


        public enum Sheet
        {
            //文本数据
            TEXT_CONFIG,
            //文本风格样式
            STYLE_CONFIG,
        }

    }
}
