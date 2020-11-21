using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TimeMachineOscEvent
{
    public string name;
    public string address;
    public int index;
    public OscMessage oscMessage;
}
public class TimeMachineOscController : MonoBehaviour
{
    [SerializeField] private TimeMachineTrackManeger timeMachineTrackManeger;
    [SerializeField] private TimeMachineOscEventUI timeMachineOscEventUI;
    [SerializeField] private TimeMachineOscEventUI timeMachinePlainOscEventUI;
    [SerializeField] private RectTransform oscEventUiContainer; 
    [SerializeField] private List<TimeMachineOscEvent> timeMachineOscEvents;
    [SerializeField] private Button addNewEventButton;
    [SerializeField] private OscIn oscIn;
    [SerializeField] private OscOut oscOut;
    // private OscMessage testMessage;
    void Start()
    {
        // Init();
        // testMessage = new OscMessage();
        // testMessage.address= "/M1";
        // testMessage.Add(0);
        timeMachineTrackManeger.onEndInitialize.AddListener(Init);
    }

    private void Init()
    {
        for( int i = oscEventUiContainer.childCount - 1; i >= 0; --i ){
            DestroyImmediate( oscEventUiContainer.GetChild( i ).gameObject );
        }
        timeMachineOscEvents.Clear();
        
        foreach (var c in timeMachineTrackManeger.clipValues)
        {
            var oscEventValue = new TimeMachineOscEvent();
            oscEventValue.address = $"/{c.name}";
            oscEventValue.index = c.index;
            oscEventValue.name = c.name;
            timeMachineOscEvents.Add(oscEventValue);
            oscIn.MapInt(  oscEventValue.address,OnReceiveMoveClipEvent );
            var ui =Instantiate(timeMachineOscEventUI, oscEventUiContainer);
            ui.oscEvent = oscEventValue;
            var message = new OscMessage();
            message.address = ui.oscAddress;
            message.Add(c.index);
            ui.SetTestOscMessage(oscOut,message);
           
        }
        // oscIn
    }

    private void OnEnable()
    {
        
    }

    public void AddNewEvent()
    {
        var oscEventValue = new TimeMachineOscEvent();
        oscEventValue.address = "/Test";
        oscEventValue.index = 0;
        oscEventValue.name = "Test";
        timeMachineOscEvents.Add(oscEventValue);
        // oscIn.MapInt(  oscEventValue.address,OnReceiveMoveClipEvent );
        var ui =Instantiate(timeMachinePlainOscEventUI, oscEventUiContainer);
        ui.oscEvent = oscEventValue;
        var message = new OscMessage();
        message.address = ui.oscAddress;
        message.Add(oscEventValue.index);
        ui.SetTestOscMessage(oscOut,message);
    }

    public void OnReceiveMoveClipEvent( int index )
    {
        timeMachineTrackManeger.MoveClip(index);
    }

    // Update is called once per frame
    void Update()
    {

        // if (Input.GetKeyDown("1"))
        // {
        //     testMessage.Set(0,0);
        //     oscOut.Send(testMessage);
        // }
    }
}
