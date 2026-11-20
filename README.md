# ConformalHeatShields

Hitzeschilde à la **ConformalDecals** – decal-artig überall platzierbar.
Beruht auf dem Decal-System von Andrew Cassidy (ConformalDecals v0.4.2 ist
in dieser Spiel-Installation bereits vorhanden) und benötigt **B9PartSwitch**
(auch installiert).

---

## Was drin ist

| Teil | Name | Beschreibung |
|------|------|--------------|
| **CDH-1 Ablative Coating Tiles** | `chs-ablator-coating` | Decal-basierte, skalierbare Ablator-Beschichtung (Kachel-Look). Ankleben wie ein Flag-Decal: Skalierung 0.25–20×, Tiefe, Deckkraft im Part-Menü einstellbar. 4 Mustervarianten (B9-Umschalter). |
| **CDH-2 Large Ablative Coating Panel** | `chs-ablator-coating-large` | **Große Preset-Version**: spawnt direkt bei 6×, Range 2–20× — eine ganze Tankwand in einem Rutsch. Gleiche Varianten & Textur. |
| **CDH-4 Heat Sink** | `chs-heat-sink` | **Aktiver Hitzesenker** (klein): radial an alles anklebbar. Zieht Wärme aus allen Parts im Umkreis (Innen + Hüllen-Skin, auch abgekoppelte Parts) auf sich: erst gratis in die Kernmasse, dann zahlt **ElectricCharge** den Überlauf (25 kJ pro EC ≈ 12 EC/s bei 300 kW Sog). Ohne Strom brown-out — dauerhaft ohne Strom → platzt als Erster. |
| **CDH-5 Advanced Heat Sink** | `chs-heat-sink-adv` | **Die große Version** (Large-Modell): Radius **frisch 8 m**, Slider bis 30 m, größerer Gratis-Speicher (650 K), stärkerer Sog. Gleicher Strom-Betrieb wie der CDH-4. |
| **CDH-6 Elite Heat Sink** | `chs-heat-sink-elite` | **Der beste**: körperlich so klein wie der CDH-4, aber **gleicher Radius wie der CDH-5 (8 m)** — und am stromsparendsten (50 kJ pro EC ≈ 6 EC/s bei 300 kW, 2× effizienter als die Standard-Senker). Stärkster Sog, kältester Deckel (400 K). |
| **CDH-7 Shield Projector** | `chs-shield-projector` | **Das Energie-Schild** 🛡️: legt eine **sichtbare Energie-Blase** um alles im Umkreis (Radius **frisch 14 m**, Slider bis 30 m). Flacher Stromverbrauch **pro Meter** (~0,56 EC/s bei 14 m) — Hitze wird vom Feld abgelenkt statt gespeichert. Blase glüht bei Reentry orange, geht dunkel bei Stromausfall. |
| **CDH-10 Peel-N-Stik Heat Shield (0.625m)** | `chs-radial-0625` | Vanilla-Hitzeschild (0.625 m) mit Klebe-Rückseite – radial überall montierbar, inkl. Verkohle-Effekt. |
| **CDH-11 Peel-N-Stik Heat Shield (1.25m)** | `chs-radial-125` | dito, 1.25 m |
| **CDH-12 Peel-N-Stik Heat Shield (2.5m)** | `chs-radial-25` | dito, 2.5 m |
| **CDH-13 Peel-N-Stik Heat Shield (3.75m)** | `chs-radial-375` | dito, 3.75 m |

Alle Teile liegen in der Kategorie **Thermal**. Die radialen Schilder sind
klassische Abrativschilder (Stock-Modelle), die überall *angedrückt*
werden können und mit „Jettison Heat Shield" wieder abgeworfen werden
können.

---

## Texturen (dein Part! 😄)

Die CDH-1-Beschichtung lädt:

```
GameData/ConformalHeatShields/Parts/Textures/chs-decal-atlas.dds
```

✅ **ERLEDIGT** – deine Meta-AI-Grafik (2×2-Quadranten) ist bereits eingearbeitet:
aus `image_20261107_173343.jpg` wurde `chs-decal-atlas.png` (1024×1024) gebaut.
KSPCommunityFixes konvertiert PNGs beim Laden automatisch zu DXT5 und cached
sie (aller DDS-Encoder-Weg war hier fehleranfällig – unmipmapped DDS lädt
nicht, PNG ist auf dieser Installation der bewiesene Weg). Die vier Designs:

