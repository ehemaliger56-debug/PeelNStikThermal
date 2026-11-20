# NetKAN pull request draft

Everything below is ready to copy when opening the pull request. No PR has been
opened yet.

## Target

- Repository: `KSP-CKAN/NetKAN`
- Add this file: `NetKAN/PeelNStikThermal.netkan`
- Source file: `CKAN/PeelNStikThermal.netkan` in this repository

## PR title

Add Peel-N-Stik Thermal (PeelNStikThermal) 1.0.0

## PR body

```markdown
This adds Peel-N-Stik Thermal, a set of stick-on ablative heat shields and
active heat sinks for KSP 1.12.

- Mod name: Peel-N-Stik Thermal
- Identifier: PeelNStikThermal
- Version: 1.0.0
- KSP version: 1.12
- License: restricted (All Rights Reserved; the repository includes a LICENSE
  file stating this)
- Repository: https://github.com/ehemaliger56-debug/PeelNStikThermal
- Release: https://github.com/ehemaliger56-debug/PeelNStikThermal/releases/tag/1.0.0
- Download: https://github.com/ehemaliger56-debug/PeelNStikThermal/releases/download/1.0.0/PeelNStikThermal-1.0.0.zip
- Dependencies: ModuleManager, ConformalDecals, B9PartSwitch, ReStock
- Recommended: Firefly

Validation:

- `netkan.exe --verbose PeelNStikThermal.netkan` completed without warnings
  or errors.
- Generated metadata: identifier `PeelNStikThermal`, name `Peel-N-Stik Thermal`,
  license `restricted`, download size 15,929,809 bytes.
- SHA-256: `25A53696214033A539C7F5071BA2F054ADD432C67FFB79B10FB74D26E2766AEE`
- The ZIP contains only `GameData/PeelNStikThermal` contents; no tool files or
  repository metadata are included.

Thanks for reviewing.
```

## Steps when ready

1. Fork `KSP-CKAN/NetKAN` into the author's GitHub account.
2. Create a branch, e.g. `add-peelnstikthermal`.
3. Copy `CKAN/PeelNStikThermal.netkan` from this repository to
   `NetKAN/PeelNStikThermal.netkan` in the fork.
4. Commit, push, and open the pull request against `KSP-CKAN/NetKAN` using the
   title and body above.
5. Wait for CI and the CKAN team's review. After a merge the bot publishes the
   generated `.ckan` to the CKAN index and the mod becomes searchable.

## Pre-flight check

- [x] GitHub repository is public and renamed to `PeelNStikThermal`.
- [x] Release `1.0.0` is published with the matching ZIP asset.
- [x] `.netkan` URLs and `$kref` point to the public repository.
- [x] `netkan.exe` inflation succeeds with no warnings.
- [x] LICENSE file states All Rights Reserved.
- [x] Mod is also documented in `README.md` and `CKAN/README.md`.
