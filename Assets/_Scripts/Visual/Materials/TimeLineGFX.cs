using UnityEngine;

public class TimeLineGFX : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private TimeLine _timeLine;
    [SerializeField] private string _percentPropertyRef = "_PercentCompleted";
    [SerializeField] private string _unFillColorPropertyRef = "_Color";
    [SerializeField] private string _fillColorPropertyRef = "_Complete_Color";
    private int _percentPropertyID;
    private int _fillColorPropertyID;
    private int _unFillColorPropertyID;
    private float _percentCompleted;

    private void Start()
    {
        _percentPropertyID = Shader.PropertyToID(_percentPropertyRef);
        _unFillColorPropertyID = Shader.PropertyToID(_unFillColorPropertyRef);
        _fillColorPropertyID = Shader.PropertyToID(_fillColorPropertyRef);
    }

    void Update()
    {
        _percentCompleted = _timeLine.PercentPassed;
        ChangeMaterialProperties();
    }


    private void ChangeMaterialProperties()
    {
        _material.SetFloat(_percentPropertyID, _percentCompleted);
    }

    public void ChangeColors()
    {
        _material.SetColor(_unFillColorPropertyRef, ColorController.Instance.Palette.BasicColorHDR);
        _material.SetColor(_fillColorPropertyRef, ColorController.Instance.Palette.FillColorHDR);
    }
}
