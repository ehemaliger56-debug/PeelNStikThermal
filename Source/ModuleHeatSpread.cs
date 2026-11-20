// ============================================================
// ConformalHeatShields - ModuleHeatSpread
//
// Two independent features, both driven from the coating part:
//
//  1) ABLATOR SPREAD (protection)
//     Distributes a thin ablative layer (dynamic ModuleAblator +
//     Ablator resource) over parts near the coating. Parts that
//     are directly attached are always covered; extra reach via
//     radius. Crewed parts and real heat shields are skipped.
//
//  2) TWIN DECALS (visual)  [editor only]
//     Spawns real copies ("twins") of this coating part on the
//     surface of directly adjacent parts (raycast placement),
//     so the decal pattern visually covers several parts and the
//     decals remain native (craft-file persistence, variants,
//     jettison, drag cubes - everything stock-behaved).
//     Twins carry isSpreadTwin = true and never cascade.
//     Ctrl: "Remove Spread Layer & Decals" removes them again.
//
// Compiled against KSP 1.12 (Assembly-CSharp, UnityEngine.CoreModule)
// ============================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using ConformalDecals;
using UnityEngine;

namespace CHS
{
    [KSPModule("Heat Coating Spread")]
    public class ModuleHeatSpread : PartModule
    {
        // ---- tunables (persisted per craft) ----
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true,
                  guiName = "Spread Radius", guiUnits = " m", guiFormat = "F1")]
        [UI_FloatRange(minValue = 0.5f, maxValue = 8f, stepIncrement = 0.5f)]
        public float spreadRadius = 3f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true,
                  guiName = "Ablator Layer per Part", guiUnits = "", guiFormat = "F0")]
        [UI_FloatRange(minValue = 5f, maxValue = 100f, stepIncrement = 5f)]
        public float ablatorPerPart = 25f;

        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true,
                  guiName = "Spawn Twin Decals on Neighbors")]
        // default OFF: the decal already spans parts via projectMultiple,
        // and auto-spawned copies stacked on the mounting spot made the
        // coating impossible to pick up in the editor
        public bool spawnTwins = false;

        // marker on twins (persisted so one pass per craft is enough)
        [KSPField(isPersistant = true)]
        public bool isSpreadTwin = false;

        // marker stored in dynamically added ablator's charModuleName
        private const string Marker = "CHSSPREAD";
        private const int MaxParts = 20;
        private const int MaxTwins = 8;

        private float lastSpread = -10f;

        public override string GetInfo()
        {
            return "Distributes ablative padding to nearby parts and (in the editor) attaches twin coating decals to adjacent surfaces. Crewed parts and real heat shields are left untouched.";
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            try
            {
                if (HighLogic.LoadedSceneIsEditor)
                {
                    GameEvents.onEditorShipModified.Add(OnShipModified);
                }
                if (HighLogic.LoadedSceneIsEditor || HighLogic.LoadedSceneIsFlight)
                {
                    if (autoSpread()) SpreadLayers(false);
                }
            }
            catch (Exception e)
            {
                Log("OnStart failed: " + e);
            }
        }

        private void OnDestroy()
        {
            try
            {
                GameEvents.onEditorShipModified.Remove(OnShipModified);
            }
            catch (Exception e)
            {
                Log("OnDestroy failed: " + e);
            }
        }

        private void OnShipModified(ShipConstruct ship)
        {
            if (!autoSpread()) return;
            // debounce: skip bursts, but never skip the very first spread
            if (lastSpread >= 0f && Time.realtimeSinceStartup - lastSpread < 0.5f) return;
            SpreadLayers(false);
            SpawnTwins(false);
        }

        private bool autoSpread()
        {
            // a twin never re-invokes the system itself
            if (isSpreadTwin) return false;
            return true;
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Distribute Coating to Nearby Parts", active = true)]
        public void DistributeNow()
        {
            SpreadLayers(true);
            SpawnTwins(true);
        }

        [KSPEvent(guiActive = false, guiActiveEditor = true, guiName = "Remove Spread Layer & Decals", active = true)]
        public void RemoveSpread()
        {
            Clear();
            RemoveAllTwins(true);
            autoSpreadSet(false); // manual removal switches auto mode off
        }

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Toggle Auto Spread", active = true)]
        public void ToggleAuto()
        {
            bool now = !autoSpreadFlag();
            autoSpreadSet(now);
            ScreenMess(String.Format("CHS coating: auto spread {0}", now ? "ON" : "OFF"));
        }

        // -----------------------------------------------------
        // 1) ablator spread (protection)
        // -----------------------------------------------------
        private void SpreadLayers(bool manual)
        {
            try
            {
                List<Part> all = Collect();
                if (all == null) return;

                Vector3 origin = part.transform.position;
                float r2 = spreadRadius * spreadRadius;

                List<Part> candidates = new List<Part>();
                foreach (Part p in all)
                {
                    if (p == null || p == part) continue;
                    if (p.CrewCapacity > 0) continue;                              // no coating over kerbals
                    if (p.FindModuleImplementing<ModuleAblator>() != null) continue; // own/other shield already there

                    // directly attached neighbors are ALWAYS protected; radius is extra reach
                    bool adjacent = (p.parent == part) || part.children.Contains(p);
                    if (!adjacent && (p.transform.position - origin).sqrMagnitude > r2) continue;
                    candidates.Add(p);
                }

                // prefer the closest parts
                candidates.Sort(delegate (Part a, Part b)
                {
                    float da = (a.transform.position - origin).sqrMagnitude;
                    float db = (b.transform.position - origin).sqrMagnitude;
                    return da.CompareTo(db);
                });

                int n = 0;
                foreach (Part p in candidates)
                {
                    if (n >= MaxParts) break;
                    if (AddLayer(p)) n++;
                }

                if (n > 0) lastSpread = Time.realtimeSinceStartup;

                if (manual || (HighLogic.LoadedSceneIsEditor && n > 0))
                {
                    ScreenMess(String.Format(
                        "CHS coating: layer distributed to {0} parts (radius {1} m)",
                        n.ToString(), spreadRadius.ToString("0.0")));
                }
                Log(String.Format("distributed to {0} parts (radius {1:F1} m)", n.ToString(), spreadRadius));
            }
            catch (Exception e)
            {
                Log("Spread failed: " + e);
            }
        }

        private bool AddLayer(Part p)
        {
            try
            {
                ModuleAblator ab = p.AddModule("ModuleAblator", true) as ModuleAblator;
                if (ab == null)
                {
                    Log("could not add ModuleAblator to " + p.name);
                    return false;
                }

                ab.ablativeResource = "Ablator";
                ab.lossExp = -7500.0;
                ab.lossConst = 0.1;
                ab.pyrolysisLossFactor = 6000.0;
                ab.reentryConductivity = 0.01;
                ab.ablationTempThresh = 500.0;
                ab.useChar = false;
                ab.charModuleName = Marker; // marker for identification

                if (p.Resources["Ablator"] == null)
                {
                    // flowState=false so the padding does not drain other tanks; it is per-part
                    p.Resources.Add("Ablator", (double)ablatorPerPart, (double)ablatorPerPart,
                                    false, true, false, true, PartResource.FlowMode.None);
                }
                return true;
            }
            catch (Exception e)
            {
                Log("AddLayer failed on " + p.name + ": " + e);
                return false;
            }
        }

        private void Clear()
        {
            try
            {
                List<Part> all = Collect();
                if (all == null) return;
                int n = 0;
                foreach (Part p in all)
                {
                    if (p == null || p == part) continue;
                    ModuleAblator ab = p.FindModuleImplementing<ModuleAblator>();
                    if (ab == null) continue;
                    if (!(Marker.Equals(ab.charModuleName))) continue;
                    ab.enabled = false;
                    PartResource res = p.Resources["Ablator"];
                    if (res != null) res.amount = 0.0;
                    n++;
                }
                Log("cleared " + n.ToString() + " spread layers");
                if (HighLogic.LoadedSceneIsEditor && n > 0)
                    ScreenMess("CHS coating: spread layer removed from " + n.ToString() + " parts");
            }
            catch (Exception e)
            {
                Log("Clear failed: " + e);
            }
        }

        // -----------------------------------------------------
        // 2) twin decals (visual, editor only)
        // -----------------------------------------------------
        private void SpawnTwins(bool manual)
        {
            if (isSpreadTwin) return;          // twins never cascade
            if (HighLogic.LoadedSceneIsEditor == false) return;
            try
            {
                object fetch = EditorLogicType();
                if (fetch == null) return;
                FieldInfo shipF = fetch.GetType().GetField("ship");
                ShipConstruct ship = shipF != null ? shipF.GetValue(fetch) as ShipConstruct : null;
                if (ship == null) return;
                if (manual == false && spawnTwins == false) return;

                Vector3 origin = part.transform.position;
                int spawned = 0;

                // candidate surfaces: directly attached parts (both directions)
                List<Part> neighbors = new List<Part>();
                if (part.parent != null) neighbors.Add(part.parent);
                neighbors.AddRange(part.children);

                foreach (Part nb in neighbors)
                {
                    if (nb == null || spawned >= MaxTwins) break;
                    if (nb.CrewCapacity > 0) continue;                                     // no decals over kerbals
                    if (TwinExistsOn(ship, nb)) continue;                                  // idempotent
                    if (nb.FindModuleImplementing<ModuleConformalDecal>() != null
                        && nb.FindModuleImplementing<ModuleHeatSpread>() != null) continue; // it IS a coating?

                    if (SpawnTwin(fetch, ship, nb)) spawned++;
                }

                Log("twins spawned: " + spawned.ToString());
                if (manual && isSpreadTwin == false)
                {
                    ScreenMess(String.Format("CHS coating: twin decals attached: {0}", spawned.ToString()));
                }
            }
            catch (Exception e)
            {
                Log("SpawnTwins failed: " + e);
            }
        }

        private bool TwinExistsOn(ShipConstruct ship, Part neighbor)
        {
            // a twin attached to the same neighbor already covers it
            if (neighbor.FindModuleImplementing<ModuleConformalDecal>() != null
                && neighbor.FindModuleImplementing<ModuleHeatSpread>() != null
                && (neighbor.FindModuleImplementing<ModuleHeatSpread>().isSpreadTwin
                     || object.ReferenceEquals(neighbor, this.part)))
            {
                return true;
            }
            for (int i = 0; i < ship.parts.Count; i++)
            {
                Part p = ship.parts[i];
                if (p == null || p == part) continue;
                if (!object.ReferenceEquals(p.parent, neighbor)) continue;
                ModuleHeatSpread m = p.FindModuleImplementing<ModuleHeatSpread>();
                if (m != null && m.isSpreadTwin) return true;
            }
            return false;
        }

        private bool SpawnTwin(object fetch, ShipConstruct ship, Part neighbor)
        {
            try
            {
                // --- raycast from the coating toward the neighbor's center ---
                Vector3 origin = part.transform.position;
                Vector3 dir = neighbor.transform.position - origin;
                float dist = dir.magnitude;
                if (dist < 0.05f) return false;
                dir /= dist;

                RaycastHit[] hits = Physics.RaycastAll(origin, dir, dist + 6f);
                if (hits != null && hits.Length > 0)
                {
                    System.Array.Sort(hits, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));

                    bool hitOk = false;
                    Vector3 hp = Vector3.zero, hn = Vector3.up;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        Part hp2 = hits[i].transform.GetComponentInParent<Part>();
                        if (hp2 == null) continue;
                        if (object.ReferenceEquals(hp2, part)) continue;        // skip ourselves
                        if (IsInHierarchy(hp2, neighbor))
                        {
                            hp = hits[i].point;
                            hn = hits[i].normal;
                            hitOk = true;
                            break;
                        }
                        // a part in between blocks the line; try next hit
                    }
                    if (!hitOk) return false;

                    // Never place a twin onto the spot this coating already
                    // covers: the ray toward the neighbor's center usually
                    // hits the surface right at/under the coating itself, and
                    // a stacked copy there makes the original impossible to
                    // grab in the editor (it looks glued to the part).
                    ModuleConformalDecal selfDecal = part.FindModuleImplementing<ModuleConformalDecal>();
                    float minSep = 0.25f + 0.5f * (selfDecal != null ? selfDecal.scale : 1f);
                    if ((hp - part.transform.position).sqrMagnitude < minSep * minSep)
                    {
                        Log("twin spot overlaps this coating - skipped");
                        return false;
                    }

                    // --- duplicate ourselves (the editor's own copy routine) ---
                    Part twin = InvokeDuplicatePart(fetch, this.part);
                    if (twin == null) return false;
                    try { twin.transform.position = hp; } catch (Exception e) { Log("place pos: " + e); }

                    // orient: +Z toward surface normal; pattern 'up' aligned with our rotation
                    Vector3 refUp = part.transform.up;
                    if (Mathf.Abs(Vector3.Dot(refUp, hn)) > 0.95f) refUp = part.transform.right;
                    Vector3 projUp = Vector3.ProjectOnPlane(refUp, hn);
                    if (projUp.sqrMagnitude < 1e-6f) projUp = Vector3.up;
                    try { twin.transform.rotation = Quaternion.LookRotation(hn, projUp.normalized); } catch (Exception e) { Log("place rot: " + e); }

                    // --- copy current decal look ---
                    ModuleConformalDecal sm = part.FindModuleImplementing<ModuleConformalDecal>();
                    ModuleConformalDecal tm = twin.FindModuleImplementing<ModuleConformalDecal>();
                    if (sm != null && tm != null)
                    {
                        try
                        {
                            tm.scale = sm.scale;
                            tm.depth = sm.depth;
                            tm.opacity = sm.opacity;
                            tm.tileRect = sm.tileRect;
                            tm.tileIndex = sm.tileIndex;
                            tm.tileSize = sm.tileSize;
                        }
                        catch (Exception e) { Log("copy decal: " + e); }
                        safeCall(tm, "UpdateScale");
                        safeCall(tm, "UpdateMaterials");
                    }

                    // --- mark the twin: never cascades, identifiable ---
                    ModuleHeatSpread ms = twin.FindModuleImplementing<ModuleHeatSpread>();
                    if (ms != null)
                    {
                        ms.isSpreadTwin = true;
                        ms.autoSpreadSet(false);
                    }

                    // --- attach ---
                    MethodInfo attachM = fetch.GetType().GetMethod("attachPart",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (attachM == null) { Log("attachPart missing"); DestroyTwin(twin); return false; }

                    // Attachment struct is internal - build via reflection
                    Type attType = typeof(Part).Assembly.GetType("Attachment");
                    if (attType == null) { Log("Attachment type missing"); DestroyTwin(twin); return false; }
                    object at = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(attType);
                    SetField(attType, at, "possible", true);
                    SetField(attType, at, "collision", false);
                    SetField(attType, at, "mode", AttachModes.SRF_ATTACH);
                    SetField(attType, at, "potentialParent", neighbor);
                    SetField(attType, at, "caller", twin);
                    AttachNode twinNode = twin.srfAttachNode != null ? twin.srfAttachNode : twin.FindAttachNode("srfAttach");
                    safeCall(neighbor, "UpdateSrfAttachNode");
                    AttachNode parentSrfNode = neighbor.srfAttachNode;
                    SetField(attType, at, "callerPartNode", twinNode);
                    SetField(attType, at, "otherPartNode", parentSrfNode);
                    SetField(attType, at, "position", twin.transform.position);
                    SetField(attType, at, "rotation", twin.transform.rotation);
                    if (twinNode == null || parentSrfNode == null)
                    {
                        Log("no srf nodes on " + neighbor.name);
                        DestroyTwin(twin);
                        return false;
                    }

                    try
                    {
                        attachM.Invoke(fetch, new object[] { twin, at });
                    }
                    catch (Exception e)
                    {
                        Log("attachPart failed: " + e.ToString() + (e.InnerException != null ? " / " + e.InnerException : ""));
                        DestroyTwin(twin);
                        return false;
                    }

                    bool ok = (twin.parent == neighbor) || neighbor.children.Contains(twin) || ship.parts.Contains(twin);
                    if (ok)
                    {
                        try { twin.RemoveFromSymmetry(); } catch { }
                        return true;
                    }
                    Log("attach result not confirmed for " + neighbor.name);
                    DestroyTwin(twin);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log("SpawnTwin failed on " + neighbor.name + ": " + e);
            }
            return false;
        }

        private void RemoveAllTwins(bool manual)
        {
            try
            {
                if (HighLogic.LoadedSceneIsEditor == false) return;
                object fetch = EditorLogicType();
                if (fetch == null) return;
                FieldInfo shipF = fetch.GetType().GetField("ship");
                ShipConstruct ship = shipF != null ? shipF.GetValue(fetch) as ShipConstruct : null;
                if (ship == null) return;

                MethodInfo rm = fetch.GetType().GetMethod("removeFromShip",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                List<Part> doomed = new List<Part>();
                for (int i = 0; i < ship.parts.Count; i++)
                {
                    Part p = ship.parts[i];
                    if (p == null || p == part) continue;
                    ModuleHeatSpread m = p.FindModuleImplementing<ModuleHeatSpread>();
                    if (m == null || !m.isSpreadTwin) continue;
                    doomed.Add(p);
                }
                for (int i = 0; i < doomed.Count; i++)
                {
                    Part p = doomed[i];
                    try { if (rm != null) rm.Invoke(fetch, new object[] { p }); } catch (Exception e) { Log("removeFromShip: " + e); }
                    UnityEngine.Object.Destroy(p.gameObject);
                }
                Log("removed twins: " + doomed.Count.ToString());
                if (manual && doomed.Count > 0)
                    ScreenMess("CHS coating: removed " + doomed.Count.ToString() + " twin decals");
            }
            catch (Exception e)
            {
                Log("RemoveAllTwins failed: " + e);
            }
        }

        // -----------------------------------------------------
        // helpers
        // -----------------------------------------------------
        private static bool IsInHierarchy(Part child, Part root)
        {
            Part cur = child;
            int guard = 0;
            while (cur != null && guard++ < 64)
            {
                if (object.ReferenceEquals(cur, root)) return true;
                cur = cur.parent;
            }
            return false;
        }

        private static void SetField(Type t, object target, string name, object value)
        {
            try
            {
                FieldInfo f = t.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (f != null) f.SetValue(target, value);
                else Log("attachment field missing: " + name);
            }
            catch (Exception e)
            {
                Log("SetField " + name + ": " + e);
            }
        }

        private Part InvokeDuplicatePart(object fetch, Part src)
        {
            try
            {
                MethodInfo mi = fetch.GetType().GetMethod("DuplicatePart",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi == null) { Log("DuplicatePart missing"); return null; }
                object r = mi.Invoke(fetch, new object[] { src });
                return r as Part;
            }
            catch (Exception e)
            {
                Log("DuplicatePart invoke: " + e.ToString() + (e.InnerException != null ? " / " + e.InnerException : ""));
                return null;
            }
        }

        private object EditorLogicType()
        {
            try
            {
                Type t = typeof(Part).Assembly.GetType("EditorLogic");
                if (t == null) return null;
                PropertyInfo pi = t.GetProperty("fetch", BindingFlags.Public | BindingFlags.Static);
                if (pi != null) return pi.GetValue(t, null);
                FieldInfo fi = t.GetField("fetch", BindingFlags.Public | BindingFlags.Static);
                if (fi != null) return fi.GetValue(null);
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void DestroyTwin(Part twin)
        {
            try { UnityEngine.Object.Destroy(twin.gameObject); }
            catch (Exception e) { Log("destroy twin: " + e); }
        }

        private static void safeCall(object o, string methodName)
        {
            try
            {
                MethodInfo mi = o.GetType().GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (mi != null) mi.Invoke(o, null);
            }
            catch
            {
                // optional
            }
        }

        /// list of parts in the same construct (vessel, or editor attach-tree)
        private List<Part> Collect()
        {
            List<Part> result = new List<Part>();
            if (part == null) return result;
            if (part.vessel != null)
            {
                foreach (Part p in part.vessel.parts) result.Add(p);
                return result;
            }
            // editor fallback: walk the whole attach tree
            HashSet<Part> seen = new HashSet<Part>();
            Stack<Part> todo = new Stack<Part>();
            todo.Push(part);
            while (todo.Count > 0)
            {
                Part p = todo.Pop();
                if (p == null || seen.Contains(p)) continue;
                seen.Add(p);
                result.Add(p);
                if (p.parent != null) todo.Push(p.parent);
                foreach (Part c in p.children) todo.Push(c);
            }
            return result;
        }

        private static void ScreenMess(string text)
        {
            try
            {
                ScreenMessages.PostScreenMessage(text, 4f, ScreenMessageStyle.UPPER_CENTER);
            }
            catch
            {
                // ignore
            }
        }

        private static void Log(string message)
        {
            Debug.Log("[CHSSpread] " + message);
        }

        // ---- autoSpread wrapper -----------------------------------------
        // autoSpread used to be a plain bool field; keep a per-instance
        // flag so both distribute/removal events can toggle it and the
        // field stays persistent for saves.
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = true, guiName = "Auto Spread")]
        public bool _autoSpread = true;

        private bool autoSpreadFlag()
        {
            return _autoSpread;
        }

        private void autoSpreadSet(bool v)
        {
            _autoSpread = v;
        }
    }
}

