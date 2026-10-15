# Egg Merge Rush

Отдельная Unity-игра (не связана с другими проектами).

Merge яйца → питомцы → уровни. Энергия, мягкая валюта, rewarded-реклама (stub), автообновление через `updates/version.json`.

## Стек

- Unity 6000.0.85f1 (Android first, iOS later)
- Package id: `com.romangames.eggmergerush`
- Локальная версия: `Assets/Scripts/Core/UpdateConfig.cs` → `AppVersion`
- Манифест: `updates/version.json`

## Сборка APK

1. Unity Hub → Editor 6000.0.85f1 + Android Build Support (SDK/NDK/OpenJDK)
2. Открыть эту папку как проект
3. Меню **EggMerge → Build Android APK**  
   или:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build-android.ps1
```

APK: `Builds/Android/EggMergeRush.apk`

## Автообновление

Игра читает:

`https://raw.githubusercontent.com/romangolotvin/egg-merge-rush/main/updates/version.json`

В хабе кнопка «Проверить обновление» / скачать APK при новой версии.

## Геймплей (MVP)

- Поле 5×6, merge яиц T1–T5
- Энергия: макс 8, −1/уровень, +1/15 мин, реклама +3
- Continue / x2 монеты / ускорение инкубации
- Яйцо T3+ → питомец с бонусом к монетам
