

using UnityEngine;

public class DefaultState : ActionBaseState
{
    public float scrollDirection;
    
    public override void EnterState(ActionStateManager actions)
    {

    }

    public override void UpdateState(ActionStateManager actions)
    {
        actions.rightHandAim.weight = Mathf.Lerp(actions.rightHandAim.weight, 1.0f, 10.0f * Time.deltaTime);
        if (actions.leftHandIK.weight == 0.0f) actions.leftHandIK.weight = 1.0f;
        //actions.leftHandIK.weight = Mathf.Lerp(actions.leftHandIK.weight, 1.0f, 10.0f * Time.deltaTime);
        
        if (Input.GetKeyDown(KeyCode.R) && CanReload(actions))
        {
            actions.SwitchState(actions.Reload);
        }
        else if (Input.mouseScrollDelta.y != 0.0f)
        {
            scrollDirection = Input.mouseScrollDelta.y;
            actions.SwitchState(actions.Swap);
        }
    }

    private bool CanReload(ActionStateManager action)
    {
        if (action.ammo.currentAmmo == action.ammo.clipSize) return false; 
        if (action.ammo.extraAmmo == 0) return false;
        return true;
    }
}