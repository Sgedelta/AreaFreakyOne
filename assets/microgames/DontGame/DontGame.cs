using Godot;

public partial class DontGame : MicroBase
{

	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;
	
	private bool _buttonPressed = false;

	private Sprite2D _player;

	public DontGame()
	{

	}

	// Called when the node enters the scene tree for the first time. - Equivalent of Unity's Start
	public override void _Ready()
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;

		_player = GetNode<Sprite2D>("%Player");


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

		//if a button gets pressed, turn player red		
		if(_buttonPressed == true){
			ConfettiOrExplode();
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
		if (_gm.DEBUG_MESSAGES && @event is InputEventJoypadMotion)
		{
			GD.Print("Up: " + @event.GetActionStrength("Up"));
			GD.Print("Down: " + @event.GetActionStrength("Down"));
			GD.Print("Left: " + @event.GetActionStrength("Left"));
			GD.Print("Right: " + @event.GetActionStrength("Right"));
		}
		
		// how you would process something that is bound to a joystick or trigger - anything with analog input. also works for WASD (mostly, as long as we normalize later)
		if (@event.IsActionPressed("Up") || @event.IsActionReleased("Up") || @event.IsActionPressed("Down") || @event.IsActionReleased("Down"))
		{
			_buttonPressed = true;
		}

		if (@event.IsActionPressed("Left") || @event.IsActionReleased("Left") || @event.IsActionPressed("Right") || @event.IsActionReleased("Right"))
		{
			_buttonPressed = true;
		}

		if (@event.IsActionPressed("B1"))
		{
			_buttonPressed = true;
		}

	}

	protected override void Init(int difficulty)
	{
		//Difficulty does not scale :)
	}

	protected override void Start()
	{
		_gameTimer = new Timer();
		_gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;

		_gameTimer.Timeout += () => 
		{
			if(_gameWon != MicroState.LOST)
			{
				_gameWon = MicroState.WON;
				ConfettiOrExplode();
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
	
	private void ConfettiOrExplode(){
		_gameWon = _buttonPressed ? MicroState.LOST : MicroState.WON;
		GD.Print(_buttonPressed ? "LOSE" : "WIN");
		
		if(_buttonPressed){
			Sprite2D sprite = new Sprite2D();
			
			Texture2D texture = GD.Load<Texture2D>("res://assets/microgames/DontGame/DontGameAssets/DontGame_Explosion_Sprite.png");
			sprite.Texture = texture;
			
			sprite.Position = new Vector2(1814,974);
			
			AddChild(sprite);
		}
		else{
			Sprite2D sprite = new Sprite2D();
			
			Texture2D texture = GD.Load<Texture2D>("res://assets/microgames/DontGame/DontGameAssets/DontGame_Confetti_Sprite.png");
			sprite.Texture = texture;
			
			sprite.Position = new Vector2(1814,974);
			
			AddChild(sprite);
		}
	}

}
