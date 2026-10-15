class_name EggTile
extends Button

signal tapped(tile: EggTile)

var board_x: int = 0
var board_y: int = 0
var tier: int = 0
var selected: bool = false

func _ready() -> void:
	focus_mode = Control.FOCUS_NONE
	toggle_mode = false
	custom_minimum_size = Vector2(88, 88)
	size_flags_horizontal = Control.SIZE_EXPAND_FILL
	size_flags_vertical = Control.SIZE_EXPAND_FILL
	pressed.connect(func(): tapped.emit(self))
	_refresh_style()

func set_egg(t: int) -> void:
	tier = t
	_refresh_style()

func set_selected(on: bool) -> void:
	selected = on
	_refresh_style()
	if on:
		var tw := create_tween()
		tw.tween_property(self, "scale", Vector2(1.08, 1.08), 0.08)
	else:
		scale = Vector2.ONE

func pop_anim() -> void:
	var tw := create_tween()
	tw.tween_property(self, "scale", Vector2(1.2, 1.2), 0.08)
	tw.tween_property(self, "scale", Vector2.ONE, 0.12)

func _refresh_style() -> void:
	var sb := StyleBoxFlat.new()
	if tier <= 0:
		sb.bg_color = Color(1, 1, 1, 0.08)
		sb.set_corner_radius_all(18)
		sb.border_width_left = 2
		sb.border_width_right = 2
		sb.border_width_top = 2
		sb.border_width_bottom = 2
		sb.border_color = Color(1, 1, 1, 0.12)
		text = ""
	else:
		var c := GameConfig.tier_color(tier)
		var g := GameConfig.tier_glow(tier)
		sb.bg_color = c
		sb.set_corner_radius_all(22)
		sb.border_width_left = 3
		sb.border_width_right = 3
		sb.border_width_top = 3
		sb.border_width_bottom = 3
		sb.border_color = g if selected else c.darkened(0.15)
		sb.shadow_color = g
		sb.shadow_size = 10 if selected else 4
		sb.shadow_offset = Vector2(0, 2)
		text = "T%d\n🥚" % tier
	add_theme_stylebox_override("normal", sb)
	var hover := sb.duplicate()
	hover.bg_color = sb.bg_color.lightened(0.08)
	add_theme_stylebox_override("hover", hover)
	add_theme_stylebox_override("pressed", hover)
	add_theme_font_size_override("font_size", 20)
	add_theme_color_override("font_color", Color("1B1030") if tier > 0 else Color(1, 1, 1, 0.35))
	add_theme_color_override("font_hover_color", Color("1B1030"))
	add_theme_constant_override("outline_size", 0)
