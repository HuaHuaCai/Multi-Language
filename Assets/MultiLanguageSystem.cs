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
