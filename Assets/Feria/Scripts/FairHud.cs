using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Feria
{
    public sealed class FairHud : MonoBehaviour
    {
        public FairGame game;
        public TMP_FontAsset font;
        public Sprite broomIcon, clothIcon, emblem;
        TextMeshProUGUI progress, prompt, notice, tool, menuTitle, practiceLabel, sessionLabel;
        GameObject menu, crosshair;
        bool started;
        Color ink = new Color(.13f, .22f, .24f);
        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = 1f;
            gameObject.AddComponent<GraphicRaycaster>();
            var eventSystem = new GameObject("Input UI", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var top = Panel("Objetivos", transform, new Vector2(310, 116), new Vector2(28, -26), new Vector2(0, 1), new Vector2(0, 1), new Color(.96f, .94f, .85f, .94f));
            Label("Puesto", top, "EL RINCÓN DE LAS CANICAS", 18, ink, new Vector2(284, 26), new Vector2(14, -12), new Vector2(0, 1), new Vector2(0, 1));
            progress = Label("Tareas", top, "", 18, ink, new Vector2(284, 75), new Vector2(14, -42), new Vector2(0, 1), new Vector2(0, 1));
            var lower = Panel("Herramientas", transform, new Vector2(680, 58), new Vector2(0, 25), new Vector2(.5f, 0), new Vector2(.5f, 0), new Color(.08f, .16f, .18f, .9f));
            tool = Label("Herramienta", lower, "", 20, Color.white, new Vector2(650, 48), new Vector2(15, 0), new Vector2(0, .5f), new Vector2(0, .5f));
            tool.alignment = TextAlignmentOptions.Midline;
            prompt = Label("Accion", transform, "", 24, Color.white, new Vector2(950, 50), new Vector2(0, 110), new Vector2(.5f, 0), new Vector2(.5f, 0)); prompt.alignment = TextAlignmentOptions.Center;
            notice = Label("Aviso", transform, "", 22, new Color(1, .91f, .65f), new Vector2(1000, 70), new Vector2(0, -35), new Vector2(.5f, 1), new Vector2(.5f, 1)); notice.alignment = TextAlignmentOptions.Center;
            var cross = Label("Punto", transform, "+", 23, new Color(1, 1, 1, .85f), new Vector2(22, 28), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f)); cross.alignment = TextAlignmentOptions.Center; crosshair = cross.gameObject;
            menu = Panel("Pausa", transform, new Vector2(1600, 900), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Color(.035f, .09f, .11f, .85f)).gameObject;
            var overlay = menu.GetComponent<RectTransform>(); overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            var card = Panel("Tarjeta", menu.transform, new Vector2(690, 790), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Color(.97f, .94f, .85f));
            Label("Antetitulo", card, "UNA MAÑANA PARA DEVOLVER EL COLOR", 18, new Color(.54f, .31f, .16f), new Vector2(610, 32), new Vector2(40, -35), new Vector2(0, 1), new Vector2(0, 1));
            menuTitle = Label("Titulo", card, "Después de la feria", 47, ink, new Vector2(610, 80), new Vector2(40, -78), new Vector2(0, 1), new Vector2(0, 1));
            Label("Descripcion", card, "Barre el confeti, recupera el puesto y vuelve a encender sus luces.", 23, ink, new Vector2(610, 74), new Vector2(40, -171), new Vector2(0, 1), new Vector2(0, 1));
            Label("Controles", card, "WASD  Caminar     Ratón  Mirar\nClic mantenido  Usar herramienta\n1  Escoba     2  Paño     E  Interactuar\nTab  Localizar tarea     F5  Guardar\n\nAcerca el carrito y barre hacia su recogedor amarillo.\nUsa el paño sobre las manchas del mostrador.", 21, ink, new Vector2(610, 235), new Vector2(40, -264), new Vector2(0, 1), new Vector2(0, 1));
            SettingSlider("Volumen", card, 225, 0, 1, PlayerPrefs.GetFloat("feria.volume",.7f), game.SetVolume);
            SettingSlider("Sensibilidad", card, 160, .035f, .18f, PlayerPrefs.GetFloat("feria.sensitivity",.095f), game.SetSensitivity);
            Button("Comenzar", card, "Entrar · Enter", new Vector2(40, 78), () => { started = true; game.SetPaused(false); },190);
            Button("Guardar", card, "Guardar", new Vector2(250, 78), () => game.Save(),190);
            Button("Practica", card, "Limpieza libre", new Vector2(460, 78), game.TogglePractice,190);
            practiceLabel=card.Find("Practica/Texto").GetComponent<TextMeshProUGUI>();
            sessionLabel=Label("Estado", card, "VERSIÓN 0.3 · Arte original · Guardado local automático", 16, new Color(.38f, .42f, .4f), new Vector2(610, 35), new Vector2(40, 22), new Vector2(0, 0), new Vector2(0, 0));
            Icon("Emblema",card,emblem,new Vector2(573,-78),new Vector2(74,74),new Vector2(0,1),new Vector2(0,1));
            Icon("Icono escoba",lower,broomIcon,new Vector2(15,0),new Vector2(39,39),new Vector2(0,.5f),new Vector2(0,.5f));
            tool.rectTransform.anchoredPosition=new Vector2(50,0); tool.rectTransform.sizeDelta=new Vector2(610,48);
        }
        void Update()
        {
            if (menu == null) return;
            if(!game.Paused) started=true;
            menu.SetActive(game.Paused); crosshair.SetActive(!game.Paused);
            menuTitle.text = started ? "Una pequeña pausa" : "Después de la feria";
            practiceLabel.text=game.InPractice?"Volver a partida":"Limpieza libre";
            sessionLabel.text=game.InPractice?"LIMPIEZA LIBRE · Tu partida se conserva · "+game.Metrics.Summary:"VERSIÓN 0.3 · Arte original · Guardado local automático";
            int clean = Mathf.RoundToInt(game.CleanProgress * 100);
            progress.text = $"Suelo: {Mathf.RoundToInt(100f * game.Collected / game.litter.Length)}% despejado\nMostrador: {clean}% limpio\nCartel: {(game.SignFixed ? "colocado" : "por colocar")}";
            tool.text = game.MovingCart ? "MOVIENDO CARRITO   ·   E para estacionar" : (game.Tool == 0 ? "1  ESCOBA   ·   2  Paño   ·   Tab  Ayuda" : "1  Escoba   ·   2  PAÑO   ·   Tab  Ayuda");
            var icon=transform.Find("Herramientas/Icono escoba"); if(icon) icon.GetComponent<UnityEngine.UI.Image>().sprite=game.Tool==0?broomIcon:clothIcon;
            prompt.text = game.Paused ? "" : game.Prompt(); notice.text = game.Notice;
        }
        RectTransform Panel(string name, Transform parent, Vector2 size, Vector2 position, Vector2 anchor, Vector2 pivot, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot; rt.sizeDelta = size; rt.anchoredPosition = position; go.GetComponent<UnityEngine.UI.Image>().color = color; return rt;
        }
        TextMeshProUGUI Label(string name, Transform parent, string text, int size, Color color, Vector2 dimensions, Vector2 position, Vector2 anchor, Vector2 pivot)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot; rt.sizeDelta = dimensions; rt.anchoredPosition = position;
            var label = go.GetComponent<TextMeshProUGUI>(); label.font = font; label.text = text; label.fontSize = size; label.color = color; label.raycastTarget = false; label.overflowMode = TextOverflowModes.Overflow; return label;
        }
        void Button(string name, Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction action,float width=285)
        {
            var rt = Panel(name, parent, new Vector2(width, 53), position, Vector2.zero, Vector2.zero, new Color(.13f, .4f, .39f));
            var button = rt.gameObject.AddComponent<UnityEngine.UI.Button>(); button.onClick.AddListener(action);
            var label = Label("Texto", rt, text, 20, Color.white, new Vector2(width, 53), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f)); label.alignment = TextAlignmentOptions.Center;
        }
        void Icon(string name,Transform parent,Sprite sprite,Vector2 position,Vector2 size,Vector2 anchor,Vector2 pivot)
        {
            if(!sprite) return;
            var rt=Panel(name,parent,size,position,anchor,pivot,Color.white); var img=rt.GetComponent<UnityEngine.UI.Image>(); img.sprite=sprite; img.preserveAspect=true; img.raycastTarget=false;
        }
        void SettingSlider(string name,Transform parent,float y,float min,float max,float value,UnityEngine.Events.UnityAction<float> changed)
        {
            Label(name+" etiqueta",parent,name,20,ink,new Vector2(170,35),new Vector2(40,y+12),Vector2.zero,Vector2.zero);
            var track=Panel(name,parent,new Vector2(390,10),new Vector2(250,y+25),Vector2.zero,new Vector2(0,.5f),new Color(.75f,.76f,.69f));
            var slider=track.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.minValue=min; slider.maxValue=max;
            var handle=Panel("Control",track,new Vector2(22,28),Vector2.zero,Vector2.zero,new Vector2(.5f,.5f),new Color(.13f,.4f,.39f));
            slider.handleRect=handle; slider.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>(); slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.SetValueWithoutNotify(value); slider.onValueChanged.AddListener(changed);
        }
    }
}
