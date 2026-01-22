using System;
using System.Collections.Generic;
using Godot;
using Vector2 = Godot.Vector2;

public partial class ContainerGame : MicroBase
{
	//Authors: Vignesh Muralidharan
	//Last Modified: 01/22/26
    //This is a microgame where the player must drop two little goobers into the container bucket that moves from left to right regularly.

	//Stuff being [Export]ed to the editor

	private Timer _gameTimer;
	[Export] private float _gameTime = 10f; //the length of the game (how long(in seconds) do you have to succeed)
	[Export] private int _bucketFrameDelay = 255; //the number of frames the bucket will wait at the extremities of the viewport before changing direction

	[Export] private float _speedFactor = 2000f; //creating a variable so the speed factor itself can be changed in editor
	private float _speed;

	//==========CONSTANTS=========== 
	//describing limits of bucket movement 
	//(so that it doesn't clip outside the viewport)
	private int _RIGHT_LIMIT = 3487;
	private int _LEFT_LIMIT = 335;
 
	//==========SPRITES===========
	private Sprite2D _hand; //TEMPORARY SPRITE, will have to be updated
	private Node2D _bucket;
	private RigidBody2D _goober1;
	private RigidBody2D _goober2;


	//===============GAME VARS============
	private int _dir = 1;
	static RandomNumberGenerator rng = new RandomNumberGenerator();
	private int _storedDir = 1;
	private int _count = 0;
	private float _temp = rng.Randf();
	private int _dropping = -1;
	private bool _dropped1;
	private bool _dropped2;
	private bool _caught1 = false;
	private bool _caught2 = false;
	private bool _failed;

	//=============HELPER METHODS===============
	public void OnBucketCatch(Node body) //runs when the goober hits the inside of the bucket
	{
		if (body is RigidBody2D item)
		{
			if (item.Name == "Goober1")
			{
				_caught1 = true;
			}
			else if (item.Name == "Goober2")
			{
				_caught2 = true;
			}
			Callable.From(() => {item.Reparent(_bucket);}).CallDeferred(); //"catch" the goober
		}
	}
	public void OnBucketMiss(Node body) //runs when the goober hits the under-bucket area
	{
		_failed = true;
	}

	//=============OVERRIDDEN FUNCTIONS============
	public override void _Ready() //function to be called when first entered (SETUP)
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;

