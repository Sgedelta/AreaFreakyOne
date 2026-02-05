using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;

public partial class DefendGame : MicroBase
{
	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;
	private Godot.RandomNumberGenerator _rng;

	[Export] private Area2D _defender;
	private Sprite2D _defenderSprite;

	private Vector2 _screenSize;
	private Vector2 _defenderBoundsX;
	private Vector2 _defenderBoundsY;
	private int defenderHeight;
	private int defenderWidth;

	//private int difficulty = 1;
	[Export] private Sprite2D _alien;
	private Timer _shootTimer;
	[Export] private float _shootTime;
	[Export] private PackedScene _fireballReference;
	[Export] private int _fireballSpeed = 10;
	public DefendGame()
	{

	}
	protected override void Init(int difficulty)
	{
		//_shootTime += difficulty;
	}
	protected override void Start()
	{
		//All the instatiation we need
		_screenSize = GetViewportRect().Size;
		_defenderSprite = _defender.GetNode<Sprite2D>("Sprite2D");
		_rng = new Godot.RandomNumberGenerator();

		_gameTimer = new Timer();
		_gameTimer.WaitTime = _gameTime;
		_gameTimer.OneShot = true;

		//Makes the lil guy shoot then move on a given timer
		_shootTimer = new Timer();
		_shootTimer.WaitTime = _shootTime;
		_shootTimer.Timeout += () => Shoot(_alien);

		//Handles game over state
		_gameTimer.Timeout += () =>
		{
			if (_gameWon != MicroState.LOST)
			{
				_gameWon = MicroState.WON;
			}
			_gameStarted = false; //honestly, I would maybe move this to tracking the _gameWon, and preventing all input if _gameWon is not ONGOING, but I want to get the example done lol
			_gameTimer.QueueFree();
			_shootTimer.QueueFree();
			End();

		};
		//Adds timers to scene
		AddChild(_gameTimer);
		_gameTimer.Start();

		AddChild(_shootTimer);
		_shootTimer.Start();
		//Starts the game
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
		GD.Print(progress);
	}
	//These prob don't need to be here but it don't hurt no one
	protected override void End()
	{
		base.End();
	}
	protected override void ReportProgress(float progress)
	{
		base.ReportProgress(progress);
	}


	public override void _Ready()
	{
		//Gets game manager, otherwise it's the same as the example game
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		// End unsubscribes these, but anything else you subscribe to these signals NEEDS to be unsubscribed
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;
		if (DEBUG_AUTOSTART)
		{
			Init(0);
			Start();
		}
	}
	//update
	public override void _Process(double delta)
	{
		//don't do anything until the game starts
		if (!_gameStarted)
		{
			return;
		}
		if (_gameWon != MicroState.ONGOING) { return; }

		//Collision check for the barrels to see if you lose. If the fireball reaches the barrels, you lose
		if (GetNode<Area2D>("%Barrels").HasOverlappingAreas())
		{
			GD.Print("womp womp");
			_gameWon = MicroState.LOST;
			_shootTimer.Stop();
		}

		CalculateProgress();
	}
	//input checking
	public override void _Input(InputEvent @event)
	{
		if (_gameWon != MicroState.ONGOING) { return; }
		//gets mouse position, then clamps the defender to the bounds of the screen
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			_defender.Position = new Vector2(2600, Mathf.Clamp(eventMouseMotion.Position.Y, 250, 1860));
		}
		if (!_gm.IsMnK && @event is InputEventJoypadMotion)
		{
			//Gets the strength of the controller input, in a -1 to 1 axis
			float axis =
				Input.GetActionStrength("Down") -
				Input.GetActionStrength("Up");
			GD.Print(axis);
			//Moves the defender to the bounds of the screen given the predetermined axis above
			float y = Mathf.Lerp(250f, 1860f, (axis + 1f) * 0.5f);
			_defender.Position = new Vector2(2600, y);


		}
	}
	private void Shoot(Sprite2D alien)
	{
		//Finds a random Y position on the bounds of the screen, then if it's too close (within 200 pixels), it will find a new position
		float _goToYPos = _rng.RandfRange(-100, 1670);
		if (MathF.Abs(_goToYPos - alien.Position.Y) < 200)
		{
			GD.Print("das close chief");
			_goToYPos = _goToYPos - alien.Position.Y >= 0 ? Mathf.Clamp(_goToYPos + 200, -100, 1670) : Mathf.Clamp(_goToYPos - 200, -100, 1670);
		}
		//Moves the alien
		alien.Position = new Vector2(720, _goToYPos);
		//GD.Print($"shmoovin to {alien.Position}");
		//Creates new fireball given from fireball.tscn, then sets its speed to shoot across the screen
		Fireball _fireballToAdd = (Fireball)_fireballReference.Instantiate();
		_fireballToAdd.Position = new Vector2(alien.Position.X, alien.Position.Y + 300);
		_fireballToAdd.Speed = _fireballSpeed;
		//Adds the fireball to the scene
		GetTree().Root.GetNode("DefendGame").AddChild(_fireballToAdd);
	}
}
