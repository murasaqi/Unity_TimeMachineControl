using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;



#if UNITY_EDITOR

[CustomEditor(typeof(TimeMachineTrackManeger))]
public class TimeMachineTrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var timeMachineControlManger = target as TimeMachineTrackManeger;
        if (GUILayout.Button("Init GUI"))
        {
            timeMachineControlManger.InitGui();
        }
    }
}
#endif
[Serializable]
public struct TimeMachineControlClipValue
{
    public int index;
    public TimeMaschineClipEvent clipEvent;
    public string name;
    public double duration;
    public double start;
}


[Serializable]
public enum TimeMaschineClipEvent
{
    PAUSE,
    LOOP,
    SKIP,
    THOROUGH
}


[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(PlayableDirector))]
// [ExecuteInEditMode] 

public class TimeMachineTrackManeger : MonoBehaviour
{
    
    public delegate void NextStateHandler();
    public delegate void InitHandler();

    public delegate void ForceMoveClip(int index);
    public event NextStateHandler OnNextState;
    public event InitHandler OnInit;
    public event ForceMoveClip OnForceMoveClip;
    
    [SerializeField] private Vector2 minWindowSize = new Vector2(800,280);
    [SerializeField] private TimelineAsset timelineAsset;
    [SerializeField] private RectTransform clipContainer;
    [SerializeField] private RectTransform windowGrabHeader;
    [SerializeField] private RectTransform timelineGuiWindow;
    [SerializeField] private RectTransform windowResizeButton;
    [SerializeField] private ClipButton uiClipPrefab;
    [SerializeField] private Color throughColor = Color.yellow;
    [SerializeField] private Color pauseColor = Color.yellow;
    // [SerializeField] private Color onEndClipColor = Color.gray;
    [SerializeField] private int clipGuiHeight = 60;
    [SerializeField] private int pixelPerSec = 10;
    [SerializeField] private int clipStartX = 20;
    [SerializeField] private int clipSpace = 0;
    [SerializeField] private int trackEndMargin = 200;
    private PlayableDirector playableDirector;

    [SerializeField] private TMP_InputField timeCode;
    [SerializeField] private Button playButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Sprite iconPlay;
    [SerializeField] private Sprite iconPause;
    [SerializeField] private Texture2D iconHandLock;
    [SerializeField] private Texture2D iconHand;
    [SerializeField] private Texture2D iconEnlarge;
    
    [SerializeField] private RectTransform seekBar;
    // [SerializeField] private int seekBarWidth = 20;
    [SerializeField] private RectTransform seekArea;
    [SerializeField] private Button playLeftArrow;
    [SerializeField] private Button playRightArrow;
    [SerializeField] private AutoUpdateGridScale autoUpdateGridScale;

    public List<TimeMachineControlClipValue> clipValues = new List<TimeMachineControlClipValue>();
    private List<ClipButton> clipsButtons = new List<ClipButton>();
    private Vector2 grabPosition = new Vector2();
    private Vector2 onGrabStartWindowPosition = new Vector2();
    private Vector2 onGrabStartWindowSize = new Vector2();
    private Canvas canvas;

    public UnityEvent onEndInitialize;
    private Color clipColor(TimeMaschineClipEvent clipEvent)
    {
        if (clipEvent == TimeMaschineClipEvent.PAUSE)
        {
            return pauseColor;
        }
        else
        {
            return throughColor;
        }
    }
    
    private Color onEndClipColor(TimeMaschineClipEvent clipEvent)
    {
        Color c;
        if (clipEvent == TimeMaschineClipEvent.PAUSE)
        {

            c = pauseColor;
        }
        else
        {
            c = throughColor;
        }

        c.a = 0.3f;

        return c;
    }

    // private Color onEndClipColor = Color.yellow;
    public int currentClipCount = 0;
    
    public int clipCount
    {
        get => transform.childCount;
    }

   
    public double frameDuration => 1d / timelineAsset.editorSettings.fps;
    private void OnValidate()
    {
        
    }

    private void Start()
    {
        playableDirector = GetComponent<PlayableDirector>();
        if(playableDirector.state != PlayState.Playing)playableDirector.Play();
        // InitGui();
    }

    private void OnEnable()
    {
        
    }


