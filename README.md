# Egg Merge Rush

Merge-игра с яйцами, питомцами, уровнями и энергией. Цель первой сборки — **Android APK**. iOS позже.

## Что уже есть (MVP)

- Merge-поле 5×6, яйца T1–T5
- Уровни с растущей целью очков и меньшим числом ходов
- Энергия: макс **8**, **−1** за уровень, **+1 / 15 мин**, реклама **+3**
- Continue / x2 монеты / ускорение инкубации — rewarded (пока stub)
- Инкубация яйца T3+ → питомец с бонусом к монетам
- Сохранение в `persistentDataPath`

## Важно

На этом ПК **Unity Editor не установлен**. Чтобы собрать APK:

1. Поставь [Unity Hub](https://unity.com/download)
2. Установи **Unity 6 LTS** или **2022.3 LTS**
3. В модулях отметь **Android Build Support** (SDK + NDK + OpenJDK)
4. Hub → Open → папка `C:\Users\Roman\egg-merge-rush`
5. Дождись импорта, открой сцену `Assets/Scenes/Main.unity`
6. Play — проверить геймплей
7. Меню **EggMerge → Build Android APK**  
   или `File → Build Settings → Android → Build`  
   APK: `Builds/Android/EggMergeRush.apk`

## Package id

`com.romangames.eggmergerush`

## Реклама

`AdService` — заглушка (в Editor сразу «успех»). Перед релизом в стор подключи Unity LevelPlay или AdMob и замени вызовы в `Assets/Scripts/Ads/AdService.cs`.

## iOS (потом)

Тот же проект: переключить platform на iOS, Apple Developer + signing. Логику менять не нужно.
