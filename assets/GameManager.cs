using Godot;
using Godot.Collections;
using System;
using System.Linq;


public partial class GameManager : Node2D
{
	//Authors: Sam Easton
	//The Game Manager, which is the source of truth for the game
	// GameManager is a global/autoload class - meaning that it should be accessable everywhere via root/GameManager and should not need singleton logic

	//=========VARIABLES=========

	//==internal vars==
	RandomNumberGenerator rng;
	MicroBase loadedGame;

	//Holds all the details about various debug functions
	[ExportGroup("Debug")]
	//a global toggle for generalized debug messages - check this when debugging things beyond a temporary check that will be deleted!
	// potential todo - change to bitmask/similar to allow for multiple toggles without a billion variables? such as toggling progress reports or general messages or load messages, etc
	// or just a resource? idk
	[Export] private bool _debugMessages = false;
	public bool DEBUG_MESSAGES { get { return _debugMessages; } }

	//If left empty, will force the given game to be loaded instead of a random game
	[Export] private string _debugLoadGame = "";
	public string DEBUG_LOAD_GAME { get { return _debugLoadGame; } }

	//Holds data for loading, starting, and the meta-analysis of games
	[ExportGroup("Games")]
	[Export] private Godot.Collections.Array<GameInfo> _gameInfos = new Godot.Collections.Array<GameInfo>();
	private Godot.Collections.Dictionary<string, PackedScene> _gameSceneDict = new Godot.Collections.Dictionary<string, PackedScene>();
	private Godot.Collections.Dictionary<string, float> _gameWeightDict = new Godot.Collections.Dictionary<string, float>();


	//Holds data for the flow of the game, such as lives, timing, etc
	[ExportGroup("Flow")]
	[Export] private int _startingLives = 3;
	public int StartingLives { get { return _startingLives; } }
	public int CurrentLives;

	public int CurrentDifficulty = 0;


	//=========SIGNALS=========

	/// <summary>
	/// Sent when the Microgame should start
	/// </summary>
	[Signal]
	public delegate void StartGameEventHandler();

	/// <summary>
	/// Sent when the Microgame scene has fully loaded, to make sure that all data is initialized correctly
	/// </summary>
	[Signal] 
	public delegate void InitializeGameEventHandler(int difficulty);

	//=========GODOT METHODS=========

	public GameManager()
	{   
	   

	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CurrentLives = _startingLives;
		rng = new RandomNumberGenerator(); //if we want seeds, this is where they'd go

		//construct packedScene dict and initial Weights
		foreach (GameInfo info in _gameInfos)
		{
			_gameSceneDict.Add(info.ID, GD.Load<PackedScene>(info.GameScene.ResourcePath));
		}
		StoreWeightDict();

		if(DEBUG_MESSAGES)
		{
			GD.Print($"[GM] {_gameSceneDict}");
			GD.Print($"[GM] {_gameWeightDict}");
		}

		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

	//=========METHODS=========

	public void OnGameEnd(int wonInput)
	{
		//cast back to correct form
		MicroState endState = (MicroState)wonInput;

		//hide the game
		Tween GameTransition = GetTree().CreateTween();

		//close
		GameTransition.TweenProperty(GetNode("%DoorL"), "position:x", 960.0f, 1.0f).From(-960.0f); //values are temp
		GameTransition.Parallel().TweenProperty(GetNode("%DoorR"), "position:x", 2880.0f, 1.0f).From(4800.0f); //values are temp
		//unload game
		GameTransition.TweenCallback(Callable.From(() =>
		{
			loadedGame?.QueueFree();
			loadedGame = null;
		}));

		//do whatever we need to do to transfer games
		//TODO 

		//load a new game
		GameTransition.TweenCallback(Callable.From(() => { BeginNewGame(); }));
	}

	public void BeginNewGame()
	{
		if(DEBUG_MESSAGES) { GD.Print("[GM] Loading a new Game!"); }

		//assumed: weight dictionary and difficulty have been updated
		PackedScene newGameScene = PickNewGame();

		//tween to transition
		Tween GameTransition = GetTree().CreateTween();

		//load a new game and THEN hook up to it and tell it to init
		GameTransition.TweenCallback(Callable.From(() =>
		{
			loadedGame = (MicroBase)newGameScene.Instantiate();
			loadedGame.DEBUG_AUTOSTART = false; //overwrite to prevent multistarts
			GetTree().Root.AddChild(loadedGame);
		}));
		GameTransition.TweenCallback(Callable.From(() =>
		{
			loadedGame.GameEnd += OnGameEnd;
			loadedGame.GameProgressReport += HandleProgress;
			

			EmitSignal(SignalName.InitializeGame);

		}));

		//open doors
		GameTransition.TweenProperty(GetNode("%DoorL"), "position:x", -960.0f, 1.0f).From(960.0f); //values are temp
		GameTransition.Parallel().TweenProperty(GetNode("%DoorR"), "position:x", 4800.0f, 1.0f).From(2880.0f); //values are temp

		//tell game to start
		GameTransition.TweenCallback(Callable.From(() =>
		{
			EmitSignal(SignalName.StartGame);
		}));
	}

	public void HandleProgress(float progressRatio)
	{
		//TODO
	}

	public PackedScene PickNewGame()
	{
		if(!string.IsNullOrEmpty(DEBUG_LOAD_GAME))
		{
			return _gameSceneDict[DEBUG_LOAD_GAME];
		}

		int chosenGameIndex = (int)rng.RandWeighted(_gameWeightDict.Values.ToArray());

		string chosenGameID = _gameWeightDict.Keys.ToArray()[chosenGameIndex];
		
		if(DEBUG_MESSAGES)
		{
			GD.Print($"[GM] Chosen to load {chosenGameIndex} from Index {chosenGameIndex}");
		}

		return _gameSceneDict[chosenGameID];
	}



	public void StoreWeightDict()
	{
		//clear old info
		_gameWeightDict.Clear();

		//construct new info based on current difficulty
		foreach (GameInfo info in _gameInfos)
		{
			//find the difficulty
			int[] difficulties = info.DifficultyWeights.Keys.ToArray();
			float[] weights = info.DifficultyWeights.Values.ToArray();
			float foundWeight = 0;

			//lerps internally and extends lowest and highest values to all difficulties out of range. 
			for(int i = 0; i < difficulties.Length; i++)
			{
				int diff = difficulties[i];
				//if it is this difficulty, use the weight
				if (diff == CurrentDifficulty)
				{
					foundWeight = weights[i];
				}
				//if the found one is higher, we've passed it (or it starts higher than the current difficulty) - lerp the weights (or just use that point) 
				else if (diff > CurrentDifficulty)
				{
					if(i != 0)
					{
						foundWeight = Mathf.Lerp(weights[i - 1], weights[i], ((float)(CurrentDifficulty - difficulties[i - 1]) / (float)( diff - difficulties[i - 1])));
					} else
					{
						foundWeight = weights[i];
					}
				}
				//if the found one is lower, keep going unless this is the last run.
				// this is the last run of the loop, so we just take the weight of the last used 
				else if (diff < CurrentDifficulty && i == difficulties.Length-1)
				{
					foundWeight = weights[i];
				}
			}

			//add it
			_gameWeightDict.Add(info.ID, foundWeight);
		}
	}
   


}