    public int GetTrackWidth()
    {
        var width =  Mathf.CeilToInt((float) playableDirector.duration) * pixelPerSec + clipStartX + trackEndMargin;
        if (clipValues != null) width += clipSpace * clipValues.Count;

        return width;
    }

    private void OnDestroy()
    {
        for( int i = clipContainer.childCount - 1; i >= 0; --i ){
            DestroyImmediate( clipContainer.GetChild( i ).gameObject );
        }
    }
   

    public void InitGui()
    {

        canvas = GetComponent<Canvas>();
        for( int i = clipContainer.childCount - 1; i >= 0; --i ){
            DestroyImmediate( clipContainer.GetChild( i ).gameObject );
        }
        clipsButtons = new List<ClipButton>();
        if (clipValues != null)
        {
            var x = (float)clipStartX;
            foreach (var clip in clipValues)
            {
                
                var c = AddClip(clip);
                var clipRect  = c.GetComponent<RectTransform>();
                clipRect.anchoredPosition = new Vector2(x,0);
                x += clipRect.rect.width;
                clipsButtons.Add(c);
                x += clipSpace;
            }
            
        }
        clipContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(GetTrackWidth(),clipGuiHeight);
        playableDirector.Pause();
        
        
        playButton.onClick.AddListener(() =>
        {
            if (playableDirector.state == PlayState.Playing)
            {
                OnButtonPause();
            }
            else
            {
                OnButtonPlay();
            }
        });
        
        MoveSeekBar();

        var seekEventTrigger = seekBar.GetComponent<EventTrigger>();
        var onDragEvent = new EventTrigger.Entry();
        onDragEvent.eventID = EventTriggerType.Drag;
        onDragEvent.callback.AddListener((e) =>
        {
            
            OnSeek();
        });
        
        var onEndDragEvent = new EventTrigger.Entry();
        onEndDragEvent.eventID = EventTriggerType.EndDrag;
        onEndDragEvent.callback.AddListener((e) =>
        {
            OnSeek();
        });
        seekEventTrigger.triggers.Add(onDragEvent);
        
        resetButton.onClick.AddListener(ResetTimeline);
    
        autoUpdateGridScale.UpdateScale(GetTrackWidth(),(int)clipContainer.rect.height);


        var grabEventTrigger = windowGrabHeader.gameObject.AddComponent<EventTrigger>();
        
        var onGrabStart = new EventTrigger.Entry();
        onGrabStart.eventID = EventTriggerType.BeginDrag;
        onGrabStart.callback.AddListener((e) =>
        {
            // Input.mousePosition.x;
            grabPosition = GetMousePos();
            onGrabStartWindowPosition = timelineGuiWindow.anchoredPosition;
            // Cursor.SetCursor( ,Vector2.zero, MouseCursor.Arrow);
        });
        
        var onGrab = new EventTrigger.Entry();
        onGrab.eventID = EventTriggerType.Drag;
        onGrab.callback.AddListener((e) =>
        {
            var diff = GetMousePos() - grabPosition;
            var pos = onGrabStartWindowPosition + diff;
            // Debug.Log(pos);
            pos = new Vector2(Mathf.Max(pos.x, 0),Mathf.Min(pos.y, 0));
            timelineGuiWindow.anchoredPosition = pos;
            // Input.mousePosition.x;
            // InitMousePosition();
        });
        
        grabEventTrigger.triggers.Add(onGrabStart);
        grabEventTrigger.triggers.Add(onGrab);

        var windowResizeEventTrigger = windowResizeButton.gameObject.AddComponent<EventTrigger>();
        
        var onPointer = new EventTrigger.Entry();
        onPointer.eventID = EventTriggerType.PointerEnter;
        onPointer.callback.AddListener((e) =>
        {
            Cursor.SetCursor(iconEnlarge,Vector2.zero, CursorMode.Auto);
        });
        
        var onBeginResize = new EventTrigger.Entry();
        onBeginResize.eventID = EventTriggerType.BeginDrag;
        onBeginResize.callback.AddListener((e) =>
        {
            onGrabStartWindowSize = new Vector2(timelineGuiWindow.rect.width,timelineGuiWindow.rect.height);
            grabPosition = GetMousePos();
            Cursor.SetCursor(iconEnlarge,Vector2.zero, CursorMode.Auto);
        });
        
        var onResize = new EventTrigger.Entry();
        onResize.eventID = EventTriggerType.Drag;
        onResize.callback.AddListener((e) =>
        {
            var diff = GetMousePos() - grabPosition;
            var size = (onGrabStartWindowSize - new Vector2(-diff.x, diff.y));
            size = new Vector2(Mathf.Max(size.x, minWindowSize.x), Mathf.Max(size.y,minWindowSize.y));
            timelineGuiWindow.sizeDelta = size;
            Cursor.SetCursor(iconEnlarge,Vector2.zero, CursorMode.Auto);
        });
        
        var onResizeEnd = new EventTrigger.Entry();
        onResizeEnd.eventID = EventTriggerType.EndDrag;
        onResizeEnd.callback.AddListener((e) =>
        {
            Cursor.SetCursor(null,Vector2.zero, CursorMode.Auto);
        });
        
        windowResizeEventTrigger.triggers.Add(onBeginResize);
        windowResizeEventTrigger.triggers.Add(onResize);
        windowResizeEventTrigger.triggers.Add(onPointer);
        windowResizeEventTrigger.triggers.Add(onResizeEnd);

        clipContainer.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
        playLeftArrow.onClick.AddListener(MovePreviousClip);
        playRightArrow.onClick.AddListener(MoveNextClip);

        onEndInitialize?.Invoke();
    }

