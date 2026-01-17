using Godot;
using System;

public partial class Alien : Sprite2D
{
	[Export] public float _moveSpeed = 200f;
	[Export] public float _minMoveTime = 0.5f;
	[Export] public float _maxMoveTime = 2.0f;

	private Vector2 _dir = Vector2.Zero;
	private float _moveTimer = 0f;
	private float _nextMoveTime = 1f;
	private bool _frozen = false;

	public override void _Ready()
	{
		_nextMoveTime = (float)GD.RandRange(_minMoveTime, _maxMoveTime);
		_dir = GetRandomDirection();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(_frozen) return;

		float dt = (float)delta;
		_moveTimer += dt;

		if (_moveTimer > _nextMoveTime)
		{
			_dir = GetRandomDirection();
			_nextMoveTime = (float)GD.RandRange(_minMoveTime, _maxMoveTime);
			_moveTimer = 0f;
		}

		Position += _dir * _moveSpeed * dt;
		Position = new Vector2(Mathf.Clamp(Position.X, 370, 3620), Position.Y);
	}

	private Vector2 GetRandomDirection()
	{
		int rand = (int)GD.Randi() % 3;
		if (rand == 0) return new Vector2(-1, 0);
		if (rand == 1) return new Vector2(1, 0);
		return Vector2.Zero;
	}

	public void SetDifficulty(int difficulty)
	{
		_moveSpeed = 150f + difficulty * 50f;
		_minMoveTime = Mathf.Max(0.2f, 1.0f - difficulty * 0.2f);
		_maxMoveTime = Mathf.Max(0.5f, 2.0f - difficulty * 0.3f);
	}

	public void Freeze()
	{
		_frozen = true;
	}
}
