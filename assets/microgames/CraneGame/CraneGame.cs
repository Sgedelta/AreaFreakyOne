using Godot;

public partial class CraneGame : MicroBase 
{
	[Export] private float _craneSpeed = 500.0f;
	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;

	private Vector2 _dir = Vector2.Zero;
	private Sprite2D _crane;
	private Alien _alien;

	private bool _canInput = false;
	private bool _hasDropped = false;

	public override void _Ready()
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		_gm.InitializeGame += Init; 
		_gm.StartGame += Start;
		
		_crane = GetNode<Sprite2D>("%PlayerCrane");
		_alien = GetNode<Alien>("Alien") as Alien;

		if (DEBUG_AUTOSTART)
		{
			Init(0);
			Start();
		}
	}

	public override void _Process(double delta)
	{
		if (!_gameStarted || _hasDropped)
		{
			return;
		}

		float dt = (float)delta;

		//move crane based on dir
		_crane.Position += _dir * _craneSpeed * dt;
		_crane.Position = new Vector2(Mathf.Clamp(_crane.Position.X, 200, 3600), _crane.Position.Y);
		
		CalculateProgress();
	}

	// Called when input is recieved
	public override void _Input(InputEvent @event)
	{
		//don't do anything until the game starts
		if (!_gameStarted || !_canInput || _hasDropped)
		{
			return;
		}

		if (
			@event.IsActionPressed("Left") || @event.IsActionPressed("Right") ||
			@event.IsActionReleased("Left") || @event.IsActionReleased("Right")
		)
		{
			_dir.X = -@event.GetActionStrength("Left") + @event.GetActionStrength("Right");
		}

		if(_dir.Length() > 0)
		{
			_dir = _dir.Normalized();
		}

		if (@event.IsActionPressed("B1"))
		{
			CraneDrop();
		}
	}

	protected override void Init(int difficulty)
	{
		_crane.Position = new Vector2(1890, -27);
		_alien.Position = new Vector2(1880, 2020);

		_dir = Vector2.Zero; 
		_hasDropped = false; 

		_alien.SetDifficulty(difficulty);
	}

	protected override void Start()
	{
		_gameTimer = new Timer();
		_gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;

		_gameTimer.Timeout += TimeUp;

		AddChild(_gameTimer);
		_gameTimer.Start();

		_canInput = true;
		_gameStarted = true;
		_gameWon = MicroState.ONGOING;
	}

	protected override void CalculateProgress()
	{
		float progress = 1;

		if (_gameTimer != null) 
		{
			progress = (float)_gameTimer.TimeLeft / _gameTime;
		}
		ReportProgress(progress);
	}

	private void TimeUp()
	{
		if (!_hasDropped)
		{
			CraneDrop();
		}
	}

	private void CraneDrop()
	{
		_hasDropped = true;
		_canInput = false;

		_alien.Freeze();
		_gameTimer.Stop();

		bool win = Mathf.Abs(_crane.Position.X - _alien.Position.X) < 800f;
		_gameWon = win ? MicroState.WON : MicroState.LOST;

		GD.Print(win ? "WIN" : "LOSE");

		Tween dropTween = CreateTween();
		dropTween.TweenProperty(_crane, "position:y", 1060, 0.5f);

		dropTween.Finished += () =>
		{
			Tween liftTween = CreateTween();
			liftTween.Parallel().TweenProperty(_crane, "position:y", -27, 0.5f);
			if (win)
			{
				liftTween.Parallel().TweenProperty(_alien, "position:y", 933, 0.5f);
			}
			liftTween.Finished += End;
		};
	}

}
