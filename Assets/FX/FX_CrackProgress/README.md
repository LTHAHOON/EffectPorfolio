# Branching Crack Progress (URP)

`SG_CrackProgress` reveals a branching crack in the order stored in the mask texture.

## Packed mask

- **R**: crack shape/intensity (`0` outside the crack, `1` on the crack)
- **G**: reveal order (`0` at the origin, `1` at the tips)
- Import custom masks with **sRGB disabled**, **Wrap Mode = Clamp**, and no color compression when possible.

## Graph logic

```text
Sample Mask.G - Edge Softness -> Smoothstep Edge 1
Sample Mask.G                 -> Smoothstep Edge 2
Progress                      -> Smoothstep In

Sample Mask.R * Smoothstep -> Alpha
Crack Color * Emission     -> Base Color
Normal Texture -> Normal Strength -> Normalize
Fake Light Direction       -> Normalize
Dot(Normal, Light) -> Saturate -> Shadow/Bright Range
Base Color * Fake Lighting -> Base Color
```

This implements a softened version of `ProgressMask <= Progress`. Every branch advances according to its own G-channel values, independent of the plane UV's X/Y direction.

## Use

1. Put `M_CrackProgress_Sample` on a Plane or Quad placed slightly above the ground.
2. Animate the material's **Progress** from `0` to `1`, or add `CrackProgressPlayer` to the renderer.
3. Replace `T_CrackProgress_Sample` with your own packed RG mask.
4. Use **Edge Softness** to control the softness of the moving reveal front.
5. Use **Crack Size** to make the crack thinner or wider, and **Border Softness** to anti-alias its side edges.

`Alpha Clip` should stay disabled on crack materials. Hard clipping discards the soft edge and makes thin cracks look stair-stepped.

The shader is transparent, unlit, double-sided, does not write depth, and does not cast or receive shadows.
The normal map is lit by a tangent-space fake light inside the graph, so it adds depth without switching to a Lit shader.
Use **Normal Strength**, **Fake Light Direction**, **Shadow Brightness**, and **Fake Light Intensity** to tune the relief.
