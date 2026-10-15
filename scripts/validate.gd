extends SceneTree

func _initialize() -> void:
	var err := OK
	for path in [
		"res://scripts/game_config.gd",
		"res://scripts/game_state.gd",
		"res://scripts/hub.gd",
		"res://scripts/level.gd",
		"res://scripts/merge_board.gd",
		"res://scripts/egg_tile.gd",
		"res://scenes/boot.tscn",
		"res://scenes/hub.tscn",
		"res://scenes/level.tscn",
	]:
		if not ResourceLoader.exists(path):
			push_error("Missing " + path)
			err = FAILED
		else:
			var res = load(path)
			if res == null:
				push_error("Load fail " + path)
				err = FAILED
			else:
				print("OK ", path)
	quit(0 if err == OK else 1)
