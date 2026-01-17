using Godot;

public partial class ExampleGame : MicroBase //Inherit from MicroBase!
{
	//Use [Export] like [SerializeField] - exposes data to editor
	//	unlike unity, a lot *more* can be serialized - i.e. Godot Arrays and Dictionaries!
	//	not using that here though.
	[Export] private float _speed = 50.0f;

	//(Generally) use Godot Arrays and Godot Dictionaries whenever you can. They work very much like JS or Python arrays, and play nicer with most things to do with Godot
	//  if we begin to worry about array effeciencies use a packed array
	// This array stores ints and floats - int being the difficulty level and float being the amount you need to shake to get there
	[Export] private Godot.Collections.Array _shakeTargets = [0, 10000.0f, 5, 25000.0f ];
	[Export] private float _shakeThreshold = 30.0f;

	private float _shakeTarget = 10.0f;
	private float _recordedShakeVels = 0.0f;

	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;

	//you can use Property Hints to do cool things in editor - see editor and docs for more
	[Export(PropertyHint.Range, "1,10,.1")] private float _sprintFactor = 2.0f;
	private bool _isSprinting = false;

	private Vector2 _dir = Vector2.Zero;
	private Sprite2D _player;

	//Constructor, can be used as an equivalent of Unity's Awake, but things like Singleton should be handled with Godot's Global system.
	// risky to use, generally - use Ready
	public ExampleGame()
	{

	}

	// Called when the node enters the scene tree for the first time. - Equivalent of Unity's Start
	public override void _Ready()
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		// End unsubscribes these, but anything else you subscribe to these signals NEEDS to be unsubscribed
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;

		_player = GetNode<Sprite2D>("%Player"); //uses godot's Unqiue Name syntax (%)


		if(DEBUG_AUTOSTART)
		{
			Init(0);
			Start();
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame. - Equivalent of Unity's Update
	public override void _Process(double delta)
	{
		//don't do anything until the game starts
		if (!_gameStarted)
		{
			return;
		}

		float dt = (float)delta; //casting to float for ease of use with vectors and such

		//move the player every frame based on dir
		_player.Position += _dir * _speed * dt * ( _isSprinting ? _sprintFactor : 1);

		//check if the threshold is high enough - if it is, change the sprite color and note that we won
		if(_recordedShakeVels >= _shakeTarget)
		{
			_gameWon = MicroState.WON;
			_player.Modulate = Color.Color8(255, 0, 0, 255);
		}

		//make sure to report progress
		CalculateProgress();
	}

	// Called when input is recieved
	public override void _Input(InputEvent @event)
	{
		//don't do anything until the game starts
		if (!_gameStarted)
		{
			return;
		}

		/*
		 * Something like this *could* work
		if(@event.IsAction("Up"))
		{
		
		
			if (@event.IsPressed())
			{
				GD.Print("Up Pressed!");
			}
			else if(@event.IsReleased())
			{
				GD.Print("Up Released!"); 
			}
		}
		 * but it also recieves echo events, so should be avoided unless you want echo events? 
		 * echos are exclusive to keyboards though, so is not ideal. I would refactor your code
		 * to not use echo events. try timers instead?
		*/

		//process button input like this
		if (@event is InputEventJoypadMotion)
		{
			GD.Print("Up: " + @event.GetActionStrength("Up"));
			GD.Print("Down: " + @event.GetActionStrength("Down"));
			GD.Print("Left: " + @event.GetActionStrength("Left"));
			GD.Print("Right: " + @event.GetActionStrength("Right"));
		}
		
		// how you would process something that is bound to a joystick or trigger - anything with analog input. also works for WASD (mostly, as long as we normalize later)
		if (@event.IsActionPressed("Up") || @event.IsActionReleased("Up") || @event.IsActionPressed("Down") || @event.IsActionReleased("Down"))
		{
			_dir.Y = -@event.GetActionStrength("Up") + @event.GetActionStrength("Down");
		}

		if (@event.IsActionPressed("Left") || @event.IsActionReleased("Left") || @event.IsActionPressed("Right") || @event.IsActionReleased("Right"))
		{
			_dir.X = -@event.GetActionStrength("Left") + @event.GetActionStrength("Right");
		}

		//fix WASD strength
		if(_dir.Length() > 0)
		{
			_dir = _dir.Normalized();
		}

		if (@event.IsActionPressed("B1"))
		{
			_isSprinting = true;
		}
		if (@event.IsActionReleased("B1"))
		{
			_isSprinting = false;
		}


		// use this for mouse MOTION events. we are not handling mouse events like double click (because of controller support, at least atm)
		// so we can treat them as buttons. 
		if(@event is InputEventMouseMotion eventMouse)
		{
			//could do any number of things here, but rn we are just going to "track velocities". simple-nothings for an example
			float movementVel = eventMouse.Velocity.Length();
			if (movementVel > _shakeThreshold) {
				_recordedShakeVels += movementVel - _shakeThreshold;
				//note; i'm not positive this won't be an issue with frame rate?  but it *should* be okay...
				//there is probably a better way to do this, where you store the last frame rate and add it to a "time being shaken" if the velocity is high enough?
				//this is an example game, I'm not that worried.
			}
		}

	}

	protected override void Init(int difficulty)
	{

		//grab data from array
		int minLevel = (int)_shakeTargets[0];
		int maxLevel = (int)_shakeTargets[2];
		float minShakeAmt = (float)_shakeTargets[1];
		float maxShakeAmt = (float)_shakeTargets[3];

		//do math on level
		float diffScale = Mathf.Clamp((float)(difficulty - minLevel) / (float)(maxLevel - minLevel), 0, 1);

		//lerp and store
		_shakeTarget = Mathf.Lerp(minShakeAmt, maxShakeAmt, diffScale);
	}

	protected override void Start()
	{
        _gameTimer = new Timer();
        _gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;

		_gameTimer.Timeout += () => 
		{
			if(_gameWon != MicroState.WON)
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

	protected override void CalculateProgress()
	{
		float progress = 1;

		if (_gameTimer != null) 
		{
			progress = (float)_gameTimer.TimeLeft / _gameTime;
		}


		ReportProgress(progress);
	}

}
