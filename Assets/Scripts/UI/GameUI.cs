using System.Text;
using EggMerge.Core;
using EggMerge.Merge;
using UnityEngine;
using UnityEngine.UI;

namespace EggMerge.UI
{
    public class GameUI : MonoBehaviour
    {
        Text _hubPanel;
        Text _levelPanel;
        Text _overlayPanel;
        Text _hudText;
        Text _statusText;
        Text _boardText;
        Text _overlayTitle;
        Text _overlayBody;
        Button[] _cellButtons;
        int _selected = -1;

        void Start()
        {
            BuildUi();
            var gm = GameManager.Instance;
            gm.StateChanged += Refresh;
            Refresh();
        }

        void Update()
        {
            // live energy timer on hub
            if (GameManager.Instance != null && GameManager.Instance.Screen == GameScreen.Hub)
                RefreshHudOnly();
        }

        void BuildUi()
        {
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            var root = CreatePanel(canvasGo.transform, "Root", new Color(0.10f, 0.14f, 0.18f, 1f));
            Stretch(root);

            _hudText = CreateText(root, "HUD", 40, TextAnchor.UpperLeft);
            var hudRt = _hudText.rectTransform;
            hudRt.anchorMin = new Vector2(0, 1);
            hudRt.anchorMax = new Vector2(1, 1);
            hudRt.pivot = new Vector2(0.5f, 1);
            hudRt.sizeDelta = new Vector2(-48, 160);
            hudRt.anchoredPosition = new Vector2(0, -24);

            _statusText = CreateText(root, "Status", 36, TextAnchor.UpperCenter);
            var st = _statusText.rectTransform;
            st.anchorMin = new Vector2(0, 1);
            st.anchorMax = new Vector2(1, 1);
            st.pivot = new Vector2(0.5f, 1);
            st.sizeDelta = new Vector2(-48, 80);
            st.anchoredPosition = new Vector2(0, -190);

            _hubPanel = CreatePanel(root, "Hub", new Color(0, 0, 0, 0));
            Stretch(_hubPanel, 280, 220);

            CreateButton(_hubPanel, "Играть (−1⚡)", new Vector2(0, 220), () => GameManager.Instance.TryStartLevel());
            CreateButton(_hubPanel, "Энергия +3 (реклама)", new Vector2(0, 80), () => GameManager.Instance.RefillEnergyWithAd());
            CreateButton(_hubPanel, "Ускорить яйцо (реклама)", new Vector2(0, -60), () => GameManager.Instance.SpeedUpHatch());
            CreateButton(_hubPanel, "Забрать питомца", new Vector2(0, -200), () => GameManager.Instance.ClaimHatch());
            CreateButton(_hubPanel, "Проверить обновление", new Vector2(0, -340), () =>
            {
                var upd = UpdateService.Instance;
                if (upd != null && upd.UpdateAvailable)
                    upd.OpenDownload();
                else
                    upd?.CheckNow();
            });

            var petsInfo = CreateText(_hubPanel, "Pets", 32, TextAnchor.UpperLeft);
            var pr = petsInfo.rectTransform;
            pr.anchorMin = new Vector2(0, 0);
            pr.anchorMax = new Vector2(1, 0.45f);
            pr.offsetMin = new Vector2(24, 24);
            pr.offsetMax = new Vector2(-24, -20);
            petsInfo.name = "PetsInfo";

            _levelPanel = CreatePanel(root, "Level", new Color(0, 0, 0, 0));
            Stretch(_levelPanel, 280, 180);
            _levelPanel.gameObject.SetActive(false);

            _boardText = CreateText(_levelPanel, "BoardHint", 30, TextAnchor.UpperCenter);
            var br = _boardText.rectTransform;
            br.anchorMin = new Vector2(0, 1);
            br.anchorMax = new Vector2(1, 1);
            br.pivot = new Vector2(0.5f, 1);
            br.sizeDelta = new Vector2(-40, 70);
            br.anchoredPosition = new Vector2(0, 0);

            BuildBoardGrid(_levelPanel);

            CreateButton(_levelPanel, "В меню", new Vector2(0, -780), () => GameManager.Instance.GoHub());

            _overlayPanel = CreatePanel(root, "Overlay", new Color(0.05f, 0.07f, 0.1f, 0.92f));
            Stretch(_overlayPanel);
            _overlayPanel.gameObject.SetActive(false);

            _overlayTitle = CreateText(_overlayPanel, "OverlayTitle", 56, TextAnchor.MiddleCenter);
            _overlayTitle.rectTransform.anchoredPosition = new Vector2(0, 220);
            _overlayBody = CreateText(_overlayPanel, "OverlayBody", 34, TextAnchor.MiddleCenter);
            _overlayBody.rectTransform.anchoredPosition = new Vector2(0, 80);
            _overlayBody.rectTransform.sizeDelta = new Vector2(900, 220);

            CreateButton(_overlayPanel, "Продолжить (реклама)", new Vector2(0, -80), () =>
            {
                var s = GameManager.Instance.Screen;
                if (s == GameScreen.Lose) GameManager.Instance.ContinueWithAd();
                else if (s == GameScreen.NoEnergy) GameManager.Instance.RefillEnergyWithAd();
                else if (s == GameScreen.Win) GameManager.Instance.DoubleRewardWithAd();
            }).name = "OverlayPrimary";

            CreateButton(_overlayPanel, "В хаб", new Vector2(0, -220), () => GameManager.Instance.GoHub());
        }

