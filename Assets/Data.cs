using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class Data {

    public static Func<char, int> AlphaPos = (ch) =>
        char.ToUpper(ch) - 'A' + 1;
    private static Func<char, int> DotCount = (ch) =>
        MorseTranslation[ch].Count(x => x == '.');
    private static Func<char, int> DashCount = (ch) =>
            MorseTranslation[ch].Count(x => x == '-');


    public static readonly Dictionary<char, string> MorseTranslation = new Dictionary<char, string>()
    {
        { 'A', ".-"   },
        { 'B', "...-" },
        { 'C', "-.-." },
        { 'D', "-.."  },
        { 'E', "."    },
        { 'F', "..-." },
        { 'G', "--."  },
        { 'H', "...." },
        { 'I', ".."   },
        { 'J', ".---" },
        { 'K', "-.-"  },
        { 'L', ".-.." },
        { 'M', "--"   },
        { 'N', "-."   },
        { 'O', "---"  },
        { 'P', ".--." },
        { 'Q', "--.-" },
        { 'R', ".-."  },
        { 'S', "..."  },
        { 'T', "-"    },
        { 'U', "..-"  },
        { 'V', "...-" },
        { 'W', ".--"  },
        { 'X', "-..-" },
        { 'Y', "-.--" },
        { 'Z', "--.." },
    };
    public static readonly Dictionary<char, Func<string, char, bool>> Rules = new Dictionary<char, Func<string, char, bool>>()
    {
        {'A', (str, ch) => "AEIOU".Any(x => x == ch)},
        {'B', (str, ch) =>  MorseTranslation[ch][0] == '.'},
        {'C', (str, ch) =>  DotCount(ch) == DashCount(ch)},
        {'D', (str, ch) =>  MorseTranslation[ch][0] == '-'},
        {'F', (str, ch) =>  MorseTranslation[ch].Length % 2 == 1},
        {'G', (str, ch) =>  MorseTranslation[ch].All(x => x == '-')},
        {'H', (str, ch) =>  str.Count(x => x == ch) == 1},
        {'I', (str, ch) =>  str.Last() == ch},
        {'J', (str, ch) =>  AlphaPos(ch) <= 13},
        {'K', (str, ch) =>  str[1] == ch},
        {'L', (str, ch) =>  DashCount(ch) > DotCount(ch)},
        {'M', (str, ch) =>  MorseTranslation[ch].Length % 2 == 0},
        {'N', (str, ch) =>  str[0] == ch},
        {'O', (str, ch) =>  ch == 'O'},
        {'P', (str, ch) =>  str[2] == ch},
        {'Q', (str, ch) =>  MorseTranslation[ch].Length == 3},
        {'R', (str, ch) =>  DotCount(ch) != DashCount(ch)},
        {'S', (str, ch) =>  MorseTranslation[ch].Length == 2},
        {'U', (str, ch) =>  DotCount(ch) > DashCount(ch)},
        {'V', (str, ch) =>  !"AEIOU".Any(x => ch == x)},
        {'W', (str, ch) =>  MorseTranslation[ch].All(x => x == '.')},
        {'X', (str, ch) =>  AlphaPos(ch) > 13},
        {'Y', (str, ch) =>  MorseTranslation[ch].Length == 4},
        {'Z', (str, ch) =>  MorseTranslation[ch].Length == 1},
    };

    public static string GenerateSequence(char ch)
    {
        string output = "";
        foreach (char unit in MorseTranslation[ch])
        {
            if (unit == '-')
                output += "xxx";
            else output += "x";
            output += ".";
        }
        return output + "..";
    }
    public static string GenerateSequence(string str)
    {
        string output = "";
        foreach (char ch in str)
            output += GenerateSequence(ch);
        output += "....";
        return output;
    }
    public static string GenerateSubLetters(char ch)
    {
        string output = "";
        foreach (char sym in MorseTranslation[ch])
            if (sym == '.'  )
                output += shortLetters.PickRandom();
            else output += longLetters.PickRandom();
        return output;
    }
    private static char[] shortLetters = MorseTranslation.Where(x => x.Value.Length <= 2).Select(x => x.Key).ToArray();
    private static char[] longLetters = MorseTranslation.Where(x => x.Value.Length > 2).Select(x => x.Key).ToArray();

}
