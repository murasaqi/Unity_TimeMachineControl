using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeMachineOscEventUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textEventName;
    [SerializeField] private TMP_InputField inPutAddress;
    [SerializeField] private Button button;
    [SerializeField] private TimeMachineOscEvent timeMachineOscEvent;
    public OscIn oscIn;
    private OscOut _oscOut;

    public TimeMachineOscController controller;
    // private string _eventName;
    public string eventName
    {
        get
        {
            if (textEventName == null) textEventName = GetComponentInChildren<TextMeshProUGUI>();
            return textEventName.text;
        }
        set
        {
            if (textEventName == null) textEventName = GetComponentInChildren<TextMeshProUGUI>();
            textEventName.text = value;
        }
    }

    public string oscAddress
    {
        get
        {
            if (inPutAddress == null) inPutAddress = GetComponentInChildren<TMP_InputField>();
            return inPutAddress.text;
        }
        set
        {
            if (inPutAddress == null) inPutAddress = GetComponentInChildren<TMP_InputField>();
            inPutAddress.text = value;
            
        }
    }

    public void InitOSCEvent()
    {
        var newOscEventValue = new TimeMachineOscEvent();
        newOscEventValue.address = oscAddress;
        newOscEventValue.index = timeMachineOscEvent.index;
        newOscEventValue.name = timeMachineOscEvent.name;
        oscIn.MapInt(  newOscEventValue.address,controller.OnReceiveMoveClipEvent );
        oscEvent = newOscEventValue;
        var message = new OscMessage();
        message.address = newOscEventValue.address;
        message.Add(newOscEventValue.index);
        testOscMessage = message;
    }
    
    public Button testButton
    {
        get
        {
            if (inPutAddress == null) button = GetComponentInChildren<Button>();
            return button;
        }
    }

    private OscMessage oscMessage;

    public OscMessage testOscMessage
    {
        get
        {
            return oscMessage;
        }
        set
        {
            oscMessage = value;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (_oscOut != null)
                {
                    oscMessage.address = timeMachineOscEvent.address;
                    oscMessage.Set(0, oscEvent.index);
                    _oscOut.Send(oscMessage);
                }
            });
        }
    }

    public void SetTestOscMessage(OscOut oscOut, OscMessage message)
    {
        _oscOut = oscOut;
        testOscMessage = message;
    }
    public TimeMachineOscEvent oscEvent
    {
        get => timeMachineOscEvent;
        set
        {
            timeMachineOscEvent = value;

            oscAddress = timeMachineOscEvent.address;
            eventName = timeMachineOscEvent.name;
            
        } 
        
    }

    // Start is called before the first frame update
    void Start()
    {
        inPutAddress.onValueChanged.AddListener((text) =>
        {
            timeMachineOscEvent.address = text;
            InitOSCEvent();
        });
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
