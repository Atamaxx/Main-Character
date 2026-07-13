using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Letters
{
    public class LetterFiller : StatefulMonoBehaviour
    {
        [Required]
        public TMP_Text TmpText;

        [SerializeField, BoxGroup("SETTINGS")]
        private bool _canBeFilled = true;

        [SerializeField, BoxGroup("SETTINGS")]
        private bool _canBeUnfilled = true;

        [SerializeField, BoxGroup("SETTINGS")]
        private int _useInkAmount = 1;

        [SerializeField, BoxGroup("SETTINGS")]
        private int _restoreInkAmount = 1;

        [SerializeField, BoxGroup("SERIALIZE")]
        private LetterCollider _letterCollider;

        [SerializeField, BoxGroup("SERIALIZE")]
        private LetterVisual _letterVisual;

        [SerializeField, BoxGroup("INTERATIONS")]
        private bool _enableInterations = false;

        [SerializeField, BoxGroup("INTERATIONS"), ShowIf("_enableInterations")]
        private ConditionActionSystem _conditionSystem;

        [SerializeField, BoxGroup("INTERATIONS")]
        private bool _enableFilledEvent = false;

        [SerializeField, BoxGroup("INTERATIONS"), ShowIf("_enableFilledEvent")]
        private UnityEvent<float> _onFilledChange;

        [SerializeField, BoxGroup("INTERATIONS"), ShowIf("_enableFilledEvent")]
        private UnityEvent<bool> _onUnfilled;

        public bool[] filledCharacters;
        private Coroutine[] fillCoroutines;

        #region UNITY

        private void Reset()
        {
            if (TmpText == null)
            {
                TmpText = GetComponent<TMP_Text>();
            }
        }

        void Start()
        {
            if (TmpText == null)
            {
                Debug.LogError("TMP Text is not assigned on " + gameObject.name);
            }
            if (_enableInterations && _conditionSystem == null)
            {
                Debug.LogError("Condition System is not assigned on " + gameObject.name);
            }

            _letterCollider.CreateCollidersForCharacters(TmpText, this);
            _letterVisual.SetAllCharactersToStartColor(TmpText);

            DoInteration();
        }

        protected override void OnStatefulAwake()
        {
            TmpText.ForceMeshUpdate();
            int charCount = TmpText.textInfo.characterCount;
            fillCoroutines = new Coroutine[charCount];

            if (filledCharacters == null || filledCharacters.Length != charCount)
            {
                filledCharacters = new bool[charCount];
            }
            //this.ExecuteNextFrame(DoInteration);
        }

        void OnEnable()
        {
            RefillLetters();
        }

        #endregion

        #region FUNCTIONS
        public void OnLetterTouched(int charIndex, InkManager inkManager)
        {
            if (charIndex < 0 || charIndex >= filledCharacters.Length)
                return;

            if (fillCoroutines[charIndex] != null)
            {
                StopCoroutine(fillCoroutines[charIndex]);
            }

            if (!filledCharacters[charIndex])
            {
                if (inkManager.CanFill && inkManager.CanUseInk(_useInkAmount) && _canBeFilled)
                {
                    inkManager.UseInk(_useInkAmount);
                    filledCharacters[charIndex] = true;
                    fillCoroutines[charIndex] = StartCoroutine(
                        _letterVisual.FillCharacterOverTime(charIndex, false, TmpText)
                    );
                    AudioSystem.Instance.PlaySFXOneShot(
                        FMODEvents.Instance.FillLetterSFX,
                        transform.position
                    );
                    DoInteration();
                }
            }
            else if (
                inkManager.CanUnfill
                && inkManager.CanRestoreInk(_restoreInkAmount)
                && _canBeUnfilled
            )
            {
                filledCharacters[charIndex] = false;
                inkManager.RestoreInk(_restoreInkAmount);
                fillCoroutines[charIndex] = StartCoroutine(
                    _letterVisual.FillCharacterOverTime(charIndex, true, TmpText)
                );

                AudioSystem.Instance.PlaySFXOneShot(
                    FMODEvents.Instance.UnfillLetterSFX,
                    transform.position
                );
                DoInteration();
            }
        }

        public void DoInteration()
        {
            if (_enableInterations && _conditionSystem != null)
                _conditionSystem.CheckAndExecute();

            if (_enableFilledEvent)
            {
                int filledCount = 0;
                foreach (bool filled in filledCharacters)
                {
                    if (filled)
                        filledCount++;
                }
                _onFilledChange?.Invoke(filledCount);

                if (filledCount == 0)
                    _onUnfilled?.Invoke(true);
                else
                    _onUnfilled?.Invoke(false);
            }
        }

        public void RefillLetters()
        {
            for (int i = 0; i < filledCharacters.Length; i++)
            {
                if (fillCoroutines[i] != null)
                    StopCoroutine(fillCoroutines[i]);

                fillCoroutines[i] = StartCoroutine(
                    _letterVisual.FillCharacterOverTime(i, !filledCharacters[i], TmpText)
                );
            }
        }

        public void FillAllLetters()
        {
            TmpText.ForceMeshUpdate();
            for (int i = 0; i < filledCharacters.Length; i++)
            {
                filledCharacters[i] = true;
            }
            RefillLetters();
            DoInteration();
        }

        public void UnfillAllLetters()
        {
            TmpText.ForceMeshUpdate();
            for (int i = 0; i < filledCharacters.Length; i++)
            {
                filledCharacters[i] = false;
            }
            RefillLetters();
            DoInteration();
        }

        public void SetEnableInterations(bool value)
        {
            _enableInterations = value;
        }
        #endregion
        #region BUTTONS
        [Button("Find Letters")]
        private void FindLetters()
        {
            TmpText.ForceMeshUpdate();
            int charCount = TmpText.textInfo.characterCount;
            filledCharacters = new bool[charCount];
        }

        [Button("Fill All Letters")]
        private void FillAllLettersButtonButton()
        {
            TmpText.ForceMeshUpdate();
            for (int i = 0; i < filledCharacters.Length; i++)
            {
                filledCharacters[i] = true;
            }
        }

        [Button("Unfill All Letters")]
        private void UnfillAllLettersButton()
        {
            TmpText.ForceMeshUpdate();
            for (int i = 0; i < filledCharacters.Length; i++)
            {
                print(filledCharacters[i]);
                filledCharacters[i] = false;
            }
        }
        #endregion

        #region RESET
        public override object CaptureState()
        {
            // Create a new state object and populate it with your variables.
            LetterFillerState state = new() { filledCharacters = (bool[])filledCharacters.Clone() };
            return state;
        }

        public override void RestoreState(object state)
        {
            // Cast the object back to your custom state class.
            if (state is not LetterFillerState savedState)
            {
                Debug.LogWarning("Invalid state object for LetterFiller on " + gameObject.name);
                return;
            }
            // Restore your variables.
            filledCharacters = (bool[])savedState.filledCharacters.Clone();

            // Update visuals and any systems that depend on these variables.
            RefillLetters();
            DoInteration();
        }
        #endregion
    }
}

[System.Serializable]
public class LetterFillerState
{
    public bool[] filledCharacters;
}
