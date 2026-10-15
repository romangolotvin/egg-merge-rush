class_name GameConfig
extends RefCounted

const APP_VERSION := "0.2.0"
const MANIFEST_URL := "https://raw.githubusercontent.com/romangolotvin/egg-merge-rush/main/updates/version.json"

const MAX_ENERGY := 8
const ENERGY_PER_LEVEL := 1
const ENERGY_REGEN_SEC := 15 * 60
const ENERGY_FROM_AD := 3

const BOARD_W := 6
const BOARD_H := 7
const MAX_TIER := 6

const BASE_MOVES := 24
const MIN_MOVES := 12
const CONTINUE_MOVES := 6
const MAX_CONTINUES := 2

const BASE_TARGET := 120
const TARGET_GROWTH := 1.09
const COIN_BASE := 35

const HATCH_SEC := {
	3: 180,
	4: 480,
	5: 900,
	6: 1200,
}

static func tier_color(tier: int) -> Color:
	match clampi(tier, 1, 6):
		1: return Color("7BDFF2")
		2: return Color("B2F7EF")
		3: return Color("EFF7F6")
		4: return Color("F7D6E0")
		5: return Color("F2B5D4")
		_: return Color("FFD166")

static func tier_glow(tier: int) -> Color:
	match clampi(tier, 1, 6):
		1: return Color("00BBF9")
		2: return Color("00F5D4")
		3: return Color("FEE440")
		4: return Color("F15BB5")
		5: return Color("9B5DE5")
		_: return Color("FF6B35")
