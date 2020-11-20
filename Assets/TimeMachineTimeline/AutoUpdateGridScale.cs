using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AutoUpdateGridScale : MonoBehaviour
{

    private RectTransform rectTransform =null;

    [SerializeField] private Material gridMat;
    // private readonly float propScaleU = Shader.PropertyToID("_Scale_U");
    [SerializeField] private Vector2 pixelPerScale = new Vector2(200,60);
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
       
    }

    public void UpdateScale(int width, int height)
    {
        UpdateScale(width/pixelPerScale.x,height/pixelPerScale.y);
    }
    

    // Update is called once per frame
    private void UpdateScale(float u, float v)
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        
        gridMat.SetFloat("_Scale_U",u);
        gridMat.SetFloat("_Scale_V",v);
        
        
    }
}
