using System;
using System.Linq;
using BepInEx.Configuration;
using ProjectGenesis.Patches.UI.Utils;
using ProjectGenesis.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.QTools
{
    // MyKeyBinder modified from LSTM: https://github.com/hetima/DSP_LSTM/blob/main/LSTM/MyKeyBinder.cs
    public class MyKeyBinder : MonoBehaviour
    {
        private static readonly KeyCode[] ModKeys =
        {
            KeyCode.RightShift, KeyCode.LeftShift, KeyCode.RightControl, KeyCode.LeftControl,
            KeyCode.RightAlt, KeyCode.LeftAlt, KeyCode.LeftCommand, KeyCode.LeftApple,
            KeyCode.LeftWindows, KeyCode.RightCommand, KeyCode.RightApple, KeyCode.RightWindows,
        };

        public Text functionText;
        public Text keyText;
        public InputField setTheKeyInput;
        public Toggle setTheKeyToggle;
        public RectTransform rectTrans;
        public UIButton inputUIButton;
        public Text conflictText;
        public Text waitingText;
        public UIButton setDefaultUIButton;
        public UIButton setNoneKeyUIButton;
        private ConfigEntry<KeyboardShortcut> _config;

        private KeyCode _lastKey;

        private bool _nextNotOn;

        public void Reset()
        {
            conflictText.gameObject.SetActive(false);
            waitingText.gameObject.SetActive(false);
            setDefaultUIButton.button.Select();
            _lastKey = KeyCode.None;
        }

        private void Update()
        {
            if (!setTheKeyToggle.isOn && inputUIButton.highlighted) setTheKeyToggle.isOn = true;

            if (!setTheKeyToggle.isOn) return;

            if (!inputUIButton._isPointerEnter && Input.GetKeyDown(KeyCode.Mouse0))
            {
                inputUIButton.highlighted = false;
                setTheKeyToggle.isOn = false;
                Reset();
            }
            else if (!inputUIButton.highlighted)
            {
                setTheKeyToggle.isOn = false;
                Reset();
            }
            else
            {
                waitingText.gameObject.SetActive(true);

                if (!TrySetValue()) return;

                setTheKeyToggle.isOn = false;
                inputUIButton.highlighted = false;
                Reset();
            }
        }

        internal static RectTransform CreateKeyBinder(float x, float y, RectTransform parent, ConfigEntry<KeyboardShortcut> config,
            string label = "", int fontSize = 18)
        {
            UIOptionWindow optionWindow = UIRoot.instance.optionWindow;
            UIKeyEntry uikeyEntry = Instantiate(optionWindow.entryPrefab);
            GameObject go;
            (go = uikeyEntry.gameObject).SetActive(true);
            go.name = "my-keybinder";
            MyKeyBinder kb = go.AddComponent<MyKeyBinder>();
            kb._config = config;

            kb.functionText = uikeyEntry.functionText;
            kb.keyText = uikeyEntry.keyText;
            kb.setTheKeyInput = uikeyEntry.setTheKeyInput;
            kb.setTheKeyToggle = uikeyEntry.setTheKeyToggle;
            kb.rectTrans = uikeyEntry.rectTrans;
            kb.inputUIButton = uikeyEntry.inputUIButton;
            kb.conflictText = uikeyEntry.conflictText;
            kb.waitingText = uikeyEntry.waitingText;
            kb.setDefaultUIButton = uikeyEntry.setDefaultUIButton;
            kb.setNoneKeyUIButton = uikeyEntry.setNoneKeyUIButton;
            kb.setNoneKeyUIButton.gameObject.SetActive(false);

            kb.functionText.text = label.TranslateFromJson();
            kb.functionText.fontSize = fontSize;
            kb.keyText.fontSize = fontSize;

            Destroy(kb.setDefaultUIButton.gameObject.GetComponentInChildren<Localizer>());
            kb.setDefaultUIButton.gameObject.GetComponentInChildren<Text>().text = "恢复默认".TranslateFromJson();

            ((RectTransform)kb.keyText.transform).anchoredPosition = new Vector2(170f, 0f);
            ((RectTransform)kb.inputUIButton.transform.parent.transform).anchoredPosition = new Vector2(330f, 0f);
            ((RectTransform)kb.setDefaultUIButton.transform).anchoredPosition = new Vector2(470f, 0f);
            ((RectTransform)kb.setNoneKeyUIButton.transform).anchoredPosition = new Vector2(570f, 0f);

            RectTransform rect = Util.NormalizeRectWithTopLeft(kb, x, y, parent);
            kb.rectTrans = rect;

            Destroy(uikeyEntry);

            kb.SettingChanged();
            config.SettingChanged += (obj, args) => { kb.SettingChanged(); };
            kb.inputUIButton.onClick += kb.OnInputUIButtonClick;
            kb.setDefaultUIButton.onClick += kb.OnSetDefaultKeyClick;

            return go.transform as RectTransform;
        }

        private bool TrySetValue()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                VFInput.UseEscape();

                return true;
            }

            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1)) return true;

            bool anyKey = GetInputKeys();

            if (anyKey || _lastKey == KeyCode.None) return false;

            string k = GetPressedKey();

            if (string.IsNullOrEmpty(k)) return false;

            _lastKey = KeyCode.None;

            _config.Value = KeyboardShortcut.Deserialize(k);

            return true;
        }

        private string GetPressedKey()
        {
            var key = _lastKey.ToString();

            if (string.IsNullOrEmpty(key)) return null;

            string mod = ModKeys.Where(Input.GetKey).Aggregate("", (current, modKey) => current + ("+" + modKey));

            if (!string.IsNullOrEmpty(mod)) key += mod;

            return key;
        }

        private bool GetInputKeys()
        {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (KeyCode item in Enum.GetValues(typeof(KeyCode)))
            {
                if (item == KeyCode.None || ModKeys.Contains(item) || !Input.GetKey(item)) continue;

                _lastKey = item;

                return true;
            }

            return false;
        }

        public void OnInputUIButtonClick(int data)
        {
            inputUIButton.highlighted = true;

            if (!_nextNotOn) return;

            _nextNotOn = false;
            inputUIButton.highlighted = false;
            setTheKeyToggle.isOn = false;
            waitingText.gameObject.SetActive(false);
        }

        public void OnSetDefaultKeyClick(int data)
        {
            _config.Value = (KeyboardShortcut)_config.DefaultValue;
            keyText.text = _config.Value.Serialize();
        }

        public void OnSetNoneKeyClick(int data)
        {
            _config.Value = (KeyboardShortcut)_config.DefaultValue;
            keyText.text = _config.Value.Serialize();
        }

        public void SettingChanged() => keyText.text = _config.Value.Serialize();
    }
}
