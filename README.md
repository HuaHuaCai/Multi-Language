# Multi-Language
Unity多语言方案

# 准备工作

## 创建数据Excel表

在这个方案中，我希望Text组件所使用的文本，字体样式等都通过读表的形式完成，这样的话策划有啥需求就能通过修改配置表的形式实现。配置表会有两个：

 1. 字体样式表
 
 StyleName| Font|  FontStyle| Size| RichText | CororR | CororG|CororB|CororA|
----|----| ------|------- | -----| -------|----|--|--|
Main  | Arial|Bold|30|True|50|50|50|255|
Battle  | Arial|Normal|40|True|100|100|100|255|
> StyleName:表示样式名称,同时用作键值
> Font:表示需要使用的字体
> FontStyle:表示需要使用的字体样式
> Size:表示字体的大小
> RichText:表示是否支持富文本
> ColorRGBA:表示字体的颜色

2.文本表
Style|Language_CN|LanguageEN|
--|--|--|
Main|中文|英语|
>Style:表示样式，需要和字体样式表中的StyleName对应


## 生成bin文件
这里并非一定要使用bin文件，可根据需求及喜好也可以使用json等形式。为了方便，我使用了==ExcelDataReader==和==ExcelDataReader.DataSet==两个插件来读取Excel表，这两个插件可通过==Nuget==安装到解决方案中。
```c
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
        
		public struct TextData
    	{
        	public int ID;
        	public string Style;
        	public string Content;
    	}
```
# Unity方面
## 创建管理类
首先创建一个类来管理多语言这部分的功能，包括数据的读取，获取当前手机默认语言，语言的切换，切换完成的通知等等,这个类应当是个单例。
```c
using MultiLanguage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class MultiLanguageSystem 
{
    static MultiLanguageSystem _instance;
    public static MultiLanguageSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MultiLanguageSystem();
                _instance.Init();
            }
            return _instance;
        }
    }

    //多语言数据
    Dictionary<int, TextData> _textData;

    //字体样式数据
    Dictionary<string, StyleData> _styleData;

    //目前所选择的语言
    public Language CurrentLanguage { get; private set; }

    //语言切换完成的通知
    private Action _onUpdate;

    private readonly string _configPath = Application.streamingAssetsPath + "/LanguageConfigs/";
    private readonly string _styleConfigName = "Style";

    /// <summary>
    /// 初始化
    /// </summary>
    void Init()
    {
        _textData = new Dictionary<int, TextData>();
        _styleData = new Dictionary<string, StyleData>();
        ReadStyleConfit(_configPath + _styleConfigName);
        CurrentLanguage = GetDefaultLanguage();
        SetLanguage(CurrentLanguage);
    }

    /// <summary>
    /// 获取默认语言
    /// </summary>
    /// <returns></returns>
    Language GetDefaultLanguage()
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.Chinese:
            case SystemLanguage.ChineseSimplified:
            case SystemLanguage.ChineseTraditional:
                return Language.Language_CN;
            default:
                return Language.Language_EN;
        }        
    }

    /// <summary>
    /// 给切换语言添加监听
    /// </summary>
    /// <param name="update"></param>
    public void AddUpdateListener(Action update)
    {
        _onUpdate += update;
    }

    /// <summary>
    /// 设置语言
    /// </summary>
    /// <param name="language"></param>
    public void SetLanguage(Language language)
    {
        CurrentLanguage = language;
        ReadLanguageConfig(_configPath + language);
        if (_onUpdate != null)
            _onUpdate();
    }

    /// <summary>
    /// 读取多语言数据
    /// </summary>
    /// <param name="path"></param>
    void ReadLanguageConfig(string path)
    {
        _textData.Clear();
        using (var steam = new FileStream(path,FileMode.Open,FileAccess.Read))
        {
            using (var reader = new BinaryReader(steam))
            {
                int count = reader.ReadInt32();
                var data = new TextData();
                for (int i = 0; i < count; i++)
                {
                    data.ID = reader.ReadInt32();
                    data.Style = reader.ReadString();
                    data.Content = reader.ReadString();
                    _textData.Add(data.ID, data);
                }
            }
        }
    }

    /// <summary>
    /// 读取字体样式数据
    /// </summary>
    /// <param name="path"></param>
    void ReadStyleConfit(string path)
    {
        using (var steam = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new BinaryReader(steam))
            {
                int count = int.Parse(reader.ReadString());

                StyleData data = new StyleData();
                string styleName = "";
                for (int i = 0; i < count; i++)
                {
                    styleName = reader.ReadString();
                    data.Font = reader.ReadString();
                    data.FontStyle = reader.ReadString();
                    data.Size = int.Parse(reader.ReadString());
                    data.RichText = reader.ReadString().Contains("True");
                    data.r = byte.Parse(reader.ReadString());
                    data.g = byte.Parse(reader.ReadString());
                    data.b = byte.Parse(reader.ReadString());
                    data.a = byte.Parse(reader.ReadString());
                    _styleData.Add(styleName, data);

                }
            }
        }
    }

    /// <summary>
    /// 根据ID获取多语言信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TextData GeTextData(int id)
    {
        if (_textData.ContainsKey(id))
        {
            return _textData[id];
        }
        else
        {
            Debug.LogError("不存在id:" + id);
            return default;
        }
    }

    /// <summary>
    /// 根据样式名字获取字体样式
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public StyleData GetStyleData(string name)
    {
        if(_styleData.ContainsKey(name))
        {
            return _styleData[name];
        }
        else
        {
            Debug.LogError("不存在文本风格:" + name);
            return default;
        }
    }

}

public enum Language
{
    Language_CN,
    Language_EN
}

public struct TextData
{
     public int ID;
     public string Style;
     public string Content;
}

public struct StyleData
{
     public string Font;
     public string FontStyle;
     public int Size;
     public bool RichText;
     public byte r;
     public byte g;
     public byte b;
     public byte a;
}

```
## 给Text组件绑定脚本来监听语言的切换
该脚本需要实现的功能是可以在编辑器设置多语言ID以及在Start在和切换语言的时候更新文本
```c
using MultiLanguage;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LanguageItem : MonoBehaviour
{
    [SerializeField]
    private int ID ;
    private Text _text;
    //字体存放路径
    private readonly string _fontsPath = "Fonts/";

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void Start()
    {
        MultiLanguageSystem.Instance.AddUpdateListener(UpdateData);
        UpdateData();
    }

    /// <summary>
    /// 更新文本数据
    /// </summary>
    void UpdateData()
    {
        TextData textData = MultiLanguageSystem.Instance.GeTextData(ID);
        if(textData.Equals(default))
        {
            Debug.LogError("为获取到正确数据id为:" + ID);
            return;
        }
        _text.text = textData.Content;

        StyleData styleData = MultiLanguageSystem.Instance.GetStyleData(textData.Style);
        if(styleData.Equals(default))
        {
            Debug.LogError("未获取到字体风格数据,名称为:" + textData.Style);
            return;
        }

        _text.font = LoadFont(styleData.Font);
        _text.fontStyle = GetFontStyle(styleData.FontStyle);
        _text.fontSize = styleData.Size;
        _text.supportRichText = styleData.RichText;
        _text.color = new Color32(styleData.r,styleData.g,styleData.b,styleData.a);
        
    }

    private Font LoadFont(string fontName)
    {
        if (_text.font.name == fontName) return _text.font;

        Font font = Resources.Load<Font>(_fontsPath + fontName);

        if (font != null) return font;
        else
        {
            Debug.LogError("加载字体不存在,字体名称为:" + fontName);
            return _text.font;
        }
    }

    FontStyle GetFontStyle(string style)
    {
        if(style == CustomFontStyle.B.ToString())
        {
            return FontStyle.Bold;
        }
        else if(style == CustomFontStyle.I.ToString())
        {
            return FontStyle.Italic;
        }
        else if(style == CustomFontStyle.ALL.ToString())
        {
            return FontStyle.BoldAndItalic;
        }
        else
        {
            return FontStyle.Normal;
        }
    }
}

public enum CustomFontStyle
{
    N,
    B,
    I,
    ALL
}

```
我为了方便，在Editor目录下新建了一个脚本来检测当创建Text组件的时候自动添加LanguageItem脚本
```c
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.UIElements;

public class AutoAddLanguageItem 
{
    [InitializeOnLoadMethod]
    private static void Init()
    {
        EditorApplication.hierarchyChanged += ChangeLanguage;
    }

    private static void ChangeLanguage()
    {
        GameObject go = Selection.activeGameObject;
        if(go != null)
        {
            Text text = go.GetComponent<Text>();
            if(text != null && go.GetComponent<LanguageItem>() == null)
            {
                go.AddComponent<LanguageItem>();
            }
        }
    }
}

```






