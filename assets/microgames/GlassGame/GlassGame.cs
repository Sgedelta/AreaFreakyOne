using Godot;
using System;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;



public partial class GlassGame : MicroBase
{
    //Author: Render T
    //

    [Export] int numberTaps = 4;
    int startNum;

    private Timer _gameTimer;
    [Export] private float _gameTime = 7.5f;

    bool alienTime = false;
    float alienTimer = 1;
    Sprite2D alienWalk;
    Sprite2D alienMew;

    //ensures that vars have been set in inspector
    public GlassGame()
    {
        
    }

    public override void _Ready()
    {
        _gm = GetTree().Root.GetNode<GameManager>("GameManager");
        //make SURE to unsubscribe before calling end!
        _gm.StartGame += Start;
        _gm.InitializeGame += Init;

        alienWalk = GetTree().Root.GetNode<Sprite2D>("Alien1");
        alienMew = GetTree().Root.GetNode<Sprite2D>("Alienmew");

        if (DEBUG_AUTOSTART)
        {
            Init(0);
            Start();
        }
    }

    public override void _Process(double delta)
    {
        //don't do anything until the game starts
        if (!_gameStarted)
        {
            return;
        }

        float dt = (float)delta;
        if (alienTime)
        {
            alienTimer -= dt;
            if (alienTimer > 0.2 && alienTimer < 0.4)
                alienWalk.Texture = GD.Load<Texture2D>("res://assets/microgames/GlassGame/alien2.png");
            if (alienTimer > 0.4 && alienTimer < 0.6)
                alienWalk.Texture = GD.Load<Texture2D>("res://assets/microgames/GlassGame/alien3.png");
            if (alienTimer > 0.6 && alienTimer < 0.8)
                alienWalk.Texture = GD.Load<Texture2D>("res://assets/microgames/GlassGame/alien4.png");
            if (alienTimer > 0.8 && alienTimer < 1)
                alienWalk.Texture = GD.Load<Texture2D>("res://assets/microgames/GlassGame/alien5.png");
            else
            {
                alienMew.Visible = true;
            }

        }

        CalculateProgress();
    }

    //======OVERLOAD METHODS======

    /// <summary>
    /// The initialization function of the Microgame
    /// take a difficulty and sets the internal variables to the correct amount
    /// also sets positions, spawns enemies, and otherwise visually prepares the game
    /// </summary>

    public override void _Input(InputEvent @event)
    {
        //don't do anything until the game starts
        if (!_gameStarted)
        {
            return;
        }

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left) 
            {
                numberTaps -= 1;
                Console.WriteLine("increment to " +  numberTaps);
                if (numberTaps < 0)
                {
                    alienTime = true;
                    alienWalk.Visible = true;
                }
            }
        }
    }

    protected virtual void Init(int difficulty)
    {
        numberTaps = (4 + difficulty*2);
    }

    /// <summary>
    /// A method that begins the game, starting timers and beginning game logic
    /// </summary>
    protected override void Start()
    {
        _gameTimer = new Timer();
        _gameTimer.WaitTime = _gameTime;
        _gameTimer.OneShot = true;

        _gameTimer.Timeout += () =>
        {
            if (_gameWon != MicroState.WON)
            {
                _gameWon = MicroState.LOST;
            }
            _gameStarted = false;
            _gameTimer.QueueFree();

            _gm.StartGame -= Start;
            _gm.InitializeGame -= Init;

            End();

        };

        AddChild(_gameTimer);
        _gameTimer.Start();


        _gameStarted = true;
        _gameWon = MicroState.ONGOING;
    }

    /// <summary>
    /// A method that calculates the progress to be reported 
    /// </summary>
    protected override void CalculateProgress()
    {
        float progress = 1;

        if (_gameTimer != null)
        {
            progress = (float)_gameTimer.TimeLeft / _gameTime;
        }

        ReportProgress(progress);
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
