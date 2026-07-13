using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Letters
{
    public class LetterInteractConditions : MonoBehaviour
    {
        [SerializeField] private LetterFiller letterFiller;

        // Check if all visible letters are filled
        public bool AreAllLettersFilled()
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

        // Check if a specific set of letters are all filled with correct multiplicity
        // For example: "off" means 1 'o' and 2 'f' characters must be filled.
        public bool AreSpecificLettersFilled(string lettersToCheck)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            string fullText = letterFiller.TmpText.text;
            lettersToCheck = lettersToCheck.ToLower();

            // Count occurrences needed
            Dictionary<char, int> requiredCounts = new Dictionary<char, int>();
            foreach (char c in lettersToCheck)
            {
                char lower = char.ToLower(c);
                if (!requiredCounts.ContainsKey(lower))
                    requiredCounts[lower] = 0;
                requiredCounts[lower]++;
            }

            // For each letter required, ensure we have that many filled occurrences
            foreach (var kvp in requiredCounts)
            {
                char requiredChar = kvp.Key;
                int requiredAmount = kvp.Value;

                int filledCount = 0;
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    char textChar = char.ToLower(fullText[charInfo.index]);
                    if (textChar == requiredChar && letterFiller.filledCharacters[i])
                    {
                        filledCount++;
                        if (filledCount == requiredAmount)
                            break; // Found enough of this char filled
                    }
                }

                // If we couldn't find enough filled occurrences, return false
                if (filledCount < requiredAmount)
                    return false;
            }

            return true;
        }

        // Check if at least X letters are filled
        public bool AreAtLeastXLettersFilled(int x)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
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

        // Check if no letters are filled
        public bool AreNoLettersFilled()
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                if (letterFiller.filledCharacters[i])
                    return false;
            }

            return true;
        }

        // Check if letters in a given string are filled in order
        // For example, "cat" means find a 'c' filled, after that find an 'a' filled, after that find a 't' filled.
        public bool AreLettersInOrderFilled(string pattern)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            string fullText = letterFiller.TmpText.text.ToLower();
            pattern = pattern.ToLower();

            int patternIndex = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                char textChar = fullText[charInfo.index];
                // Check if current char matches the needed pattern char and is filled
                if (textChar == pattern[patternIndex] && letterFiller.filledCharacters[i])
                {
                    patternIndex++;
                    if (patternIndex == pattern.Length)
                        return true;
                }
            }

            return false; // Not all found in order
        }

        // !Check if a fraction of letters in lettersToCheck are filled
        // fraction = 0.5 means at least half of the letters in lettersToCheck must be filled.
        // public bool AreLettersPartiallyFilled(string lettersToCheck, float fraction)
        // {
        //     if (fraction < 0f) fraction = 0f;
        //     if (fraction > 1f) fraction = 1f;

        //     if (letterFiller == null || letterFiller.tmpText == null)
        //         return false;

        //     TMP_TextInfo textInfo = letterFiller.tmpText.textInfo;
        //     string fullText = letterFiller.tmpText.text.ToLower();
        //     lettersToCheck = lettersToCheck.ToLower();

        //     // Collect all occurrences of specified letters
        //     List<int> matchingIndices = new List<int>();
        //     foreach (char c in lettersToCheck)
        //     {
        //         for (int i = 0; i < textInfo.characterCount; i++)
        //         {
        //             TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
        //             if (!charInfo.isVisible) continue;

        //             char textChar = fullText[charInfo.index];
        //             if (textChar == c)
        //             {
        //                 matchingIndices.Add(i);
        //             }
        //         }
        //     }

        //     if (matchingIndices.Count == 0)
        //         return false; // No such letters in text

        //     int filledCount = 0;
        //     foreach (int idx in matchingIndices)
        //     {
        //         if (letterFiller.filledCharacters[idx])
        //             filledCount++;
        //     }

        //     float filledFraction = (float)filledCount / matchingIndices.Count;
        //     return filledFraction >= fraction;
        // }

        /// <summary>
        /// Checks if all instances of a given letter are filled.
        /// For example, if the text is "banana" and letter='a',
        /// returns true only if all 'a's are filled.
        /// </summary>
        public bool AreAllInstancesOfLetterFilled(char letter)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            string fullText = letterFiller.TmpText.text.ToLower();
            char target = char.ToLower(letter);

            bool foundLetter = false;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                char c = fullText[charInfo.index];
                if (c == target)
                {
                    foundLetter = true;
                    if (!letterFiller.filledCharacters[i])
                        return false;
                }
            }

            // If the letter doesn't appear at all, this depends on game logic:
            // If no occurrences means trivially true, return true. Otherwise false.
            // Here we consider no occurrences as false, because we can't fill what doesn't exist.
            return foundLetter;
        }

        /// <summary>
        /// Checks if exactly X visible letters are filled.
        /// </summary>
        public bool AreExactlyXLettersFilled(int x)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            int filledCount = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;
                if (letterFiller.filledCharacters[i])
                    filledCount++;
                if (filledCount > x)
                    return false; // More than x filled, fail early
            }

            return filledCount == x;
        }

        /// <summary>
        /// Checks if all words are fully filled.
        /// Here we define words as sequences of visible letters separated by spaces.
        /// Every letter in a word must be filled for the word to count as filled.
        /// Returns true only if every word in the text is fully filled.
        /// </summary>
        public bool AreAllWordsFullyFilled()
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            string fullText = letterFiller.TmpText.text;

            // We'll consider a word as a sequence of visible characters separated by spaces.
            // We'll iterate over characters and group them into words, checking fill states.
            // If a word has at least one visible character not filled, that word fails.
            // If any word fails, return false.

            bool inWord = false;
            bool wordIsFullyFilled = true;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                {
                    // Encountered a non-visible character (possibly space or punctuation)
                    // If we were in a word, check if that word was fully filled
                    if (inWord)
                    {
                        if (!wordIsFullyFilled)
                            return false; // Found a word not fully filled

                        // Reset for the next word
                        inWord = false;
                        wordIsFullyFilled = true;
                    }
                    continue;
                }

                // If we reach here, we have a visible character
                if (!inWord)
                    inWord = true;

                // Check if this character is filled
                if (!letterFiller.filledCharacters[i])
                    wordIsFullyFilled = false;
            }

            // End of text - check the last word if we ended inside a word
            if (inWord && !wordIsFullyFilled)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if at least a certain fraction of all visible letters are filled.
        /// fraction = 0.5 means at least 50% of visible letters are filled.
        /// </summary>
        public bool AreAtLeastXPercentOfAllLettersFilled(float fraction)
        {
            if (fraction < 0f) fraction = 0f;
            if (fraction > 1f) fraction = 1f;

            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            int totalVisible = 0;
            int filledCount = 0;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                totalVisible++;
                if (letterFiller.filledCharacters[i])
                    filledCount++;
            }

            if (totalVisible == 0)
                return false; // No visible letters means can't meet a fraction

            float filledFraction = (float)filledCount / totalVisible;
            return filledFraction >= fraction;
        }

        /// <summary>
        /// !Checks if at least as many occurrences of letterA are filled as letterB.
        /// For example, if letterA='a', letterB='b', true if #filled 'a' >= #filled 'b'.
        /// </summary>
        // public bool AreAtLeastAsManyOfLetterAAsLetterBFilled(char letterA, char letterB)
        // {
        //     if (letterFiller == null || letterFiller.tmpText == null)
        //         return false;

        //     TMP_TextInfo textInfo = letterFiller.tmpText.textInfo;
        //     string fullText = letterFiller.tmpText.text.ToLower();
        //     char a = char.ToLower(letterA);
        //     char b = char.ToLower(letterB);

        //     int filledA = 0;
        //     int filledB = 0;

        //     for (int i = 0; i < textInfo.characterCount; i++)
        //     {
        //         TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
        //         if (!charInfo.isVisible) continue;

        //         char c = fullText[charInfo.index];
        //         if (c == a && letterFiller.filledCharacters[i])
        //             filledA++;
        //         if (c == b && letterFiller.filledCharacters[i])
        //             filledB++;
        //     }

        //     return filledA >= filledB;
        // }

        public bool AreRangeFilled(Vector2 range)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;
            int start = Mathf.FloorToInt(range.x);
            int end = Mathf.FloorToInt(range.y);

            // Safety checks to ensure range is valid
            if (start < 0) start = 0;
            if (end >= textInfo.characterCount) end = textInfo.characterCount - 1;
            if (start > end) return false; // Invalid or empty range

            // Iterate through each index from start..end
            for (int i = start; i <= end; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                {
                    // Optionally skip invisible chars or treat them as automatically failing.
                    // Here, we'll say if it's invisible, we skip it:
                    continue;
                }

                // If the character is visible but not filled, the condition fails
                if (!letterFiller.filledCharacters[i])
                    return false;
            }

            // If we never returned false, everything in the range is filled
            return true;
        }

        public bool AreLetterWithIndexFilled(int index)
        {
            if (letterFiller == null || letterFiller.TmpText == null)
                return false;

            TMP_TextInfo textInfo = letterFiller.TmpText.textInfo;

            // Out of bounds? Return false or handle differently
            if (index < 0 || index >= textInfo.characterCount)
                return false;

            TMP_CharacterInfo charInfo = textInfo.characterInfo[index];
            // Must be visible AND filled
            if (!charInfo.isVisible)
                return false;

            return letterFiller.filledCharacters[index];
        }
    }
}