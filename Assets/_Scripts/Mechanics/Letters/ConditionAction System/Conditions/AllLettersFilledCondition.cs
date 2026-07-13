using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Letters
{
    public class AllLettersFilledCondition : Condition
    {
        [SerializeField] private List<LetterFiller> _letterFillers;
        
        public override bool IsConditionMet()
        {
            if (_letterFillers == null || _letterFillers.Count == 0)
                return false;
            
            foreach (var letterFiller in _letterFillers)
            {
                if (!AreAllLettersFilled(letterFiller))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreAllLettersFilled(LetterFiller letterFiller)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;

            // Iterate through all visible characters
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                if (!letterFiller.filledCharacters[i])
                    return false;
            }

            return true;
        }
    }
}
