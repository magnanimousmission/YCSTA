using UnityEngine;

public class PlayerInputEventArgs
{
    public Vector2 move = Vector2.zero;
    public bool sprint = false;
    internal bool interact = false;
    internal bool jump = false;
    internal bool roll = false;
}