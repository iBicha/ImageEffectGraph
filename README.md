##### :warning: This repo is extremely experimental, hacky and buggy. For now, it is everything you don't want to use in production. 

# ImageEffectGraph
Image effects for post processing stack created with shader graph for Unity

This repo is attempt to extend ShaderGraph to create postprocessing effects compatible with the Postprocessing Stack, using the either the new Scriptable Render Pipeline (HD/Lightweight) or legacy graphics.

### Getting started
To get started, create a effect graph using the context menu `Assets -> Create -> Shader -> Image Effect Graph`. Create a material, and assign the shader to it. Finally, add the `Render With Material` effect to your postprocessing volume, and assign the material to it.

Please note that there are a couple of effect examples (Invert colors, and camera transitions)

```
Please note that the example comes with 3 scenes, for legacy, HD pipeline, and lightweight graphics.
Make sure to select an appropriate pipeline asset when trying a scene.
```

![](Preview/transition.gif)
<img src="https://raw.github.com/iBicha/ImageEffectGraph/master/Preview/invert.png" width="600">

# Camera transition effect textures
I think the original idea of the camera transition effect was from [here](https://www.youtube.com/watch?v=LnAoD7hgDxw) (the shader is pretty different though, and is created with shader graph)
Nonetheless, the [texture files](Assets/Sample/Assets/Textures) for the transitions were definitely from the package of that tutorial (Thus belongs to their creator, and are not under MIT license - but under CC 4.0 last time I checked).
