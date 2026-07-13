An atmospheric adventure narrative puzzle game.

Main Character is a 2D narrative-driven puzzle game built entirely from the ground up in Unity.
It explores themes of loneliness, alienation, morality, and truth through a seamless blend of custom-built mechanics, diegetic interfaces, and atmospheric audio-visual synergy.

Everything in this repository, from core gameplay architecture to asset integration, was developed solo.
    Engine: Unity (2D)
    Language: C#
    Audio Middleware: FMOD
    Graphics/Rendering: Custom Shaders (HLSL) & Shader Graph

  Key Technical Highlights
    Rendering & Procedural Animation: Visuals heavily rely on custom-authored shaders. Assets are processed directly through shaders to achieve a distinct artistic style, maintaining strict performance overhead. 
    Animations are either procedurally generated via code or driven directly within the shaders.
    Dynamic Audio Integration (FMOD): Fully integrated dynamic soundscapes, including custom SFX, ambient tracks, and original music. The audio is handled via FMOD to ensure seamless transitions, parallel processing, and deep cohesion with gameplay states.
    Diegetic User Interface: UI architecture is designed to be completely invisible and woven directly into the game world. Instead of traditional menus, navigation and interactions occur within full scenes, creating an unbroken, immersive player experience.


  Gameplay & Narrative Design
The core mechanic revolves around deep interaction with text and interpretation.
    The Dynamic: The narrative is built on an asymmetrical dialogue between the Author (who speaks to the player) and the Main Character (the player, who cannot respond directly).
    Exploration: Levels are designed with a layered approach, containing hidden areas and secrets that encourage replayability. The environment is built to reveal new narrative details on subsequent playthroughs, much like re-reading a complex piece of literature.
    Synergy: The game relies on the synergetic fusion of all its art forms (visuals, FMOD audio, and text). The architecture ensures that these systems work in parallel so that the atmosphere remains unbroken even if the player misses a specific clue.

  All assets are originally produced for this project:
    Textures and sprites are captured, processed, and shader-manipulated internally.
    The soundtrack is currently composed utilizing MIDI architecture, with planned implementation of live recorded acoustic/electro-acoustic guitar pieces and vocal tracks perfectly synchronized with the game's emotional beats.
