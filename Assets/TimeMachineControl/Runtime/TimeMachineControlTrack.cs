using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[TrackColor(255f/255f, 255f/255f, 94f/255f)]
[TrackClipType(typeof(TimeMachineControlClip))]
[TrackBindingType(typeof(TimeMachineTrackManeger))]
public class TimeMachineControlTrack : TrackAsset
{

    private Playable playable;
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var mixer = ScriptPlayable<TimeMachineControlMixer>.Create(graph, inputCount);
        mixer.GetBehaviour().clips = GetClips().ToList();
        mixer.GetBehaviour().m_PlayableDirector = go.GetComponent<PlayableDirector>();

        return mixer;
        
    }

    protected override void OnAfterTrackDeserialize()
    {
        // var mixer = playable as TimeMachineControlMixer;
        // playable.GetBehaviour(Init(playable as Playable);
    }
}
