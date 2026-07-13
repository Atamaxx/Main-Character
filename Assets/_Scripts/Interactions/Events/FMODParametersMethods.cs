using UnityEngine;

public class FMODParametersMethods : MonoBehaviour
{
    [SerializeField] private string _parameterName;


    public void SetMusicGlobalParameter(float value)
    {
        AudioSystem.Instance.SetMusicGlobalParameter(_parameterName, value);
    }

    public void SetMusicLocalParameter(float value)
    {
        AudioSystem.Instance.SetMusicParameter(_parameterName, value);
    }

    public void SetAmbienceLocalParameter(float value)
    {
        AudioSystem.Instance.SetAmbienceParameter(_parameterName, value);
    }
}
