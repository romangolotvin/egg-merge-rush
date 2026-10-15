extends Control

var _energy_lbl: Label
var _coins_lbl: Label
var _pets_lbl: Label
var _hatch_lbl: Label
var _status_lbl: Label
var _timer: Timer

func _ready() -> void:
	_build()
	GameState.changed.connect(_refresh)
	Updater.check_done.connect(_on_update)
	_refresh()
	Updater.check_now()
	_timer = Timer.new()
	_timer.wait_time = 1.0
	_timer.timeout.connect(_refresh)
	add_child(_timer)
	_timer.start()

func _build() -> void:
	# Background layers
	var bg := ColorRect.new()
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	bg.color = Color("1A1035")
	add_child(bg)

	add_child(_blob(_c("FF6BCB", 0.35), Vector2(80, 120), Vector2(280, 280)))
	add_child(_blob(_c("06D6A0", 0.28), Vector2(420, 900), Vector2(320, 320)))
	add_child(_blob(_c("FFD166", 0.22), Vector2(480, 200), Vector2(220, 220)))

	var margin := MarginContainer.new()
	margin.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	margin.add_theme_constant_override("margin_left", 28)
	margin.add_theme_constant_override("margin_right", 28)
	margin.add_theme_constant_override("margin_top", 36)
	margin.add_theme_constant_override("margin_bottom", 28)
	add_child(margin)

	var root := VBoxContainer.new()
	root.add_theme_constant_override("separation", 16)
	margin.add_child(root)

	var title := UiKit.label("Egg Merge Rush", 42, Color("FFD166"), HORIZONTAL_ALIGNMENT_CENTER)
	root.add_child(title)
	root.add_child(UiKit.label("Соединяй яйца • копи питомцев • рви уровни", 18, Color("E0D4FF"), HORIZONTAL_ALIGNMENT_CENTER))

	var top := HBoxContainer.new()
	top.add_theme_constant_override("separation", 12)
	root.add_child(top)

	var energy_card := UiKit.card(Color("9B5DE5"))
	energy_card.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_energy_lbl = UiKit.label("⚡ 8/8", 24, Color.WHITE, HORIZONTAL_ALIGNMENT_CENTER)
	energy_card.add_child(_energy_lbl)
	top.add_child(energy_card)

	var coin_card := UiKit.card(Color("F15BB5"))
	coin_card.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	_coins_lbl = UiKit.label("🪙 0", 24, Color.WHITE, HORIZONTAL_ALIGNMENT_CENTER)
	coin_card.add_child(_coins_lbl)
	top.add_child(coin_card)

	var level_card := UiKit.card(Color("00BBF9"))
	level_card.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	level_card.add_child(UiKit.label("Ур. %d" % GameState.level, 24, Color.WHITE, HORIZONTAL_ALIGNMENT_CENTER))
	level_card.name = "LevelCard"
	top.add_child(level_card)

	var play := UiKit.fancy_button("▶  ИГРАТЬ  (−1⚡)", Color("06D6A0"), Vector2(0, 88))
	play.pressed.connect(_on_play)
	root.add_child(play)

	var row := HBoxContainer.new()
	row.add_theme_constant_override("separation", 12)
	root.add_child(row)

	var ad_energy := UiKit.fancy_button("+3⚡ реклама", Color("FF6B35"), Vector2(0, 70))
	ad_energy.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	ad_energy.pressed.connect(_on_ad_energy)
	row.add_child(ad_energy)

	var ad_hatch := UiKit.fancy_button("Ускорить яйцо", Color("9B5DE5"), Vector2(0, 70))
	ad_hatch.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	ad_hatch.pressed.connect(_on_speed_hatch)
	row.add_child(ad_hatch)

	var claim := UiKit.fancy_button("Забрать питомца", Color("F15BB5"), Vector2(0, 70))
	claim.pressed.connect(_on_claim)
	root.add_child(claim)

	var update_btn := UiKit.fancy_button("Проверить обновление", Color("00BBF9"), Vector2(0, 64))
	update_btn.pressed.connect(func():
		if Updater.available:
			Updater.open_download()
		else:
			Updater.check_now()
	)
	root.add_child(update_btn)

	var info := UiKit.card(_c("2F214F", 0.92), 20)
	info.size_flags_vertical = Control.SIZE_EXPAND_FILL
	var info_box := VBoxContainer.new()
	info_box.add_theme_constant_override("separation", 8)
	info.add_child(info_box)
	_hatch_lbl = UiKit.label("Инкубация: —", 20, Color("FFD166"))
	_pets_lbl = UiKit.label("Питомцы: пока нет", 20, Color("E0D4FF"))
	_status_lbl = UiKit.label("v%s" % GameConfig.APP_VERSION, 18, Color("B8A9E0"))
	info_box.add_child(_hatch_lbl)
	info_box.add_child(_pets_lbl)
	info_box.add_child(_status_lbl)
	info_box.add_child(UiKit.label("Цель: собери яйца одного уровня рядом и получи следующий тир. Чем выше тир — тем жирнее очки и шанс запустить инкубацию!", 17, Color("D7CCF5")))
	root.add_child(info)

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