| Quadrant im Bild | Beschreibung |
|------------------|--------------|
| oben links       | Silber-Thermaldecke (Knitterfolie) |
| oben rechts      | Weiße Schutzkacheln |
| unten links      | Dunkle Ablator-Kacheln |
| unten rechts     | Verbrannter Ablator (Glut-Risse) |

**Quadranten-Zuordnung** (cfg-Konvention, Ursprung ‚unten links' – empirisch aus
dem Generic-Atlas verifiziert):

| Position | Tile-Rect (cfg) | Variant (aktueller Titel) |
|----------|-----------------|---------------------------|
| unten links  | `tile = 0, 0, 512, 512`       | *Ablative Tiles* |
| unten rechts | `tile = 512, 0, 512, 512`     | *Scorched Ablator* (verbrannt) |
| oben links   | `tile = 0, 512, 512, 512`     | *Thermal Blanket* (Folie) |
| oben rechts  | `tile = 512, 512, 512, 512`   | *Clean Tile* (weiß) |

Neue Texturen einspielen: einfach ein neues 2×2-Bild (JPG/PNG) erzeugen und mir
den Pfad nennen – der Konverter (`%TEMP%\opencode\chs-tex.ps1`) komponiert und
encodiert den DDS automatisch neu.

---

## Dateien

```
ConformalHeatShields/
├── Patches/
│   └── ConformalHeatShields-Aero.cfg
├── Parts/
│   ├── heatShield-coating.cfg
│   ├── heatShield-coating-large.cfg
│   ├── heatShield-radial-0625.cfg
│   ├── heatShield-radial-125.cfg
│   ├── heatShield-radial-25.cfg
│   ├── heatShield-radial-375.cfg
│   ├── Textures/
│   │   └── chs-decal-atlas.png
├── Plugins/
│   └── CHSSpread.dll
├── Source/
│   └── ModuleHeatSpread.cs
└── README.md
```

---

## Coating-Spread — Schutz auf Nachbarteile verteilen

> **Neu (experimentell):** Ein eigenes kleines Plugin (`CHSSpread.dll`,
> Quelle in `Source/ModuleHeatSpread.cs`) verteilt die schützende
> Ablator-Schicht der Beschichtung **auf die umliegenden Parts**.

So funktioniert's:

- Beim Platzieren im VAB und bei jeder Bauteil-Änderung (Debounce 0,5 s)
  verteilt das Modul eine dünne Ablator-Schicht an **jedes freie Part im
  Radius**. In der Flugszene passiert das einmalig beim Szenenstart.
- **Standard-Radius 3 m** (Slider *„Spread Radius“*, 0,5–8 m), gemessen von
  der Part-Mitte der Beschichtung — für große Strukturen einfach aufdrehen.
- **Pro umgebenem Part** entsteht ein dynamisches Mini-`ModuleAblator` plus
  einer kleinen `Ablator`-Ressource (Slider *„Ablator Layer per Part“*,
  5–100 Einheiten, Standard 25).
- **Übersprungen werden** (bewusst!): Crew-teile (Kapseln, Cockpits —
  niemals Beschichtung über Kerbals 😄), Parts, die bereits einen echten
  `ModuleAblator` tragen (Stock-Hitzeschilde, die CDH-Teile selbst) und
  die Beschichtungs-Part selbst. Deckel: max. 20 Parts pro Beschichtung.
- **Part-Menü (Rechtsklick):**
  - **Distribute Coating to Nearby Parts** — manuell neu ausbreiten
  - **Remove Spread Layer** — verteilten Schutz zurücknehmen
    (schaltet Auto-Spread gleich mit aus)
  - **Spread Radius** / **Ablator Layer per Part** — Tuning-Slider
  - **Auto Spread** — verteilt bei jeder Craft-Änderung/buf Laden erneut
- **Idempotent:** bereits geschützte Parts werden nie doppelt beschichtet;
  die verteilte Ressource hat `FlowMode = None`, d. h. sie leert sich beim
  Wiedereintritt nur lokal und zieht kein Ablator aus Anderswo.

