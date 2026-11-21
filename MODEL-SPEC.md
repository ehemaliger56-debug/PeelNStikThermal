# Part model guide

Model contributions are very welcome — a hand-made mesh would make a big
difference for these parts. This page lists the conventions so a submitted
model drops straight into the part configs.

## What would help most

- **Heat sinks (CDH-4 to CDH-7):** all four parts share one mesh, so a single
  model upgrades the whole family. This is the top priority.
- **CDH-8 Strip Heat Sink:** a separate, narrower model is wanted for this
  one - a slim strip for trusses and wing edges. Same pad-at-z=0 orientation
  as the other sinks, but a long and narrow footprint (roughly 0.7 m x 0.15 m
  at scale 1, body about 0.8 m tall). The part config already points at
  `PeelNStikThermal/Assets/chs-sink-strip`; drop the `.mu` and texture there
  and it will pick them up.
- **Coatings (CDH-1, CDH-2):** a slim conformal panel look, used at many
  scales.
- **Radial shields (CDH-10 to CDH-13):** alternative shield shapes are
  welcome too.

## Formats

| What you have | What to send |
|---|---|
| Unity + KSP PartTools, or Blender with a KSP export tool | A finished `.mu` plus its texture — ready to drop in |
| Blender, SketchUp, 3D modeling software in general | A source mesh (`.obj`, `.fbx`, `.dae`) plus a PNG texture — I'll handle the import |

## Orientation

- The part origin sits at the **centre of the mounting pad** (the flat side
  that touches the hull).
- The pad lies in the local **X/Y plane at z = 0** and faces **-Z**, toward
  the hull.
- The visible body extends along **+Z**, away from the hull.
- The attach node in the config is at the origin with normal `(0, 0, 1)`.
  KSP ignores the model's root transform for attachment, so the mesh itself
  must be authored with this orientation.

## Size

- Reference mesh at scale 1: pad roughly **0.5 m across**, body roughly
  **1.0-1.3 m tall** and **0.1 m deep**. Different proportions are fine;
  the per-part scale can be adjusted.
- Current scale values: CDH-4 **1.0**, CDH-8 **1.0** (own model), CDH-5 **2.0**,
  CDH-6 **1.775**, CDH-7 **1.0**.
- Keep it low-poly: **under 5,000 triangles**. One mesh and one material are
  enough.

## Texture

- A **PNG** diffuse texture is ideal; **1024x1024** is plenty.
- Optional normal or specular maps are welcome too.
- Filenames are not important — anything clear will do.

## Look (just a suggestion)

- A dark adhesive pad at the base, in the spirit of the Peel-N-Stik shields.
- An ablative surface above it: quilted, honeycomb or tile patterns fit the
  coating style of the rest of the mod.
- Optional metal frame or cooling fins along the sides.
- A subtle warm-to-cool gradient from the exposed face toward the hull.

## Sending it

Open an issue or a pull request in the repository and attach the files, or
drop them into the mod folder and let me know where they are. I'll wire the
model into the part configs and test it in game; if anything needs rotating
or resizing, we can sort that out together. Thanks!
