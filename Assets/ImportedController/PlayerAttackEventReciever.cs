using UnityEngine;

public class PlayerAttackEventReciever : MonoBehaviour
{
    public delegate void ToggleHitbox();
    public static event ToggleHitbox OnToggleHitbox;

    public delegate void DoToggleIFrames();
    public static event DoToggleIFrames OnDoToggleIFrames;

    public delegate void Blocking();
    public static event Blocking OnToggleBlocking;
    

    public void EnableHitbox()//called from animation events
    {
        OnToggleHitbox?.Invoke();
        //please get into perforce NOW!!!!
    }

    public void DoAttack()
    {
        //Do attack things here
    }

    public void ToggleIFrames()
    {
        OnDoToggleIFrames?.Invoke();
    }

    public void ToggleBlocking()
    {
        OnToggleBlocking?.Invoke();
    }
}