### Twin-Decals — sichtbare Beschichtung auf Nachbarteilen

> Über den reinen Ablator-Schutz hinaus spawnt das Modul im Editor **echte
> Zwillings-Decals** (`Twin Decals`) auf direkt angrenzenden Flächen.

- Nach dem Ankleben werden für alle direkt angrenzenden Parts per Raycast
  Podest-Punkt + Flächennormale ermittelt und dort **echte Kopien des
  Coating-Parts** positioniert (Editor-eigene `DuplicatePart`/`attachPart`-Maschine).
- Zwillings-Decals sind vollwertige Parts: sie landen im Craft-File
  (Persistenz!), skalieren/umfärben/simulieren wie jedes andere Coating,
  tragen ihre eigene Ablator-Ressource und Verbrennen korrekt.
- Sie ähneln ihrem Ursprung (`isSpreadTwin`) und **cascaden nicht**.
- Grenzen: max. 8 Zwillinge pro Coating, nur direkt angrenzende Teile,
  keine Crew-Teile, keine bereits dekalisierten Flächen (Idempotenz).
- **Muster-Ausrichtung** folgt der Rotatin des Originals (Kacheln laufen
  auf nebeneinanderliegenden Flächen in dieselbe Richtung).
- Entfernen: **Rechtsklick auf das Original → "Remove Spread Layer & Decals"**
  — nimmt Zwillinge UND verteilte Ablator-Schichten in einem Zug zurück
  (Auto Spread damit mit aus). Einzelne Zwillinge selbst löschen wie jedes
  normale Part.

**Hinweis:** Das visuelle Übergreifen auf Nachbar-Parts übernimmt
ConformalDecals selbst (Multiflächen-Projektion). Die Twin-Kopien sind
deshalb **standardmäßig aus** — sie hatten die unangenehme Eigenschaft,
eine Kopie direkt auf/unter das Coating zu setzen, sodass sich das
Coating im Editor nicht mehr greifen ließ. Wer sie trotzdem will: den
Schalter „Spawn Twin Decals on Neighbors" im Part-Menü aktivieren
(ein Stapel-Schutz verhindert Überlappungen mit dem Original).

### Coating — Wirkung prüfen (im VAB)

1. Coating an ein Part kleben → oben mittig erscheint
   `CHS coating: layer distributed to N parts (radius 3.0 m)`.
2. **Rechtsklick auf ein NACHBAR-Part** → im Menü steht **Ablator: 25/25**.
3. Im `KSP.log` gibt es zu jedem Aufruf eine Zeile
   `[CHSSpread] distributed to N parts (radius 3.0 m)`.

Wichtig: Der verteilte Schutz ist **unsichtbar** — Nachbarteile bekommen
keine eigene Decal-Farbe, nur die thermische Schicht (Ressource + Modul).

### CDH-4 Ablative Heat Sink — der aktive Hitzesenker

Ein **echtes thermisches Gerät** (kein Decal): radial an jedes beliebige
Part anklebbar — auch seitlich an Rümpfe, Tanks, Träger.

**Was er tut (in Flight):**

- Zieht jede Physik-Tick Wärme aus **allen Parts im Umkreis** — auch
  aus **abgeworfenen/fremden Parts** (z. B. ein abgekoppeltes
  Hitzeschild!) — (Absorption Radius, 0,5–30 m, im Editor einstellbar):
  **Innen-Temperatur und Hüllen-Skin**, beides landet in seinem Core.
- Funktioniert bei **allen Wärmequellen**: Wiedereintritts-Plasma auf den
  Hüllen, heiße Triebwerke, Bohrer, Reaktoren — je heißer das Part,
  desto stärker der Sog. Energieerhaltend: die Quelle verliert exakt,
  was der Senker gewinnt.
- **Wärme-Vakuum mit 450-K-Deckel**: Der Sog zielt auf eine feste
  Obergrenze (`pullTargetTemp`, 450 K) statt auf die eigene Temperatur —
  der Senker saugt also **weiter, selbst wenn er schon heißer ist** als
  die Quelle, und hält die Skin der Quelle unter der
  Ablations-Schwelle (500 K). Folge: Ein daneben klebender Hitzeschild
  bleibt ruhig und **verbraucht keinen eigenen Ablator mehr** — ist der
  Senker weg (geplatzt), übernimmt der Schild wieder mit seinem
  erhaltenen Vorrat.
