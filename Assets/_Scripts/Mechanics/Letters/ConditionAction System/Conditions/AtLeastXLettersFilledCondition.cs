using TMPro;
using UnityEngine;

namespace Letters
{

    public class AtLeastXLettersFilledCondition : Condition
    {
        [SerializeField] private LetterFiller _letterFiller;

        [SerializeField] private int minLettersFilled = 3;

        public override bool IsConditionMet()
        {
            return AreAtLeastXLettersFilled(minLettersFilled, _letterFiller);
        }


        public bool AreAtLeastXLettersFilled(int x, LetterFiller letterFiller)
        {
            if (letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            int filledCount = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                if (letterFiller.filledCharacters[i])
                {
                    filledCount++;
                    if (filledCount >= x)
                        return true;
                }
            }

            return false;
        }
    }
}