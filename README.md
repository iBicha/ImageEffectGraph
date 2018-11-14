# ImageEffectGraph
Image effects for post processing stack created with shader graph for Unity

![](Preview/showcase.gif)

This repo is attempt to extend ShaderGraph to create postprocessing effects compatible with the Postprocessing Stack, using the either the new Scriptable Render Pipeline (HD/Lightweight) or legacy graphics.

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

![](Preview/transition.gif)
<img src="https://raw.github.com/iBicha/ImageEffectGraph/master/Preview/invert.png" width="600">

`Old screenshot: the "Main Texture" input is now separated from the sampler, use a "Sample Texture 2D" to sample it. (see examples and/or create a new effect)`

### acknowledgement
##### Camera transition effect textures
I think the original idea of the camera transition effect was from [here](https://www.youtube.com/watch?v=LnAoD7hgDxw) (the shader is pretty different though, and is created with shader graph)
Nonetheless, the [texture files](Assets/Sample/Assets/Textures) for the transitions were definitely from the package of that tutorial (Thus belongs to their creator, and are not under MIT license - but under CC 4.0 last time I checked).
##### TV effect
Taken straight from [keijiro/ShaderGraphExamples](https://github.com/keijiro/ShaderGraphExamples/tree/master/Assets/Examples/TV)
##### Overlay effect texture
Texture from [keijiro/SketchyFx (OTF_Crumpled_Paper_08.jpg)](https://github.com/keijiro/SketchyFx/blob/master/Assets/Textures/OTF_Crumpled_Paper_08.jpg) by Brent Leimenstoll
