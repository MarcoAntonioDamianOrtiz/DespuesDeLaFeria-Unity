using System.Collections.Generic;
using System.IO;
using Feria;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class FairSceneBuilder
{
    static string Root = "Assets/Feria";
    static TMP_FontAsset font;
    static Dictionary<string, Material> mats = new Dictionary<string, Material>();
    static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
    [MenuItem("Feria/Crear primera zona")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Sal del modo de juego antes de crear una escena.");
        Directory.CreateDirectory(Root + "/Art"); Directory.CreateDirectory(Root + "/Scenes"); AssetDatabase.Refresh();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); mats.Clear();
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "/Art/FairFont.asset");
        if (!font)
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (!font) throw new System.Exception("Importa TMP Essential Resources antes de crear la escena.");
        }
        var cream = Mat("Marfil", "#F3E2BC"); var teal = Mat("Verde feria", "#397F80"); var coral = Mat("Coral", "#CE715B");
        var dark = Mat("Metal oscuro", "#304C50"); var wood = Mat("Madera", "#9D7356"); var yellow = Mat("Amarillo", "#F4C963");
        var floor = Mat("Pavimento", "#B8B0A1"); var rail = Mat("Barandilla", "#576F6D");
        Box("Explanada", new Vector3(0, -.16f, 0), new Vector3(14, .3f, 18), floor);
        // Modular pavement edges and inlaid carnival stripes give the cleaned floor a payoff.
        for (int x = -6; x <= 6; x++) for (int z = -8; z <= 8; z++)
        {
            Box("Junta", new Vector3(x, -.003f, z + .49f), new Vector3(.98f, .005f, .018f), cream, false);
        }
        for (int i = 0; i < 3; i++) Box("Camino de color", new Vector3(-1.5f + i * .24f, .007f, -1.1f), new Vector3(.14f, .005f, 11.5f), i == 0 ? coral : i == 1 ? cream : teal, false);
        for (int i = 0; i < 18; i++)
        {
            float a = i * Mathf.PI * 2 / 18; var tile = Box("Sol del pavimento", new Vector3(Mathf.Sin(a) * 1.2f + 2.8f, .009f, Mathf.Cos(a) * 1.2f - 2), new Vector3(.16f, .008f, .44f), yellow, false); tile.transform.rotation = Quaternion.Euler(0, a * Mathf.Rad2Deg, 0);
        }
        Fence(-6.8f, rail, wood); Fence(6.8f, rail, wood);
        for (int x = -6; x <= 6; x++) Box("Valla trasera", new Vector3(x, .55f, 8.6f), new Vector3(.1f, 1.1f, .13f), wood);
        Box("Travesaño", new Vector3(0, .8f, 8.6f), new Vector3(13.5f, .08f, .1f), rail);
        Booth(new Vector3(0, 0, 5.3f), teal, cream, coral, wood);
        // Smaller closed stalls establish a fair, without adding more playable jobs.
        Booth(new Vector3(-4.7f, 0, 7), coral, cream, teal, wood, .57f);
        Booth(new Vector3(4.7f, 0, 7), yellow, cream, teal, wood, .57f);
        Text("Rótulo", "EL RINCÓN\nDE LAS CANICAS", new Vector3(0, 3.02f, 3.80f), 5.3f, .75f, .37f, cream);
        Text("Lateral", "LA FERIA\nDESPIERTA", new Vector3(-4.7f, 1.65f, 6.16f), 2.7f, .7f, .25f, cream);
        Text("Lateral", "PRONTO\nMÁS COLOR", new Vector3(4.7f, 1.65f, 6.16f), 2.7f, .7f, .25f, dark);
        var game = new GameObject("Feria · Primera mañana").AddComponent<FairGame>();
        var cart = new GameObject("Carrito recogedor").transform; cart.position = new Vector3(2.7f, 0, -4.4f); game.cart = cart;
        ChildBox(cart, "Caja", new Vector3(0, .5f, 0), new Vector3(1.1f, .7f, .7f), teal);
        ChildBox(cart, "Borde", new Vector3(0, .87f, 0), new Vector3(1.2f, .08f, .8f), cream);
        ChildBox(cart, "Recogedor amarillo", new Vector3(0, .04f, .93f), new Vector3(1.35f, .07f, 1.05f), yellow, false);
        ChildBox(cart, "Asa", new Vector3(0, 1.05f, -.42f), new Vector3(1.05f, .07f, .07f), dark);
        for (int i = -1; i <= 1; i += 2) { ChildBox(cart, "Soporte asa", new Vector3(i * .48f, .85f, -.42f), new Vector3(.06f, .4f, .06f), dark); var wheel = Primitive("Rueda", PrimitiveType.Cylinder, cart.TransformPoint(new Vector3(i * .61f, .23f, 0)), new Vector3(.43f, .09f, .43f), dark, false); wheel.transform.rotation = Quaternion.Euler(0, 0, 90); wheel.transform.SetParent(cart); }
        Text("Carrito texto", "RECOGIDA", cart.position + new Vector3(0, .56f, -.36f), 1, .3f, .14f, cream).transform.SetParent(cart);
        var sign = Box("Cartel caído", new Vector3(-3.5f, .45f, 2.8f), new Vector3(.9f, .7f, .09f), yellow); sign.transform.rotation = Quaternion.Euler(0, -12, 65); game.sign = sign.transform;
        var signText = Text("Cartel", "ABIERTO", Vector3.zero, .9f, .4f, .12f, dark); signText.transform.SetParent(sign.transform, false); signText.transform.localPosition = new Vector3(0, 0, -.06f);
        var switchBox = Box("Interruptor del puesto", new Vector3(2.65f, 1.05f, 3.72f), new Vector3(.38f, .46f, .15f), dark); game.switchBox = switchBox.transform;
        Text("Interruptor texto", "ENCENDER", new Vector3(2.65f, 1.43f, 3.62f), .95f, .2f, .10f, dark);
        var lever = Box("Palanca", new Vector3(2.65f, 1.07f, 3.59f), new Vector3(.1f, .22f, .1f), coral);
        var patches = new List<Renderer>(); var grime = Mat("Manchas", "#706356");
        for (int x = 0; x < 8; x++) for (int z = 0; z < 2; z++) patches.Add(Box("Mancha mostrador", new Vector3(-1.4f + x * .4f, 1.135f, 3.85f + z * .35f), new Vector3(.34f, .008f, .32f), grime, false).GetComponent<Renderer>());
        game.dirtPatches = patches.ToArray();
        // A bounded tabletop marble track is the single reactivation reward.
        Box("Tablero canicas", new Vector3(0, 1.33f, 5.15f), new Vector3(2.55f, .15f, 2), wood);
        for (int row = 0; row < 6; row++)
        {
            var track = Box("Rampa de canicas", new Vector3(0, 1.58f - row * .027f, 5.8f - row * .24f), new Vector3(2.1f, .035f, .08f), row % 2 == 0 ? coral : yellow, false); track.transform.rotation = Quaternion.Euler(0, 0, row % 2 == 0 ? -2 : 2);
        }
        game.marble = Primitive("Canica · E para soltar", PrimitiveType.Sphere, new Vector3(0, 1.72f, 5.8f), Vector3.one * .17f, Mat("Canica", "#4CA5BC"), false).transform;
        var bulbs = new List<Renderer>(); var lights = new List<Light>(); var bulbOff = Mat("Bombilla apagada", "#9C8F72"); game.litBulb = Mat("Bombilla encendida", "#FFE5A0", true);
        for (int i = 0; i < 13; i++) bulbs.Add(Primitive("Bombilla puesto", PrimitiveType.Sphere, new Vector3(-2.7f + i * .45f, 2.57f, 3.73f), Vector3.one * .13f, bulbOff, false).GetComponent<Renderer>());
        for (int i = -1; i <= 1; i += 2) { var light = new GameObject("Luz cálida").AddComponent<Light>(); light.type = LightType.Point; light.color = C("#FFD08C"); light.range = 5; light.intensity = .15f; light.transform.position = new Vector3(i * 2, 2.4f, 3.3f); lights.Add(light); }
        game.bulbs = bulbs.ToArray(); game.fairLights = lights.ToArray();
        // Strings of bunting and lamps frame the compact playable courtyard.
        for (int i = 0; i < 22; i++)
        {
            float x = -6 + i * .57f; float y = 3.7f - Mathf.Sin((i / 21f) * Mathf.PI) * .5f;
            var flag = Box("Banderín", new Vector3(x, y, -.8f), new Vector3(.28f, .35f, .015f), i % 3 == 0 ? teal : i % 3 == 1 ? coral : yellow, false); flag.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 8 : -8);
        }
        for (int i = -1; i <= 1; i += 2) { Box("Poste guirnalda", new Vector3(i * 6, 1.95f, -.8f), new Vector3(.12f, 3.9f, .12f), wood); Bench(new Vector3(i * 5.8f, 0, .8f), wood, dark); }
        var trash = new GameObject("Residuos · posiciones persistentes").transform;
        var debris = new List<Transform>(); var colors = new[] { coral, teal, yellow, cream, Mat("Papel rosa", "#D697A5"), Mat("Papel azul", "#8BA6C0") };
        var random = new System.Random(4821);
        for (int i = 0; i < 720; i++)
        {
            float x = (float)random.NextDouble() * 10 - 5; float z = (float)random.NextDouble() * 9 - 6;
            float width = .055f + (float)random.NextDouble() * .08f;
            bool cup = i % 38 == 0;
            var piece = Primitive(cup ? "Vaso " + i : "Confeti " + i, cup ? PrimitiveType.Cylinder : PrimitiveType.Cube, new Vector3(x, cup ? .09f : .026f + (i % 3) * .003f, z), cup ? new Vector3(.12f, .085f, .12f) : new Vector3(width, .008f, width * 1.9f), colors[i % colors.Length], false);
            piece.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360, 0); piece.transform.SetParent(trash); debris.Add(piece.transform);
        }
        game.litter = debris.ToArray();
        var playerObject = new GameObject("Jugador"); playerObject.transform.position = new Vector3(0, .05f, -7.7f);
        var controller = playerObject.AddComponent<CharacterController>(); controller.height = 1.8f; controller.radius = .28f; controller.center = new Vector3(0, .9f, 0); controller.stepOffset = .25f;
        var player = playerObject.AddComponent<FairPlayer>(); game.player = player; player.game = game;
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData)); cameraObject.tag = "MainCamera"; cameraObject.transform.SetParent(playerObject.transform, false); cameraObject.transform.localPosition = new Vector3(0, 1.65f, 0); cameraObject.transform.localRotation = Quaternion.Euler(25, 0, 0);
        var camera = cameraObject.GetComponent<Camera>(); camera.fieldOfView = 72; camera.nearClipPlane = .05f; camera.farClipPlane = 100; camera.backgroundColor = C("#E4B5A0"); camera.clearFlags = CameraClearFlags.SolidColor; player.view = camera;
        var camData = camera.GetUniversalAdditionalCameraData(); camData.renderPostProcessing = true; camData.volumeLayerMask = 1;
        var broom = new GameObject("Escoba visible").transform; broom.SetParent(camera.transform, false); broom.localPosition = new Vector3(.45f, -.52f, .75f); player.broom = broom;
        var handle = ChildBox(broom, "Mango", new Vector3(0, -.03f, .25f), new Vector3(.035f, .035f, 1.1f), wood, false); handle.transform.localRotation = Quaternion.Euler(-32, 0, 0);
        ChildBox(broom, "Cabezal", new Vector3(0, -.33f, .74f), new Vector3(.5f, .11f, .16f), teal, false);
        for (int i = 0; i < 12; i++) ChildBox(broom, "Cerdas", new Vector3(-.22f + i * .04f, -.42f, .74f), new Vector3(.024f, .13f, .14f), yellow, false);
        var ui = new GameObject("Interfaz").AddComponent<FairHud>(); ui.game = game; ui.font = font; game.hud = ui;
        var sun = new GameObject("Sol de amanecer").AddComponent<Light>(); sun.type = LightType.Directional; sun.color = C("#FFE0BA"); sun.intensity = 1.65f; sun.shadows = LightShadows.Soft; sun.transform.rotation = Quaternion.Euler(27, -35, 0);
        RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = C("#B9C9DB"); RenderSettings.ambientEquatorColor = C("#D6C6B0"); RenderSettings.ambientGroundColor = C("#8B837C"); RenderSettings.fog = true; RenderSettings.fogColor = C("#E4B5A0"); RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 25; RenderSettings.fogEndDistance = 70;
        var profilePath = Root + "/Art/Amanecer.asset"; var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        if (!profile) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, profilePath); }
        if (!profile.TryGet<Tonemapping>(out var tone)) { tone = profile.Add<Tonemapping>(true); AssetDatabase.AddObjectToAsset(tone, profile); } tone.mode.Override(TonemappingMode.ACES);
        if (!profile.TryGet<Bloom>(out var bloom)) { bloom = profile.Add<Bloom>(true); AssetDatabase.AddObjectToAsset(bloom, profile); } bloom.intensity.Override(.5f); bloom.threshold.Override(.9f);
        if (!profile.TryGet<Vignette>(out var vignette)) { vignette = profile.Add<Vignette>(true); AssetDatabase.AddObjectToAsset(vignette, profile); } vignette.intensity.Override(.2f);
        var volume = new GameObject("Ambiente de amanecer").AddComponent<Volume>(); volume.isGlobal = true; volume.sharedProfile = profile;
        var urp = UniversalRenderPipeline.asset; if (!urp) throw new System.Exception("Se necesita URP."); urp.supportsHDR = true; EditorUtility.SetDirty(urp);
        QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1);
        PlayerSettings.companyName = "FeriaIndie"; PlayerSettings.productName = "Despues de la feria"; PlayerSettings.runInBackground = true;
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Root + "/Scenes/PrimeraManana.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "/Scenes/PrimeraManana.unity", true) };
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = playerObject;
        Debug.Log("FERIA_READY: 720 residuos, carrito, mostrador, cartel y canicas.");
    }
    static Material Mat(string name, string color, bool emission = false)
    {
        if (mats.TryGetValue(name, out var existing)) return existing;
        string path = Root + "/Art/" + name + ".mat"; var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, path); }
        material.color = C(color); material.SetFloat("_Smoothness", .15f); material.enableInstancing = true;
        if (emission) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", C(color) * 2.4f); }
        mats[name] = material; return material;
    }
    static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material, bool collider = true)
    {
        var go = GameObject.CreatePrimitive(type); go.name = name; go.transform.position = position; go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
    }
    static GameObject Box(string name, Vector3 position, Vector3 scale, Material material, bool collider = true) => Primitive(name, PrimitiveType.Cube, position, scale, material, collider);
    static GameObject ChildBox(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool collider = true)
    {
        var go = Box(name, Vector3.zero, scale, material, collider); go.transform.SetParent(parent, false); go.transform.localPosition = position; return go;
    }
    static TextMeshPro Text(string name, string text, Vector3 position, float width, float height, float size, Material material)
    {
        var go = new GameObject(name); go.transform.position = position; go.transform.rotation = Quaternion.identity;
        var label = go.AddComponent<TextMeshPro>(); label.font = font; label.text = text; label.fontSize = size * 10; label.color = material.color; label.alignment = TextAlignmentOptions.Center; label.rectTransform.sizeDelta = new Vector2(width, height); return label;
    }
    static void Booth(Vector3 center, Material paint, Material cream, Material accent, Material wood, float scale = 1)
    {
        var root = new GameObject("Puesto").transform; root.position = center;
        ChildBox(root, "Fondo", new Vector3(0, 1.3f, .8f), new Vector3(5.8f, 2.6f, .12f), paint);
        ChildBox(root, "Mostrador", new Vector3(0, 1.05f, -1.2f), new Vector3(5.8f, .15f, .9f), cream);
        ChildBox(root, "Frente", new Vector3(0, .5f, -1.4f), new Vector3(5.8f, 1, .12f), paint);
        for (int i = -1; i <= 1; i += 2) ChildBox(root, "Pilar", new Vector3(i * 2.85f, 1.6f, -1.35f), new Vector3(.15f, 3.2f, .15f), wood);
        ChildBox(root, "Rótulo fondo", new Vector3(0, 3, -1.35f), new Vector3(5.9f, .8f, .18f), paint);
        for (int i = 0; i < 12; i++) { var stripe = ChildBox(root, "Toldo", new Vector3(-2.75f + i * .5f, 2.7f, -.2f), new Vector3(.5f, .09f, 2.6f), i % 2 == 0 ? cream : accent); stripe.transform.localRotation = Quaternion.Euler(-8, 0, 0); }
        root.localScale = Vector3.one * scale;
    }
    static void Fence(float x, Material rail, Material wood)
    {
        for (int z = -8; z <= 8; z++) Box("Valla", new Vector3(x, .6f, z), new Vector3(.12f, 1.2f, .1f), wood);
        Box("Pasamanos", new Vector3(x, .9f, 0), new Vector3(.1f, .09f, 17), rail);
    }
    static void Bench(Vector3 p, Material wood, Material metal)
    {
        Box("Banco", p + new Vector3(0, .5f, 0), new Vector3(.6f, .12f, 2), wood);
        for (int i = -1; i <= 1; i += 2) Box("Pata banco", p + new Vector3(0, .25f, i * .7f), new Vector3(.4f, .5f, .1f), metal);
    }
}
