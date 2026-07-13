using TMPro;
using UnityEngine;

namespace Letters
{
    public class LetterIndexCondition : Condition
    {
        [SerializeField] private LetterFiller _letterFiller;
        [SerializeField] private int _index = 0;

        public override bool IsConditionMet()
        {
            return AreLetterWithIndexFilled(_index, _letterFiller);
        }

        public bool AreLetterWithIndexFilled(int _index, LetterFiller letterFiller)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;

            // Out of bounds? Return false or handle differently
            if (_index < 0 || _index >= textInfo.characterCount)
                return false;

            TMP_CharacterInfo charInfo = textInfo.characterInfo[_index];
            // Must be visible AND filled
            if (!charInfo.isVisible)
                return false;

            return letterFiller.filledCharacters[_index];
        }
    }
}