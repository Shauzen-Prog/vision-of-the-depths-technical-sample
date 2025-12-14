using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStateHFSM : IState<DrillStateId>
{
    private IFiniteStateMachine<DrillStateId> _fsm;
    private IEventBus _eventBus;
    private DrillStateReferences _references;
    
    public IdleStateHFSM(DrillStateReferences references)
    {
        _references = references;
    }
    
    public void OnEnter()
    {
       _references.playButton.enabled = true;
       _references.stopButton.enabled = true;
       
       _references.modeLever.enabled = true;
       _references.powerLever.enabled = true;
       
       _references.modeLight.enabled = true;
       _references.powerLight.enabled = true;
       _references.heatLight.enabled = true;
       
       //para funcione
       _references.drillAnimator.SetBool("isActive", false);
       _references.tutorialSkipWithSpace.enabled = false;
       _eventBus.Publish(new UpdateMaterialToDrillUI(false));
       _eventBus.Publish(new BarControllerEvent(false));
       
    }

    public void OnExit()
    {
     
    }

    public void SetController(IFiniteStateMachine<DrillStateId> fsm)
    {
        _fsm = fsm;
    }

    public void InjectEventBus(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }
}
