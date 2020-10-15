# IA_IATK
Immersive analytics project utilizing the HoloLens 2 and the IATK

## Requires:
[Unity 2019.2.8f1](https://unity3d.com/get-unity/download/archive)

[IATK Library](https://github.com/benjaminchlee/IATK)

[Microsoft MRTK](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Installation.html)

Visual Studio

Microsoft Hololens 2

## Steps to set up new project

1. [Start a project follow all applicable steps, some might vary due to the version of unity used](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/unity/tutorials/mr-learning-base-02)
* A major differance is in the player settings which for unity 2019.2.8f1 -> project settings -> player -> virtual reality SDKs -> + -> Windows mixed reality
2. Drag and drop contents of IATK assets folder to unity project assets
3. Rightclick in unity to add game object -> iatk -> datasource
4. Set CSV location in "Data" setting of [IATK] New Data Source
5. rightclick add -> [IATK] New Visalisation
6. Set parameters in visualisation
* axis
* data source
* geometry
* anything else
7. Launch build .SLR from build folder
8. Change build settings to Release, ARM64, Remote Machine
9. Project -> Properties -> Debugging -> Machine Name -> Hololens 2 IPv4 
10. Debug -> Start without debuging