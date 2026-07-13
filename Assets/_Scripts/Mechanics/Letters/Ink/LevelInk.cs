using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LevelInk : MonoBehaviour
{
    [BoxGroup("RESTRICTIONS")] public int MaxInkAmount = 5; // how many letters can be filled 
    [BoxGroup("RESTRICTIONS"), SerializeField] private int _minAuraSpawnRate = 5;
    [BoxGroup("RESTRICTIONS"), SerializeField] private int _maxAuraSpawnRate = 150;
    [SerializeField] private VFXController _vfxController;
    public int CurrentInkAmount = 5;
    [SerializeField] private int _currentAuraSpawnRate;


    private void Start() {
        UpdateOnUseInk();
    }

    private void FixedUpdate()
    {
        _vfxController.Aura(_minAuraSpawnRate, _currentAuraSpawnRate);
    }

    public bool CanUseInk(int amount)
    {
        return CurrentInkAmount - amount >= 0;
    }

    public void UseInk(int amount)
    {
        int changes = CurrentInkAmount;
        CurrentInkAmount -= amount;
        if (CurrentInkAmount < 0) CurrentInkAmount = 0;

        if (changes != CurrentInkAmount)
            UpdateOnUseInk();
    }

    public void RestoreInk(int amount)
    {
        int changes = CurrentInkAmount;
        CurrentInkAmount += amount;
        CurrentInkAmount = Mathf.Clamp(CurrentInkAmount, 0, MaxInkAmount);

        if (changes != CurrentInkAmount)
        {
            UpdateOnUseInk();
        }
    }


    private void UpdateOnUseInk()
    {
        _currentAuraSpawnRate = (int)Mathf.Lerp(_minAuraSpawnRate, _maxAuraSpawnRate, (float)CurrentInkAmount / MaxInkAmount);
        _vfxController.RedrawBody(MaxInkAmount - CurrentInkAmount, MaxInkAmount);
    }
}
