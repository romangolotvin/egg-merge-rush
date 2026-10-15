extends Node

signal changed
signal coins_changed(v: int)
signal energy_changed(v: int)

var coins: int = 0
var energy: int = GameConfig.MAX_ENERGY
var last_energy_unix: float = 0.0
var level: int = 1
var best_tier: int = 1
var pets: Dictionary = {} # id -> {level, count}
var hatch: Dictionary = {} # egg_tier, finish_unix, pet_id
var ads_watched: int = 0
var total_merges: int = 0

const SAVE_PATH := "user://save.json"

func _ready() -> void:
	load_game()
	apply_energy_regen()
	changed.emit()

func _process(_dt: float) -> void:
	if energy >= GameConfig.MAX_ENERGY:
		return
	if apply_energy_regen():
		energy_changed.emit(energy)
		changed.emit()

func save_game() -> void:
	var data := {
		"coins": coins,
		"energy": energy,
		"last_energy_unix": last_energy_unix,
		"level": level,
		"best_tier": best_tier,
		"pets": pets,
		"hatch": hatch,
		"ads_watched": ads_watched,
		"total_merges": total_merges,
	}
	var f := FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if f:
		f.store_string(JSON.stringify(data))

func load_game() -> void:
	if not FileAccess.file_exists(SAVE_PATH):
		last_energy_unix = Time.get_unix_time_from_system()
		return
	var f := FileAccess.open(SAVE_PATH, FileAccess.READ)
	if f == null:
		return
	var parsed: Variant = JSON.parse_string(f.get_as_text())
	if typeof(parsed) != TYPE_DICTIONARY:
		return
	var d: Dictionary = parsed
	coins = int(d.get("coins", 0))
	energy = int(d.get("energy", GameConfig.MAX_ENERGY))
	last_energy_unix = float(d.get("last_energy_unix", Time.get_unix_time_from_system()))
	level = int(d.get("level", 1))
	best_tier = int(d.get("best_tier", 1))
	pets = d.get("pets", {})
	hatch = d.get("hatch", {})
	ads_watched = int(d.get("ads_watched", 0))
	total_merges = int(d.get("total_merges", 0))

func apply_energy_regen() -> bool:
	if energy >= GameConfig.MAX_ENERGY:
		last_energy_unix = Time.get_unix_time_from_system()
		return false
	var now := Time.get_unix_time_from_system()
	var elapsed := now - last_energy_unix
	if elapsed < GameConfig.ENERGY_REGEN_SEC:
		return false
	var gained := int(elapsed / GameConfig.ENERGY_REGEN_SEC)
	if gained <= 0:
		return false
	var before := energy
	energy = mini(GameConfig.MAX_ENERGY, energy + gained)
	var used := energy - before
	last_energy_unix += used * GameConfig.ENERGY_REGEN_SEC
	save_game()
	return used > 0

func time_to_next_energy() -> int:
	apply_energy_regen()
	if energy >= GameConfig.MAX_ENERGY:
		return 0
	var now := Time.get_unix_time_from_system()
	var left := int(GameConfig.ENERGY_REGEN_SEC - (now - last_energy_unix))
	return maxi(0, left)

func can_play() -> bool:
	apply_energy_regen()
	return energy >= GameConfig.ENERGY_PER_LEVEL

func spend_energy() -> bool:
	apply_energy_regen()
	if energy < GameConfig.ENERGY_PER_LEVEL:
		return false
	if energy >= GameConfig.MAX_ENERGY:
		last_energy_unix = Time.get_unix_time_from_system()
	energy -= GameConfig.ENERGY_PER_LEVEL
	save_game()
	energy_changed.emit(energy)
	changed.emit()
	return true

func add_energy(amount: int) -> void:
	apply_energy_regen()
	energy = mini(GameConfig.MAX_ENERGY, energy + amount)
	if energy >= GameConfig.MAX_ENERGY:
		last_energy_unix = Time.get_unix_time_from_system()
	save_game()
	energy_changed.emit(energy)
	changed.emit()

func add_coins(amount: int) -> void:
	if amount <= 0:
		return
	coins += amount
	save_game()
	coins_changed.emit(coins)
	changed.emit()

func pet_coin_mult() -> float:
	var m := 1.0
	for id in pets.keys():
		var p: Dictionary = pets[id]
		var lvl := int(p.get("level", 1))
		var cnt := int(p.get("count", 1))
		m += 0.04 * lvl * cnt
	return m

func target_score(for_level: int = -1) -> int:
	var lv := level if for_level < 0 else for_level
	return int(round(GameConfig.BASE_TARGET * pow(GameConfig.TARGET_GROWTH, lv - 1)))

func moves_for_level(for_level: int = -1) -> int:
	var lv := level if for_level < 0 else for_level
	return maxi(GameConfig.MIN_MOVES, GameConfig.BASE_MOVES - int((lv - 1) / 3.0))

func coin_reward() -> int:
	return int(round((GameConfig.COIN_BASE + level * 4) * pet_coin_mult()))

func try_start_hatch(tier: int) -> bool:
	if tier < 3:
		return false
	if not hatch.is_empty() and int(hatch.get("egg_tier", 0)) > 0:
		return false
	var sec: int = GameConfig.HATCH_SEC.get(tier, 600)
	var pets_ids := ["chick", "bunny", "dragon", "fox", "owl"]
	hatch = {
		"egg_tier": tier,
		"finish_unix": Time.get_unix_time_from_system() + sec,
		"pet_id": pets_ids[randi() % pets_ids.size()],
	}
	save_game()
	changed.emit()
	return true

func hatch_ready() -> bool:
	if hatch.is_empty() or int(hatch.get("egg_tier", 0)) <= 0:
		return false
	return Time.get_unix_time_from_system() >= float(hatch.get("finish_unix", 0))

func hatch_left() -> int:
	if hatch.is_empty():
		return 0
	return maxi(0, int(float(hatch.get("finish_unix", 0)) - Time.get_unix_time_from_system()))

func speed_hatch_done() -> void:
	if hatch.is_empty():
		return
	hatch["finish_unix"] = Time.get_unix_time_from_system()
	ads_watched += 1
	save_game()
	changed.emit()

func claim_hatch() -> String:
	if not hatch_ready():
		return ""
	var id := str(hatch.get("pet_id", "chick"))
	hatch = {}
	if pets.has(id):
		var p: Dictionary = pets[id]
		p["count"] = int(p.get("count", 1)) + 1
		p["level"] = mini(10, int(p.get("level", 1)) + 1)
		pets[id] = p
	else:
		pets[id] = {"level": 1, "count": 1}
	save_game()
	changed.emit()
	return id

static func pet_name(id: String) -> String:
	match id:
		"chick": return "Цыпёнок"
		"bunny": return "Кролик"
		"dragon": return "Дракончик"
		"fox": return "Лисичка"
		"owl": return "Совёнок"
		_: return id

static func pet_emoji(id: String) -> String:
	match id:
		"chick": return "🐥"
		"bunny": return "🐰"
		"dragon": return "🐉"
		"fox": return "🦊"
		"owl": return "🦉"
		_: return "🐾"