func _refresh() -> void:
	var next := GameState.time_to_next_energy()
	var t := "полная" if next <= 0 else UiKit.format_time(next)
	_energy_lbl.text = "⚡ %d/%d\n+1 через %s" % [GameState.energy, GameConfig.MAX_ENERGY, t]
	_coins_lbl.text = "🪙 %d" % GameState.coins
	var lc := get_node_or_null("MarginContainer/VBoxContainer/HBoxContainer/LevelCard")
	# level card is first-built without path certainty — update via children scan
	for child in get_children():
		pass
	# Update level label inside top cards
	_update_level_card()

	if GameState.hatch.is_empty() or int(GameState.hatch.get("egg_tier", 0)) <= 0:
		_hatch_lbl.text = "Инкубация: нет активного яйца (собери T3+ в уровне)"
	elif GameState.hatch_ready():
		_hatch_lbl.text = "Инкубация: ГОТОВО! Забери питомца 🎁"
	else:
		_hatch_lbl.text = "Инкубация T%d: %s" % [int(GameState.hatch.get("egg_tier", 0)), UiKit.format_time(GameState.hatch_left())]

	if GameState.pets.is_empty():
		_pets_lbl.text = "Питомцы: пока нет — они дают бонус к монетам"
	else:
		var parts: PackedStringArray = []
		for id in GameState.pets.keys():
			var p: Dictionary = GameState.pets[id]
			parts.append("%s %s ур.%d" % [GameState.pet_emoji(id), GameState.pet_name(id), int(p.get("level", 1))])
		_pets_lbl.text = "Питомцы: " + ", ".join(parts)

	_status_lbl.text = "v%s  •  лучший тир T%d  •  merge: %d" % [GameConfig.APP_VERSION, GameState.best_tier, GameState.total_merges]

func _update_level_card() -> void:
	# Find the cyan card label
	var nodes := find_children("*", "Label", true, false)
	for n in nodes:
		var l := n as Label
		if l and l.text.begins_with("Ур."):
			l.text = "Ур. %d" % GameState.level

func _on_play() -> void:
	if not GameState.can_play():
		_status_lbl.text = "Нет энергии! Смотри рекламу: +3⚡"
		return
	if not GameState.spend_energy():
		return
	get_tree().change_scene_to_file("res://scenes/level.tscn")

func _on_ad_energy() -> void:
	_status_lbl.text = "Смотрим рекламу…"
	var ok := await Ads.show_rewarded("energy")
	if ok:
		GameState.add_energy(GameConfig.ENERGY_FROM_AD)
		GameState.ads_watched += 1
		GameState.save_game()
		_status_lbl.text = "+%d энергии!" % GameConfig.ENERGY_FROM_AD

func _on_speed_hatch() -> void:
	if GameState.hatch.is_empty() or int(GameState.hatch.get("egg_tier", 0)) <= 0:
		_status_lbl.text = "Сначала запусти инкубацию яйцом T3+"
		return
	if GameState.hatch_ready():
		_status_lbl.text = "Уже готово — забери питомца"
		return
	var ok := await Ads.show_rewarded("hatch")
	if ok:
		GameState.speed_hatch_done()
		_status_lbl.text = "Яйцо готово!"

func _on_claim() -> void:
	var id := GameState.claim_hatch()
	if id == "":
		_status_lbl.text = "Пока нечего забирать"
		return
	_status_lbl.text = "Вылупился %s %s!" % [GameState.pet_emoji(id), GameState.pet_name(id)]

func _on_update(available: bool, notes: String) -> void:
	_status_lbl.text = notes if notes != "" else Updater.last_status
	if available:
		_status_lbl.text = "Есть обновление! Жми «Проверить обновление» ещё раз для скачивания."
