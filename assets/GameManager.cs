using Godot;
using System;

public partial class GameManager : Node2D
{
    //Authors: Sam Easton
    //The Game Manager, which is the source of truth for the game
    // GameManager is a global class 


    /// <summary>
    /// Sent when the Microgame should start
    /// </summary>
    [Signal]
    public delegate void StartGameEventHandler();

    /// <summary>
    /// Sent when the Microgame scene has fully loaded, to make sure that all data is initialized correctly
    /// </summary>
    [Signal] 
    public delegate void InitializeGameEventHandler(int difficulty);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnGameEnd()
    {

    }

    public void BeginNewGame()
    {

    }

   


}
