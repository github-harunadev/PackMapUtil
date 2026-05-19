# PackMapUtil

A lazy util made for myself. Simple utility that can help convert existing PackMaps to various formats.

<img src="https://raw.githubusercontent.com/github-harunadev/PackMapUtil/refs/heads/main/screenshot.png" width=360>

Example:
- MetallicSmoothness(RA) --> OcclusionRoughnessMetallic(RGB)
- SpecularSmoothness(RGBA) --> MetallicSmoothness(RA)
- or any custom maps using custom channel input/outputs

## How to install
- Download [latest release](https://github.com/github-harunadev/PackMapUtil/releases/latest) from Releases
- Import to your project

## How to use
- On Unity Editor menu bar (top of the window), open `harunadev > PackMap Util`
  - or you can first select the source map texture and then open PackMap Util
  - or you can first select multiple source map textures and then open PackMap Util
- Insert source map (if you haven't yet)
- Select which channels the source map is using for each property (ex: Unreal ORM: Occlusion => R, Smoothness => G, Metallic => B)
  - Check on `Source Is Smoothness` if the smoothness of source map is roughness
  - Check on `Source Is Specular` if source map's RGB channel is specular
- Select which channels the target (result) map will be using for each property
- (Optional) Insert prefix/postfix (if none, original file will be overwritten!)
- Press `Save PackMap` Button (or `Save Packmaps` if multiple sources are selected)
- Check the result and enjoy

## How does it work
Use material and custom Unlit shader. Pixel processing is done on the shader via Graphics.Blit

## AI Usage (In any case you hate AI in any chance)
92% Human code I think

## License
MIT License
