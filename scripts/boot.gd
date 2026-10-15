extends Control

func _ready() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	bg.color = Color("1A1035")
	add_child(bg)
	var center := CenterContainer.new()
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(center)
	var box := VBoxContainer.new()
	box.add_theme_constant_override("separation", 12)
	center.add_child(box)
	box.add_child(UiKit.label("Egg Merge Rush", 44, Color("FFD166"), HORIZONTAL_ALIGNMENT_CENTER))
	box.add_child(UiKit.label("Загрузка…", 22, Color("E0D4FF"), HORIZONTAL_ALIGNMENT_CENTER))
	await get_tree().create_timer(0.45).timeout
	get_tree().change_scene_to_file("res://scenes/hub.tscn")
