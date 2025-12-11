using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using Unity.MLAgents.Policies;
using UnityEngine;

public class OnnxSwitcher : MonoBehaviour
{
    [SerializeField] private BehaviorParameters behaviorParameters;
    [SerializeField] private NNModel onnxModel1;
    [SerializeField] private NNModel onnxModel2;

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
            behaviorParameters.Model = onnxModel1;
        }
        else if (state == 2)
        {
            behaviorParameters.Model = onnxModel2;
        }
    }
}
