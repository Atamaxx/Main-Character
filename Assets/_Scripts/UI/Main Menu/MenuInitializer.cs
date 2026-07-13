using UnityEngine;
using UnityEngine.Events;

public class MenuInitializer : MonoBehaviour
{
    [SerializeField] private UnityEvent _onInit;
    [SerializeField] private CinemachineManager _cmManager;
    [SerializeField] private MenuController _menuController;
    [SerializeField] private NavigationGroup _mainMenuGroup;
    [SerializeField] private NavigationGroup _chapterSelectGroup;


    private void Start()
    {
        // Change state to Starting - which will now transition to Gameplay automatically
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.State == GameState.LevelCompletion)
            {
                _cmManager.SwitchToCameraByName("CM_chapters");
                _menuController.SetActiveGroup(_chapterSelectGroup);
            }
            else
            {
                _cmManager.SwitchToCameraByName("CM_main_menu");
                _menuController.SetActiveGroup(_mainMenuGroup);
            }
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }

        // Fire any custom initialization events
        _onInit?.Invoke();
    }
}