		if (DEBUG_AUTOSTART)
		{ //starts game that youre on by default
			Init(0);
			Start();
		}
	}

	public override void _Process(double delta)
	{
		//equivalent of unity's update(), runs eveyr frame
		//dont do anything til game starts

		if (_gameWon != MicroState.ONGOING) return;

		float dt = (float) delta; //"casting to float for ease of use with vectors and such"
		//Move the bucket
		_bucket.Position += new Vector2(_dir * _speed * dt, 0);
		//check if bucket is at extremities; only x-axis matters
		if (_bucket.Position[0] >= _RIGHT_LIMIT || _bucket.Position[0] <= _LEFT_LIMIT)
		{
			//check if bucket has already been stopped
			if (_dir == 0)
			{
				//check if the bucket has waited enough
				if (_count == _bucketFrameDelay)
				{
					//reset count, start moving in the stored direction
					_count = 0;
					_dir = _storedDir;
				}
				else
				{
					//wait one frame
					_count += 1;
				}
			}
			else
			{
				//check if bucket is about to leave screen and if yes, snap to the closest limit
				if (Math.Abs(_RIGHT_LIMIT - _bucket.Position[0]) < Math.Abs(_LEFT_LIMIT - _bucket.Position[0]))
				{
					_bucket.Position = new Vector2(_RIGHT_LIMIT, _bucket.Position[1]);
				}
				else
				{
					_bucket.Position = new Vector2(_LEFT_LIMIT, _bucket.Position[1]);
				}
				
				_storedDir = _dir * -1; //reverse direction
				_dir = 0; //begin waiting
			}
		}
		//win loss check
		if(_caught1 && _caught2)
		{
			_gameWon = MicroState.WON;
			Tween winTween = GetTree().CreateTween();
			//winning animation feedback:: close the bucket.
			//first, make it visible
			GetNode<Sprite2D>("%Lid").Visible = true;
			GetNode<Sprite2D>("%Lid").GlobalPosition = new Vector2(_bucket.Position[0], 0);
			//start from above the bucket, rotate and scale into it
			winTween.TweenProperty(GetNode("%Lid"),"position:y",_bucket.Position[1] - 250.0f, 1.0f).From(_bucket.Position[1] - 750.0f);
			winTween.Parallel().TweenProperty(GetNode("%Lid"), "rotation", 0.0f, 1.0f).From(45.0f);
			winTween.Parallel().TweenProperty(GetNode("%Lid"), "scale:x", 1.482f, 0.75f).From(9f);
			winTween.TweenCallback(Callable.From(End));
			
		}
		else if (_failed)
		{
			_gameWon = MicroState.LOST;
			GD.Print("Game is lost.");
			Tween lossTween = GetTree().CreateTween();
			lossTween.TweenProperty(GetNode("%Bucket"), "rotation_degrees", 90, 0.25f); //turn the bucket over, so the goober falls out if its in there
			//lossTween.TweenProperty(GetNode("%Goober1"), "position.x")
			//lossTween.TweenProperty() //do losing animation via tween here

			lossTween.TweenCallback(Callable.From(End));
		}

		CalculateProgress();
	}

	public override void _Input(InputEvent @event)
	{
		if (_gameWon != MicroState.ONGOING) return; //run when game is still going 

		//processing button input
		if (@event.IsActionPressed("B1"))
		{
			//checking drop based on the random start, ensuring once 2nd guy is dropped hand doesnt move.
			if (_dropping == 0)
			{
				_dropped1 = true;
				_goober1.GravityScale = 3f;
				if (!_dropped2)
				{
					_dropping = 1;
					_hand.Position = new Vector2(3483,149);
				}
				
			}
			else
			{
				_dropped2 = true;
				_goober2.GravityScale = 3f;
				if (!_dropped1)
				{
					_dropping = 0;
					_hand.Position = new Vector2(367,149);
				}
			}
			if (DEBUG_MESSAGES)GD.Print("dropped.");
		}
	}

	protected override void Init(int difficulty)
	{
		//this is where level is prepped and made ready to play.
		if (DEBUG_MESSAGES) GD.Print("Container Game Init Called.");

		//get all the items that need to be edited
		_hand = GetNode<Sprite2D>("%Hand");
		_goober1 = GetNode<RigidBody2D>("%Goober1");
		_goober2 = GetNode<RigidBody2D>("%Goober2");
		_bucket = GetNode<Node2D>("%Bucket");

		//make the lid invisible so it doesnt appear closed
		GetNode<Sprite2D>("%Lid").Visible = false;

		//subscribing to areas areas and their relevant functions
		var catchArea = _bucket.GetNode<Area2D>("CatchArea");
		catchArea.BodyEntered += OnBucketCatch;
		var loseArea = GetNode<Area2D>("LoseArea");
		loseArea.BodyEntered += OnBucketMiss;
		
		//updating speed of bucket based on difficulty (UNNECESSARILY COMPLICATED BUT IT WORKS)
		_speed = _speedFactor * (int) Math.Ceiling(difficulty/2.0);

		//goobers floating unless moved 
		_goober1.GravityScale = 0f;
		_goober2.GravityScale = 0f;

		_dropping = (_temp < 0.5) ? 0 : 1;

		if (_dropping == 0)
		{
			_hand.Position = new Vector2(367,149);
		}
		else
		{
			_hand.Position = new Vector2(3483,149);
		}
	}

	protected override void Start()
	{
		//creating timer, and saying that the game is lost if you run out
		_gameTimer = new Timer();
		_gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;
		_gameTimer.Timeout += () =>
		{
			if(_gameWon != MicroState.WON) _gameWon = MicroState.LOST;
			_gameTimer.QueueFree();

			_gm.StartGame -= Start;
			_gm.InitializeGame -= Init;

			End();
		};

		if (DEBUG_MESSAGES) GD.Print("Game is Starting!");
		AddChild(_gameTimer);
		_gameTimer.Start();
		_gameWon = MicroState.ONGOING;
	}

	protected override void CalculateProgress()
	{
		//return time left for chud timer
		float progress = 1;
		if (_gameTimer != null)
		{
			progress = (float) _gameTimer.TimeLeft/_gameTime;
		}
		ReportProgress(progress);
	}
}
