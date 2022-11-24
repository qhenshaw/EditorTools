# Editor Tools
[![Unity 2021.3+](https://img.shields.io/badge/unity-2021.3%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-brightgreen.svg)](LICENSE.md)

This package comes with several useful tools focused on organization, level design, and lighting.

## System Requirements
Unity 2021.3+. Will likely work on earlier versions but this is the version I tested with.

## Installation
Use the Package Manager and use Add package from git URL, using the following: 
```
https://github.com/qhenshaw/EditorTools.git
```

## Included
The follows tools are included in the paackage. The Prefab Placer tool requires the Odin Inspector plugin in the project to function.

### Custom Hierarchy Drawer
Redraws any items in the hierarchy that have ```= ``` at the start of their name with bold text and a darker background. Useful for highlighting empty category transforms.

### Prefab Swapper
```
Open through Tools => Prefab Swapper
```
This tool allows you to swap GameObjects in the scene with Prefabs, optionally copying the rotation and scale of the original.

### Prefab Placer (requires Odin Inspector plugin)
```
Open through Tools => Prefab Placer
```
This tool allows you to paint prefabs in the scene with a number of customizable values. Objects to be painted on require colliders.

### Light Probe Volume
```
Add to scene through GameObject => Light => Light Probe Volume
```
Adds any number of customizable volumes to the scene that will raycast against contained surfaces and automatically distribute Light Probes. Recalculatng probe positions can take several minutes in large scenes with high density.

### UCX Collision Importer
```
Automatically processes incoming 3D models
```
The UCX collision importer mimics Unreal's automatic handling of any meshes inside models named using the ```UCX_``` naming convention and adds mesh colliders to them.

### Material Creator
```
A materal creator for use with the unified Unity/Unreal pipeline and Uber shaders. This will be of no use outside of my classes.
```
Select the BaseColor/PackMap/NormalMap textures in the project view and create the proper material through:  
```Right Click => Create => Art Pipeline => [Material]```
