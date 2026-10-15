class_name MergeBoard
extends VBoxContainer

signal score_changed(score: int, moves: int)
signal won(score: int)
signal lost(score: int)
signal high_tier(tier: int)

const W := GameConfig.BOARD_W
const H := GameConfig.BOARD_H

var cells: Array = []
var tiles: Array = []
var score: int = 0
var moves_left: int = 20
var highest: int = 1
var target: int = 100
var selected: EggTile = null
var busy: bool = false
var ended: bool = false
var _rng := RandomNumberGenerator.new()
var grid: GridContainer
var hint: Label

func _ready() -> void:
	add_theme_constant_override("separation", 10)
	hint = UiKit.label("Тапни яйцо, потом соседа того же уровня ✨", 16, Color("CDBFFF"), HORIZONTAL_ALIGNMENT_CENTER)
	add_child(hint)
	grid = GridContainer.new()
	grid.columns = W
	grid.size_flags_vertical = Control.SIZE_EXPAND_FILL
	grid.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	grid.add_theme_constant_override("h_separation", 8)
	grid.add_theme_constant_override("v_separation", 8)
	add_child(grid)

func setup(p_moves: int, p_target: int, seed_v: int) -> void:
	if grid == null:
		await ready
	moves_left = p_moves
	target = p_target
	score = 0
	highest = 1
	ended = false
	selected = null
	_rng.seed = seed_v
	_build_model()
	await _rebuild_tiles()
	_spawn_initial()
	_refresh_all()
	score_changed.emit(score, moves_left)

func add_moves(n: int) -> void:
	moves_left += n
	ended = false
	score_changed.emit(score, moves_left)

func _build_model() -> void:
	cells.clear()
	for y in H:
		var row: Array = []
		row.resize(W)
		row.fill(0)
		cells.append(row)

func _rebuild_tiles() -> void:
	for c in grid.get_children():
		c.queue_free()
	await get_tree().process_frame
	tiles.clear()
	for y in H:
		var row: Array = []
		for x in W:
			var t := EggTile.new()
			t.board_x = x
			t.board_y = y
			t.tapped.connect(_on_tile_tapped)
			grid.add_child(t)
			row.append(t)
		tiles.append(row)

func _spawn_initial() -> void:
	for i in W * 2 + 3:
		var empties := _empty_slots()
		if empties.is_empty():
			break
		var slot: Vector2i = empties[_rng.randi() % empties.size()]
		cells[slot.y][slot.x] = _roll_tier()

func _roll_tier() -> int:
	var r := _rng.randf()
	if r < 0.60:
		return 1
	if r < 0.86:
		return 2
	if r < 0.96:
		return 3
	return 4

func _empty_slots() -> Array[Vector2i]:
	var out: Array[Vector2i] = []
	for y in H:
		for x in W:
			if int(cells[y][x]) == 0:
				out.append(Vector2i(x, y))
	return out

func _refresh_all() -> void:
	for y in H:
		for x in W:
			var tile: EggTile = tiles[y][x]
			tile.set_egg(int(cells[y][x]))
			tile.set_selected(selected != null and selected.board_x == x and selected.board_y == y)

func _on_tile_tapped(tile: EggTile) -> void:
	if busy or ended or moves_left <= 0:
		return
	if selected == null:
		if tile.tier <= 0:
			return
		selected = tile
		_refresh_all()
		return
	if selected == tile:
		selected = null
		_refresh_all()
		return
	var x1 := selected.board_x
	var y1 := selected.board_y
	var x2 := tile.board_x
	var y2 := tile.board_y
	if absi(x1 - x2) + absi(y1 - y2) != 1:
		selected = tile if tile.tier > 0 else null
		_refresh_all()
		return
	await _try_move(x1, y1, x2, y2)

func _try_move(x1: int, y1: int, x2: int, y2: int) -> void:
	busy = true
	var a := int(cells[y1][x1])
	var b := int(cells[y2][x2])
	if a > 0 and b > 0 and a == b:
		var nt := mini(GameConfig.MAX_TIER, a + 1)
		cells[y2][x2] = nt
		cells[y1][x1] = 0
		highest = maxi(highest, nt)
		score += nt * 14 + (10 if nt >= 4 else 0)
		GameState.total_merges += 1
		GameState.best_tier = maxi(GameState.best_tier, nt)
		high_tier.emit(nt)
		if tiles[y2][x2]:
			tiles[y2][x2].pop_anim()
	else:
		cells[y1][x1] = b
		cells[y2][x2] = a
	moves_left -= 1
	selected = null
	_gravity_and_spawn()
	_refresh_all()
	score_changed.emit(score, moves_left)
	GameState.save_game()
	busy = false
	if score >= target:
		ended = true
		won.emit(score)
	elif moves_left <= 0:
		ended = true
		lost.emit(score)

func _gravity_and_spawn() -> void:
	for x in W:
		var stack: Array[int] = []
		for y in range(H - 1, -1, -1):
			var v := int(cells[y][x])
			if v > 0:
				stack.append(v)
		for y in range(H - 1, -1, -1):
			var idx := H - 1 - y
			if idx < stack.size():
				cells[y][x] = stack[idx]
			else:
				cells[y][x] = _roll_tier() if _rng.randf() < 0.75 else 0
