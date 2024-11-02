using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseAttackBehaviour : StateMachineBehaviour
{
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        float randomValue = Random.Range(0f, 1f);
        float comboAttackProbability = 0.5f;

        if (randomValue < comboAttackProbability)
        {
            animator.SetTrigger(AnimationStrings.comboAttackTrigger);
        }
        else
        {
            animator.SetTrigger(AnimationStrings.dashAttackTrigger);
        }
    }
}
