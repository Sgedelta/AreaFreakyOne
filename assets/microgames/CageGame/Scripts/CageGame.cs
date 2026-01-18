using Godot;
using System;

public partial class CageGame : MicroBase
{
	[Export] private float _speed = 50.0f;

	[Export] private Godot.Collections.Array _shakeTargets = [0, 10000.0f, 5, 25000.0f];
	[Export] private float _shakeThreshold = 30.0f;

	private float _shakeTarget = 10.0f;
	private float _recordedShakeVels = 0.0f;

	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;

	[Export(PropertyHint.Range, "1,10,.1")] private float _sprintFactor = 2.0f;
	private bool _isSprinting = false;
	
	private Vector2 _dir = Vector2.Zero;
	private Sprite2D _player;
	private CharacterBody2D _characterBody2D;

	private Sprite2D _alien;

	[Export] private int _difficulty = 0;
	private PackedScene _obstacle;

	public CageGame()
	{

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_gm = GetTree().Root.GetNode<GameManager>("GameManager");
		_gm.StartGame += Start;
		_gm.InitializeGame += Init;

		_player = GetNode<Sprite2D>("%PlayerSprite");
		_alien = GetNode<Sprite2D>("%AlienSprite");

		_characterBody2D = GetNode<CharacterBody2D>("%CharacterBody2D");

		_obstacle = GD.Load<PackedScene>("res://assets/microgames/CageGame/Prefabs/obstacle_prefab.tscn");

		if (DEBUG_AUTOSTART)
		{
			Init(1);
			Start();
		}

		if (_obstacle == null)
		{
			GD.Print("Obstacle not loaded");
			return;
		}

		Node nodeObstacle = _obstacle.Instantiate();

		GD.Print(_difficulty);

		Godot.Collections.Array obstacles = [nodeObstacle];

		switch (_difficulty)
		{
			case 1:
				if (nodeObstacle is Node2D node2D)
				{
					node2D.GlobalPosition = new Vector2(1962.0f, 951.0f);
				}
				AddChild(nodeObstacle);
				break;
			case 2:
				
				break;
			case 3:
			default:
				
				break;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (!_gameStarted) return;

		float dt = (float)delta;

		Move(dt);

		//check if the threshold is high enough - if it is, change the sprite color and note that we won
		if (_recordedShakeVels >= _shakeTarget)
		{
			_gameWon = MicroState.WON;
			_player.Modulate = Color.Color8(255, 0, 0, 255);
		}

		if (_gameTimer != null)
		{
			CalculateProgress();
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (!_gameStarted) return;

		if (_gameTimer == null)
		{
			_dir = Vector2.Zero;
			return;
		}

		if (_gm.DEBUG_MESSAGES && @event is InputEventJoypadMotion)
		{
			GD.Print("Up: " + @event.GetActionStrength("Up"));
			GD.Print("Down: " + @event.GetActionStrength("Down"));
			GD.Print("Left: " + @event.GetActionStrength("Left"));
			GD.Print("Right: " + @event.GetActionStrength("Right"));
		}

		if (@event.IsActionPressed("Up") || @event.IsActionReleased("Up") || @event.IsActionPressed("Down") || @event.IsActionReleased("Down"))
		{
			_dir.Y = -@event.GetActionStrength("Up") + @event.GetActionStrength("Down");
		}

		if (@event.IsActionPressed("Left") || @event.IsActionReleased("Left") || @event.IsActionPressed("Right") || @event.IsActionReleased("Right"))
		{
			_dir.X = -@event.GetActionStrength("Left") + @event.GetActionStrength("Right");
		}

		if (_dir.Length() > 0)
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
		SetDifficulty(difficulty);

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
			if (_gameWon != MicroState.WON)
			{
				_gameWon = MicroState.LOST;
				AnimateAlienFlee();
			}
			_gameStarted = false; 
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

	private void _on_player_2d_area_entered(Area2D area)
	{
		if (area.IsInGroup("Alien"))
		{	
			GD.Print("Alien caught - You Won!");

			_gameTimer = null; // is this the proper way to stop the timer?
			_gameWon = MicroState.WON;
		}
	}

	private void AnimateAlienFlee()
	{
		GD.Print("You Lose!");
		GD.Print("Animate Alien Emote and Fleeing Away!");
	}

	private void Move(float dt)
	{
		// Basic non-physics based movement
		//_player.Position += _dir * _speed * dt * (_isSprinting ? _sprintFactor : 1);

		// Physicsbased movement
		float deltaSpeed = _speed * dt * (_isSprinting ? _sprintFactor : 1);
		_characterBody2D.MoveLocalX(_dir.X * deltaSpeed);
		_characterBody2D.MoveLocalY(_dir.Y * deltaSpeed);
		
		_characterBody2D.MoveAndSlide();
	}

	private void SetDifficulty(int difficulty)
	{
		_difficulty = difficulty;
	}
}
