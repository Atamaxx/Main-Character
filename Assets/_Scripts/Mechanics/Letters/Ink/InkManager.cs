using UnityEngine;

public abstract class InkManager : StatefulMonoBehaviour
{
    // Abstract properties and methods...
    public abstract int CurrentInkAmount { get; protected set; }
    public abstract int MaxInkAmount { get; protected set; }
    public abstract int MinInkAmount { get; protected set; }
    public abstract bool CanFill { get; protected set; }
    public abstract bool CanUnfill { get; protected set; }

    public abstract bool CanUseInk(int amount);
    public abstract bool CanRestoreInk(int amount);
    public abstract void UseInk(int amount);
    public abstract void RestoreInk(int amount);

    protected override void OnStatefulAwake()
    {
        // Perform any InkManager-specific initialization here.
        OnAwake();
    }

    protected virtual void OnAwake() { }

    [System.Serializable]
    private struct InkManagerSaveData
    {
        public int currentInkAmount;
    }

    public override object CaptureState()
    {
        InkManagerSaveData data = new InkManagerSaveData { currentInkAmount = CurrentInkAmount };
        return data;
    }

    public override void RestoreState(object state)
    {
        if (state is InkManagerSaveData data)
        {
            CurrentInkAmount = data.currentInkAmount;
        }
        else
        {
            Debug.LogError("Failed to restore state: invalid state object.");
        }
    }
}