        void BuildBoardGrid(Transform parent)
        {
            var gridGo = new GameObject("Grid", typeof(RectTransform));
            gridGo.transform.SetParent(parent, false);
            var rt = gridGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1);
            rt.anchorMax = new Vector2(0.5f, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -90);
            rt.sizeDelta = new Vector2(980, 1100);

            var grid = gridGo.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(170, 170);
            grid.spacing = new Vector2(16, 16);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = GameConfig.BoardWidth;
            grid.childAlignment = TextAnchor.UpperCenter;

            var w = GameConfig.BoardWidth;
            var h = GameConfig.BoardHeight;
            _cellButtons = new Button[w * h];
            for (var i = 0; i < _cellButtons.Length; i++)
            {
                var idx = i;
                var btn = CreateCellButton(gridGo.transform, idx);
                _cellButtons[i] = btn;
            }
        }

        Button CreateCellButton(Transform parent, int index)
        {
            var go = new GameObject($"Cell_{index}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.22f, 0.30f, 0.38f, 1f);
            var btn = go.GetComponent<Button>();
            var label = CreateText(go.transform, "Label", 40, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            label.text = "";
            btn.onClick.AddListener(() => OnCellClick(index));
            return btn;
        }

        void OnCellClick(int index)
        {
            var gm = GameManager.Instance;
            if (gm.Screen != GameScreen.Level || gm.Board == null)
                return;

            if (_selected < 0)
            {
                _selected = index;
                RefreshBoard();
                return;
            }

            if (_selected == index)
            {
                _selected = -1;
                RefreshBoard();
                return;
            }

            UiIndexToBoard(_selected, gm.Board, out var x1, out var y1);
            UiIndexToBoard(index, gm.Board, out var x2, out var y2);
            gm.Board.TryMergeOrSwap(x1, y1, x2, y2);
            _selected = -1;
        }

        static void UiIndexToBoard(int index, MergeBoard board, out int x, out int y)
        {
            x = index % board.Width;
            var uiY = index / board.Width;
            y = board.Height - 1 - uiY;
        }

        void Refresh()
        {
            var gm = GameManager.Instance;
            RefreshHudOnly();
            _statusText.text = gm.StatusMessage;

            var hub = gm.Screen == GameScreen.Hub;
            var level = gm.Screen == GameScreen.Level;
            var overlay = gm.Screen is GameScreen.Win or GameScreen.Lose or GameScreen.NoEnergy;

            _hubPanel.gameObject.SetActive(hub);
            _levelPanel.gameObject.SetActive(level || overlay && gm.Board != null);
            _overlayPanel.gameObject.SetActive(overlay);

            if (hub)
                RefreshHubExtras();

            if (gm.Board != null)
                RefreshBoard();

            if (overlay)
                RefreshOverlay();
        }

        void RefreshHudOnly()
        {
            var gm = GameManager.Instance;
            var next = gm.Energy.TimeToNext();
            var timer = gm.Energy.Energy >= gm.Energy.Max
                ? "полная"
                : $"{next.Minutes:00}:{next.Seconds:00}";
            _hudText.text =
                $"{GameConfig.AppName}  v{UpdateConfig.AppVersion}\n⚡ {gm.Energy.Energy}/{gm.Energy.Max}  (+1 через {timer})\n" +
                $"🪙 {gm.Coins.Coins}    Ур. {gm.CurrentLevel}";
        }

        void RefreshHubExtras()
        {
            var gm = GameManager.Instance;
            var pets = _hubPanel.Find("PetsInfo")?.GetComponent<Text>();
            if (pets == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("Питомцы:");
            if (gm.Save.pets.Count == 0)
                sb.AppendLine("пока нет — собери яйцо T3+ в уровне");
            else
            {
                foreach (var p in gm.Save.pets)
                {
                    var info = PetCatalog.Get(p.id);
                    sb.AppendLine($"• {info.Name} ур.{p.level} x{p.count} — {info.BonusText}");
                }
            }

            if (gm.Hatch.HasActive)
            {
                var left = gm.Hatch.TimeLeft();
                sb.AppendLine(gm.Hatch.IsReady()
                    ? "\nЯйцо готово — забери питомца!"
                    : $"\nИнкубация T{gm.Hatch.Active.eggTier}: {left.Minutes:00}:{left.Seconds:00}");
            }
            else
            {
                sb.AppendLine("\nИнкубация: нет активного яйца");
            }

            pets.text = sb.ToString();
        }

        void RefreshBoard()
        {
            var board = GameManager.Instance.Board;
            if (board == null) return;

            _boardText.text =
                $"Очки {board.Score}/{GameManager.Instance.TargetScore}   Ходы {board.MovesLeft}\n" +
                "Тап: выбрать → соседняя клетка: merge/ход";

            for (var y = 0; y < board.Height; y++)
            for (var x = 0; x < board.Width; x++)
            {
                // UI grid is top-to-bottom; board y=0 is bottom after gravity.
                var uiRow = board.Height - 1 - y;
                var idx = uiRow * board.Width + x;
                var cell = board.Get(x, y);
                var label = _cellButtons[idx].GetComponentInChildren<Text>();
                var img = _cellButtons[idx].GetComponent<Image>();
                var selected = _selected == idx;

                if (cell.IsEmpty)
                {
                    label.text = "";
                    img.color = selected
                        ? new Color(0.35f, 0.45f, 0.55f)
                        : new Color(0.18f, 0.24f, 0.30f);
                }
                else
                {
                    label.text = $"T{cell.Tier}\n🥚";
                    img.color = selected ? TierColor(cell.Tier) * 1.25f : TierColor(cell.Tier);
                }
            }
        }

        void RefreshOverlay()
        {
            var gm = GameManager.Instance;
            var primary = _overlayPanel.Find("OverlayPrimary")?.GetComponentInChildren<Text>();

            switch (gm.Screen)
            {
                case GameScreen.Win:
                    _overlayTitle.text = "Победа!";
                    _overlayBody.text = $"+{gm.LastReward} монет\nМожно x2 за рекламу";
                    if (primary != null) primary.text = "Монеты x2 (реклама)";
                    break;
                case GameScreen.Lose:
                    _overlayTitle.text = "Поражение";
                    _overlayBody.text =
                        $"Смотри и продолжай (+{GameConfig.ContinueExtraMoves} ходов)\n" +
                        $"Осталось продолжений: {GameConfig.MaxContinuesPerLevel - gm.ContinuesUsed}";
                    if (primary != null) primary.text = "Продолжить (реклама)";
                    break;
                case GameScreen.NoEnergy:
                    _overlayTitle.text = "Нет энергии";
                    _overlayBody.text =
                        $"Макс {GameConfig.MaxEnergy}⚡\n+1 каждые 15 мин\nРеклама: +{GameConfig.EnergyFromAd}⚡";
                    if (primary != null) primary.text = $"+{GameConfig.EnergyFromAd} энергии (реклама)";
                    break;
            }
        }

        static Color TierColor(int tier)
        {
            return tier switch
            {
                1 => new Color(0.75f, 0.82f, 0.90f),
                2 => new Color(0.55f, 0.78f, 0.95f),
                3 => new Color(0.45f, 0.85f, 0.65f),
                4 => new Color(0.95f, 0.75f, 0.35f),
                _ => new Color(0.95f, 0.45f, 0.55f)
            };
        }

        static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go.GetComponent<RectTransform>();
        }

        static Text CreateText(Transform parent, string name, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null)
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = size;
            t.color = Color.white;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        static Button CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(820, 120);
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = new Color(0.20f, 0.55f, 0.48f, 1f);
            var text = CreateText(go.transform, "Text", 40, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            text.text = label;
            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(action);
            return btn;
        }

        static void Stretch(RectTransform rt, float top = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(0, bottom);
            rt.offsetMax = new Vector2(0, -top);
        }
    }
}
