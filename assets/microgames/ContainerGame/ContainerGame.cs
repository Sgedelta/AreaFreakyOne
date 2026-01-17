using System;
using System.Collections.Generic;
using Godot;
using Vector2 = Godot.Vector2;

public partial class ContainerGame : MicroBase
{
	//Stuff being [Export]ed to the editor

	private Timer _gameTimer;
	[Export] private float _gameTime = 10f;
	[Export] private int _bucketFrameDelay = 255; //the number of frames the bucket will wait at the extremities of the viewport before changing direction
	[Export] private float _speed = 200f;

	//CONSTANTS, describing limits of bucket movement (so that it doesn't clip outside the viewport)
	private int _RIGHT_LIMIT = 3487;
	private int _LEFT_LIMIT = 335;

	//SPRITES
	//private Sprite2D _hand; //NOT USED; no hand sprite yet
	private Node2D _bucket;
	private RigidBody2D _goober1;
	private RigidBody2D _goober2;


	//OTHER VARs
	private int _dir = 1;
	private int _storedDir = 1;
	private int _count = 0;
	private bool _dropped1;
	private bool _dropped2;

	private bool _failed;

	public override void _Ready() //function to be called when first entered (SETUP)
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;

		//_hand = getNode<Sprite2D>("%Hand");
		_goober1 = GetNode<RigidBody2D>("%Goober1");
		_goober2 = GetNode<RigidBody2D>("%Goober2");
		_bucket = GetNode<Node2D>("%Bucket");
		var catchArea = _bucket.GetNode<Area2D>("CatchArea");
		catchArea.BodyEntered += OnBucketCatch;
		
		_goober1.GravityScale = 0f;
		_goober2.GravityScale = 0f;

		if (DEBUG_AUTOSTART){
			Init(0);
			Start();
		}
	}

	public void OnBucketCatch(Node body)
	{
		if (body is RigidBody2D item)
		{
			//Stop physics motion
			item.LinearVelocity = Vector2.Zero;
			item.AngularVelocity = 0.0f;

			//freeze the item's simulation
			item.Freeze = true;

			//snap to the bucket first
			item.GlobalPosition = _bucket.GlobalPosition + new Vector2(0,10); //offset so its INSIDE the bucket

			//turn down gravity, mark item as no longer falling
			if (item.Name == "Goober1")
			{
				_dropped1 = false;
				var gooberCollider = _goober1.GetNode<CollisionPolygon2D>("CollisionPolygon2D");
				gooberCollider.Disabled = true;
			}
			else if(item.Name == "Goober2"){_dropped2 = false;}
		}
	}
	public override void _Process(double delta)
	{
		//dont do anything til game starts

		if (_gameWon != MicroState.ONGOING){
			return;
		}
		if (DEBUG_MESSAGES){GD.Print("Freeze:" + _goober1.Freeze + "\nGravityScale:" + _goober1.GravityScale);}

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

		CalculateProgress();
	}

	public override void _Input(InputEvent @event){
		if (_gameWon != MicroState.ONGOING){
			return;
		}

		//processing button input
		if (@event.IsActionPressed("B1")){
			_dropped1 = true;
			_goober1.GravityScale = 3f;
			GD.Print("dropped.");
		}
	}

	protected override void Init(int difficulty){
		//this is where you prep the level? i think?
		//WHAT IS THIS FUNCTION EVEN FOR
	}

	protected override void Start(){
		_gameTimer = new Timer();
		_gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;
		_gameTimer.Timeout += () =>
		{
			if(_gameWon != MicroState.WON){
				_gameWon = MicroState.LOST;
			}
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

	protected override void CalculateProgress(){
		float progress = 1;
		if (_gameTimer != null){
			progress = (float) _gameTimer.TimeLeft/_gameTime;
		}
		ReportProgress(progress);
	}
}
