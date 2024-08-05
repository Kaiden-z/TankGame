using Godot;
using System;

public partial class PlayerBase : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float MouseSensitivity = 0.003f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = 9.8f;

	public Node3D head;
	public Camera3D camera;
	
	private int authorityId;

	public override void _Ready() {
		head = GetNode<Node3D>("Head");
		camera = GetNode<Camera3D>("Head/Camera3D");
		//Input.MouseMode = Input.MouseModeEnum.Captured;

		GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").SetMultiplayerAuthority(int.Parse(Name));
		authorityId = GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority();
		
		if (Multiplayer.GetUniqueId() == authorityId) {
			camera.Current = true;
		}
	}

	public override void _Input(InputEvent ievent)
	{
		if (ievent is InputEventMouseMotion) 
		{
			InputEventMouseMotion mouseMotion = ievent as InputEventMouseMotion;
			head.RotateY(-mouseMotion.Relative.X * MouseSensitivity);
			camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
			
			Vector3 cameraRot = camera.Rotation;
			cameraRot.X = Mathf.Clamp(cameraRot.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f));
			camera.Rotation = cameraRot;
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.GetUniqueId() == GetNode<MultiplayerSynchronizer>("MultiplayerSynchronizer").GetMultiplayerAuthority())
		{
			Vector3 velocity = Velocity;

			// Add the gravity.
			if (!IsOnFloor())
				velocity.Y -= gravity * (float)delta;

			// Handle Jump.
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
				velocity.Y = JumpVelocity;

			// Get the input direction and handle the movement/deceleration.
			// As good practice, you should replace UI actions with custom gameplay actions.
			Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
			Vector3 direction = (head.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * Speed;
				velocity.Z = direction.Z * Speed;
			}
			else
			{
				velocity.X = 0.0f;
				velocity.Z = 0.0f;
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}
}