- **Eigene Verarbeitung in zwei Stufen (kein ModuleAblator)**:
  1. **Gratis-Speicher**: Zugezogene Hitze läuft erst kostenlos in die
     eigene Kernmasse — bis `processFloorTemp` (550 K) erreicht ist
     (≈ 48.000 kJ Puffer). Lauter Warmluft-Kontakt kostet also
     **keinen Strom** — kein Stromverbrauch bei der kleinsten
     Atmosphären-Berührung.
  2. **Strom-Stufe**: Erst der **Überlauf** darüber wird bezahlt —
     `kJPerECUnit` = 25 kJ pro EC (300 kW Sog ≈ 12 EC/s). Unter
     Dauerlast zeigt das Part-Menü den Draw live an; direkter
     Plume-Beschuss auf den Senker selbst zählt mit.
- **Brown-out statt Sprengsatz**: Bei Strommangel staut sich die
  Hitze im Kern (glüht sichtbar auf) — kommt Strom zurück, wird der
  Stau abgebaut und er erholt sich vollständig. Hält die Knappheit
  an, steigt der Kern weiter und **platzt bei 1800 K**
  (`maxTemp`, bewusst unter den ~2000 K von Tanks) — er stirbt als
  Erster, die Nachbarn im Radius überleben. Danach ist der Schutz
  weg — Nächster bitte.

**Live-Anzeigen im Flug** (Rechtsklick): *Core Temp*, *Skin Temp*,
*Absorbed Heat* (kJ gesammelt) + **Power Draw (EC/s)**.

**Tuning (cfg `heatShield-sink.cfg`):** `absorbRate` (Sog-Stärke,
Standard 1,0), `skinPullMult` (Skin-Sog-Faktor, Standard 4 — hält
Schild-Skins ruhig), `pullTargetTemp` (Deckel, 450),
`kJPerECUnit` (kJ pro EC, 25), `processFloorTemp` (Gratis-Speicher,
550), `maxTemp` (1800 — die Platzt-Grenze ohne Strom).

**Praxis-Tipp:** Einen Senker pro heiße Zone (Triebwerkssection, Nase).
Er kühlt die Nachbarn aktiv — ein Crewed-Part im Radius wird mitgekühlt,
nicht übersprungen.

### CDH-5 / CDH-6 — die Advanced Heat Sinks

| | **CDH-5 Advanced** | **CDH-6 Elite** |
|---|---|---|
| Modell | Large-Radiator (groß) | Small-Radiator (so klein wie der CDH-4) |
| Ladung | **ElectricCharge** | **ElectricCharge** |
| Energie | 25 kJ pro EC → 300 kW Sog ≈ **12 EC/s** | 50 kJ pro EC → 300 kW ≈ **6 EC/s** (2× effizienter) |
| Radius | **8 m frisch platziert**, Slider bis 30 m | **8 m — gleicher Radius wie der CDH-5**, Slider bis 30 m |
| Sog | `absorbRate` 1,2, `skinPullMult` 4, Deckel 450 K | `absorbRate` **1,5**, `skinPullMult` 5, Deckel **400 K** |
| Tech | advConstruction | experimentalScience |
| Ohne Strom | **Brown-out**: Hitze staut sich; Strom zurück → erholt er sich; dauerhaft ohne → 💥 | dito |

Alle Senker (CDH-4…7) nutzen dieselbe Plugin-Maschine: Sog +
Gratis-Speicher + EC-Abrechnung + Brown-out am Core (1800 K). Alle
zeigen im Flug **Power Draw (EC/s)** — damit du deine
Reaktoren/Batterien dimensionieren kannst. Alles andere bleibt
beim einen Regler: *Absorption Radius*.

