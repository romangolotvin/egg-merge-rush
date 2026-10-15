extends Control

var board: MergeBoard
var hud_score: Label
var hud_moves: Label
var hud_target: Label
var overlay: ColorRect
var overlay_title: Label
var overlay_body: Label
var btn_continue: Button
var btn_double: Button
var continues_used: int = 0
var last_reward: int = 0
var finished: bool = false

func _ready() -> void:
	_build()
	await get_tree().process_frame
	var moves := GameState.moves_for_level()
	var target := GameState.target_score()
	hud_target.text = "Цель %d" % target
	board.score_changed.connect(_on_score)
	board.won.connect(_on_win)
	board.lost.connect(_on_lose)
	board.high_tier.connect(_on_tier)
	await board.setup(moves, target, GameState.level * 1337 + GameState.coins + 17)

func _build() -> void:
	var bg := ColorRect.new()
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	bg.color = Color("140B2E")
	add_child(bg)
	add_child(_blob(_c("F15BB5", 0.22), Vector2(-40, -40), Vector2(260, 260)))
	add_child(_blob(_c("00F5D4", 0.18), Vector2(480, 980), Vector2(300, 300)))
	add_child(_blob(_c("FFD166", 0.16), Vector2(500, 80), Vector2(200, 200)))

	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	margin.add_theme_constant_override("margin_left", 16)
	margin.add_theme_constant_override("margin_right", 16)
	margin.add_theme_constant_override("margin_top", 28)
	margin.add_theme_constant_override("margin_bottom", 16)
	add_child(margin)

	var root := VBoxContainer.new()
	root.add_theme_constant_override("separation", 10)
	margin.add_child(root)

	var top := HBoxContainer.new()
	top.add_theme_constant_override("separation", 8)
	root.add_child(top)

	hud_score = _stat_card(top, Color("9B5DE5"), "Очки 0")
	hud_target = _stat_card(top, Color("00BBF9"), "Цель")
	hud_moves = _stat_card(top, Color("F15BB5"), "Ходы")

	root.add_child(UiKit.label("Уровень %d — собирай merge-комбо!" % GameState.level, 26, Color("FFD166"), HORIZONTAL_ALIGNMENT_CENTER))

	var board_card := UiKit.card(Color("241845"), 28)
	board_card.size_flags_vertical = Control.SIZE_EXPAND_FILL
	root.add_child(board_card)

	board = MergeBoard.new()
	board.size_flags_vertical = Control.SIZE_EXPAND_FILL
	board_card.add_child(board)

	var back := UiKit.fancy_button("← В меню", Color("6C5CE7"), Vector2(0, 64))
	back.pressed.connect(func(): get_tree().change_scene_to_file("res://scenes/hub.tscn"))
	root.add_child(back)

	_build_overlay()

func _stat_card(parent: Node, color: Color, text: String) -> Label:
	var card := UiKit.card(color, 18)
	card.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	var lbl := UiKit.label(text, 22, Color.WHITE, HORIZONTAL_ALIGNMENT_CENTER)
	card.add_child(lbl)
	parent.add_child(card)
	return lbl

func _c(hex: String, a: float) -> Color:
	var c := Color(hex)
	c.a = a
	return c

func _blob(color: Color, pos: Vector2, size: Vector2) -> ColorRect:
	var c := ColorRect.new()
	c.color = color
	c.position = pos
	c.size = size
	c.mouse_filter = Control.MOUSE_FILTER_IGNORE
	return c

func _build_overlay() -> void:
	overlay = ColorRect.new()
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	overlay.color = Color(0.05, 0.02, 0.12, 0.84)
	overlay.visible = false
	overlay.mouse_filter = Control.MOUSE_FILTER_STOP
	add_child(overlay)

	var center := CenterContainer.new()
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	overlay.add_child(center)

	var card := UiKit.card(Color("3A226E"), 28)
	card.custom_minimum_size = Vector2(560, 440)
	center.add_child(card)

	var box := VBoxContainer.new()
	box.add_theme_constant_override("separation", 14)
	card.add_child(box)

	overlay_title = UiKit.label("Заголовок", 36, Color("FFD166"), HORIZONTAL_ALIGNMENT_CENTER)
	overlay_body = UiKit.label("Текст", 22, Color("EDE4FF"), HORIZONTAL_ALIGNMENT_CENTER)
	box.add_child(overlay_title)
	box.add_child(overlay_body)

	btn_continue = UiKit.fancy_button("▶ Продолжить (реклама)", Color("FF6B35"))
	btn_continue.pressed.connect(_on_continue)
	box.add_child(btn_continue)

	btn_double = UiKit.fancy_button("💰 Монеты x2 (реклама)", Color("F15BB5"))
	btn_double.pressed.connect(_on_double)
	box.add_child(btn_double)

	var hub := UiKit.fancy_button("В хаб", Color("00BBF9"))
	hub.pressed.connect(func(): get_tree().change_scene_to_file("res://scenes/hub.tscn"))
	box.add_child(hub)

func _on_score(score: int, moves: int) -> void:
	hud_score.text = "Очки %d" % score
	hud_moves.text = "Ходы %d" % moves

func _on_tier(tier: int) -> void:
	if tier >= 3:
		GameState.try_start_hatch(tier)

func _on_win(score: int) -> void:
	if finished:
		return
	finished = true
	last_reward = GameState.coin_reward()
	GameState.add_coins(last_reward)
	GameState.level += 1
	GameState.save_game()
	overlay_title.text = "Победа! 🎉"
	overlay_body.text = "Очки: %d\n+%d монет\nДальше уровень %d" % [score, last_reward, GameState.level]
	btn_continue.visible = false
	btn_double.visible = true
	btn_double.disabled = false
	overlay.visible = true
	if GameState.level % 3 == 0:
		Ads.show_interstitial("between")

func _on_lose(score: int) -> void:
	if finished:
		return
	finished = true
	overlay_title.text = "Ходы кончились 😅"
	overlay_body.text = "Очки: %d / %d\nДосмотри ролик и получи ещё ходы!" % [score, board.target]
	btn_continue.visible = continues_used < GameConfig.MAX_CONTINUES
	btn_double.visible = false
	overlay.visible = true

func _on_continue() -> void:
	if continues_used >= GameConfig.MAX_CONTINUES:
		return
	var ok := await Ads.show_rewarded("continue")
	if not ok:
		return
	continues_used += 1
	GameState.ads_watched += 1
	GameState.save_game()
	finished = false
	overlay.visible = false
	board.add_moves(GameConfig.CONTINUE_MOVES)

func _on_double() -> void:
	if last_reward <= 0:
		return
	var ok := await Ads.show_rewarded("double")
	if not ok:
		return
	GameState.add_coins(last_reward)
	GameState.ads_watched += 1
	GameState.save_game()
	overlay_body.text = "Удвоено! Ещё +%d монет 💎" % last_reward
	btn_double.disabled = true
	last_reward = 0
