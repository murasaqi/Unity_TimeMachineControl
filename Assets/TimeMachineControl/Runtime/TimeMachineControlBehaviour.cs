using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;


[System.Serializable]

public class TimeMachineControlBehaviour : PlayableBehaviour
{
    [FormerlySerializedAs("state")] [SerializeField] public TimeMaschineClipEvent timeMachineClipEvent;
    [SerializeField] public bool isFinishRole;
    public string displayName { get; set; }
}