**An/Aus im Flug:** Rechtsklick → *Sink Active* (On/Off) oder die
Action Groups *Toggle Heat Sink* / *Heat Sink On* / *Heat Sink Off*.
Aus bedeutet: kein Sog, kein Stromverbrauch, keine Blase und keine
Firefly-FX-Unterdrückung — die Temperatur-Anzeigen bleiben live.
Der Zustand wird aus dem VAB in den Flug übernommen.

**Fuse-Warnung:** *Core Status* zeigt NOMINAL / CORE HOT / CORE CRITICAL /
FUSE BLOWN. Ab 1600 K (`fuseWarningTemp`, cfg-tunbar) kommt einmal pro
Aufheizung eine ScreenMessage (150 K Hysterese, danach re-arm). Beim
Ablator-Fuse gibt's zusätzlich „FUSE BLOWN", sobald die Ladung unter Last
leer ist.

### CDH-7 Shield Projector — das Energie-Schild 🛡️

Der Film-Moment: statt Hitze zu *schlucken*, hält der CDH-7 ein
**Feld** um alles im Umkreis — und das Feld ist **sichtbar**: eine
halbtransparente Energie-Blase, bläulich im Idle, **orange-glühend
unter Reentry-Last**, dunkel bei Stromausfall.

- **Flache Abrechnung pro Meter**: `ecPerMeterSecond` = 0,04 →
  **0,56 EC/s bei 14 m**, 0,72 bei 18 m, 1,2 bei 30 m — egal wie viel
  Hitze ankommt. Das Feld lenkt die Hitze ab (zieht sie aus den Parts
  und wirft sie gegen Strom weg), statt sie zu speichern.
- **Radius frisch 14 m** (größer als der CDH-6), Slider bis 30 m.
- **Show Range**: Rechtsklick → *Show Range* zeigt die Reichweite als
  Mess-Kugel (wie beim Deployable-Science-Arm) — im Editor **und**
  im Flug, Einstellung bleibt gespeichert. Gibt es bei allen vier
  Senkern; beim CDH-7 ist sie standardmäßig an.
- **Strom weg** → Blase dunkel → Hitze bleibt im Kern → Brown-out;
  dauerhaft ohne Strom → platzt bei 1800 K wie die ganze Familie.
- **Das Plasma brennt an der Blase**: Firefly (bessere Reentry-Effekte)
  wird per API für alle Parts im Feld auf 0 gedreht, solange das Feld
  Strom hat — der Rumpf zeigt keinen Burn, die Blase glüht als
  flackernde Feuerwalze (weißglühend unter Volllast). Bei Brown-out
  kommen die FX auf den Rumpf zurück. Ohne Firefly: keine Wirkung,
  kein Schaden.
- Rein visuell: die Blase hat keinen Collider, keine Physik, keinen
  Einfluss aufs Fahren — nur Show und Statusanzeige.

**Tuning (cfg `heatShield-sink-shield.cfg`):** `ecPerMeterSecond`
(Strom pro Meter/Sekunde, 0,04), `bubbleOpacity` (Blasen-Sichtbarkeit,
0,35), sonst Familien-Standard.

---

## Tech Tree (Karriere)

| Node (Stufe) | Teile |
|---|---|
| **survivability** (früh) | CDH-1 Coating-Tiles, CDH-2 Large Coating Panel, CDH-10 (0,625 m), CDH-11 (1,25 m), **CDH-4 Heat Sink** |
| **advConstruction** (Mitte) | **CDH-5 Advanced Heat Sink** |
| **landing** (Mitte) | CDH-12 (2,5 m) |
| **advLanding** (spät) | CDH-13 (3,75 m) |
| **experimentalScience** (spät) | **CDH-6 Elite Heat Sink** |
| **experimentalEngineering** (Ende) | **CDH-7 Shield Projector** |

Jede Senker-Stufe ist auf den Strom-Haushalt ihrer Ära kalibriert:

- **CDH-4 (survivability)**: dort gibt es nur Batterien, keine
  Erzeuger — deshalb tankt er mildes Heizen komplett gratis in seinen
  48.000-kJ-Speicher und zahlt Überlauf mit **25 kJ pro EC**
  (300 kW Sog ≈ 12 EC/s Peak). Eine Z-400-Batterie trägt einen
  suborbitalen Reentry locker.
