using Godot;
using System;

public partial class BogosBintedGame : MicroBase
{

    private Node2D _bogos;
    private Sprite2D _bogosBinted;
    private Sprite2D _bogosUnbinted;
    private Area2D _leftArea;
    private Area2D _rightArea;
    private AudioStreamPlayer2D _balalaSound;
    private AudioStreamPlayer2D _bogosSound;


    private bool _visitedLeftLast = false;

    private int _successfulShakes = 0;
    private int _goalShakes = 0;

    private Timer _gameTimer;
    private float _goalTime;

    [Export] Godot.Collections.Dictionary<int, float> ShakeDifficultyTargets;
    [Export] Godot.Collections.Dictionary<int, float> TimeDifficultyTargets;

    [Export] Vector2 centerPos;
    [Export] float centerRot;
    [Export] Vector2 leftPos;
    [Export] float leftRot;
    [Export] Vector2 rightPos;
    [Export] float rightRot;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        _gm = GetTree().Root.GetNode<GameManager>("GameManager");
        // End unsubscribes these, but anything else you subscribe to these signals NEEDS to be unsubscribed
        _gm.StartGame += Start;
        _gm.InitializeGame += Init;


        _bogos = GetNode<Node2D>("%Bogos"); //uses godot's Unqiue Name syntax (%)
        _bogosBinted = _bogos.GetChild<Sprite2D>(1);
        _bogosUnbinted = _bogos.GetChild<Sprite2D>(0);
        _leftArea = GetNode<Area2D>("%LeftArea");
        _rightArea = GetNode<Area2D>("%RightArea");
        _balalaSound = GetNode<AudioStreamPlayer2D>("%Balala");
        _bogosSound = GetNode<AudioStreamPlayer2D>("%BogosAudio");


        if (DEBUG_AUTOSTART)
        {
            Init(0);
            Start();
        }

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if(_gameWon == MicroState.ONGOING)
        {
            CalculateProgress();

        }
    }

    public override void _Input(InputEvent @event)
    {
        if(_gameWon != MicroState.ONGOING)
        {
            return;
        }

        //MnK input is handled in the collision shape callbacks
        if(!_gm.IsMnK && @event is InputEventJoypadMotion)
        {
            GD.Print("Controller input");
            if (@event.GetActionStrength("Left") > .5f)
            {
                SwapLeft();
                GD.Print("Controller Left");
            }
            else if(@event.GetActionStrength("Right") > .5f)
            {
                SwapRight();
                GD.Print("Controller right");
            } else if (@event.IsActionPressed("Left") || @event.IsActionPressed("Right"))
            {
                SwapCenter();
                GD.Print("Controller center");
            }
        }
    }


    /// <summary>
    /// The initialization function of the Microgame
    /// take a difficulty and sets the internal variables to the correct amount
    /// also sets positions, spawns enemies, and otherwise visually prepares the game
    /// </summary>
    protected override void Init(int difficulty)
    {
        ReportProgress(1);

        _bogosBinted.Modulate = Color.FromHtml("ffffff00");

        _goalShakes = (int)_gm.InterpolateDictionary(ShakeDifficultyTargets, difficulty);
        _goalTime = _gm.InterpolateDictionary(TimeDifficultyTargets, difficulty);
        
        if(_gm.DEBUG_MESSAGES)
        {
            GD.Print($"[Bogos] Goal Shakes are {_goalShakes} within {_goalTime} seconds");
        }

    }

    /// <summary>
    /// A method that begins the game, starting timers and beginning game logic
    /// </summary>
    protected override void Start()
    {
        _leftArea.MouseEntered += () =>
        {
            if (_gameWon == MicroState.ONGOING && _gm.IsMnK)
            {
                SwapLeft();
            }
        };


        _leftArea.MouseExited += () =>
        {
            if (_gameWon == MicroState.ONGOING && _gm.IsMnK)
            {
                SwapCenter();
            }
        };

        _rightArea.MouseEntered += () =>
        {
            if (_gameWon == MicroState.ONGOING && _gm.IsMnK)
            {
                SwapRight();
            }
        };

        _rightArea.MouseExited += () =>
        {
            if (_gameWon == MicroState.ONGOING && _gm.IsMnK)
            {
                SwapCenter();
            }
        };


        _gameTimer = new Timer();
        _gameTimer.WaitTime = _goalTime;
        _gameTimer.OneShot = true;

        _gameTimer.Timeout += () =>
        {
            if (_gameWon != MicroState.WON)
            {
                _gameWon = MicroState.LOST;
            }
            _gameStarted = false; //honestly, I would maybe move this to tracking the _gameWon, and preventing all input if _gameWon is not ONGOING, but I want to get the example done lol
            _gameTimer.QueueFree();

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
            progress = (float)_gameTimer.TimeLeft / _goalTime;
        }


        ReportProgress(progress);
    }

    private void CompletedShake()
    {
        _successfulShakes++;
        if (DEBUG_MESSAGES)
        {
            GD.Print("[Bogos] Completed Shake: " + _successfulShakes);
        }

        float shakePercent = Mathf.Clamp((float)_successfulShakes / _goalShakes, 0, 1);

        _bogosBinted.Modulate = new Color(1, 1, 1, shakePercent);
        _bogosUnbinted.Modulate = new Color(1, 1, 1, 1.0f - shakePercent);
        _balalaSound.Play();


        if (_successfulShakes >= _goalShakes)
        {
            WinAnim();
        }
    }

    private void WinAnim()
    {
        _gameWon = MicroState.WON;
        SwapCenter();
        _gameTimer.Stop();

        Tween winTween = GetTree().CreateTween();

        //Modulate to smiling here
        //winTween.TweenProperty();
        //winTween.Parallel().TargetProperty();

        winTween.TweenCallback(Callable.From(() => {
            _bogosSound.Play();
        
        }));

        winTween.TweenCallback(Callable.From(() => { End(); })).SetDelay(1);

        
    }


    private void SwapLeft()
    {
        //do the swapping visually
        _bogos.Position = leftPos;
        _bogos.RotationDegrees = leftRot;


        //check if it's worth a shake credit
        if (!_visitedLeftLast)
        {
            CompletedShake();
        }

        //update last visited
        _visitedLeftLast = true;
    }

    private void SwapRight()
    {
        //do the swapping visually
        _bogos.Position = rightPos;
        _bogos.RotationDegrees = rightRot;


        //check if it's worth a shake credit
        if (_visitedLeftLast)
        {
            CompletedShake();
        }

        //update last visited
        _visitedLeftLast = false;
    }

    private void SwapCenter()
    {
        _bogos.Position = centerPos;
        _bogos.RotationDegrees = centerRot;
    }


}