    private Vector2 GetMousePos()
    {
        var mousePos = Input.mousePosition;
        var magnification = canvas.GetComponent<RectTransform>().sizeDelta.x / Screen.width;
        mousePos.x = mousePos.x * magnification - canvas.GetComponent<RectTransform>().sizeDelta.x / 2;
        mousePos.y = mousePos.y * magnification - canvas.GetComponent<RectTransform>().sizeDelta.y / 2;
        return mousePos;
    }
    
    public ClipButton AddClip(TimeMachineControlClipValue clipvalues)
    {
        
        var clip = Instantiate(uiClipPrefab,clipContainer.transform);
        clip.button.enabled = false;
        clip.SetColor(clipColor(clipvalues.clipEvent));
        clip.eventType = clipvalues.clipEvent;
        clip.button.onClick.AddListener(() =>
        {
            blinkButton = null;
            clip.button.enabled = false;
            clip.SetColor(onEndClipColor(clip.eventType));
            OnNextState.Invoke();
        });
        Debug.Log(clipvalues.name);
        clip.GetComponentInChildren<TextMeshProUGUI>().text = $"{clipvalues.name} [{clipvalues.clipEvent}]";
        clip.GetComponent<RectTransform>().sizeDelta = new Vector2(pixelPerSec* (float)clipvalues.duration,clipGuiHeight); 
        // Debug.Log(clip.GetComponentInChildren<TextMeshProUGUI>().text);
        
        return clip;
    }

  
    private void OnSeek()
    {
        OnButtonPause();
        Vector2 localpoint =new Vector2();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(seekArea, Input.mousePosition, GetComponent<Canvas>().worldCamera, out localpoint);
        // Debug.Log(localpoint.x);
        seekBar.anchoredPosition = new Vector2(Mathf.Max(localpoint.x,clipStartX) ,0);
        
        var duration = Mathf.Max(seekBar.anchoredPosition.x-clipStartX,0) / pixelPerSec;
        playableDirector.time = duration;
        // OnButtonPause();
        UpdateTimeCode();
    }

    public void SeekTo(double time)
    {
        OnButtonPause();
        playableDirector.time = time;
        OnSeek();
    }
    

    private void OnButtonPlay()
    {
        playableDirector.Play();
        playButton.transform.GetChild(0).GetComponent<Image>().sprite = iconPause;
    }
    
    private void OnButtonPause()
    {
        playableDirector.Pause();
        playButton.transform.GetChild(0).GetComponent<Image>().sprite = iconPlay;
    }

    private void UpdateTimeCode()
    {
        var t = (float) playableDirector.time;
        var m = Mathf.FloorToInt(t / 60);
        var s = Mathf.FloorToInt(t -Mathf.FloorToInt(m));
        var ms = (t-Mathf.FloorToInt(t))*100;
        timeCode.text = $"{m.ToString("00")}:{s.ToString("00")}:{ms.ToString("00")}";
    }

