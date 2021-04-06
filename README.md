# ImageEffectGraph
Image effects for post processing stack created with shader graph for Unity

<p align="center">
  <img src="Preview/showcase.gif"><br>
  <img src="Preview/ar.gif"><br>
  <i>ImageEffectGraph running on an <a href="https://github.com/iBicha/MobileARTest">AR Foundation app</a></i>
</p>

This repo is attempt to extend ShaderGraph to create postprocessing effects compatible with the Postprocessing Stack, using the either the new Scriptable Render Pipeline (HD/Lightweight) or legacy graphics.

##### :warning: Unity is working to officially add "Single-pass post-effects support" to the Shader Graph package (so you don't need this repository anymore). If this feature is important to you, check out the <a href="https://portal.productboard.com/8ufdwj59ehtmsvxenjumxo82/c/55-single-pass-post-effects-support">card on our Public Roadmap about Single-pass PostFX support</a>, which would be a good place for you to share your thoughts and vote.
##### :warning: 2019.1 is not supported. Unity made its shadergraph api less accessible, making custom master nodes not possible. See <a href="https://github.com/iBicha/ImageEffectGraph/issues/11">#11</a>  
##### :warning: This repo is extremely experimental, hacky and buggy. For now, it is everything you don't want to use in production. 
(In simpler words, things are constantly getting fixes, and there are constant changes to serialization and shader generation, which might not be backward compatible with effects created with a previous version of this tool. This shall change once this repo gets to a stable state)

### Getting started
To get started, create a effect graph using the context menu `Assets -> Create -> Shader -> Image Effect Graph`. Create a material, and assign the shader to it. Finally, add the `Render With Material` effect to your postprocessing volume, and assign the material to it.

Please note that there are a couple of effect examples (Invert colors, camera transitions, TV flicker, etc...)
The demo scenes will cycle through them automatically to showcase the examples.

```
Please note that the example comes with 3 scenes, for legacy, HD pipeline, and lightweight graphics.
Make sure to select an appropriate pipeline asset when trying a scene.
```
<p align="center">
  <img src="Preview/transition.gif"><br>
  <img src="https://raw.github.com/iBicha/ImageEffectGraph/master/Preview/graph.png" width="600"><br>
</p>

### Using multiple effects
Please refer to [this](https://github.com/iBicha/ImageEffectGraph/issues/7)

### Acknowledgements
##### Camera transition effect textures
I think the original idea of the camera transition effect was from [here](https://www.youtube.com/watch?v=LnAoD7hgDxw) (the shader is pretty different though, and is created with shader graph)
Nonetheless, the [texture files](Assets/Sample/Assets/Textures) for the transitions were definitely from the package of that tutorial (Thus belongs to their creator, and are not under MIT license - but under CC 4.0 last time I checked).
##### TV effect
Taken straight from [keijiro/ShaderGraphExamples](https://github.com/keijiro/ShaderGraphExamples/tree/master/Assets/Examples/TV)
##### Overlay effect texture
Texture from [keijiro/SketchyFx (OTF_Crumpled_Paper_08.jpg)](https://github.com/keijiro/SketchyFx/blob/master/Assets/Textures/OTF_Crumpled_Paper_08.jpg) by Brent Leimenstoll
##### Regular Hexagon Tiling effect
Translated from [ShaderToy](https://www.shadertoy.com/view/4ldGWB), original shader by [_pwd_](https://www.shadertoy.com/user/_pwd_)
