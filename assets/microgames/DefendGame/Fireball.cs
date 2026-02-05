using Godot;
using Godot.NativeInterop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;

public partial class Fireball : Node2D
{
    [Export] private int _speed = 10;
    public int Speed { get { return _speed; } set { _speed = value; } }
    public override void _Process(double delta)
    {
        this.Position += new Vector2(_speed, 0);
        if (GetNode<Area2D>("%Fireball").HasOverlappingAreas()) QueueFree();
    }
    public override void _Ready()
    {

    }

}