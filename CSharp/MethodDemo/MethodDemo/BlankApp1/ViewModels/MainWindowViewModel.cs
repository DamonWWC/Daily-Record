using Prism.Mvvm;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BlankApp1.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {



            DownloadLargeFile("http://127.0.0.1:6607/mics/task/micsTaskLog/exportExcel?micsTaskLogIds=23", "D:\\");

            var jsinfo = new JsInfo
            {
                hmiurl="/url/bas",
                command="10021",
                data= "1234565",
                message= "获取当前车站的所有子系统的模式手自动控制值和说明,datetime[2025/2/8 10:06]"
            };

            var data = new { command = "10021", data=JsonSerializer.Serialize(jsinfo),message="成功"};

            var datastr = JsonSerializer.Serialize(data);
            string jsonString = "{\"name\":\"John\",\"age\":30,\"city\":\"New York\"}";
            var aa = JsonSerializer.Deserialize<JsonString>(jsonString);
            using (JsonDocument document = JsonDocument.Parse(jsonString))
            {
                JsonElement root = document.RootElement;
                var nameElement = root.GetProperty("name").ToString();
                //Console.WriteLine(nameElement.GetString());
            }


            var screens= WpfScreenHelper.Screen.AllScreens;
        }
        public void DownloadLargeFile(string url, string savePath)
        {
           
            var client = new RestClient(url);
            var request = new RestRequest();
            
            string tempFile = Path.GetTempFileName();

            try
            {
                using (var writer = File.OpenWrite(tempFile))
                {
                    request.ResponseWriter = (responseStream) =>
                    {
                        responseStream.CopyTo(writer);
                        return responseStream;
                    };
                    client.DownloadData(request); // 流写入临时文件
                }
                File.Move(tempFile, savePath); // 转移至目标路径
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile); // 必须清理临时文件❗️
            }
        }


    }
    public class JsInfo
    {
        /// <summary>
        /// 组态路径
        /// </summary>
        public string hmiurl { get; set; }
        /// <summary>
        /// 命令码
        /// </summary>
        public string command { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object data { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string message { get; set; }
    }
    public class JsonString
    {
        public string name { get; set; }
        public int age { get; set; }       
    }
}
