using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

public class FMODEventsMenu : StaticInstance<FMODEventsMenu>
{
    [field: SerializeField, BoxGroup("MENU")] public EventReference MenuMusic { get; private set; }
    //[field: SerializeField, BoxGroup("MENU")] public EventReference ButtonDeselectSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference ButtonClickSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference ButtonBackSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference ToggleSwitchSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference MenuTransitionSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference MenuPauseSFX { get; private set; }
    [field: SerializeField, BoxGroup("MENU")] public EventReference MenuResumeSFX { get; private set; }
}
