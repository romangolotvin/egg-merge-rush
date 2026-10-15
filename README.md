# Egg Merge Rush (Godot)

Яркая merge-игра с яйцами и питомцами. **Godot 4.7**, Android first.

## Запуск

Открыть папку в Godot 4.7 → Play.

## Геймплей

- Соединяй соседние яйца одного тира → следующий тир
- Набирай очки за лимит ходов
- Энергия: 8 макс, −1/уровень, +1/15 мин, реклама +3
- Яйцо T3+ запускает инкубацию питомца
- Continue / x2 монеты через rewarded (stub)

## Версия

`scripts/game_config.gd` → `APP_VERSION`  
Манифест: `updates/version.json`

## Сборка APK

```
Godot --headless --path . --export-release "Android" builds/android/EggMergeRush.apk
```