    private TimeMachineControlClipValue currentClipValue => clipValues[currentClipCount];

    public void EnableClickButton()
    {
        if (blinkButton ==null)
        {
            currentClipButton.button.enabled = true;
            blinkButton = currentClipButton;    
        }
        
    }
    
    

    public void ResetTimeline()
    {
        
        
        OnInit.Invoke();
        blinkButton = null;
        playableDirector.time = 0;
        foreach (var b in clipsButtons)
        {
            Debug.Log(b.eventType);
            b.SetColor(clipColor(b.eventType));
        }
        OnButtonPause();
        MoveSeekBar();
        UpdateTimeCode();
    }
    
    

    private void MoveSeekBar()
    {
        seekBar.anchoredPosition = new Vector2(clipStartX + (float)playableDirector.time * pixelPerSec, 4);
    }
    private ClipButton blinkButton = null;
    private ClipButton currentClipButton => clipsButtons[currentClipCount];
    private void Update()
    {

        if (playableDirector.state == PlayState.Playing)
        {
            MoveSeekBar();
            UpdateTimeCode();

        }

        if (blinkButton != null)
        {
            var c = clipColor(blinkButton.eventType);
            c.a = (Mathf.Sin(DateTime.Now.Millisecond*0.001f * Mathf.PI*2)+1)/2f * 0.5f + 0.5f;
            blinkButton.SetColor(c);
        }
      
    }

    public void MoveNextClip()
    {
        MoveClip(currentClipCount + 1);
    }
    public void MovePreviousClip()
    {
        var index = currentClipCount - 1;
        if (playableDirector.time > currentClipValue.start) index = currentClipCount;
        MoveClip(index);
    }
    public void MoveClip(int index)
    {
        OnButtonPause();

        var i = Mathf.Clamp(index, 0, clipValues.Count - 1);
        if (clipsButtons != null)
        {
            // var next = clipValues[i];
            OnForceMoveClip.Invoke(i);
            MoveSeekBar();
            UpdateTimeCode();
        }
        
    }
    
    // public void UpdateStatus()
    // {
    //     var totalWidth = 0f;
    //     var count = currentClipCount;
    //     var loopCounter = 0;
    //     var maxWidth = GetComponent<RectTransform>().rect.width;
    //
    //     
    //     
    //     if (pause)
    //     {
    //         Timeline.time = pauseTime;
    //         Timeline.Evaluate();
    //     }
    //
    //     foreach (var clip in clips)
    //     {
    //
    //         clip.SetActive(false);
    //     }
    //
    //     while (totalWidth < maxWidth)
    //     {
    //
    //         if (count >= clipValus.Count) break;
    //         var clip = clips[count];
    //         var info = clipValus[count];
    //         float width = MaxTimelineWidth * (float)info.duration;
    //         if (totalWidth + width > maxWidth) break;
    //         clip.gameObject.SetActive(true);
    //         var rect = clip.GetComponent<RectTransform>();
    //         var uiText = clip.transform.GetChild(0).GetComponent<Text>();
    //
    //         clip.GetComponent<Image>().color = waitingClipColor;
    //         clip.name = info.name;
    //         rect.sizeDelta = new Vector2(width, rect.rect.height);
    //         uiText.rectTransform.sizeDelta = new Vector2(width, rect.rect.height);
    //         uiText.text = info.name;
    //         count++;
    //         loopCounter++;
    //         totalWidth += width;
    //     }
    //
    //     if (currentClipCount >= 0)
    //     {
    //         recentClipImage = clips[currentClipCount].GetComponent<Image>();
    //     }
    //
    //
    //
    // }
    //
    // public void NextState()
    // {
    //     nextStateHandler();
    //     //            Timeline.time = 0;
    //     //            Timeline.Pause();
    // }
    //
    //
    // public void StartTimeline()
    // {
    //     pause = false;
    //     Timeline.Play();    
    //     
    // }
    //
    // public void PauseTimeline()
    // {
    //     Timeline.Pause();
    //     pauseTime = Timeline.time;
    //     pause = true;
    // }
    // public void Seek()
    // {
    //     //            isSeek = true;
    //     //            Timeline.time = seekBar.value * Timeline.duration;
    //     //            isSeek = false;
    // }

}
