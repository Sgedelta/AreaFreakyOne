using Godot;

public partial class ContainerGame : MicroBase
{
    //Stuff being [Export]ed to the editor

    [Export] private float _handSpeed = 200f;

    private Timer _gameTimer;
    [Export] private float _gameTime = 10f;


    private Sprite2D _hand;
    private Sprite2D _bucket;
    private Sprite2D _goober;


    private bool _dropped1;
    private bool _dropped2;

    private bool _failed;

    public override void _Ready() //function to be called when first entered (SETUP)
    {
        _gm = GetTree().Root.GetNode<GameManager>("GameManager");
        _gm.StartGame += Start;
        _gm.InitializeGame += Init;

        _hand = getNode<Sprite2D>("%Hand");

        if (DEBUG_AUTOSTART){
            Init(0);
            Start();
        }
    }

    public override void _Process(double delta)
    {
        //dont do anything til game starts

        if (!_gameStarted){
            return;
        }

        float dt = (float) delta; //"casting to float for ease of use with vectors and such"

        _hand.Position += _dir * _speed * dt;

        CalculateProgress();
    }

    public override void _Input(InputEvent @event){
        if (!_gameStarted){
            return;
        }

        //processing button input
        if (@event.IsActionPressed("B1")){
            _dropped = true;
        }
    }

    protected override void Init(int difficulty){
        //this is where you prep the level
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
            _gameStarted = false; //switch to tracking _gameWon and prevent input if _gameWon is not ONGOING
            _gameTimer.QueueFree();

            _gm.StartGame -= start;
            _gm.InitializeGame -= Init;

            End();
        };

        AddChild(_gameTimer);
        _gameTimer.Start();


        _gameStarted = true;
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
