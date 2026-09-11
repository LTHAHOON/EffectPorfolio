# Aknal Rock variants V2

## Changes requested

- Base Color uses the original source palette with no recoloring or grading.
- Texture offsets were removed because translating this non-tileable bake exposes UV dilation padding as stretched bands.
- Every PBR texture is now a byte-identical copy of the clean source map, with its original UV padding preserved.
- All three meshes share a clearly pointed upper silhouette.
- Displacement character differs by mesh: large/broad, small/fine, and mixed/asymmetric.

## Maps

Every `T_Aknal_Rock_XX` folder contains 2048×2048 Base Color, Height, Metallic, AO, OpenGL Normal, DirectX Normal, and Roughness maps.

Use `Normal.png` in Unity. Use `Normal_DirectX.png` for Unreal Engine and other DirectX-normal workflows.
