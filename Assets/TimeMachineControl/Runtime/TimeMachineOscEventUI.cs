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

    private OscOut _oscOut;
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
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
