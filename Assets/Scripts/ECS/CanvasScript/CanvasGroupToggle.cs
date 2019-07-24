using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupToggle : MonoBehaviour
{
    private CanvasGroup CanvasGroup
    {
        get
        {
            if(_CanvasGroup == null)
            {
                _CanvasGroup = this.GetComponent<CanvasGroup>();
            }
            return _CanvasGroup;
        }
    }
    private CanvasGroup _CanvasGroup;

    public void Hide()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        CanvasGroup.alpha = 1;
        CanvasGroup.interactable = true;
        CanvasGroup.blocksRaycasts = true;
    }
}
