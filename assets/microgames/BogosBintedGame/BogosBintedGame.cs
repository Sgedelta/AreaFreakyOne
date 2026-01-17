using Godot;
using System;

public partial class BogosBintedGame : MicroBase
{

    private Node2D _bogos;
    private Sprite2D _bogosBinted;
    private Sprite2D _bogosUnbinted;
    private Area2D _leftArea;
    private Area2D _rightArea;

    private bool _visitedLeftLast = false;

    private int _successfulShakes = 0;

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


        if (DEBUG_AUTOSTART)
        {
            Init(0);
            Start();
        }

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}

    public override void _Input(InputEvent @event)
    {
        //MnK input
        if(_gm.IsMnK)
        {

        }
    }


    /// <summary>
    /// The initialization function of the Microgame
    /// take a difficulty and sets the internal variables to the correct amount
    /// also sets positions, spawns enemies, and otherwise visually prepares the game
    /// </summary>
    protected override void Init(int difficulty)
    {
        _bogosBinted.Modulate = Color.FromHtml("ffffff00");
    }

    /// <summary>
    /// A method that begins the game, starting timers and beginning game logic
    /// </summary>
    protected override void Start()
    {




    }

    /// <summary>
    /// A method that calculates the progress to be reported 
    /// </summary>
    protected override void CalculateProgress()
    {

    }


    private void SwapLeft()
    {
        //do the swapping visually



        //check if it's worth a shake credit
        if (!_visitedLeftLast)
        {
            _successfulShakes++;
        }

        //update last visited
        _visitedLeftLast = true;
    }

    private void SwapRight()
    {
        //do the swapping visually



        //check if it's worth a shake credit
        if(_visitedLeftLast)
        {
            _successfulShakes++;
        }

        //update last visited
        _visitedLeftLast = false;
    }

    private void SwapCenter()
    {

    }
}
