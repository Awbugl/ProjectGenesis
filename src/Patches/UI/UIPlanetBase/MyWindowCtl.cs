using UnityEngine;
using UnityEngine.UI;

namespace ProjectGenesis.Patches.UI.UIPlanetBase
{ 
    /// <summary>
    ///   special thanks to https://github.com/hetima/DSP_PlanetFinder/tree/main/PlanetFinder
    /// </summary>
    internal static class MyWindowCtl
    {
        internal static T CreateWindow<T>(string name, string title = "") where T : ManualBehaviour
        {
            var srcWin = UIRoot.instance.uiGame.tankWindow;
            var src = srcWin.gameObject;
            var go = Object.Instantiate(src, srcWin.transform.parent);
            go.name = name;
            go.SetActive(false);
            Object.Destroy(go.GetComponent<UITankWindow>());
            ManualBehaviour win = go.AddComponent<T>();
            //shadow 
            for (var i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                if (child.name == "panel-bg")
                {
                    var btn = child.GetComponentInChildren<Button>();
                    //close-btn
                    if (btn != null) btn.onClick.AddListener(win._Close);
                }
                else if (child.name != "shadow" && child.name != "panel-bg")
                {
                    Object.Destroy(child);
                }
            }

            SetTitle(win, title);

            win._Create();
            win._Init(win.data);
            return (T)win;
        }

        private static void SetTitle(ManualBehaviour win, string title)
        {
            var txt = GetTitleText(win);
            if (txt) txt.text = title;
        }

        public static Text GetTitleText(ManualBehaviour win) => win.gameObject.transform.Find("panel-bg/title-text")?.gameObject.GetComponent<Text>();

        public static RectTransform GetRectTransform(ManualBehaviour win) => win.GetComponent<RectTransform>();

        public static void OpenWindow(ManualBehaviour win)
        {
            win._Open();
            win.transform.SetAsLastSibling();
        }
    }
}
