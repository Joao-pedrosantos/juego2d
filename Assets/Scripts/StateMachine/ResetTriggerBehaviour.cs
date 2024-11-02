using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTriggerBehaviour : StateMachineBehaviour
{
    public string triggerName; // Name of the trigger to reset
    public bool resetOnEnter;  // Reset trigger when entering the state
    public bool resetOnExit;   // Reset trigger when exiting the state
    public bool resetOnStateMachineEnter; // Reset trigger on state machine enter
    public bool resetOnStateMachineExit;  // Reset trigger on state machine exit

    // Called when entering a state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetOnEnter)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    // Called when exiting a state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (resetOnExit)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    // Called when entering a state machine
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (resetOnStateMachineEnter)
        {
            animator.ResetTrigger(triggerName);
        }
    }

    // Called when exiting a state machine
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (resetOnStateMachineExit)
        {
            animator.ResetTrigger(triggerName);
        }
    }
}
