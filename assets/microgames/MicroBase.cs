using Godot;
using System;

public enum MicroState
{
    WAITING,
    ONGOING,
    WON,
    LOST
}


public partial class MicroBase : Node2D
{
    //Authors: Sam Easton
    //While the MicroBase is not an abstract class (because this doesn't totally play nice), it should be treated as such
    //MicroBase is a "abstract" class that is used as the base class for all top-level scripts that run MicroGames (MicroGame Managers)
    //it handles things that all micro games share, and implements a few methods that must be overriden in the inherited classes


    //======VARS======

    [Export] private bool _debug = false;
    public bool DEBUG { get { return _debug; } }

    //unique ID of the microgame, for indexing
    [Export] protected string _id; //private variables begin with _
    /// <summary>
    /// The unique ID of the microgame, primarily used for dicitonary indexing
    /// </summary>
    public string ID { get { return _id; } }

    protected bool _gameStarted = false;
    protected MicroState _gameWon = MicroState.WAITING;


    //======SIGNALS======

    [Signal] public delegate void GameProgressReportEventHandler(float progress_ratio);
    [Signal] public delegate void GameEndEventHandler(int won); //this is actually a MicroState, but needs to be a Variant type - just cast it back on recieved


    //======CONSTRUCTORS AND GODOT METHODS======

    //ensures that vars have been set in inspector
    public MicroBase()
    {
        if (string.IsNullOrEmpty(_id))
        {
            GD.PrintErr($"Game ID of {Name} is not set correctly");
            //this will not prevent further execution, but it will warn. Throw an exception if we want it to really stop
        }
    }

    //======OVERLOAD METHODS======

    /// <summary>
    /// The initialization function of the Microgame
    /// take a difficulty and sets the internal variables to the correct amount
    /// also sets positions, spawns enemies, and otherwise visually prepares the game
    /// </summary>
    protected virtual void Init(int difficulty)
    {
        GD.PushError($"Game {_id} has not overridden Init!");
    }

    /// <summary>
    /// A method that begins the game, starting timers and beginning game logic
    /// </summary>
    protected virtual void Start()
    {
        GD.PushError($"Game {_id} has not overridden Start!");
    }

    /// <summary>
    /// A method that calculates the progress to be reported 
    /// </summary>
    protected virtual void CalculateProgress()
    {
        GD.PushError($"Game {_id} has not overridden Calculate Progress!");
    }




    //======NON OVERLOAD METHODS======
    //these methods are not designed to be overloaded by the specific games
    // they remain virtual just in case a specific game needs to overload them

    //Note: done as a method in case we need to add more logic later
    /// <summary>
    /// A method that ends the game, signaling to the Game Manager to load the next game
    /// </summary>
    protected virtual void End()
    {
        EmitSignal(SignalName.GameEnd, (int)_gameWon);   
    }

    //Note: done as a method in case we need to add more logic later
    /// <summary>
    /// A method that reports the progress to the game manager
    /// </summary>
    /// <param name="progress"></param>
    protected virtual void ReportProgress(float progress)
    {
        EmitSignal(SignalName.GameProgressReport, progress);
    }

}
