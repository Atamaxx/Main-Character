using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

public class FMODEvents : StaticInstance<FMODEvents>
{
    [field: SerializeField, BoxGroup("MUSIC")]
    public EventReference Music { get; private set; }

    [field: SerializeField, BoxGroup("AMBIENT")]
    public EventReference Ambient { get; private set; }

    [field: SerializeField, BoxGroup("BRITNEY")]
    public EventReference WalkingSFX { get; private set; }

    [field: SerializeField, BoxGroup("BRITNEY")]
    public EventReference WalkingSFXLoop { get; private set; }

    [field: SerializeField, BoxGroup("BRITNEY")]
    public string WalkingIntensityParam { get; private set; } = "walking_intensity";

    [field: SerializeField, BoxGroup("LETTERS")]
    public EventReference FillLetterSFX { get; private set; }

    [field: SerializeField, BoxGroup("LETTERS")]
    public EventReference UnfillLetterSFX { get; private set; }

    [field: SerializeField, BoxGroup("LEVEL RESET")]
    public EventReference FillCheckpointSFX { get; private set; }

    [field: SerializeField, BoxGroup("LEVEL RESET")]
    public EventReference UnfillCheckpointSFX { get; private set; }

    [field: SerializeField, BoxGroup("LEVEL RESET")]
    public EventReference LevelResetSFX { get; private set; }

    [field: SerializeField, BoxGroup("LEVEL RESET")]
    public EventReference LevelResetHoldSFX { get; private set; }

    [field: SerializeField, BoxGroup("BOOK")]
    public EventReference PageFlip { get; private set; }

    [field: SerializeField, BoxGroup("MENU")]
    public EventReference ButtonSelectSFX { get; private set; }
}
