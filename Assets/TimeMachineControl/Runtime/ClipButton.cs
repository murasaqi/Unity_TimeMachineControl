using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ClipButton : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Image colorBar;

    private Button mButton;
    public TimeMaschineClipEvent eventType { get; set; }
    public Button button
    {
        get
        {
            if (mButton == null) mButton = GetComponent<Button>();
            return mButton;
        }
        set
        {
            mButton = value;
        }
    }
    void Start()
    {
        button = GetComponent<Button>();
    }

    public void SetColor(Color color)
    {
        colorBar.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
