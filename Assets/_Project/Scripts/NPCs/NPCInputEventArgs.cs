using UnityEngine;
public class NPCInputEventArgs
{
    public Vector2 move;
    public bool sprint = false;
    internal bool oxygen = false;
    internal bool jump = false;
    internal bool roll = false;
    public GameObject target = null;
}