extends CharacterBody3D


const SPEED = 50.0
const RUN_MODIFIER = 3.0
const BACKWARDS_MODIFIER = 0.5

var QUICK_TURN_END_THRESHOLD = 0.05
const QUICK_TURN_SPEED = 5.0
const ROTATION_SPEED = 2.0

var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

var is_quick_turning = false
var quick_turn_target

func _physics_process(delta):
	_process_gravity(delta)
	_process_movement(delta)
	_process_quick_turn(delta)

	move_and_slide()

func _process_gravity(delta):
	if not is_on_floor():
		velocity.y -= gravity * delta

func _process_movement(delta):
	var input_dir = Input.get_vector("left", "right", "up", "down")
	
	var input_rotation = input_dir.x
	var input_movement = input_dir.y
	
	if input_rotation != 0 and not is_quick_turning:
		transform.basis = transform.basis.rotated(Vector3.UP, input_rotation * ROTATION_SPEED * delta * -1)
	
	if input_movement != 0 and not is_quick_turning:
		var running = Input.is_action_pressed("run")
		var run_mod = 1
		if running and input_movement < 0:
			run_mod = RUN_MODIFIER
		var backwards_mod = 1
		if input_movement > 0:
			backwards_mod = BACKWARDS_MODIFIER
			
		if input_movement > 0 and Input.is_action_just_pressed("run"):
			start_quick_turn()
		
		var movement = -(transform.basis.x * input_movement * delta) * SPEED * run_mod * backwards_mod
		velocity.x = movement.x
		velocity.z = movement.z
	else:
		velocity = Vector3(0, velocity.y, 0)

func _process_quick_turn(delta):
	if not is_quick_turning:
		return
	
	transform.basis = transform.basis.slerp(quick_turn_target.basis, QUICK_TURN_SPEED * delta).orthonormalized()
	if transform.basis.z.angle_to(quick_turn_target.basis.z) < QUICK_TURN_END_THRESHOLD:
		transform.basis = quick_turn_target.basis
		is_quick_turning = false

func start_quick_turn():
	if is_quick_turning:
		return
	
	quick_turn_target = transform.rotated(Vector3.UP, PI)
	is_quick_turning = true
