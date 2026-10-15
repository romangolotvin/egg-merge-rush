extends Node

signal check_done(available: bool, notes: String)

var remote: Dictionary = {}
var available: bool = false
var last_status: String = ""

func check_now() -> void:
	last_status = "Проверка обновлений…"
	var req := HTTPRequest.new()
	add_child(req)
	var err := req.request(GameConfig.MANIFEST_URL)
	if err != OK:
		last_status = "Нет сети"
		check_done.emit(false, last_status)
		req.queue_free()
		return
	var result: Array = await req.request_completed
	req.queue_free()
	var response_code: int = result[1]
	var body: PackedByteArray = result[3]
	if response_code != 200:
		last_status = "Манифест недоступен"
		check_done.emit(false, last_status)
		return
	var parsed: Variant = JSON.parse_string(body.get_string_from_utf8())
	if typeof(parsed) != TYPE_DICTIONARY:
		last_status = "Плохой манифест"
		check_done.emit(false, last_status)
		return
	remote = parsed
	var ver := str(remote.get("version", ""))
	available = _is_newer(ver, GameConfig.APP_VERSION)
	if available:
		last_status = "Доступна %s" % ver
	else:
		last_status = "У тебя актуальная %s" % GameConfig.APP_VERSION
	check_done.emit(available, str(remote.get("notes", last_status)))

func open_download() -> void:
	var url := str(remote.get("android", ""))
	if url != "":
		OS.shell_open(url)

func _is_newer(remote_v: String, local_v: String) -> bool:
	var a := _parts(remote_v)
	var b := _parts(local_v)
	for i in 3:
		if a[i] > b[i]:
			return true
		if a[i] < b[i]:
			return false
	return false

func _parts(v: String) -> Array[int]:
	var clean := v.trim_prefix("v").trim_prefix("V")
	var bits := clean.split(".")
	var out: Array[int] = [0, 0, 0]
	for i in mini(3, bits.size()):
		out[i] = int(bits[i])
	return out
