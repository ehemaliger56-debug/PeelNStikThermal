// ============================================================
// ConformalHeatShields - ModuleHeatSink
//
// "Ablative Heat Sink" (CDH-4): a radial part that soaks up heat
// from its surroundings and burns it in its own ablator:
//
//  - every physics tick it pulls thermal energy from every part
//    inside the absorption radius - interior AND skin, on this
//    vessel OR any nearby one (e.g. a jettisoned shield).
//    Energy conserving: the source loses exactly what the sink
//    gains. Interior AND skin heat both land in the sink's CORE
//    (the sponge - its own skin is not the point; radiator models
//    have ~zero skin thermal mass, which used to kill the whole
//    skin branch).
//  - the pull aims at pullTargetTemp, not at our own temperature:
//    a heat vacuum. A shield next to the sink is held below its
//    ablation threshold (500 K) and stops burning its own ablator
//    while the sink lives.
//  - PROCESSING IS DONE HERE, not by ModuleAblator, in two stages:
//    heat is soaked into the core's thermal mass for FREE until it
//    reaches processFloorTemp; only the overflow above that is paid
//    with the charge resource (kJPerAblatorUnit kJ per unit) and
//    removed again. Mild warmth costs nothing - the charge is only
//    spent under real sustained load.
//  - SHIELD MODE (ecPerMeterSecond > 0): the projector pays a flat
//    rate proportional to the radius (EC/s = radius * ecPerMeterSecond)
//    instead of per-kJ billing; while the field is paid, everything
//    it collected is deflected (removed). Movie energy shield. With
//    shieldBubble the field shows as a translucent glowing sphere:
//    faint blue while holding, orange-red under load, dark when dry.
//  - when the ablator is spent and heat is still coming in, the
//    sink blows like a fuse: it keeps absorbing, heats up fast
//    (red-hot flash) and pops at maxTemp - deliberately below the
//    neighbours' limits. Sacrificial fuse: it dies first.
//    "Core Status" reads NOMINAL / CORE HOT / CORE CRITICAL / FUSE BLOWN
//    and warns once at fuseWarningTemp (1600 K default, cfg-tunable).
//
//  - MASTER SWITCH: "Sink Active" (right-click) or the
//    "Toggle / On / Off Heat Sink" action groups. Off stops the pull,
//    the billing and the field FX at once; it persists from the editor
//    into flight, and the range sphere hides while the sink is off.
//
// Editor: one slider (Absorption Radius). Flight: live readouts.
// ============================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CHS
{
    [KSPModule("CHS Heat Sink")]
    public class ModuleHeatSink : PartModule
    {
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true,
                  guiName = "Absorption Radius", guiUnits = " m", guiFormat = "F1")]
        [UI_FloatRange(minValue = 0.5f, maxValue = 30f, stepIncrement = 0.25f)]
        public float absorbRadius = 2f;

        // translucent sphere at exactly the absorption radius - the
        // "how far is my range" readout, like the deployed-science
        // range sphere. Toggleable in editor AND flight, persists.
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true,
                  guiName = "Show Range")]
        [UI_Toggle(enabledText = "On", disabledText = "Off")]
        public bool showRange = false;

        // master switch: off = no heat pull, no charge billing, no
        // bubble and no Firefly FX suppression. Persisted so a switch
        // in the VAB survives into flight. Action groups can flip it
        // mid-flight (handy for EC management / saving the fuse).
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true,
                  guiName = "Sink Active")]
        [UI_Toggle(enabledText = "On", disabledText = "Off")]
        public bool sinkEnabled = true;

        [KSPAction("Toggle Heat Sink")]
        public void ActionToggleSink(KSPActionParam param)
        {
            sinkEnabled = !sinkEnabled;
        }

        [KSPAction("Heat Sink On")]
        public void ActionSinkOn(KSPActionParam param)
        {
            sinkEnabled = true;
        }

        [KSPAction("Heat Sink Off")]
        public void ActionSinkOff(KSPActionParam param)
        {
            sinkEnabled = false;
        }

        [KSPField(isPersistant = false)]
        public float rangeOpacity = 0.22f;

        // fraction of the remaining temperature gap equalized per second
        // (cfg-only tuning knob; higher = stronger pull)
        [KSPField(isPersistant = false)]
        public float absorbRate = 1.0f;

        // extra multiplier for SKIN heat (hull surfaces, plumes, plasma) -
        // the critical path for keeping shields below their ablation
        // threshold. Equilibrium skin temp ~= pullTargetTemp +
        // plumeFlux / (sourceSkinMass * rate * skinPullMult)
        [KSPField(isPersistant = false)]
        public float skinPullMult = 4f;

        // "heat vacuum" ceiling: sources get pulled toward this temperature
        // even when the sink itself is already hotter. 450 sits below the
        // stock ablation threshold (500) so shields next to the sink go
        // quiet (cfg tuning)
        [KSPField(isPersistant = false)]
        public float pullTargetTemp = 450f;

        // processing: kJ of pulled heat consumed per unit of Ablator.
        // 150 units * 100 kJ = 15000 kJ of absorbed heat per sink
        [KSPField(isPersistant = false)]
        public float kJPerAblatorUnit = 100f;

        // powered mode (CDH-6 style): consume ElectricCharge instead of
        // Ablator. Out of power -> unprocessed heat piles up in the core;
        // once power returns the pile is processed away again (brown-out
        // is recoverable). Starved long enough, it pops at maxTemp.
        [KSPField(isPersistant = false)]
        public bool useElectricCharge = false;

        // powered mode efficiency: kJ of heat handled per unit of
        // ElectricCharge. 3 => a 300 kW pull draws 100 EC/s - hungry
        [KSPField(isPersistant = false)]
        public float kJPerECUnit = 3f;

        // shield mode (CDH-7 style): instead of paying per kJ, the
        // projector keeps a protective field up and pays a flat rate
        // proportional to the radius (EC/s = absorbRadius *
        // ecPerMeterSecond) - very cheap even for huge fields. While
        // the field is paid, everything it collects is deflected
        // (removed) instead of stored. Dry field -> brown-out like the
        // rest of the family.
        [KSPField(isPersistant = false)]
        public float ecPerMeterSecond = 0f;

        // shield mode visual: translucent glowing bubble around the
        // field (movie-style energy shield). Faint blue while holding,
        // orange-red under load, dark when the power is dry.
        [KSPField(isPersistant = false)]
        public bool shieldBubble = false;

        [KSPField(isPersistant = false)]
        public float bubbleOpacity = 0.35f;

        // Firefly reentry effects: while the field is paid, the plugin
        // drives Firefly's per-part FX override to zero for every part
        // inside the field - the plasma then "burns" at the bubble
        // (our own glow) instead of on the hull. Overrides are released
        // automatically when the field dies or a part leaves the radius.
        // (Uses FireflyAPI if present; without Firefly nothing happens.)
        [KSPField(isPersistant = false)]
        public bool fireflySuppressFX = false;

        // billing threshold: the core soaks heat for FREE up to this
        // temperature (thermal mass storage); only the overflow above it
        // is billed to the charge resource. Also caps the sink's working
        // temperature while charge lasts.
        [KSPField(isPersistant = false)]
        public float processFloorTemp = 450f;

        // live flight readouts (auto-refreshed from the fields)
        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false,
                  guiName = "Core Temp", guiUnits = " K", guiFormat = "F0")]
        public double coreTemp;

        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false,
                  guiName = "Skin Temp", guiUnits = " K", guiFormat = "F0")]
        public double skinTemp;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = false,
                  guiName = "Absorbed Heat", guiUnits = " kJ", guiFormat = "F0")]
        public double absorbedKJ;

        // powered mode only (guiActive set in OnStart): current draw
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false,
                  guiName = "Power Draw", guiUnits = " EC/s", guiFormat = "F1")]
        public double powerDraw;

        // fuse status light: NOMINAL / CORE HOT / CORE CRITICAL /
        // FUSE BLOWN. The critical warning fires once per heat-up and
        // re-arms once the core has cooled back below the threshold.
        [KSPField(isPersistant = false, guiActive = true, guiActiveEditor = false,
                  guiName = "Core Status")]
        public string coreStatus = "NOMINAL";

        [KSPField(isPersistant = false)]
        public float fuseWarningTemp = 1600f;

        private const double MinTemp = 4.0;
        private double tickKJ;
        private bool fuseBlown;
        private bool fuseWarned;

        // sphere visuals runtime state (range indicator + movie bubble)
        private GameObject rangeSphere;
        private Material rangeMat;
        private GameObject movieBubble;
        private Material movieMat;
        private static Texture2D shellTex;
        private float lastLoadKW;
        private bool lastPaid;
        private bool bubbleFailed;

        // parts whose Firefly FX we currently suppress
        private readonly List<Part> suppressed = new List<Part>();
        private bool lastPaidPrev;
        private bool lastSinkEnabled = true;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            try
            {
                coreTemp = part.temperature;
                skinTemp = part.skinTemperature;
                Fields["powerDraw"].guiActive = useElectricCharge;
                lastSinkEnabled = sinkEnabled;
                UpdateCoreStatus();
            }
            catch (Exception e)
            {
                Log("OnStart: " + e);
            }
        }

        private void FixedUpdate()
        {
            try
            {
                if (HighLogic.LoadedSceneIsFlight == false) return;
                if (FlightGlobals.ready == false) return;

                // pick up switches from the right-click UI as well as
                // from action groups (both just write sinkEnabled)
                if (lastSinkEnabled != sinkEnabled)
                {
                    lastSinkEnabled = sinkEnabled;
                    OnSinkStateChanged();
                }
                if (sinkEnabled == false)
                {
                    // switched off: no pull, no billing, no field.
                    // Keep the readouts live so a parked, off sink still
                    // shows how hot its core is getting.
                    ReleaseAllFirefly();
                    lastPaid = false;
                    powerDraw = 0.0;
                    coreTemp = part.temperature;
                    skinTemp = part.skinTemperature;
                    UpdateCoreStatus();
                    return;
                }

                double dt = TimeWarp.fixedDeltaTime;
                float r2 = absorbRadius * absorbRadius;
                Vector3 selfPos = part.transform.position;

                // collect in-radius parts - ANY vessel (a jettisoned or
                // detached shield counts, debris counts, everything)
                List<Part> near = new List<Part>();
                List<Vessel> vessels = FlightGlobals.Vessels;
                for (int vi = 0; vi < vessels.Count; vi++)
                {
                    Vessel vv = vessels[vi];
                    if (vv == null || vv.parts == null) continue;
                    for (int i = 0; i < vv.parts.Count; i++)
                    {
                        Part p = vv.parts[i];
                        if (p == null || ReferenceEquals(p, part)) continue;
                        if ((p.transform.position - selfPos).sqrMagnitude > r2) continue;
                        near.Add(p);
                    }
                }
                if (near.Count == 0)
                {
                    // field has nothing to cover: hand any FX overrides back
                    ReleaseAllFirefly();
                    coreTemp = part.temperature;
                    skinTemp = part.skinTemperature;
                    UpdateCoreStatus();
                    return;
                }

                // pull from the hottest gap first
                near.Sort(delegate (Part a, Part b)
                {
                    double ga = (a.temperature + a.skinTemperature);
                    double gb = (b.temperature + b.skinTemperature);
                    return gb.CompareTo(ga);
                });

                tickKJ = 0.0;
                powerDraw = 0.0;
                lastPaid = false;
                for (int i = 0; i < near.Count; i++)
                {
                    PullFrom(near[i], dt);
                }
                absorbedKJ += tickKJ;
                lastLoadKW = (float)(tickKJ / Math.Max(dt, 1e-9) * 0.001);

                // the ablator consumes what we took in this tick
                Process(tickKJ);

                // firefly FX: covered parts stop burning at the hull -
                // while the field is paid, the burn happens at our bubble
                SyncFirefly(near);

                coreTemp = part.temperature;
                skinTemp = part.skinTemperature;
                UpdateCoreStatus();
            }
            catch (Exception e)
            {
                Log("FixedUpdate failed: " + e);
            }
        }

        private string PartTitle()
        {
            return part.partInfo != null ? part.partInfo.title : part.name;
        }

        // status light + one-shot early warning before the 1800 K pop.
        // Powered sinks brown-out gradually, so this gives real lead
        // time; ablator sinks get the explicit FUSE BLOWN message when
        // their charge runs dry under load (see Process).
        private void UpdateCoreStatus()
        {
            double t = part.temperature;
            if (fuseBlown)
            {
                coreStatus = "FUSE BLOWN";
                return;
            }
            if (t >= fuseWarningTemp)
            {
                coreStatus = "CORE CRITICAL";
                if (fuseWarned == false)
                {
                    fuseWarned = true;
                    Log(string.Format("CORE CRITICAL: {0:F0} K (pop at {1:F0} K)",
                                      t, part.maxTemp));
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        ScreenMessages.PostScreenMessage(
                            "[CHS] " + PartTitle() + ": CORE CRITICAL — " +
                            t.ToString("F0") + " K, fuse near!",
                            3f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                return;
            }
            coreStatus = t >= fuseWarningTemp - 150f ? "CORE HOT" : "NOMINAL";
            if (t < fuseWarningTemp - 150f) fuseWarned = false;
        }

        // one-shot cleanup + feedback whenever the master switch flips.
        // Firefly overrides are handed back immediately so the hull
        // starts burning again the moment the field goes down.
        private void OnSinkStateChanged()
        {
            if (sinkEnabled == false)
            {
                ReleaseAllFirefly();
                lastPaid = false;
                lastPaidPrev = false;
                powerDraw = 0.0;
                tickKJ = 0.0;
            }
            Log("Sink " + (sinkEnabled ? "ON" : "OFF"));
            if (HighLogic.LoadedSceneIsFlight)
            {
                ScreenMessages.PostScreenMessage(
                    "[CHS] " + PartTitle() + (sinkEnabled ? ": sink ON" : ": sink OFF"),
                    2f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        // move heat from src onto this part's core. The pull aims at
        // pullTargetTemp (heat vacuum), never at our own current
        // temperature - the sink keeps sucking even when it is hotter
        // than the source, concentrating all heat in the sacrificial part.
        private void PullFrom(Part src, double dt)
        {
            double effT = Math.Min(part.temperature, pullTargetTemp);

            // --- interior heat ---
            double gap = src.temperature - effT;
            if (gap > 0)
            {
                double tmS = Math.Max(src.thermalMass, 1e-9);
                double tmD = Math.Max(part.thermalMass, 1e-9);
                double E = gap * absorbRate * dt * Math.Min(tmS, tmD);
                double avail = gap * tmS;
                if (E > avail) E = Math.Max(0.0, avail);
                if (E > 0)
                {
                    src.temperature -= E / tmS;
                    part.temperature += E / tmD;
                    tickKJ += E;
                }
            }

            // --- skin heat (reentry plasma, plumes, hot hulls) ---
            // pulled into our CORE: the sink's own skin is irrelevant, so
            // no dependency on our (near-zero) skin thermal mass. Guarded
            // only by the SOURCE's skin mass.
            double gapS = src.skinTemperature - effT;
            if (gapS > 0)
            {
                double tmS = Math.Max(src.skinThermalMass, 1e-9);
                double tmD = Math.Max(part.thermalMass, 1e-9);
                if (tmS > 1e-9)
                {
                    double E = gapS * absorbRate * skinPullMult * dt * Math.Min(tmS, tmD);
                    double avail = gapS * tmS;
                    if (E > avail) E = Math.Max(0.0, avail);
                    if (E > 0)
                    {
                        src.skinTemperature -= E / tmS;
                        part.temperature += E / tmD;
                        tickKJ += E;
                    }
                }
            }
        }

        // dispose of the heat we took in via the charge resource
        // (Ablator - or ElectricCharge in powered mode). Billing model:
        // the core soaks heat into its own thermal mass FOR FREE until
        // it reaches processFloorTemp - only the overflow above the
        // floor is paid with the charge resource and removed from the
        // core again. So mild warmth (thin atmosphere, gentle skin
        // heating) costs nothing; the charge is only spent under real
        // sustained load. Ablator out under load -> fuse blow (fast
        // heat-up, stock overheat explosion at maxTemp). Powered out ->
        // brown-out: heat piles up, recoverable once power returns.
        private void Process(double pulledKJ)
        {
            double tmD = Math.Max(part.thermalMass, 1e-9);

            // overflow above the floor (includes heat from direct
            // exposure of the sink itself)
            double floorKJ = Math.Max(0.0, (part.temperature - processFloorTemp) * tmD);
            if (pulledKJ <= 0 && floorKJ <= 0) return;

            // --- SHIELD MODE: flat per-radius billing + deflection ---
            // the projector pays a flat rate to keep the field up; while
            // the field is paid, everything it collected is deflected
            // (removed) instead of billed per kJ - that is what makes
            // the consumption tiny. Dry field -> the heat stays in the
            // core -> brown-out, pop at maxTemp like the rest.
            if (ecPerMeterSecond > 0f)
            {
                // the field stands as long as it has parts to cover and
                // is billed per meter even on ticks with no heat flow -
                // otherwise the suppression would drop exactly when the
                // field is winning (everything pinned at the target temp,
                // pull momentarily zero) and the burn would jump back to
                // the hull mid-reentry
                double dtS = Math.Max(TimeWarp.fixedDeltaTime, 1e-6);
                double want = absorbRadius * ecPerMeterSecond * dtS;
                double gotF = 0.0;
                try
                {
                    gotF = part.RequestResource("ElectricCharge", want);
                }
                catch (Exception e)
                {
                    Log("RequestResource failed: " + e);
                }
                powerDraw = gotF / dtS;
                lastPaid = gotF >= want * 0.999;

                if (lastPaid)
                {
                    // field holds: deflect everything we collected
                    double avail = Math.Max(0.0, (part.temperature - MinTemp) * tmD);
                    double deflected = Math.Min(pulledKJ + floorKJ, avail);
                    if (deflected > 0)
                    {
                        part.temperature -= deflected / tmD;
                    }
                }
                return;
            }

            string res = useElectricCharge ? "ElectricCharge" : "Ablator";
            double kJPerUnit = useElectricCharge ? kJPerECUnit : kJPerAblatorUnit;

            double demand = floorKJ / Math.Max(kJPerUnit, 1e-6);
            double got = 0.0;
            if (demand > 0)
            {
                try
                {
                    got = part.RequestResource(res, demand);
                }
                catch (Exception e)
                {
                    Log("RequestResource failed: " + e);
                }
                powerDraw = got / Math.Max(TimeWarp.fixedDeltaTime, 1e-6);
            }

            // remove the paid-for overflow from the core again
            double processed = Math.Min(got * kJPerUnit, floorKJ);
            if (processed > 0)
            {
                part.temperature -= processed / tmD;
            }

            // spent fuse only for the ablator version - it never comes back.
            // the powered version brown-outs instead: unprocessed heat stays
            // in the core and heats it; if starvation lasts until maxTemp the
            // stock thermal system pops it - if power returns first, the pile
            // is processed away and the sink recovers.
            if (useElectricCharge == false && demand > 0 && got < demand * 0.999)
            {
                if (fuseBlown == false)
                {
                    fuseBlown = true;
                    Log(string.Format(
                        "Fuse blow: charge empty under load (absorbed {0:F0} kJ total, core {1:F0} K)",
                        absorbedKJ, part.temperature));
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        ScreenMessages.PostScreenMessage(
                            "[CHS] " + PartTitle() + ": FUSE BLOWN — charge empty under load!",
                            3f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                double rise = Math.Max(60.0, Math.Max(pulledKJ, floorKJ) / tmD);
                part.temperature = Math.Min(part.maxTemp + 100.0, part.temperature + rise);
            }
        }

        // --- Firefly reentry FX override ------------------------------
        // While the field is paid, every part inside the radius gets its
        // Firefly FX overridden to zero: no plasma on the hull. The burn
        // is then shown at our own bubble. When the field dies (brown-
        // out, no parts in radius, part destroyed) the overrides are
        // released and Firefly keeps score again.

        private void SyncFirefly(List<Part> near)
        {
            // give back parts that left the field (or died)
            for (int i = suppressed.Count - 1; i >= 0; i--)
            {
                Part p = suppressed[i];
                bool stillIn = p != null && near.Contains(p);
                if (stillIn == false)
                {
                    ReleaseFirefly(p);
                    suppressed.RemoveAt(i);
                }
            }

            // one-shot diagnostics: why did the burn jump to the hull?
            if (lastPaid != lastPaidPrev)
            {
                lastPaidPrev = lastPaid;
                if (lastPaid)
                {
                    Log("Field paid - Firefly FX suppressed inside the radius");
                }
                else
                {
                    Log(string.Format(
                        "Field DRY (draw {0:F2} EC/s) - Firefly FX released, burn shows on the hull",
                        powerDraw));
                }
            }

            if (fireflySuppressFX == false || lastPaid == false)
            {
                // brown-out: physics is keeping score again - show the burn
                if (suppressed.Count > 0)
                {
                    for (int i = 0; i < suppressed.Count; i++)
                    {
                        ReleaseFirefly(suppressed[i]);
                    }
                    suppressed.Clear();
                }
                return;
            }

            // refresh the overrides every tick (idempotent; also heals
            // races with a second overlapping sink)
            for (int i = 0; i < near.Count; i++)
            {
                Part p = near[i];
                SuppressFirefly(p);
                if (suppressed.Contains(p) == false) suppressed.Add(p);
            }
        }

        private void SuppressFirefly(Part p)
        {
            if (p == null) return;
            try
            {
                FireflyAPI.IFireflyModule fm = p.GetComponent<FireflyAPI.IFireflyModule>();
                if (fm == null) return;
                fm.OverridePhysics = true;
                fm.OverridenBy = "ConformalHeatShields";
                fm.OverrideEffectStrength = 0f;
                fm.OverrideEffectState = 0f;
            }
            catch (Exception e)
            {
                Log("Firefly suppress: " + e);
            }
        }

        private void ReleaseFirefly(Part p)
        {
            try
            {
                if (p == null) return;
                FireflyAPI.IFireflyModule fm = p.GetComponent<FireflyAPI.IFireflyModule>();
                if (fm == null) return;
                try
                {
                    if (fm.OverridenBy == "ConformalHeatShields") fm.ResetOverride();
                }
                catch (Exception)
                {
                    // fall through - nothing critical
                }
            }
            catch (Exception)
            {
                // Firefly gone/missing - nothing to release
            }
        }

        private void ReleaseAllFirefly()
        {
            for (int i = suppressed.Count - 1; i >= 0; i--)
            {
                ReleaseFirefly(suppressed[i]);
                suppressed.RemoveAt(i);
            }
        }

        // --- sphere visuals ------------------------------------------
        // 1) RANGE SPHERE: translucent measurement sphere at exactly
        //    the absorption radius (the "how far is my range" readout,
        //    like the deployed-science range sphere). Toggle via the
        //    Show Range button, works in editor AND flight, persists.
        // 2) MOVIE BUBBLE (shield mode only): additive-glow energy
        //    shield around the field - faint blue while holding,
        //    orange-red under load, dark (flickers off) when dry.
        // Visual only: no collider, no physics on either.

        private void Update()
        {
            HandleRangeSphere();
            if (shieldBubble) HandleMovieBubble();
        }

        private void HandleRangeSphere()
        {
            bool scene = HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor;
            if (showRange == false || sinkEnabled == false || scene == false)
            {
                if (rangeSphere != null && rangeSphere.activeSelf) rangeSphere.SetActive(false);
                return;
            }
            if (rangeSphere == null)
            {
                CreateSphere(ref rangeSphere, ref rangeMat);
                if (rangeSphere == null) return;
            }
            ApplyTransform(rangeSphere);
            // soft measurement look: pale yellow-white; in flight a dry
            // field tints the sphere pale RED (payment lost - brown-out)
            float o = Mathf.Clamp01(rangeOpacity);
            bool dry = HighLogic.LoadedSceneIsFlight && lastPaid == false;
            Color rc = dry ? new Color(1f, 0.35f, 0.30f) : new Color(1f, 0.92f, 0.55f);
            rangeMat.SetColor("_TintColor", new Color(rc.r * o, rc.g * o, rc.b * o, o));
            if (rangeSphere.activeSelf == false) rangeSphere.SetActive(true);
        }

        private void HandleMovieBubble()
        {
            if (movieBubble == null)
            {
                CreateSphere(ref movieBubble, ref movieMat);
                if (movieBubble == null || movieMat == null) return;
            }
            bool show = HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && lastPaid;
            if (show == false)
            {
                if (movieBubble.activeSelf) movieBubble.SetActive(false);
                return;
            }
            ApplyTransform(movieBubble);
            // cool cyan while holding -> orange under load -> white-hot
            // fireball at full load; a flicker keeps the plasma alive.
            // (With fireflySuppressFX the covered hull shows no burn -
            // this bubble IS the burn.)
            float load = Mathf.Clamp01(lastLoadKW / 400f);
            Color cool = new Color(0.25f, 0.75f, 1.0f);
            Color hot = new Color(1.0f, 0.35f, 0.10f);
            Color c = Color.Lerp(cool, hot, load);
            if (load > 0.5f)
            {
                c = Color.Lerp(c, new Color(1.0f, 0.88f, 0.65f), (load - 0.5f) * 2f);
            }
            float flicker = 0.85f + 0.15f * Mathf.Sin(Time.time * 29f + lastLoadKW * 13f);
            float a = Mathf.Clamp01(bubbleOpacity * (0.35f + 0.65f * load) *
                                    (0.8f + 0.6f * load * flicker));
            movieMat.SetColor("_TintColor", new Color(c.r * a, c.g * a, c.b * a, a));
            if (movieBubble.activeSelf == false) movieBubble.SetActive(true);
        }

        private void ApplyTransform(GameObject go)
        {
            go.transform.position = part.transform.position;
            float d = absorbRadius * 2f;
            go.transform.localScale = new Vector3(d, d, d);
        }

        private void CreateSphere(ref GameObject go, ref Material mat)
        {
            if (bubbleFailed) return;
            if (HighLogic.LoadedSceneIsFlight == false &&
                HighLogic.LoadedSceneIsEditor == false) return;
            try
            {
                Shader sh = Shader.Find("KSP/Particles/Additive");
                if (sh == null) sh = Shader.Find("Particles/Additive");
                if (sh == null)
                {
                    bubbleFailed = true;
                    Log("field spheres: no additive shader found - visuals off");
                    return;
                }
                EnsureShellTexture();
                if (shellTex == null) return;

                GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Collider col = o.GetComponent<Collider>();
                if (col != null) Destroy(col);
                o.name = part.name + " FieldSphere";
                int layer = LayerMask.NameToLayer("TransparentFX");
                if (layer >= 0) o.layer = layer;

                Renderer r = o.GetComponent<Renderer>();
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;

                mat = new Material(sh);
                mat.mainTexture = shellTex;
                r.material = mat;

                go = o;
                o.SetActive(false);
            }
            catch (Exception e)
            {
                bubbleFailed = true;
                Log("field sphere: " + e);
            }
        }

        // soft shell texture: bright limb (a sphere's surface reads
        // thickest at the rim), faint haze in the middle - shared by
        // every sphere of every sink
        private static void EnsureShellTexture()
        {
            if (shellTex != null) return;
            Texture2D tex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dx = (x - 31.5f) / 32f;
                    float dy = (y - 31.5f) / 32f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float shell = Mathf.Clamp01(dist * 1.25f);
                    float haze = 0.12f * (1f - dist);
                    float v = Mathf.Clamp01(shell + haze);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, v * v));
                }
            }
            tex.Apply();
            shellTex = tex;
        }

        private void OnDestroy()
        {
            if (rangeSphere != null) Destroy(rangeSphere);
            if (movieBubble != null) Destroy(movieBubble);
            if (rangeMat != null) Destroy(rangeMat);
            if (movieMat != null) Destroy(movieMat);
            rangeSphere = null;
            movieBubble = null;
            rangeMat = null;
            movieMat = null;
        }

        private static void Log(string message)
        {
            Debug.Log("[CHSSink] " + message);
        }
    }
}
