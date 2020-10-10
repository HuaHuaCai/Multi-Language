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
