# ConformalHeatShields für CKAN veröffentlichen

CKAN findet Mods **nicht** durch Scannen des lokalen `GameData`-Ordners.
Es braucht (1) eine `.netkan`-Metadatendatei und (2) einen Download, den
CKAN auflösen kann. Für diese Mod: GitHub Release.

## 1. GitHub vorbereiten

1. Repo `ConformalHeatShields` anlegen (Name muss zum Identifier passen).
2. Inhalt dieses Modordners hochladen: `GameData/ConformalHeatShields/**`,
   `README.md`, `Source/`, `CKAN/` und eine `LICENSE`-Datei mit dem Text
   *„All rights reserved. No permission to redistribute or modify is granted."*
3. `<DEIN_GITHUB_USER>` in `ConformalHeatShields.netkan` durch deinen
   GitHub-Namen ersetzen (kommt in `homepage`, `repository`, `bugtracker`
   und `$kref` vor).

## 2. Release-ZIP bauen

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1 -Version 1.0.0
```

Das erzeugt `CKAN\ConformalHeatShields-1.0.0.zip`. Wichtig: Im ZIP muss
`ConformalHeatShields/` die oberste Ebene sein — genau wie im `GameData`-Ordner.
Das ZIP kommt als Asset an ein GitHub Release (Tag `1.0.0`).

## 3. Metadaten prüfen

```powershell
# netkan.exe von https://ksp-ckan.s3-us-west-2.amazonaws.com/netkan.exe
.\netkan.exe --verbose ConformalHeatShields.netkan
```

 Bei Erfolg entsteht `ConformalHeatShields-1.0.0.ckan`. Optional lokal testen:

```powershell
.\ckan.exe install -c ConformalHeatShields-1.0.0.ckan
.\ckan.exe show ConformalHeatShields
```

## 4. Öffentlich listen lassen

PR im Repo `KSP-CKAN/NetKAN` öffnen und die Datei als
`NetKAN/ConformalHeatShields.netkan` hinzufügen. Die CI prüft sie automatisch;
nach dem Review durch das CKAN-Team taucht die Mod in der CKAN-Liste auf.

Alternative ohne Team-Review: eigenes Metadata-Repo. Die `.ckan`-Datei in ein
GitHub-Repo legen und in CKAN unter *Settings → Metadata Repositories* die URL
`https://github.com/<user>/<repo>/archive/main.tar.gz` hinzufügen. Nur für
dich bzw. Leute, die dieses Repo manuell abonnieren.

## Abhängigkeiten in den Metadaten

| Relation | Mods | Warum |
|---|---|---|
| `depends` | ModuleManager, ConformalDecals, B9PartSwitch, ReStock | Aero-Patch, Decal-System, Muster-Varianten, Radial-Schild-Modelle |
| `recommends` | Firefly | optional: Plasma-FX werden an der Blase statt am Rumpf gezeigt |
| `suggests` | SystemHeat, HeatControl | thematisch verwandte Radiatoren/Heat-Systeme |

Lizenz: `restricted` ist der CKAN-Schlüssel für „All Rights Reserved" (nicht
weiterverteilbar). Das CKAN-Team kann bei Bedarf eine eindeutige
`LICENSE`-Datei im Repo verlangen.
