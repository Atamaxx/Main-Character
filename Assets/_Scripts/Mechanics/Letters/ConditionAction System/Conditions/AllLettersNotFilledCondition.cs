using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Letters
{
    public class AllLettersNotFilledCondition : Condition
    {
        [SerializeField] private List<LetterFiller> _letterFillers;
        
        public override bool IsConditionMet()
        {
            if (_letterFillers == null || _letterFillers.Count == 0)
                return false;
            
            foreach (var letterFiller in _letterFillers)
            {
                if (!AreAllLettersNotFilled(letterFiller))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreAllLettersNotFilled(LetterFiller letterFiller)
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

                // If any visible character is filled, the condition is not met
                if (letterFiller.filledCharacters[i])
                    return false;
            }

            return true;
        }
    }
}
