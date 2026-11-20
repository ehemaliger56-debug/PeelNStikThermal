# Peel-N-Stik Thermal

Decal-style heat shields you can stick almost anywhere, plus a family of
active heat sinks that pull heat out of nearby parts. For KSP 1.12.

Repository: https://github.com/ehemaliger56-debug/PeelNStikThermal

---

## What's included

| Part | Identifier | Description |
|------|------------|-------------|
| **CDH-1 Ablative Coating Tiles** | `chs-ablator-coating` | Scalable stick-on ablator sheet with a tile look. Attach it like a flag decal; adjust scale, height and opacity in the part menu. Four surface patterns. |
| **CDH-2 Large Ablative Coating Panel** | `chs-ablator-coating-large` | Large preset that spawns at 6x scale - cover a whole tank wall in one go. Same patterns as CDH-1. |
| **CDH-4 Heat Sink** | `chs-heat-sink` | Compact active heat sink, attachable radially to any hull. Pulls heat from everything in range into its core, stores mild heat for free, then spends ElectricCharge on sustained load. |
| **CDH-5 Advanced Heat Sink** | `chs-heat-sink-adv` | Larger version with a wider field (fresh radius 8 m), a bigger free bank and a stronger pull. |
| **CDH-6 Elite Heat Sink** | `chs-heat-sink-elite` | As compact as CDH-4 with CDH-5's field and the best power efficiency in the family. |
| **CDH-7 Shield Projector** | `chs-shield-projector` | Projects a visible energy bubble over everything in range (fresh radius 14 m, up to 30 m). Heat is deflected instead of stored, for a flat EC/s per metre of radius. |
| **CDH-10 Peel-N-Stik Heat Shield (0.625 m)** | `chs-radial-0625` | Small structural ablative shield with adhesive backing, mountable radially anywhere. |
| **CDH-11 Peel-N-Stik Heat Shield (1.25 m)** | `chs-radial-125` | Same, 1.25 m. |
| **CDH-12 Peel-N-Stik Heat Shield (2.5 m)** | `chs-radial-25` | Same, 2.5 m. |
| **CDH-13 Peel-N-Stik Heat Shield (3.75 m)** | `chs-radial-375` | Same, 3.75 m. |

All parts are in the **Thermal** category. The radial shields can be
pressed onto any surface and jettisoned later with "Jettison Heat Shield".

---

## Coatings

Choose one of four patterns in the part menu: **Ablative Tiles**,
**Scorched Ablator**, **Thermal Blanket** and **Clean Tile**. The pattern
survives resizing. Scale a panel with the **Scale** slider (CDH-1: 0.25-20x,
CDH-2: 2-20x) and set how far it floats above the surface with **Depth**.

Coatings are real thermal parts: they ablate on reentry and protect the
surface underneath them.

### Spreading protection

A coating can share its ablator layer with nearby parts:

- Placement in the VAB and every craft change gives a thin ablator layer
  to each free part in radius (0.5-8 m, default 3 m).
- Covered parts get their own small ablator supply (5-100 units, default 25).
- Crew parts, parts that already have an ablator, and the coating itself
  are skipped. Up to 20 parts per coating.
- Right-click menu:
  - **Distribute Coating to Nearby Parts** - spread again manually
  - **Remove Spread Layer** - take the protection back
  - **Spread Radius** / **Ablator Layer per Part** - tuning sliders
  - **Auto Spread** - re-spread after every craft change or load

To check it in the VAB: place a coating, then right-click a neighbouring
part - its menu shows **Ablator: 25/25**.

### Twin decals

A coating can also place visible copies on directly adjacent surfaces:

- Each twin is a full part: it persists in the craft file, scales and
  recolours like the original, carries its own ablator and burns correctly.
- Twins follow the original's rotation and do not cascade.
- Up to 8 twins per coating; crew parts and already covered surfaces are
  skipped.
- Enable **Spawn Twin Decals on Neighbors** in the part menu to use them.
- **Remove Spread Layer & Decals** on the original removes twins and
  distributed layers together.

---

## Heat sinks

CDH-4 through CDH-7 share the same machinery:

