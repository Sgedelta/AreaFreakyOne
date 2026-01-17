using Godot;
using System;

public partial class FeedGame : MicroBase 
{
	/* Authors: Carl Browning
	 * Last Modified: 1/17/2026
	 * Summary: The implemenation of the FeedGame design.
	 */

	private Timer _gameTimer;
	[Export] private float _gameTime = 7.5f;

	[Export] private int _minFoods;
	[Export] private int _maxFoods;

	[Export] private Godot.Collections.Array<bool> foods;

	RandomNumberGenerator _rng;

	// Called when the node enters the scene tree for the first time. - Equivalent of Unity's Start
	public override void _Ready()
	{
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

	// Called every frame. 'delta' is the elapsed time since the previous frame. - Equivalent of Unity's Update
	public override void _Process(double delta)
	{
		//don't do anything until the game starts
		if (!_gameStarted)
		{
			return;
		}

		float dt = (float)delta; //casting to float for ease of use with vectors and such

	   
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

		
	}

	protected override void Init(int difficulty)
	{
		float diffScale = Mathf.Clamp((float)(difficulty - _minFoods) / (float)(_maxFoods - _minFoods), 0, 1);
		int foodCount = Mathf.RoundToInt(Mathf.Lerp((float)_minFoods, (float)_maxFoods, diffScale));

		_rng = new RandomNumberGenerator();

		for (int i = 0; i < foodCount; i++)
		{
			if (_rng.Randf() < .5)
			{
				foods.Add(true);
			}
			else
			{
				foods.Add(false);
			}

		}
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
