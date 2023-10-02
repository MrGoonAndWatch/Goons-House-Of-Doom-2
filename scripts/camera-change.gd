extends Area3D

@export var target_camera: Camera3D

func _on_body_entered(body: Node3D):
	if not target_camera:
		print("no camera specified on " + name)
		pass
	
	if body.is_in_group("player"):
		print("changing camera to " + target_camera.name)
		target_camera.current = true
