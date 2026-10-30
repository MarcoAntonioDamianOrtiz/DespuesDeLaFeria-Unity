using System;
using System.IO;
using UnityEngine;

namespace Feria
{
    [Serializable] public class FairSave
    {
        public int version = 1;
        public Vector3[] litter;
        public bool[] removed;
        public float[] dirt;
        public Vector3 cart;
        public float cartYaw;
        public Vector3 player;
        public float playerYaw;
        public bool signFixed, restored;
        public float playSeconds;
    }
    public sealed class FairGame : MonoBehaviour
    {
        public FairPlayer player;
        public Transform cart, sign, switchBox, marble, broomHead;
        public Transform[] litter;
        public Renderer[] dirtPatches;
        public Light[] fairLights;
        public Renderer[] bulbs;
        public Material litBulb;
        public Material feedbackMaterial;
        public FairHud hud;
        public bool Paused { get; private set; } = true;
        public bool MovingCart { get; private set; }
        public int Tool { get; set; }
        public bool SignFixed { get; private set; }
        public bool Restored { get; private set; }
        public float PlaySeconds { get; private set; }
        public int Collected { get; private set; }
        public int SweepContacts { get; private set; }
        public bool suppressSave;
        public string Notice { get; private set; } = "";
        public string SavePath => Path.Combine(Application.persistentDataPath, "feria-slot-1.json");
        public bool FloorDone => Collected >= litter.Length;
        public bool CounterDone { get { foreach (var d in dirt) if (d < 1) return false; return true; } }
        public float CleanProgress { get { float sum = 0; foreach (var d in dirt) sum += d; return dirt.Length == 0 ? 1 : sum / dirt.Length; } }
        bool[] removed;
        Vector3[] velocity;
        float[] dirt;
        Vector3 lastSweep;
        bool hasSweep;
        float autoSave, noticeTime, marbleTime = -1;
        Vector3 marbleStart;
        AudioSource brushAudio, fxAudio;
        AudioClip pickupSound, bellSound;
        Material unlitBulb;
        Vector3 signStart;
        Quaternion signRotation;
        LineRenderer sweepRing;
        TMPro.TextMeshPro hintMarker;
        float hintTime;
        void Awake()
        {
            removed = new bool[litter.Length]; velocity = new Vector3[litter.Length]; dirt = new float[dirtPatches.Length];
            marbleStart = marble.position; signStart = sign.position; signRotation = sign.rotation;
            if (bulbs.Length > 0) unlitBulb = bulbs[0].sharedMaterial;
            AudioListener.volume = PlayerPrefs.GetFloat("feria.volume", .7f);
            brushAudio = gameObject.AddComponent<AudioSource>(); brushAudio.loop = true; brushAudio.volume = 0;
            brushAudio.clip = MakeSound("Escoba", .55f, 0, true); brushAudio.Play();
            fxAudio = gameObject.AddComponent<AudioSource>(); fxAudio.volume = .22f;
            pickupSound = MakeSound("Papel recogido", .09f, 660, false);
            bellSound = MakeSound("Campanilla", .9f, 1046, false);
            Load(); SetPaused(true);
            CreateFeedback();
        }
        void Update()
        {
            if (noticeTime > 0) { noticeTime -= Time.unscaledDeltaTime; if (noticeTime <= 0) Notice = ""; }
            UpdateFeedback();
            if (Paused) return;
            float dt = Mathf.Min(Time.deltaTime, .05f);
            PlaySeconds += Time.deltaTime;
            if (MovingCart)
            {
                Vector3 desired = player.transform.position + player.transform.forward * 1.65f;
                desired.y = 0;
                desired.x = Mathf.Clamp(desired.x, -5.9f, 5.9f); desired.z = Mathf.Clamp(desired.z, -7.4f, 3.6f);
                cart.position = Vector3.Lerp(cart.position, desired, dt * 10);
                cart.rotation = Quaternion.Slerp(cart.rotation, Quaternion.Euler(0, player.transform.eulerAngles.y + 180, 0), dt * 10);
            }
            Vector3 mouth = cart.position + cart.forward * .95f;
            int gained = 0;
            for (int i = 0; i < litter.Length; i++)
            {
                if (removed[i]) continue;
                var p = litter[i].position;
                if (velocity[i].sqrMagnitude > .00005f)
                {
                    p += velocity[i] * dt;
                    p.x = Mathf.Clamp(p.x, -5.8f, 5.8f); p.z = Mathf.Clamp(p.z, -7.1f, 3.4f);
                    litter[i].position = p;
                    velocity[i] *= Mathf.Exp(-7 * dt);
                }
                if (FlatDistance(p, mouth) < .8f) { RemoveLitter(i); gained++; }
            }
            if (gained > 0) fxAudio.PlayOneShot(pickupSound, Mathf.Min(.7f, .12f + gained * .03f));
            // Finish tiny remnants only after the player has cleared 97% of the floor.
            if (Collected >= Mathf.CeilToInt(litter.Length * .97f) && !FloorDone)
            {
                for (int i = 0; i < litter.Length; i++) if (!removed[i]) RemoveLitter(i);
                Tell("¡Explanada despejada! Los últimos papelitos ya están recogidos.");
            }
            if (marbleTime >= 0)
            {
                marbleTime += dt;
                float t = Mathf.Clamp01(marbleTime / 7);
                float z = Mathf.Lerp(marbleStart.z, marbleStart.z - 1.35f, t);
                float x = marbleStart.x + Mathf.Sin(t * Mathf.PI * 6) * .95f;
                marble.position = new Vector3(x, marbleStart.y - t * .15f + Mathf.Sin(t * Mathf.PI * 12) * .025f, z);
                marble.Rotate(85 * dt, 0, 120 * dt);
                if (t >= 1) { marbleTime = -1; fxAudio.PlayOneShot(bellSound); }
            }
            autoSave += Time.deltaTime;
            if (autoSave > 25) { autoSave = 0; Save(false); }
            brushAudio.volume = Mathf.MoveTowards(brushAudio.volume, 0, dt * .2f);
        }
        public void UseTool(bool working)
        {
            if (!working || Paused) { hasSweep = false; return; }
            var ray = player.view.ViewportPointToRay(new Vector3(.5f, .5f));
            if (Tool == 0)
            {
                if (!TrySweepPoint(out Vector3 point)) { hasSweep = false; return; }
                Sweep(point, hasSweep ? point - lastSweep : player.transform.forward * .02f, Time.deltaTime);
                lastSweep = point; hasSweep = true;
            }
            else if (Physics.Raycast(ray, out RaycastHit hit, 3.5f))
            {
                CleanAt(hit.point, Time.deltaTime);
            }
        }
        public int Sweep(Vector3 point, Vector3 movement, float dt)
        {
            if (MovingCart) return 0;
            Vector3 direction = movement; direction.y = 0;
            if (direction.sqrMagnitude < .0001f) direction = player.transform.forward;
            direction.y = 0; direction.Normalize();
            int contacts = 0;
            float strength = 3.4f + Mathf.Min(movement.magnitude / Mathf.Max(dt, .001f), 5);
            for (int i = 0; i < litter.Length; i++)
            {
                if (removed[i]) continue;
                float d = FlatDistance(litter[i].position, point);
                if (d > .9f) continue;
                Vector3 side = litter[i].position - point; side.y = 0;
                velocity[i] += (direction * strength + side * .45f) * (1 - d / 1.05f) * Mathf.Min(dt, .05f) * 17;
                velocity[i] = Vector3.ClampMagnitude(velocity[i], 4.5f); contacts++;
            }
            if (contacts > 0) { SweepContacts += contacts; brushAudio.volume = .13f; }
            return contacts;
        }
        public int CleanAt(Vector3 point, float dt)
        {
            int count = 0;
            for (int i = 0; i < dirt.Length; i++)
            {
                if (dirt[i] >= 1 || Vector3.Distance(dirtPatches[i].transform.position, point) > .48f) continue;
                dirt[i] = Mathf.Clamp01(dirt[i] + dt * .9f); UpdateDirt(i); count++; brushAudio.volume = .09f;
            }
            return count;
        }
        void UpdateDirt(int i)
        {
            dirtPatches[i].gameObject.SetActive(dirt[i] < 1);
            dirtPatches[i].transform.localScale = new Vector3(.34f * (1 - dirt[i]), .008f, .32f * (1 - dirt[i]));
        }
        void RemoveLitter(int i) { removed[i] = true; litter[i].gameObject.SetActive(false); velocity[i] = Vector3.zero; Collected++; }
        public bool TrySweepPoint(out Vector3 point)
        {
            var ray = player.view.ViewportPointToRay(new Vector3(.5f,.5f));
            var ground = new Plane(Vector3.up, new Vector3(0,.04f,0));
            point = default;
            if (!ground.Raycast(ray, out float distance) || distance < 0 || distance > 5) return false;
            if (Physics.Raycast(ray,out var hit,distance) && hit.collider.gameObject.name != "Explanada") return false;
            point = ray.GetPoint(distance);
            return Mathf.Abs(point.x) < 6 && point.z > -7.3f && point.z < 3.5f;
        }
        void CreateFeedback()
        {
            var go = new GameObject("Área de barrido");
            sweepRing = go.AddComponent<LineRenderer>(); sweepRing.loop = true; sweepRing.useWorldSpace = true;
            sweepRing.positionCount = 48; sweepRing.widthMultiplier = .018f;
            // A serialized material reference keeps the shader in standalone builds.
            var material = new Material(feedbackMaterial != null ? feedbackMaterial : litBulb); material.color = new Color(.95f,.79f,.36f);
            sweepRing.sharedMaterial = material;
            sweepRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; sweepRing.receiveShadows = false;
            var marker = new GameObject("Ayuda de tarea"); hintMarker = marker.AddComponent<TMPro.TextMeshPro>();
            hintMarker.font = hud.font; hintMarker.text = "↓"; hintMarker.fontSize = 7; hintMarker.color = new Color(1,.82f,.32f);
            hintMarker.alignment = TMPro.TextAlignmentOptions.Center; marker.SetActive(false);
        }
        void UpdateFeedback()
        {
            if (sweepRing == null) return;
            bool visible = !Paused && !MovingCart && Tool == 0 && TrySweepPoint(out _);
            sweepRing.gameObject.SetActive(visible);
            if (visible && TrySweepPoint(out var point))
                for (int i=0;i<48;i++) { float angle=i*Mathf.PI*2/48; sweepRing.SetPosition(i,point+new Vector3(Mathf.Sin(angle)*.9f,.008f,Mathf.Cos(angle)*.9f)); }
            if (hintTime > 0 && !Paused)
            {
                hintTime -= Time.deltaTime;
                hintMarker.transform.rotation = player.view.transform.rotation;
            }
            hintMarker.gameObject.SetActive(!Paused && hintTime > 0);
        }
        public Vector3 RequestHint()
        {
            Vector3 target = switchBox.position;
            string message = Restored ? "El puesto está listo. Prueba la canica." : "Acércate al interruptor del puesto.";
            if (!FloorDone)
            {
                float closest=float.MaxValue;
                for(int i=0;i<litter.Length;i++) if(!removed[i]) { float d=FlatDistance(player.transform.position,litter[i].position); if(d<closest) { closest=d; target=litter[i].position; } }
                message="Quedan residuos aquí. Barre hacia el recogedor amarillo.";
            }
            else if(!CounterDone)
            {
                for(int i=0;i<dirt.Length;i++) if(dirt[i]<1) { target=dirtPatches[i].transform.position; break; }
                message="Usa el paño (2) sobre esta mancha del mostrador.";
            }
            else if(!SignFixed) { target=sign.position; message="Mira el cartel y pulsa E para colocarlo."; }
            else if(Restored) target=marble.position;
            if(hintMarker!=null) { hintMarker.transform.position=target+Vector3.up*.55f; hintTime=6; }
            Tell(message); return target;
        }
        public void SetVolume(float volume) { AudioListener.volume=Mathf.Clamp01(volume); PlayerPrefs.SetFloat("feria.volume",AudioListener.volume); }
        public void SetSensitivity(float value) { player.sensitivity=Mathf.Clamp(value,.035f,.18f); PlayerPrefs.SetFloat("feria.sensitivity",player.sensitivity); }
        static float FlatDistance(Vector3 a, Vector3 b) { a.y = b.y = 0; return Vector3.Distance(a, b); }
        bool LookingAt(Transform target, float range)
        {
            Vector3 position = target == cart ? cart.position + Vector3.up * .65f : target.position;
            Vector3 delta = position - player.view.transform.position;
            return delta.magnitude < range && Vector3.Dot(delta.normalized, player.view.transform.forward) > .76f;
        }
        public string Prompt()
        {
            if (MovingCart) return "E · Estacionar carrito";
            if (!SignFixed && LookingAt(sign, 3.5f)) return "E · Colocar cartel";
            if (LookingAt(switchBox, 3.5f)) return Restored ? "El puesto vuelve a brillar" : (FloorDone && CounterDone && SignFixed ? "E · ¡Encender el puesto!" : "Termina las tres tareas para encender");
            if (Restored && LookingAt(marble, 4)) return "E · Soltar una canica";
            if (LookingAt(cart, 3.4f)) return "E · Mover carrito";
            return Tool == 0 ? "Mantén clic · Barrer hacia el recogedor" : "Mantén clic · Limpiar el mostrador";
        }
        public void Interact()
        {
            if (MovingCart) { MovingCart = false; Save(false); return; }
            if (!SignFixed && LookingAt(sign, 3.5f)) { FixSign(); Save(false); return; }
            if (LookingAt(switchBox, 3.5f)) { if (!TryRestore()) Tell("Falta despejar, limpiar o colocar el cartel."); return; }
            if (Restored && LookingAt(marble, 4)) { ReleaseMarble(); return; }
            if (LookingAt(cart, 3.4f)) { MovingCart = true; Tell("Lleva el carrito cerca del montón. E para estacionar."); }
        }
        public void FixSign() { SignFixed = true; sign.SetPositionAndRotation(new Vector3(-2.95f, 1.3f, 3.85f), Quaternion.identity); fxAudio.PlayOneShot(pickupSound); }
        public bool TryRestore()
        {
            if (Restored) return true;
            if (!FloorDone || !CounterDone || !SignFixed) return false;
            Restored = true; ApplyLights(); fxAudio.PlayOneShot(bellSound);
            Tell("¡El Rincón de las Canicas vuelve a la vida! Acércate a la canica."); Save(false); return true;
        }
        void ApplyLights() { foreach (var l in fairLights) l.intensity = Restored ? 2.2f : .15f; foreach (var b in bulbs) b.sharedMaterial = Restored ? litBulb : unlitBulb; }
        public void ReleaseMarble() { if (!Restored || marbleTime >= 0) return; marbleTime = 0; marble.position = marbleStart; fxAudio.PlayOneShot(pickupSound); }
        public void TogglePause() { SetPaused(!Paused); }
        public void SetPaused(bool paused)
        {
            Paused = paused; Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = paused;
            if (brushAudio != null && paused) brushAudio.volume = 0;
            if (paused) PlayerPrefs.Save();
        }
        public void Tell(string message) { Notice = message; noticeTime = 7; }
        public FairSave Snapshot()
        {
            var data = new FairSave { litter = new Vector3[litter.Length], removed = (bool[])removed.Clone(), dirt = (float[])dirt.Clone(), cart = cart.position, cartYaw = cart.eulerAngles.y, player = player.transform.position, playerYaw = player.transform.eulerAngles.y, signFixed = SignFixed, restored = Restored, playSeconds = PlaySeconds };
            for (int i = 0; i < litter.Length; i++) data.litter[i] = litter[i].position;
            return data;
        }
        public void Apply(FairSave data)
        {
            if (data == null || data.version != 1 || data.litter == null || data.removed == null || data.dirt == null || data.litter.Length != litter.Length || data.removed.Length != litter.Length || data.dirt.Length != dirt.Length) throw new InvalidDataException("Formato de partida incompatible.");
            if (!Finite(data.player) || !Finite(data.cart) || !float.IsFinite(data.playerYaw) || !float.IsFinite(data.cartYaw) || !float.IsFinite(data.playSeconds) || data.playSeconds < 0) throw new InvalidDataException("Posición no válida.");
            foreach (var p in data.litter) if (!Finite(p) || Mathf.Abs(p.x) > 7 || Mathf.Abs(p.z) > 9 || p.y < 0 || p.y > 1) throw new InvalidDataException("Residuo no válido.");
            foreach (var value in data.dirt) if (!float.IsFinite(value) || value < 0 || value > 1) throw new InvalidDataException("Limpieza no válida.");
            if (Mathf.Abs(data.player.x) > 6.6f || Mathf.Abs(data.player.z) > 8.4f || data.player.y < -.2f || data.player.y > 3 || Mathf.Abs(data.cart.x) > 6 || data.cart.z < -7.5f || data.cart.z > 3.7f) throw new InvalidDataException("Partida fuera del recinto.");
            if (data.restored && (!data.signFixed || Array.Exists(data.removed, value => !value) || Array.Exists(data.dirt, value => value < 1))) throw new InvalidDataException("Objetivos incoherentes.");
            Collected = 0; MovingCart = false;
            for (int i = 0; i < litter.Length; i++) { litter[i].position = data.litter[i]; removed[i] = data.removed[i]; litter[i].gameObject.SetActive(!removed[i]); velocity[i] = Vector3.zero; if (removed[i]) Collected++; }
            dirt = (float[])data.dirt.Clone(); for (int i = 0; i < dirt.Length; i++) UpdateDirt(i);
            cart.SetPositionAndRotation(data.cart, Quaternion.Euler(0, data.cartYaw, 0)); player.Place(data.player, data.playerYaw);
            SignFixed = data.signFixed; if (SignFixed) FixSign(); else sign.SetPositionAndRotation(signStart,signRotation);
            Restored = data.restored; PlaySeconds = data.playSeconds; ApplyLights();
            marbleTime=-1; marble.position=marbleStart;
        }
        static bool Finite(Vector3 v) => float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
        public void Save(bool notify = true)
        {
            if (suppressSave) return;
            try
            {
                new LocalSaveStore(SavePath).Write(Snapshot());
                if (notify) Tell("Partida guardada en este equipo.");
            }
            catch (Exception e) { Debug.LogWarning("No se pudo guardar: " + e.Message); Tell("No se pudo guardar. Comprueba el espacio y los permisos."); }
        }
        void Load()
        {
            new LocalSaveStore(SavePath).TryLoad(Apply,out bool recovered,message=>Debug.LogWarning(message));
            if(recovered) Tell("Partida recuperada desde el respaldo.");
        }
        void OnApplicationQuit() { if (removed != null) Save(false); PlayerPrefs.Save(); Cursor.lockState = CursorLockMode.None; }
        void OnApplicationFocus(bool focus) { if (!focus && !Paused) SetPaused(true); }
        static AudioClip MakeSound(string name, float duration, float frequency, bool noise)
        {
            const int rate = 22050; float[] samples = new float[(int)(rate * duration)]; var random = new System.Random(74);
            float smooth = 0;
            for (int i = 0; i < samples.Length; i++) { float t = (float)i / rate; smooth = smooth * .65f + ((float)random.NextDouble() * 2 - 1) * .35f; samples[i] = noise ? smooth * .45f : Mathf.Sin(t * frequency * Mathf.PI * 2) * Mathf.Exp(-t * 9) * .35f; }
            var clip = AudioClip.Create(name, samples.Length, 1, rate, false); clip.SetData(samples, 0); return clip;
        }
    }
}
