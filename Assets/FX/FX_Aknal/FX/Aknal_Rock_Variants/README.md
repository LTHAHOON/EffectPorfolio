# Aknal Rock variants

This folder is generated from `Aknal_Rock_01_Complete.fbx` and its 2048×2048 PBR texture set.

## Geometry check

- Source mesh: 3,442 vertices / 6,880 triangles
- Open boundary edges: 0
- Duplicate vertices within 0.0001 scene units: 0
- UV vertices: 3,536

No physical geometry seam was found, so the source asset was not destructively re-welded. The visible color outside UV islands is texture padding that helps suppress mip-map seams.

## Generated assets

- `Aknal_Rock_02.fbx` — squat, broad variation / Ember Moss palette
- `Aknal_Rock_03.fbx` — taller, narrower variation / Cold Verdigris palette
- `Aknal_Rock_04.fbx` — asymmetric variation / Violet Ash palette

Each `T_Aknal_Rock_XX` folder contains Base Color, Height, Metallic, AO, OpenGL Normal, DirectX Normal, and Roughness maps at 2048×2048.

For Unity, use `Normal.png` as the normal map. For Unreal Engine or other DirectX-normal workflows, use `Normal_DirectX.png`.
