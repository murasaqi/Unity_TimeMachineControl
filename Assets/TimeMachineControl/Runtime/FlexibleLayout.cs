using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FlexibleLayout : MonoBehaviour
{
    
    
    [SerializeField] private RectTransform referenceRect;
    [SerializeField] private bool width;
    [SerializeField] private Vector2 initPosition = Vector2.zero;
    private RectTransform thisRect = null;
    // Start is called before the first frame update
    void Start()
    {
        thisRect = GetComponent<RectTransform>();
        thisRect.anchoredPosition = initPosition;
    }

    private void OnEnable()
    {
        thisRect = GetComponent<RectTransform>();
        thisRect.anchoredPosition = initPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(thisRect == null ) thisRect = GetComponent<RectTransform>();
        if (referenceRect != null)
        {
            if (referenceRect.sizeDelta.x != thisRect.sizeDelta.x)
            {
                if (width)
                {
                    thisRect.sizeDelta = new Vector2(referenceRect.sizeDelta.x,thisRect.sizeDelta.y);    
                }
                
            }
        }
       
    }
}
