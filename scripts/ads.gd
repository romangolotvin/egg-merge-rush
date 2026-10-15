extends Node

## Stub rewarded ads. Replace with AdMob later.

func show_rewarded(placement: String) -> bool:
	print("[Ads] rewarded stub: ", placement)
	await get_tree().create_timer(0.35).timeout
	return true

func show_interstitial(placement: String) -> void:
	print("[Ads] interstitial stub: ", placement)
