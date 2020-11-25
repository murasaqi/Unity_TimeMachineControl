using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEditor;


public class TimeMachineControlMixer : PlayableBehaviour
{
    private List<TimelineClip> m_clips;
    internal List<TimelineClip> clips
    {
        get { return m_clips; }
        set { m_clips = value; }
    }
    internal PlayableDirector m_PlayableDirector;
    private List<TimeMachineControlBehaviour> inputs = new List<TimeMachineControlBehaviour>();
    private TimeMachineControlBehaviour currentInput = null;
    public bool initialized = false;
    private TimeMachineTrackManeger trackBinding = null;
    private int currentInputCount = 0;
    
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        trackBinding = playerData as TimeMachineTrackManeger;

        if (!trackBinding)
            return;

        if (!initialized)
        {
            Init(playable);
            trackBinding.OnNextState += OnNextState;
            trackBinding.OnInit += InitEvents;
            trackBinding.OnForceMoveClip += ForceMoveClip;


        }

        
        if (trackBinding.clipCount != clips.Count)
        {
            InitClipViewer();
        }



        double time = m_PlayableDirector.time;
        var counter = 0;
        foreach (var clip in clips)

        {

            var inputPlayable = (ScriptPlayable<TimeMachineControlBehaviour>)playable.GetInput(counter);
            var input = inputPlayable.GetBehaviour();
            if (clip.start <= time && clip.end >= time)
            {

                currentInput = input;
                currentInputCount = counter;
                trackBinding.currentClipCount = currentInputCount;
                if (trackBinding.currentClipCount != counter)
                {

                    trackBinding.currentClipCount = counter;
                    // trackBinding.UpdateStatus();
                    break;
                }

                if (!input.isFinishRole && input.timeMachineClipEvent == TimeMaschineClipEvent.SKIP)
                {
                    m_PlayableDirector.time = clip.end-trackBinding.frameDuration;
                    input.isFinishRole = true;
                    break;

                }
            }
            
            if (!input.isFinishRole && input.timeMachineClipEvent == TimeMaschineClipEvent.PAUSE && time >= clip.end - trackBinding.frameDuration)
            {

                trackBinding.EnableClickButton();

                m_PlayableDirector.time = clip.end- trackBinding.frameDuration;
                break;

            }

            if (!input.isFinishRole && input.timeMachineClipEvent == TimeMaschineClipEvent.LOOP && time >= clip.end - trackBinding.frameDuration)
            {
                if (m_PlayableDirector.state == PlayState.Playing) m_PlayableDirector.time = clip.start;
            }


            counter++;

        }

        // if (currentInput != null)
        // {
        //     if (currentInput.isFinishRole == false && currentInput.timeMaschineClipEvent == TimeMaschineClipEvent.LOOP)
        //     {
        //         trackBinding.currentClipStatus = TimeMaschineClipEvent.LOOP;
        //     }
        //
        //     if (currentInput.isFinishRole == true && currentInput.timeMaschineClipEvent == TimeMaschineClipEvent.LOOP)
        //     {
        //         trackBinding.currentClipStatus = TimeMaschineClipEvent.THOROUGH;
        //     }
        // }


    }

    private void InitClipViewer()
    {
        
    }

    public void InitEvents()
    {
        foreach (var input in inputs)
        {
            input.isFinishRole = false;
        }
        
    }

    private void Init(Playable playable)
    {
        var count = 0;
        var timelineControlBehaviourList = new List<TimeMachineControlClipValue>();
        inputs = new List<TimeMachineControlBehaviour>();
        foreach (var c in clips)
        {
            var inputPlayable = (ScriptPlayable<TimeMachineControlBehaviour>)playable.GetInput(count);
            var input = inputPlayable.GetBehaviour();
            inputs.Add(input);
            var clipValues = new TimeMachineControlClipValue();
            clipValues.name = c.displayName;
            clipValues.duration = c.duration;
            clipValues.index = count;
            clipValues.clipEvent = input.timeMachineClipEvent;
            clipValues.start = c.start;
            timelineControlBehaviourList.Add(clipValues);
            input.isFinishRole = false;
            count++;
        }

        trackBinding.clipValues = timelineControlBehaviourList;
        trackBinding.InitGui();
        initialized = true;
    }

    private void ForceMoveClip(int index)
    {
        if (inputs != null)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                if (i < index)
                {
                    inputs[i].isFinishRole = true;
                }

                if (i == index)
                {
                    inputs[i].isFinishRole = false;
                    m_PlayableDirector.time = clips[i].start;
                    m_PlayableDirector.Pause();
                    currentInput = inputs[i];
                    currentInputCount = i;
                    trackBinding.currentClipCount = currentInputCount;
                }
            }
        }
    }
    private void OnNextState()

    {
        currentInput.isFinishRole = true;
        m_PlayableDirector.time = clips[currentInputCount].end;

    }
}
