using Godot;
using System;

public partial class MainGame : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetTree().Root.GetNode<GameManager>("GameManager").BeginNewGame(); //TEMPORARY
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
