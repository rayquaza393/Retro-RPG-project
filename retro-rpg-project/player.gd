extends CharacterBody3D

@export var speed = 5.0
@export var mouse_sensitivity = 0.05

func _input(event):
	if event.is_action_pressed("ui_cancel"):
		get_tree().quit()


var yaw = 0.0
var pitch = 0.0

func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)

func _unhandled_input(event):
	if event is InputEventMouseMotion:
		yaw -= event.relative.x * mouse_sensitivity
		pitch -= event.relative.y * mouse_sensitivity
		pitch = clamp(pitch, -90, 90)
		rotation_degrees.y = yaw
		$Camera3D.rotation_degrees.x = pitch

func _physics_process(delta):
	var input_dir = Vector3.ZERO
	if Input.is_action_pressed("move_forward"):
		input_dir -= transform.basis.z
	if Input.is_action_pressed("move_back"):
		input_dir += transform.basis.z
	if Input.is_action_pressed("move_left"):
		input_dir -= transform.basis.x
	if Input.is_action_pressed("move_right"):
		input_dir += transform.basis.x

	input_dir = input_dir.normalized()
	velocity.x = input_dir.x * speed
	velocity.z = input_dir.z * speed

	move_and_slide()