- They pull **interior and skin heat** from every part in range, including
  detached parts and debris. The pull aims at a fixed temperature ceiling,
  so it keeps working even when the sink itself is already hot - a heat
  shield next to it stays below its ablation threshold and stops burning
  its own ablator.
- Energy is conserved: the source loses exactly what the sink gains.
- Pulled heat fills the core's thermal mass for free up to a per-part
  floor temperature. Only the overflow is paid for with ElectricCharge
  (or, for CDH-7, with a flat field rate).
- If power runs out, heat piles up in the core. Restoring power clears the
  pile and the sink recovers. If starvation lasts, the core reaches 1800 K
  and the sink blows like a fuse - deliberately below the limit of tanks,
  so its neighbours survive.

| | **CDH-4** | **CDH-5 Advanced** | **CDH-6 Elite** | **CDH-7 Shield** |
|---|---|---|---|---|
| Fresh radius | 2 m | 8 m | 8 m | 14 m |
| Free bank up to | 550 K | 650 K | 400 K | - |
| Power per 300 kW pull | about 12 EC/s | about 12 EC/s | about 6 EC/s | flat 0.56 EC/s at 14 m |
| Pull strength | 1.0 | 1.2 | 1.5 | 1.5 |
| Temperature ceiling | 450 K | 450 K | 400 K | 400 K |

Each sink has an **Absorption Radius** slider in the editor, plus a live
**Power Draw** readout in flight so you can size your batteries and
reactors.

### On/off and status

- Right-click -> **Sink Active** (On/Off), or use the action groups
  **Toggle Heat Sink**, **Heat Sink On** and **Heat Sink Off**.
- Off means no pull, no power draw, no bubble and no FX suppression. The
  temperature readouts stay live, and the setting carries from the VAB
  into flight.
- **Core Status** reads NOMINAL / CORE HOT / CORE CRITICAL / FUSE BLOWN.
  A screen message warns once per heat-up from 1600 K, and the ablator
  version reports a blown fuse when its charge runs dry under load.

### CDH-7 field

- The bubble is visible: faint blue at idle, orange under reentry load,
  dark when power is out. Its colour and brightness follow the heat load.
- **Show Range** displays the radius as a measurement sphere in the
  editor and in flight (on by default for CDH-7).
- With Firefly installed, reentry effects on parts inside the field are
  suppressed while the field is paid - the burn appears at the bubble
  instead of on the hull. Without Firefly, nothing changes.
- The bubble is visual only: no collider, no physics, no effect on flight.

---

## Tech tree (career)

| Node | Parts |
|---|---|
| **survivability** (early) | CDH-1, CDH-2, CDH-4, CDH-10 (0.625 m), CDH-11 (1.25 m) |
| **advConstruction** (mid) | CDH-5 |
| **landing** (mid) | CDH-12 (2.5 m) |
| **advLanding** (late) | CDH-13 (3.75 m) |
| **experimentalScience** (late) | CDH-6 |
| **experimentalElectrics** (end) | CDH-7 |

Each tier matches the power budget of its era: CDH-4 runs on batteries,
CDH-5 on fuel cells, CDH-6 is the efficient endgame sink, and CDH-7's flat
per-metre draw keeps large fields affordable.

---

## Tips

- One sink per hot zone (engine section, nose) is usually enough. Crewed
  parts in range are cooled too.
- CDH-6's small size makes it a good choice for cramped hulls.
- CDH-2 spawns large - handy for covering a tank wall quickly.
- Multiple panels side by side: shift-click the same mount point to copy
  a part, then drag **Scale**.

---

## Model contributions welcome

I'd love to give every part its own proper model, and I could really use a
hand with that. If you make KSP part models, please feel free to send one my
way — open an issue or a pull request in the repository and attach the files.
One good sink model covers all four heat sinks, since they share a mesh.
`MODEL-SPEC.md` has the orientation and size conventions. Thanks for any help!

---

## CKAN

CKAN metadata is in `CKAN/`. A GitHub release with the mod ZIP exists; the
mod is not listed in CKAN yet because the pull request is still under
review: https://github.com/KSP-CKAN/NetKAN/pull/11888. See `CKAN/README.md`.

## Requirements

- ConformalDecals
- B9PartSwitch
- ModuleManager
- ReStock (for the radial shield models)

Firefly is optional and enables the energy-bubble reentry effects.