- **CDH-5 (advConstruction)**: jetzt existieren Brennstoffzellen —
  gleiche Standard-Effizienz (25 kJ pro EC), aber 8-m-Feld und
  252.000 kJ Gratis-Speicher. Große Reentries werden zur
  Batterie-Frage statt zur Glückssache.
- **CDH-6 (experimentalScience)**: 2× effizienter (50 kJ pro EC),
  stärkster Sog, kältester Deckel — der Reentry-Alltag im Endgame.
- **CDH-7 (experimentalEngineering)**: eigenes Billing — flach pro
  Meter (0,56 EC/s bei 14 m), sichtbares Feld, FX-Suppression.

---

## Technische Hinweise / Tuning

- **`Patches/ConformalHeatShields-Aero.cfg`**: ConformalDecals setzt für alle
  Teile mit `ModuleConformalDecal` den Drag-Cube auf 0 („decals are just paint").
  Unsere Beschichtung ist aber ein *echtes* thermisches Teil – deshalb löscht
  das Patch den Null-Cube, damit KSP einen echten Drag-Cube erzeugt und der
  Schild auf Wiedereintritt **ablate (vergasen)** kann. Willst du die
  Beschichtung als reine „Farbe" ohne Aero-Effekt, datei löschen.
- **Kein Verkohlen auf der Decal-Beschichtung**: der ConformalDecals-Shader
  hat keinen `_BurnColor`-Kanal (Stock-`ModuleColorChanger`-Trick geht da
  nicht). Die radialen Schilder (Stock-Modelle) haben den Effekt ✔.
- **Massen skalieren beim Skalieren nicht**: `scale` verändert nur das Modell,
  Masse/Ablator bleiben konstant. Wenn dich das stört: `mass` bzw.
  `amount/maxAmount` in `heatShield-coating.cfg` anpassen.
- **Tuning-Vorschläge** (`heatShield-coating*.cfg`):
  - `mass`, `RESOURCE.amount`, `cost` – „dicke" Beschichtung
  - `scaleRange = X, Y` – wie groß man sie ziehen kann (CDH-1: 0.25–20, CDH-2: 2–20)
  - `defaultScale` – Größe beim Anbringen (CDH-2 spawnt bei 6× für schnelle Deckung)
  - `defaultDepth` – wie „weit oben" sie über der Oberfläche schwebt
- **Schnell platzieren:**
  1. Panel aus Thermal-Kategorie nehmen → aufs Zielvolumen klicken (CDH-2 spawnt groß).
  2. Rechtsklick aufs Panel → **Scale** draggen (bzw. bei Bedarf rotieren/gmirror über Mod-Tools).
  3. Mehrere Panels nebeneinander = Shift-Klick auf denselben Bauplatz (Part-Kopie) → zügige Flächendeckung.
  4. Textur-Variante über den B9-Umschalter nachziehen — der bleibt beim Skalieren erhalten.
- Ausprobieren im Flug: auf Wiedereintritt von Kerbin/Mun fliegen und im
  Part-Menü (Rechtsklick) Ablator-Fortschritt beobachten.

---

## CKAN

Alles für die Veröffentlichung über CKAN liegt in `CKAN/`
(`.netkan`-Metadaten, Anleitung und `build-release.ps1` für das Release-ZIP).
Die Mod ist dort noch **nicht** gelistet — dafür braucht es ein GitHub-Release
und einen PR im `KSP-CKAN/NetKAN`-Repo. Details: `CKAN/README.md`.

## Kompatibilität

- ConformalDecals **Required** (ist installiert, v0.4.2, KSP 1.12)
- B9PartSwitch **Required** (Mustervarianten; installiert)
- ModuleManager (installiert)
- **ReStock**: Die radialen Schilder (CDH-10…13) nutzen die ReStock-Modelle
  (`ReStock/Assets/Aero/restock-heatshield-*`), weil ReStock die
  Squad-Original-Modelle aus der Datenbank entfernt. Die Schilder bekommen
  dieselben Farbvarianten wie die Stock-Hitzeschilde (Classic/Black/Red).
- **KSPCommunityFixes**: ohne es lädt die PNG-Atlas-Textur langsamer
  (mit KSPCF wird beim ersten Laden zu DXT5 konvertiert und gecached).
