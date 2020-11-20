using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;


public class TimeMachineControlClip : PlayableAsset
{
    [SerializeField] public TimeMachineControlBehaviour template;
    [FormerlySerializedAs("state")] [SerializeField] public TimeMaschineClipEvent timeMaschineClipEvent = TimeMaschineClipEvent.THOROUGH;
    [SerializeField] public bool isFinishRole = false;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TimeMachineControlBehaviour>.Create(graph, template);
        var behaviour = playable.GetBehaviour();
        behaviour.timeMachineClipEvent = timeMaschineClipEvent;
        behaviour.isFinishRole = isFinishRole;
        return playable;
    }
}
