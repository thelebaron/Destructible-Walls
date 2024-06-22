

# Authoring animations

### Root motion
Root motion should ideally be constrained to a transform node that is named "RootController". 
See https://www.youtube.com/watch?v=G9MuiikNe1c for details on doing so inside of Cascadeur.

# Issues
On models where rigs have Strip Bones enabled, this can result in the character being distorted. To fix, strip bones
should be disabled, and when binding the skin in Maya, "Remove unused influences" should also be unchecked.



old readme
# unity-dots-animation
Simple animation system for the unity dots stack.

## Samples

### Benchmark Scence
![Sample Gif](Samples~/sample.gif)

To execute the sample:

1. Open or create a unity 2022.2 project using the URP pipeline
2. Add the package using the package manager -> "Add package from git url" using the https url of this repo
3. Go to the samples tab of this package in the package manager and open the benchmark sample
4. Open the "SampleScene"

## Usage

### Setup
1. Install the package using the package manager -> "Add package from git url" using the https url of this repo
2. Add the `AnimationsAuthoring` component to the root entity of a rigged model/prefab
3. Add animation clips to the "clips" list
4. Add the `SkinnedMeshAuthoring` component to any children that should be deformed (have a skinned mesh renderer)

Now the first animation clip should be executed on entering playmode.

### Playing & switching animations

Use the `AnimationAspect` to to easily play and switch animations.
The animation clip will also start from 0 even if the same index is used again.

This example plays the animationClip with index 1 for all entities:
```csharp
var clipIndex = 1;
foreach (var animationAspect in SystemAPI.Query<AnimationAspect>())
{
    animationAspect.Play(clipIndex);
}
```

For more advanced usage, you can modify the `AnimationPlayer` component directly.
