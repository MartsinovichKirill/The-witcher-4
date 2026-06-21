using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace WitcherRightVersion.Debugging
{
    // Runtime developer console toggled with the `~` / backquote key.
    // Commands (a leading slash is optional):
    //   /fly            toggle free-fly noclip movement (WASD + Space up, Ctrl/C down, Shift faster)
    //   /speed <n>      set the fly speed
    //   /tp <x> <y> <z> teleport the player
    //   /village /swamp /forest /tower /ash   jump to a zone hub
    //   help            list commands
    public sealed class DeveloperConsole : MonoBehaviour
    {
        private bool _open;
        private bool _fly;
        private string _input = string.Empty;
        private readonly List<string> _log = new List<string>();
        private float _flySpeed = 16f;
        private Vector2 _scroll;

        private Transform _player;
        private MonoBehaviour _playerController;
        private CharacterController _characterController;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Log("Консоль разработчика готова. ~ — открыть/закрыть. Введите help.");
        }

        private void ResolvePlayer()
        {
            if (_player != null)
            {
                return;
            }

            var go = GameObject.FindGameObjectWithTag("Player");
            if (go == null)
            {
                return;
            }

            _player = go.transform;
            _playerController = go.GetComponent("PlayerController") as MonoBehaviour;
            _characterController = go.GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                _open = !_open;
                _input = string.Empty;
            }

            if (_fly && !_open)
            {
                DoFly();
            }
        }

        private void DoFly()
        {
            ResolvePlayer();
            if (_player == null)
            {
                return;
            }

            var cam = Camera.main;
            var fwd = cam != null ? cam.transform.forward : _player.forward;
            var right = cam != null ? cam.transform.right : _player.right;
            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += fwd;
            if (Input.GetKey(KeyCode.S)) move -= fwd;
            if (Input.GetKey(KeyCode.D)) move += right;
            if (Input.GetKey(KeyCode.A)) move -= right;
            if (Input.GetKey(KeyCode.Space)) move += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) move += Vector3.down;

            if (move.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var speed = _flySpeed * (Input.GetKey(KeyCode.LeftShift) ? 3f : 1f);
            _player.position += move.normalized * speed * Time.deltaTime;
        }

        private void SetFly(bool on)
        {
            ResolvePlayer();
            _fly = on;

            // Disable the normal controller (gravity/move) and the CharacterController
            // collisions so the player can fly through anything to inspect the map.
            if (_playerController != null)
            {
                _playerController.enabled = !on;
            }

            if (_characterController != null)
            {
                _characterController.enabled = !on;
            }

            Log(on
                ? "Полёт ВКЛ: WASD — лететь, Space — вверх, Ctrl/C — вниз, Shift — быстрее."
                : "Полёт ВЫКЛ.");
        }

        private void Teleport(Vector3 pos, string label)
        {
            ResolvePlayer();
            if (_player == null)
            {
                Log("Игрок не найден.");
                return;
            }

            var hadCc = _characterController != null && _characterController.enabled;
            if (hadCc) _characterController.enabled = false;
            _player.position = pos;
            if (hadCc) _characterController.enabled = true;
            Log("Перемещение: " + label + " " + pos);
        }

        private void Execute(string raw)
        {
            Log("> " + raw);
            var cmd = raw.Trim();
            if (cmd.StartsWith("/"))
            {
                cmd = cmd.Substring(1);
            }

            if (string.IsNullOrEmpty(cmd))
            {
                return;
            }

            var parts = cmd.Split(' ');
            switch (parts[0].ToLowerInvariant())
            {
                case "fly":
                    SetFly(!_fly);
                    break;
                case "speed":
                    if (parts.Length > 1 && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
                    {
                        _flySpeed = Mathf.Clamp(s, 1f, 300f);
                        Log("Скорость полёта: " + _flySpeed);
                    }
                    else
                    {
                        Log("Использование: /speed <число>");
                    }

                    break;
                case "tp":
                    if (parts.Length >= 4
                        && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                        && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                        && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                    {
                        Teleport(new Vector3(x, y, z), "координаты");
                    }
                    else
                    {
                        Log("Использование: /tp <x> <y> <z>");
                    }

                    break;
                case "village":
                    Teleport(new Vector3(0f, 2f, -8f), "Деревня");
                    break;
                case "swamp":
                    Teleport(new Vector3(12f, 3f, -72f), "Болото (Эльса)");
                    break;
                case "forest":
                    Teleport(new Vector3(-68f, 3f, 8f), "Лес (Ивар)");
                    break;
                case "tower":
                    Teleport(new Vector3(0f, 3f, 70f), "Башня (финал)");
                    break;
                case "ash":
                    Teleport(new Vector3(72f, 3f, 4f), "Пепельный тракт");
                    break;
                case "help":
                    Log("/fly — полёт. /speed <n> — скорость. /tp <x y z> — телепорт.");
                    Log("/village /swamp /forest /tower /ash — прыжок к зоне.");
                    break;
                default:
                    Log("Неизвестная команда: " + parts[0] + " — введите help.");
                    break;
            }
        }

        private void Log(string msg)
        {
            _log.Add(msg);
            if (_log.Count > 160)
            {
                _log.RemoveAt(0);
            }

            _scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            if (!_open)
            {
                return;
            }

            var w = Screen.width;
            var panelH = Mathf.Min(300f, Screen.height * 0.5f);

            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(0f, 0f, w, panelH + 30f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, richText = false };
            _scroll = GUI.BeginScrollView(new Rect(8f, 6f, w - 16f, panelH - 6f), _scroll, new Rect(0f, 0f, w - 44f, _log.Count * 18f + 8f));
            for (var i = 0; i < _log.Count; i++)
            {
                GUI.Label(new Rect(2f, i * 18f, w - 48f, 18f), _log[i], labelStyle);
            }

            GUI.EndScrollView();

            var e = Event.current;
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                if (!string.IsNullOrEmpty(_input))
                {
                    Execute(_input);
                    _input = string.Empty;
                }

                e.Use();
            }

            GUI.SetNextControlName("DevConsoleInput");
            _input = GUI.TextField(new Rect(8f, panelH + 4f, w - 16f, 22f), _input);
            _input = _input.Replace("`", string.Empty).Replace("~", string.Empty);
            GUI.FocusControl("DevConsoleInput");
        }
    }
}
