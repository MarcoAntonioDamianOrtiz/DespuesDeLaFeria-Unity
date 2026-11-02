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
        public ParkSession park;
        public TMP_FontAsset font;
        public Sprite broomIcon, clothIcon, emblem;
        TextMeshProUGUI progress, prompt, notice, tool, menuTitle, practiceLabel, sessionLabel;
        GameObject menu, crosshair,marblePanel;
        TextMeshProUGUI marbleTitle,marbleResult,zoneTitle,startLabel;
        RectTransform marbleCursor;
        UnityEngine.UI.Image fade;
        UnityEngine.UI.Button travelButton;
        bool started;
        RectTransform uiRoot,zoneFill,contextFill,menuCard;
        CanvasGroup contextGroup;
        TextMeshProUGUI contextTitle,contextHint;
        TextMeshProUGUI broomName,clothName;
        UnityEngine.UI.Image broomSlot,clothSlot;
        Sprite rounded;
        Texture2D roundedTexture;
        float nextObjectives,shownProgress;
        int lastTarget=-1;
        Rect lastSafe;
        Color ink = new Color(.13f, .22f, .24f);
        void Start()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if(!EventSystem.current)new GameObject("Input UI", typeof(EventSystem), typeof(InputSystemUIInputModule));
            MakeRounded();uiRoot=new GameObject("Zona segura",typeof(RectTransform)).GetComponent<RectTransform>();uiRoot.SetParent(transform,false);FitSafeArea();
            var top = Panel("Objetivos", uiRoot, new Vector2(360, 218), new Vector2(26, -26), new Vector2(0, 1), new Vector2(0, 1), new Color(.97f, .95f, .87f, .96f));
            zoneTitle=Label("Puesto", top, game.definition.displayName.ToUpperInvariant(), 19, ink, new Vector2(320, 48), new Vector2(20, -16), new Vector2(0, 1), new Vector2(0, 1));
            progress = Label("Tareas", top, "", 19, ink, new Vector2(320, 122), new Vector2(20, -65), new Vector2(0, 1), new Vector2(0, 1));
            var totalTrack=Panel("Progreso de zona",top,new Vector2(320,6),new Vector2(20,18),Vector2.zero,Vector2.zero,new Color(.80f,.81f,.72f));
            zoneFill=Panel("Avance",totalTrack,new Vector2(0,6),Vector2.zero,Vector2.zero,Vector2.zero,new Color(.17f,.43f,.40f));
            var lower = Panel("Herramientas", uiRoot, new Vector2(336, 70), new Vector2(0, 22), new Vector2(.5f, 0), new Vector2(.5f, 0), Color.clear);
            broomSlot=ToolSlot(lower,"Escoba",broomIcon,0,0);clothSlot=ToolSlot(lower,"Paño",clothIcon,1,174);
            tool = Label("Herramienta", lower, "", 18, Color.white, new Vector2(650, 40), new Vector2(168, 75), new Vector2(0, 0), new Vector2(.5f, 0));
            tool.alignment = TextAlignmentOptions.Midline;
            prompt = Label("Accion", uiRoot, "", 21, Color.white, new Vector2(880, 40), new Vector2(0, 103), new Vector2(.5f, 0), new Vector2(.5f, 0)); prompt.alignment = TextAlignmentOptions.Center;
            prompt.outlineWidth=.18f;prompt.outlineColor=new Color32(27,53,51,180);
            var context=Panel("Limpieza individual",uiRoot,new Vector2(286,82),new Vector2(0,-145),new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Color(.97f,.95f,.87f,.96f));contextGroup=context.gameObject.AddComponent<CanvasGroup>();contextGroup.blocksRaycasts=false;contextGroup.interactable=false;contextGroup.alpha=0;
            contextTitle=Label("Mancha",context,"",18,ink,new Vector2(250,25),new Vector2(18,-10),new Vector2(0,1),new Vector2(0,1));
            contextTitle.enableAutoSizing=true;contextTitle.fontSizeMin=12;contextTitle.fontSizeMax=18;contextTitle.overflowMode=TextOverflowModes.Ellipsis;
            var contextTrack=Panel("Barra",context,new Vector2(250,5),new Vector2(18,-42),new Vector2(0,1),new Vector2(0,1),new Color(.80f,.81f,.72f));
            contextFill=Panel("Avance",contextTrack,new Vector2(0,5),Vector2.zero,new Vector2(0,1),new Vector2(0,1),new Color(.17f,.43f,.40f));
            contextHint=Label("Herramienta requerida",context,"2 · Paño",15,ink,new Vector2(250,22),new Vector2(18,-53),new Vector2(0,1),new Vector2(0,1));
            notice = Label("Aviso", transform, "", 22, new Color(1, .91f, .65f), new Vector2(1000, 70), new Vector2(0, -35), new Vector2(.5f, 1), new Vector2(.5f, 1)); notice.alignment = TextAlignmentOptions.Center;
            var cross = Label("Punto", transform, "+", 23, new Color(1, 1, 1, .85f), new Vector2(22, 28), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f)); cross.alignment = TextAlignmentOptions.Center; crosshair = cross.gameObject;
            menu = Panel("Pausa", transform, new Vector2(1600, 900), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Color(.035f, .09f, .11f, .85f)).gameObject;
            var overlay = menu.GetComponent<RectTransform>(); overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one; overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            var card = Panel("Tarjeta", menu.transform, new Vector2(690, park?790:850), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Color(.97f, .94f, .85f));
            menuCard=card;
            Label("Antetitulo", card, "UNA MAÑANA PARA DEVOLVER EL COLOR", 18, new Color(.54f, .31f, .16f), new Vector2(610, 32), new Vector2(40, -35), new Vector2(0, 1), new Vector2(0, 1));
            menuTitle = Label("Titulo", card, "Después de la feria", 47, ink, new Vector2(610, 80), new Vector2(40, -78), new Vector2(0, 1), new Vector2(0, 1));
            Label("Descripcion", card, park?"Recorre los caminos y devuelve la luz a los puestos, a tu ritmo.":"Barre el confeti, recupera el puesto y vuelve a encender sus luces.", 23, ink, new Vector2(610, 74), new Vector2(40, -171), new Vector2(0, 1), new Vector2(0, 1));
            Label("Controles", card, "WASD  Caminar     Ratón  Mirar\nClic mantenido  Usar herramienta\n1  Escoba     2  Paño     E  Interactuar\nTab  Localizar tarea     F5  Guardar\n\nAcerca el carrito y barre hacia su recogedor amarillo.\nUsa el paño sobre las manchas del mostrador.", 21, ink, new Vector2(610, 235), new Vector2(40, -264), new Vector2(0, 1), new Vector2(0, 1));
            SettingSlider("Volumen", card, park?225:290, 0, 1, PlayerPrefs.GetFloat("feria.volume",.7f), game.SetVolume);
            SettingSlider("Sensibilidad", card, park?160:225, .035f, .18f, PlayerPrefs.GetFloat("feria.sensitivity",.095f), game.SetSensitivity);
            if(!park){Button("Visitar",card,"Visitar "+game.NextZoneName,new Vector2(40,155),game.Travel,610);
            travelButton=card.Find("Visitar").GetComponent<UnityEngine.UI.Button>();}
            Button("Comenzar", card, "Entrar · Enter", new Vector2(40, 78), () => { started = true; game.SetPaused(false); },190);
            Button("Guardar", card, "Guardar", new Vector2(250, 78), () => game.Save(),190);
            Button("Practica", card, "Limpieza libre", new Vector2(460, 78), game.TogglePractice,190);
            startLabel=card.Find("Comenzar/Texto").GetComponent<TextMeshProUGUI>();
            practiceLabel=card.Find("Practica/Texto").GetComponent<TextMeshProUGUI>();
            sessionLabel=Label("Estado", card, "Arte original · Guardado local automático", 16, new Color(.38f, .42f, .4f), new Vector2(610, 35), new Vector2(40, 22), new Vector2(0, 0), new Vector2(0, 0));
            var marbleCard=Panel("Canicas",transform,new Vector2(540,126),new Vector2(0,-20),new Vector2(.5f,1),new Vector2(.5f,1),new Color(.97f,.94f,.85f,.97f));marblePanel=marbleCard.gameObject;
            marbleTitle=Label("Turno",marbleCard,"APUNTA AL CENTRO",21,ink,new Vector2(500,32),new Vector2(20,-10),new Vector2(0,1),new Vector2(0,1));marbleTitle.alignment=TextAlignmentOptions.Center;
            var meter=Panel("Medidor",marbleCard,new Vector2(440,16),new Vector2(50,-53),new Vector2(0,1),new Vector2(0,1),new Color(.28f,.5f,.5f));
            Panel("Zona de 50",meter,new Vector2(322,16),Vector2.zero,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Color(.86f,.51f,.40f));
            Panel("Zona de 100",meter,new Vector2(102,16),Vector2.zero,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Color(.94f,.74f,.30f));
            marbleCursor=Panel("Aguja",meter,new Vector2(5,30),Vector2.zero,new Vector2(0,.5f),new Vector2(.5f,.5f),ink);
            marbleResult=Label("Resultado",marbleCard,"",18,ink,new Vector2(510,35),new Vector2(15,-82),new Vector2(0,1),new Vector2(0,1));marbleResult.alignment=TextAlignmentOptions.Center;marblePanel.SetActive(false);
            Icon("Emblema",card,emblem,new Vector2(573,-78),new Vector2(74,74),new Vector2(0,1),new Vector2(0,1));
            var curtain=Panel("Transición",transform,new Vector2(1600,900),Vector2.zero,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Color(.96f,.91f,.78f,1));curtain.anchorMin=Vector2.zero;curtain.anchorMax=Vector2.one;curtain.offsetMin=curtain.offsetMax=Vector2.zero;fade=curtain.GetComponent<UnityEngine.UI.Image>();
            // Existing pause and minigame cards share the same Canvas and safe area.
            var remaining=new System.Collections.Generic.List<Transform>();foreach(Transform child in transform)if(child!=uiRoot)remaining.Add(child);foreach(var child in remaining)child.SetParent(uiRoot,false);
        }
        void Update()
        {
            if (menu == null) return;
            if(lastSafe!=Screen.safeArea)FitSafeArea();
            menuCard.localScale=Vector3.one*Mathf.Min(1,Mathf.Min(uiRoot.rect.width/730,uiRoot.rect.height/(park?830:890)));
            if(park)game=park.CurrentGame;
            float fadeAmount=park?0:game.Fade;fade.color=new Color(.96f,.91f,.78f,fadeAmount);fade.raycastTarget=game.ChangingZones;fade.gameObject.SetActive(fadeAmount>0);if(travelButton)travelButton.interactable=!game.ChangingZones;
            if(!game.Paused) started=true;
            menu.SetActive(game.Paused); crosshair.SetActive(!game.Paused&&!game.MinigameActive);
            startLabel.text=started?"Continuar · Enter":"Comenzar · Enter";
            menuTitle.text = started ? "Una pequeña pausa" : "Después de la feria";
            practiceLabel.text=game.InPractice?"Volver a partida":"Limpieza libre";
            sessionLabel.text=game.InPractice?"LIMPIEZA LIBRE · Tu partida se conserva":"VERSIÓN "+Application.version+" · Arte original · Guardado local";
            if(park){sessionLabel.text=park.InPractice?"LIMPIEZA LIBRE · Tu partida se conserva":$"PARQUE · {park.RestoredCount}/{park.zones.Length} puestos recuperados · Guardado automático";zoneTitle.text=park.District.ToUpperInvariant();}
            if(Time.unscaledTime>=nextObjectives){nextObjectives=Time.unscaledTime+.15f;RefreshObjectives();}
            ShowCleaningTarget();
            Color activeSlot=new Color(.13f,.36f,.35f,.98f),idleSlot=new Color(.97f,.95f,.87f,.95f);
            broomSlot.color=Color.Lerp(broomSlot.color,game.Tool==0?activeSlot:idleSlot,Time.unscaledDeltaTime*16);clothSlot.color=Color.Lerp(clothSlot.color,game.Tool==1?activeSlot:idleSlot,Time.unscaledDeltaTime*16);
            broomName.color=game.Tool==0?Color.white:ink;clothName.color=game.Tool==1?Color.white:ink;
            broomSlot.transform.localScale=Vector3.Lerp(broomSlot.transform.localScale,Vector3.one*(game.Tool==0?1.035f:1),Time.unscaledDeltaTime*14);clothSlot.transform.localScale=Vector3.Lerp(clothSlot.transform.localScale,Vector3.one*(game.Tool==1?1.035f:1),Time.unscaledDeltaTime*14);
            tool.text = game.MovingCart ? "E · Estacionar carrito" : "";
            if(game.MinigameActive)tool.text="CANICAS   ·   E Lanzar / Repetir   ·   Esc Volver";
            if(game.MinigameActive&&game.IsDucks)tool.text="PATITOS · Ratón Apuntar · Clic Lanzar · Esc Volver";
            marblePanel.SetActive(game.MinigameActive&&!game.Paused);
            if(game.MinigameActive&&!game.IsDucks)
            {
                var toy=game.Marble;bool aiming=toy.State==MarbleGame.Phase.Aiming;
                marbleCursor.parent.gameObject.SetActive(aiming);marbleCursor.anchoredPosition=new Vector2(toy.Meter*440,0);
                marbleTitle.text=aiming?"E · LANZA EN LA FRANJA DORADA":toy.State==MarbleGame.Phase.Rolling?"ESCUCHA CÓMO BAJA…":toy.Score==100?"¡JUSTO EN EL CENTRO!":"¡UNA MÁS!";
                marbleResult.text=aiming?"Centro 100   ·   Cerca 50   ·   Laterales 10":toy.State==MarbleGame.Phase.Rolling?"Cada curva tiene su nota":$"{toy.Score} puntos   ·   Intentos: {toy.Attempts}   ·   Mejor local: {game.MarbleBest}";
            }
            if(game.MinigameActive&&game.IsDucks)
            {
                var toy=game.Ducks;marbleCursor.parent.gameObject.SetActive(false);
                marbleTitle.text=toy.State==DuckGame.Phase.Result?"¡OTRA VUELTA AL CANAL!":toy.State==DuckGame.Phase.Throwing?(toy.HasLanded?(toy.LastScore>0?"¡ARO DENTRO!":"¡CASI! PRUEBA OTRA VEZ"):"¡AHÍ VA EL ARO!"):"GORRO 100 · OTROS 50 · SIN PRISA";
                string record=game.InPractice?"Práctica":$"Mejor: {game.DuckBest}";
                marbleResult.text=$"Aros {toy.Throws}/3   ·   Puntos {toy.Score}   ·   {record}";
            }
            prompt.text = game.Paused ? "" : game.Prompt(); notice.text = game.MinigameActive?"":game.Notice;
        }
        RectTransform Panel(string name, Transform parent, Vector2 size, Vector2 position, Vector2 anchor, Vector2 pivot, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image)); var rt = go.GetComponent<RectTransform>(); rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot; rt.sizeDelta = size; rt.anchoredPosition = position;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;if(rounded){image.sprite=rounded;image.type=UnityEngine.UI.Image.Type.Sliced;}return rt;
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
            rt.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var label = Label("Texto", rt, text, 20, Color.white, new Vector2(width, 53), Vector2.zero, new Vector2(.5f, .5f), new Vector2(.5f, .5f)); label.alignment = TextAlignmentOptions.Center;
        }
        void Icon(string name,Transform parent,Sprite sprite,Vector2 position,Vector2 size,Vector2 anchor,Vector2 pivot)
        {
            if(!sprite) return;
            var rt=Panel(name,parent,size,position,anchor,pivot,Color.white); var img=rt.GetComponent<UnityEngine.UI.Image>(); img.sprite=sprite;img.type=UnityEngine.UI.Image.Type.Simple; img.preserveAspect=true; img.raycastTarget=false;
        }
        void SettingSlider(string name,Transform parent,float y,float min,float max,float value,UnityEngine.Events.UnityAction<float> changed)
        {
            Label(name+" etiqueta",parent,name,20,ink,new Vector2(170,35),new Vector2(40,y+12),Vector2.zero,Vector2.zero);
            var track=Panel(name,parent,new Vector2(390,10),new Vector2(250,y+25),Vector2.zero,new Vector2(0,.5f),new Color(.75f,.76f,.69f));
            var slider=track.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.minValue=min; slider.maxValue=max;
            track.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            var handle=Panel("Control",track,new Vector2(22,28),Vector2.zero,Vector2.zero,new Vector2(.5f,.5f),new Color(.13f,.4f,.39f));
            slider.handleRect=handle; slider.targetGraphic=handle.GetComponent<UnityEngine.UI.Image>(); slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;
            slider.SetValueWithoutNotify(value); slider.onValueChanged.AddListener(changed);
        }
        void FitSafeArea(){lastSafe=Screen.safeArea;uiRoot.anchorMin=new Vector2(lastSafe.xMin/Screen.width,lastSafe.yMin/Screen.height);uiRoot.anchorMax=new Vector2(lastSafe.xMax/Screen.width,lastSafe.yMax/Screen.height);uiRoot.offsetMin=uiRoot.offsetMax=Vector2.zero;}
        UnityEngine.UI.Image ToolSlot(Transform parent,string title,Sprite icon,int key,float x)
        {
            var slot=Panel(title,parent,new Vector2(162,66),new Vector2(x,0),Vector2.zero,Vector2.zero,new Color(.97f,.95f,.87f,.95f));
            Icon("Icono",slot,icon,new Vector2(12,0),new Vector2(40,42),new Vector2(0,.5f),new Vector2(0,.5f));
            var label=Label("Nombre",slot,(key+1)+"  "+title,18,ink,new Vector2(102,40),new Vector2(57,0),new Vector2(0,.5f),new Vector2(0,.5f));label.alignment=TextAlignmentOptions.MidlineLeft;
            if(key==0)broomName=label;else clothName=label;return slot.GetComponent<UnityEngine.UI.Image>();
        }
        void RefreshObjectives()
        {
            int total=game.litter.Length,collected=game.Collected,stains=game.SurfaceTotal,clean=game.CleanedSurfaces;float surfaces=game.CleanProgress,signs=game.SignFixed?1:0;
            if(park&&!park.ActiveZone)
            {
                total=collected=stains=clean=0;surfaces=signs=0;foreach(var zone in park.zones){total+=zone.litter.Length;collected+=zone.Collected;stains+=zone.SurfaceTotal;clean+=zone.CleanedSurfaces;surfaces+=zone.CleanProgress;signs+=zone.SignFixed?1:0;}surfaces/=park.zones.Length;signs/=park.zones.Length;
                progress.text=$"Suelo   {collected} / {total}\n{total-collected} residuos restantes\nMostradores   {clean} / {stains}\nTab · Sigue las señales del paseo";
            }
            else progress.text=$"Suelo   {collected} / {total} · {total-collected} restantes\nMostrador   {clean} / {stains} manchas\n{stains-clean} manchas restantes\nCartel   {(game.SignFixed?"Restaurado":"Pendiente de restaurar")}";
            zoneFill.sizeDelta=new Vector2(320*((float)collected/Mathf.Max(1,total)+surfaces+signs)/3,6);
        }
        void ShowCleaningTarget()
        {
            bool completed=Time.unscaledTime-game.SurfaceCompletedAt<.8f;
            int target=completed?game.LastSurfaceCompleted:game.SurfaceTarget;
            bool show=target>=0&&!game.Paused&&!game.MinigameActive&&!game.MovingCart;
            contextGroup.alpha=Mathf.MoveTowards(contextGroup.alpha,show?1:0,Time.unscaledDeltaTime*7);
            if(!show)return;
            float value=completed?1:game.SurfaceProgress;if(target!=lastTarget){shownProgress=value;lastTarget=target;}else shownProgress=Mathf.Lerp(shownProgress,value,1-Mathf.Exp(-Time.unscaledDeltaTime*20));
            string label=game.SurfaceLabelFor(target);contextTitle.text=completed?label+" · ¡Limpia!":label+" · "+Mathf.RoundToInt(shownProgress*100)+"%";
            contextFill.sizeDelta=new Vector2(250*shownProgress,5);
            contextHint.text=completed?"El mostrador recupera su color":game.Tool==1?"Mantén clic · Frotar":"2 · Equipa el paño";
        }
        void MakeRounded()
        {
            roundedTexture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Esquinas del HUD",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var pixels=new Color[1024];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++){float dx=Mathf.Max(0,Mathf.Abs(x-15.5f)-7.5f),dy=Mathf.Max(0,Mathf.Abs(y-15.5f)-7.5f);pixels[y*32+x]=new Color(1,1,1,Mathf.Clamp01(8.5f-Mathf.Sqrt(dx*dx+dy*dy)));}
            roundedTexture.SetPixels(pixels);roundedTexture.Apply();rounded=Sprite.Create(roundedTexture,new Rect(0,0,32,32),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));
        }
        void OnDestroy(){if(rounded)Destroy(rounded);if(roundedTexture)Destroy(roundedTexture);}
    }
}
