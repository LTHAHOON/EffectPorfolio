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
```

This implements a softened version of `ProgressMask <= Progress`. Every branch advances according to its own G-channel values, independent of the plane UV's X/Y direction.

## Use

1. Put `M_CrackProgress_Sample` on a Plane or Quad placed slightly above the ground.
2. Animate the material's **Progress** from `0` to `1`, or add `CrackProgressPlayer` to the renderer.
3. Replace `T_CrackProgress_Sample` with your own packed RG mask.
4. Use **Edge Softness** to control the softness of the moving reveal front.

The shader is transparent, unlit, double-sided, does not write depth, and does not cast or receive shadows.
