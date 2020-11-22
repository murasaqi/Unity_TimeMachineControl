using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TimelineSeekOscEvent
{
    public string name;
    public string address;
    public double startTime;
    public OscMessage oscMessage;
}
public class TimeMachineOscEventTemplate : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputEventName;
    [SerializeField] private TMP_InputField inputSeekTime;
    [SerializeField] private TMP_InputField inPutAddress;
    [SerializeField] private Button button;
    [SerializeField] private TimelineSeekOscEvent timelineSeekOscEvent = null;
    [SerializeField] private OscOut oscOut;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void InitOscEvent(OscOut osc, string name, string address, double seekTime)
    {
        oscOut = osc;
        if(timelineSeekOscEvent == null)timelineSeekOscEvent = new TimelineSeekOscEvent();
        timelineSeekOscEvent.address = address;
        timelineSeekOscEvent.name = name;
        timelineSeekOscEvent.startTime = seekTime;
        if(timelineSeekOscEvent.oscMessage == null)timelineSeekOscEvent.oscMessage = new OscMessage();;
        timelineSeekOscEvent.oscMessage.address = address;
        timelineSeekOscEvent.oscMessage.Add(seekTime);
        timelineSeekOscEvent.oscMessage.Set(0, seekTime);
        inputEventName.text = name;
        inPutAddress.text = address;
        inputSeekTime.text = seekTime.ToString();
        
        button.onClick.AddListener(() =>
        {
            oscOut.Send(timelineSeekOscEvent.oscMessage);
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
