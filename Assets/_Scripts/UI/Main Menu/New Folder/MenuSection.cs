using UnityEngine;
using Unity.Cinemachine;
using NaughtyAttributes;

// Define menu sections as an enum
public enum MenuSectionType
{
    MainMenu,
    ContinueStory,
    LevelSelect,
    Discoveries,
    Options
}

public class MenuSection : MonoBehaviour
{
    [BoxGroup("Section Identity")]
    [Dropdown("GetMenuSectionTypes")]
    public MenuSectionType SectionType;

    [BoxGroup("Section Identity")]
    [Required("Each menu section must have a camera")]
    [SerializeField] private CinemachineCamera _sectionCamera;



    

}