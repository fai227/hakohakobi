using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Demonstrations;
using UnityEngine;


public class StateSwitcher : MonoBehaviour
{
    [SerializeField] private DemonstrationRecorder state1Recorder;
    [SerializeField] private DemonstrationRecorder state2Recorder;

    void Update()
    {
        if (transform.position.x < 3.3f)
        {
            SwitchState(1);
        }
        else
        {
            SwitchState(2);
        }
    }

    void SwitchState(int state)
    {
        if (state == 1)
        {
            state1Recorder.Record = true;
            state2Recorder.Record = false;
        }
        else if (state == 2)
        {
            state1Recorder.Record = false;
            state2Recorder.Record = true;
        }
    }
}
