using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static class Extensions
{
    private static readonly Random rng = new();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

     public static List<float> FillListWithSum(int numbersAmount, float targetSum, float min, float max)
    {
        if (numbersAmount <= 0)
        {
            throw new ArgumentException("numbersAmount must be a positive integer");
        }
        if (targetSum < 0)
        {
            throw new ArgumentException("targetSum must be non-negative");
        }
        if (min >= max)
        {
            throw new ArgumentException("min must be less than max");
        }

        List<float> numbers = new List<float>();
        Random random = new Random(); 

        // Generate numbersAmount-1 random numbers within the specified range
        for (int i = 0; i < numbersAmount - 1; i++)
        {
            float randomNum = (float)random.NextDouble() * (max - min) + min;
            numbers.Add(randomNum);
        }

        // Calculate the remaining value to reach the target sum
        float remainingValue = targetSum - numbers.Sum();

        // Ensure the remaining value stays within the min-max range
        remainingValue = Math.Max(min, Math.Min(max, remainingValue));

        // Add the remaining value to the list 
        numbers.Add(remainingValue);

        return numbers;
    }
}
