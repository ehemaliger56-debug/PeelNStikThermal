# CDH-4 Ablative Heat Sink — Modell-Spezifikation

Für dein eigenes Modell (Platzhalter ist aktuell das Stock-Radiator-Panel).

## Formate (nach Aufwand sortiert)

| Weg | Was du lieferst | Wie es rein kommt |
|-----|-----------------|-------------------|
| **A: Unity + KSP PartTools** | fertige `.mu` + Textur | direkt einsetzbar |
| **B: Blender + KSP-Add-on** (`io_mesh_mu` / "KSP Blender Tools") | `.mu` + Textur | direkt einsetzbar |
| **C: alles andere** (Blender/SketchUp/Meta 3D …) | `.obj`/`.fbx`/`.dae` + PNG-Textur | ich konvertiere zu `.mu` und binde ein |

## Geometrie

- **Ursprung** = Mitte der **Klebefläche** (Pivot dort!)
- Klebefläche liegt in der **X/Y-Ebene am Ursprung**, zeigt nach **−Z**
  (die Fläche, die an den Rumpf drückt)
- Sichtbare **Front ragt nach +Z** (vom Rumpf weg)
- Maße (bei rescaleFactor 1): **Breite ~0,5–0,7 m, Höhe ~1,0–1,3 m,
  Dicke ~0,08–0,15 m** — anderes Verhältnis ist okay, ich passe
  `rescaleFactor` an
- Low-poly: **< 5.000 Tris**, ein Mesh/Material reicht für V1

## Textur

- **PNG** (KSPCF konvertiert automatisch zu DDS und cached — wie unser Atlas)
- 1024×1024 genügt locker
- Name z. B. `chs-heat-sink-map.png`; wenn du Normal-/Spec-Map willst:
  `chs-heat-sink-nrm.png` / `chs-heat-sink-spec.png` (optional)

## Look-Vorschlag (frei)

- Unten: dunkle Klebebasis/Pad (Peel-N-Stik-Look passend zum Mod)
- Darauf: ablative "Schwamm"-Struktur — Quilt-/Waben-/Kachelmuster,
  stilistisch an die CDH-Coating-Kacheln anlehnend
- Optional: Metallrahmen oder Kühlrippen seitlich (sieht nach "Absorber" aus)
- Farbverlauf Andeutung: heiß vorne (orange/glühend) → kühler hinten

## Ablauf danach

1. Datei(en) einfach hier ablegen, z. B. in
   `GameData/ConformalHeatShields/Assets/` (Ordner anlegen ist okay)
   oder in `Downloads` — sag mir kurz wo
2. Ich binde sie ein: `MODEL`-Node in `heatShield-sink.cfg` umstellen,
   `node_attach`/`rescaleFactor` anpassen, Textur referenzieren
3. KSP-Test → wir justieren Größe/Ausrichtung, falls nötig
