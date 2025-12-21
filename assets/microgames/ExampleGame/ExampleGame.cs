using Godot;
using System;

public partial class ExampleGame : MicroBase //Inherit from MicroBase!
{
	//Use [Export] like [SerializeField] - exposes data to editor
	//	unlike unity, a lot *more* can be serialized - i.e. Godot Arrays and Dictionaries!
	//	not using that here though.
	[Export] private float speed = 50.0f;

	//you can use Property Hints to do cool things in editor - see editor and docs for more
	[Export(PropertyHint.Range, "1,10,.1")] private float sprintFactor = 2.0f;
	private bool isSprinting = false;

	private Vector2 dir = Vector2.Zero;
	private Sprite2D player;

	//Constructor, can be used as an equivalent of Unity's Awake, but things like Singleton should be handled with Godot's Global system
	//can be used to ensure that inspector vars are actually set - see MicroBase constructor as an example
	public ExampleGame()
	{
        //nothing here, this is just as an example
    }

    // Called when the node enters the scene tree for the first time. - Equivalent of Unity's Start
    public override void _Ready()
	{
		player = GetNode<Sprite2D>("%Player"); //uses godot's Unqiue Name syntax (%)

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame. - Equivalent of Unity's Update
	public override void _Process(double delta)
	{
		float dt = (float)delta; //casting to float for ease of use with vectors and such

		//move the player every frame based on dir
		player.Position += dir * speed * dt * ( isSprinting ? sprintFactor : 1);
	}

	// Called when input is recieved
	public override void _Input(InputEvent @event)
	{
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
		if(@event is InputEventJoypadMotion)
		{
            GD.Print("Up: " + @event.GetActionStrength("Up"));
            GD.Print("Down: " + @event.GetActionStrength("Down"));
            GD.Print("Left: " + @event.GetActionStrength("Left"));
            GD.Print("Right: " + @event.GetActionStrength("Right"));
        }
        
		// how you would process something that is bound to a joystick or trigger - anything with analog input. also works for WASD (mostly, as long as we normalize later)
        if (@event.IsActionPressed("Up") || @event.IsActionReleased("Up") || @event.IsActionPressed("Down") || @event.IsActionReleased("Down"))
		{
            dir.Y = -@event.GetActionStrength("Up") + @event.GetActionStrength("Down");
		}

        if (@event.IsActionPressed("Left") || @event.IsActionReleased("Left") || @event.IsActionPressed("Right") || @event.IsActionReleased("Right"))
        {
            dir.X = -@event.GetActionStrength("Left") + @event.GetActionStrength("Right");
        }

		//fix WASD strength
		if(dir.Length() > 0)
		{
			dir = dir.Normalized();
		}

        if (@event.IsActionPressed("B1"))
        {
            isSprinting = true;
        }
        if (@event.IsActionReleased("B1"))
        {
			isSprinting = false;
        }


		// use this for mouse MOTION events. we are not handling mouse events like double click (because of controller support, at least atm)
		// so we can treat them as buttons. 
		if(@event is InputEventMouseMotion)
		{

		}

    }
}
