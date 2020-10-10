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
