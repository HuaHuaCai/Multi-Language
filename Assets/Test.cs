using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnClick()
    {
        Language language;

        switch(MultiLanguageSystem.Instance.CurrentLanguage)
        {
            case Language.Language_CN:
                language = Language.Language_EN;
                break;
            case Language.Language_EN:
                language = Language.Language_CN;
                break;
            default:
                language = Language.Language_CN;
                break;
        }

        MultiLanguageSystem.Instance.SetLanguage(language);
    }
}
