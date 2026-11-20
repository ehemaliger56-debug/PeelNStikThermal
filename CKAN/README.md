# Publishing PeelNStikThermal on CKAN

CKAN does not discover mods by scanning a local `GameData` folder. It needs
(1) a `.netkan` metadata file and (2) a download it can resolve. For this mod
the download is a GitHub Release.

## Current status

- Repository: https://github.com/ehemaliger56-debug/PeelNStikThermal
- Release `1.0.0` with `PeelNStikThermal-1.0.0.zip` is published.
- `PeelNStikThermal.netkan` in this folder is ready to use.
- Remaining step: open a pull request at `KSP-CKAN/NetKAN` adding this file as
  `NetKAN/PeelNStikThermal.netkan`. After review the mod shows up in CKAN.

## Building the release ZIP

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build-release.ps1 -Version 1.0.0
```

This creates `CKAN\PeelNStikThermal-1.0.0.zip`. The top level inside the
ZIP must be `PeelNStikThermal/`, exactly as it appears in `GameData`.
Attach the ZIP to a published GitHub release (tag `1.0.0`).

## Checking the metadata

```powershell
# netkan.exe: https://ksp-ckan.s3-us-west-2.amazonaws.com/netkan.exe
.\netkan.exe --verbose PeelNStikThermal.netkan
```

On success this produces `PeelNStikThermal-1.0.0.ckan`. To test locally:

```powershell
.\ckan.exe install -c PeelNStikThermal-1.0.0.ckan
.\ckan.exe show PeelNStikThermal
```

## Going public

Open a PR in `KSP-CKAN/NetKAN` adding the file as
`NetKAN/PeelNStikThermal.netkan`. Continuous integration checks it
automatically; after the CKAN team's review the mod appears in the client.
`NETKAN-PR.md` in this folder contains a ready-to-copy title and description.

Another option without a team review is a personal metadata repository: put
the generated `.ckan` file in a GitHub repo and add its archive URL under
*Settings -> Metadata Repositories* in CKAN. Only people who subscribe to that
repo will see it.

## Dependencies in the metadata

| Relation | Mods | Why |
|---|---|---|
| `depends` | ModuleManager, ConformalDecals, B9PartSwitch, ReStock | Aerodynamic patch, decal system, surface patterns, radial shield models |
| `recommends` | Firefly | Optional: plasma effects play at the energy bubble instead of the hull |
| `suggests` | SystemHeat, HeatControl | Related thermal mods |

License: `restricted` is the CKAN key for "All Rights Reserved". The repo
includes a `LICENSE` file stating this.
