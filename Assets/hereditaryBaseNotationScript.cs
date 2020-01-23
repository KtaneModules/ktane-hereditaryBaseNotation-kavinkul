using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using KModkit;

public class hereditaryBaseNotationScript : MonoBehaviour 
{
    public KMAudio audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] keypad;
    public MeshRenderer[] displayText;
    public GameObject[] keypadObject;

    private readonly string digits = "0123456789ABCDEF";
    private readonly string[] solvedText1 = new[] { "GOOD JOB", "VERY NICE", "GG", "GOOD GAME", "YOU DID IT", "NICE JOB", "WEll DONE"};
    private readonly string[] solvedText2 = new[] { "MODULE SOLVED", "CONGRATULATIONS", "MODULE DISARMED", "SOLVED", "DISARMED", "DEACTIVATED", "EXTRAORDINARY" };
    private string answerString = "";
    private string bottomScreenText = "";
    private string topScreenText = "";
    private List<string> givenHereditary = new List<string>();
    private List<string> incrementedHereditary = new List<string>();
    private bool moduleActivated;
    private bool[] animatingFlag = new bool[18];
    private bool[] statementsFlag = new bool[18];
    private int initialNumber;
    private int incrementedNumber;
    private int answerNumber;
    private int baseN;
    private int baseK;
    private int initialBombTimeMinutes;

    string[] moduleStatement1 = new[] { "qkTernaryConverter", "simonStores", "UltraStores" };
    string[] moduleStatement2 = new[] { "bases", "indigoCipher" };
    string serialStatement = "GOODSTEIN";

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    // Use this for initialization
    private void Start () 
    {
        moduleActivated = false;
        moduleSolved = false;
        for (int index = 0; index < animatingFlag.Length; index++)
        {
            animatingFlag[index] = false;
        }
        for (int index = 0; index < statementsFlag.Length; index++)
        {
            statementsFlag[index] = false;
        }
        displayText[0].GetComponent<TextMesh>().text = "";
        displayText[1].GetComponent<TextMesh>().text = "";
        moduleId = moduleIdCounter++;
        module.OnActivate += Activate;
        for (int index = 0; index < keypad.Length; index++)
        {
            var j = index;
            keypad[j].OnInteract += delegate () { pressButton(j); return false; };
        }
	}
	
    private void pressButton(int index)
    {
        if (!moduleSolved && moduleActivated && !animatingFlag[index])
        {
            keypad[index].AddInteractionPunch(0.125f);
            StartCoroutine(keyAnimation(index));
            if (index >= 0 && index <= 15)
            {
                if (topScreenText.Length < 13)
                {
                    topScreenText = topScreenText + digits[index];
                    displayText[0].GetComponent<TextMesh>().text = topScreenText;
                }
            }
            else if (index == 16)
            {
                topScreenText = "";
                displayText[0].GetComponent<TextMesh>().text = topScreenText;
            }
            else
            {
                Debug.LogFormat("[Hereditary Base Notation #{0}] Submitted {1}.", moduleId, topScreenText);
                if (topScreenText == answerString)
                {
                    Debug.LogFormat("[Hereditary Base Notation #{0}] Correct! Module solved.", moduleId);
                    moduleSolved = true;
                    victory_Screen();
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    module.HandlePass();
                }
                else
                {
                    Debug.LogFormat("[Hereditary Base Notation #{0}] Incorrect! Issuing a strike.", moduleId);
                    topScreenText = "";
                    displayText[0].GetComponent<TextMesh>().text = topScreenText;
                    module.HandleStrike();
                }
            }
        }
    }

    private IEnumerator keyAnimation(int index)
    {
        animatingFlag[index] = true;
        audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        for (int i = 0; i < 10; i++)
        {
            keypadObject[index].transform.localPosition += new Vector3(0, -0.00075F, 0);
            yield return new WaitForSeconds(0.001F);
        }
        for (int i = 0; i < 10; i++)
        {
            keypadObject[index].transform.localPosition += new Vector3(0, +0.00075F, 0);
            yield return new WaitForSeconds(0.001F);
        }
        animatingFlag[index] = false;
    }

    private void Activate()
    {
        initialBombTimeMinutes = (int)(info.GetTime() / 60);
        generateAnswer();
        displayText[0].GetComponent<TextMesh>().text = topScreenText;
        displayText[1].GetComponent<TextMesh>().text = bottomScreenText;
        moduleActivated = true;
    }

    private string numberToBaseNString(int baseN, int base10_num)
    {
        string result = "";
        while (base10_num > 0)
        {
            int remainder = base10_num % baseN;
            result = digits[remainder] + result;
            base10_num = base10_num / baseN;
        }
        return result;
    }

    private void generateAnswer()
    {
        //Base 3 to 7
        baseN = 3 + info.GetIndicators().Count() + int.Parse(info.GetSerialNumber().Substring(5, 1));
        while (baseN > 7)
            baseN -= 5;
        initialNumber = 0;
        switch (baseN)
        {
            case 3:
                initialNumber = Rnd.Range(1, 19683);
                break;
            case 4:
                initialNumber = Rnd.Range(1, 60001);
                break;
            case 5:
                initialNumber = Rnd.Range(1, 80001);
                break;
            default:
                initialNumber = Rnd.Range(1, 100001);
                break;
        }
        bottomScreenText = numberToBaseNString(baseN, initialNumber);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The initial number is {1}, generated in base-{2} as {3}.", moduleId, initialNumber, baseN, bottomScreenText);
        generateHereditaryFormulaWrapper(baseN, initialNumber, givenHereditary);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The initial number in hereditary base-{1} is:", moduleId, baseN);
        for (int i = givenHereditary.Count() - 1; i >= 0; i--)
            Debug.LogFormat("{0}", givenHereditary[i]);
        incrementedNumber = hereditary_Increment(baseN, initialNumber);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The number after applying incrementing the base and subtract 1, in base-10, is {1}.", moduleId, incrementedNumber);
        generateHereditaryFormulaWrapper(baseN + 1, incrementedNumber, incrementedHereditary);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The number after applying incrementing the base and subtract 1, in hereditary base-{1}, is:", moduleId, baseN + 1);
        for (int i = incrementedHereditary.Count() - 1; i >= 0; i--)
            Debug.LogFormat("{0}", incrementedHereditary[i]);
        baseK = generateAnswerBase();
        Debug.LogFormat("[Hereditary Base Notation #{0}] The target base value K is {1}.", moduleId, baseK);
        if ((baseN % 2 == 0 && baseK % 2 == 0) || (baseN % 2 == 1 && baseK % 2 == 1))
            answerNumber = hereditary_Sum(baseN + 1, incrementedNumber);
        else
            answerNumber = hereditary_Sum2(baseN + 1, incrementedNumber);
        if (answerNumber > 8000)
            answerNumber = answerNumber % 8000;
        answerString = numberToBaseNString(baseK, answerNumber);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The answer is {1}, written in base-{2} as {3}.", moduleId, answerNumber, baseK, answerString);
    }

    private int generateAnswerBase()
    {
        int result = 0;
        var booleanNames = new Dictionary<bool, string> {
            { false, "False" },
            { true, "True" }
        };

        if (info.IsIndicatorOn("FRK"))
        {
            statementsFlag[0] = true;
            result++;
        }
        if (int.Parse(info.GetSerialNumber().Substring(5, 1)) % 2 == 1)
        {
            statementsFlag[1] = true;
            result++;
        }
        if (info.GetSerialNumberNumbers().ToArray()[0] % 2 == 0)
        {
            statementsFlag[2] = true;
            result++;
        }
        if (info.IsIndicatorOff("SIG"))
        {
            statementsFlag[3] = true;
            result++;
        }
        if (info.GetModuleNames().Count() > 36)
        {
            statementsFlag[4] = true;
            result++;
        }
        if (info.GetModuleNames().Count(name => name.IndexOf("Forget", StringComparison.InvariantCultureIgnoreCase) != -1) == 1)
        {
            statementsFlag[5] = true;
            result++;
        }
        if (!info.GetSerialNumber().Substring(0, 2).Any(character => '0' <= character && character <= '9'))
        {
            statementsFlag[6] = true;
            result++;
        }
        if (info.IsIndicatorOn("BOB"))
        {
            statementsFlag[7] = true;
            result++;
        }
        if (info.IsIndicatorOff("IND"))
        {
            statementsFlag[8] = true;
            result++;
        }
        if (info.GetOnIndicators().Count() < info.GetOffIndicators().Count())
        {
            statementsFlag[9] = true;
            result++;
        }
        if (initialBombTimeMinutes <= 15)
        {
            statementsFlag[10] = true;
            result++;
        }
        if (info.GetModuleIDs().Any(id => moduleStatement1.Contains(id)))
        {
            statementsFlag[11] = true;
            result++;
        }
        if (info.GetBatteryCount() > 4)
        {
            statementsFlag[12] = true;
            result++;
        }
        if (info.GetBatteryHolderCount() == 2 || info.GetBatteryHolderCount() == 3 || info.GetBatteryHolderCount() == 5)
        {
            statementsFlag[13] = true;
            result++;
        }
        if (info.GetModuleIDs().Any(id => moduleStatement2.Contains(id)))
        {
            statementsFlag[14] = true;
            result++;
        }
        if (info.GetSerialNumberNumbers().ToList().Count() == 2)
        {
            statementsFlag[15] = true;
            result++;
        }
        if (info.GetSerialNumber().ToUpperInvariant().Any(character => serialStatement.Contains(character)))
        {
            statementsFlag[16] = true;
            result++;
        }
        if (info.GetBatteryCount(Battery.AA) > 2)
        {
            statementsFlag[17] = true;
            result++;
        }

        Debug.LogFormat("[Hereditary Base Notation #{0}] Lit indicator FRK presents: {1}", moduleId, booleanNames[statementsFlag[0]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The last digit of the serial number is odd: {1}", moduleId, booleanNames[statementsFlag[1]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The first digit of the serial number is even: {1}", moduleId, booleanNames[statementsFlag[2]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Unlit indicator SIG presents: {1}", moduleId, booleanNames[statementsFlag[3]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] More that 36 modules on the bomb: {1}", moduleId, booleanNames[statementsFlag[4]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Exactly 1 module with \"Forget\" in its name: {1}", moduleId, booleanNames[statementsFlag[5]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] The first two characters of the serial number are letters: {1}", moduleId, booleanNames[statementsFlag[6]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Lit indicator BOB presents: {1}", moduleId, booleanNames[statementsFlag[7]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Unlit indicator IND presents: {1}", moduleId, booleanNames[statementsFlag[8]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] More unlit indicators than lit indicators: {1}", moduleId, booleanNames[statementsFlag[9]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Initial bomb timer is 15 minutes or less: {1}", moduleId, booleanNames[statementsFlag[10]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Ternary Converter, Simon Stores, or UltraStores modules present on the bomb: {1}", moduleId, booleanNames[statementsFlag[11]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] More than 4 batteries on the bomb: {1}", moduleId, booleanNames[statementsFlag[12]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] There are 2, 3, or 5 batter holders on the bomb: {1}", moduleId, booleanNames[statementsFlag[13]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Bases or Indigo Cipher modules present on the bomb: {1}", moduleId, booleanNames[statementsFlag[14]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Exactly 2 digits in the serial number: {1}", moduleId, booleanNames[statementsFlag[15]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Serial number contains any letter in \"GOODSTEIN\": {1}", moduleId, booleanNames[statementsFlag[16]]);
        Debug.LogFormat("[Hereditary Base Notation #{0}] More than 2 AA batteries on the bomb: {1}", moduleId, booleanNames[statementsFlag[17]]);

        if (result < 2)
        {
            Debug.LogFormat("[Hereditary Base Notation #{0}] The base value is less than 2. The answer is now required to be submitted in base-10.", moduleId);
            result = 10;
        }
        else if (result > 16)
        {
            Debug.LogFormat("[Hereditary Base Notation #{0}] The base value is more that 16. The answer is now required to be submitted in base-8.", moduleId);
            result = 8;
        }

        return result;
    }

    // These variables are to be used with generateHereditaryFormulaWrapper function
    int currentIndex;
    int currentLevel;
    //Wrapper function  to reinitialize the currentIndex to 0 
    private void generateHereditaryFormulaWrapper(int baseN, int base10_num, List<string> result)
    {
        currentIndex = 0;
        currentLevel = 0;
        generateHereditaryFormula(baseN, base10_num, result);
        Debug.LogFormat("[Hereditary Base Notation #{0}] Line counts: {1}", moduleId, result.Count());
    }

    private void generateHereditaryFormula(int baseN, int base10_num, List<string> result)
    {
        int nth_digit = 0;
        if ( result.Count() < currentLevel + 1)
        {
            result.Add(string.Empty);
            for (int i = 0; i < currentIndex; i++)
                result[currentLevel] += " ";
        }
        while (base10_num > 0)
        {
            int remainder = base10_num % baseN;
            if (remainder != 0)
            {
                if (nth_digit == 0)
                {
                    result[currentLevel] = remainder.ToString() + result[currentLevel];
                    currentIndex++;
                }
                else
                {
                    currentLevel++;
                    generateHereditaryFormula(baseN, nth_digit, result);
                    result[currentLevel] = string.Format("{0} \u00D7 {1}{2}", remainder, baseN, result[currentLevel]);
                    currentIndex += (remainder.ToString().Length + baseN.ToString().Length + 3);
                }
                if (base10_num / baseN > 0)
                {
                    result[currentLevel] = " + " + result[currentLevel];
                    currentIndex += 3;
                }
            }
            for (int i = 0; i < result.Count(); i++)
            {
                while (result[i].Length < currentIndex)
                    result[i] = " " + result[i];
            }
            nth_digit++;
            base10_num = base10_num / baseN;
        }
        currentLevel--;
    }

    private int hereditary_Increment(int baseN, int base10_num)
    {
        int nth_digit = 0;
        int result = 0;
        while (base10_num > 0)
        {
            int remainder = base10_num % baseN;
            if (remainder != 0)
            {
                if (nth_digit == 0)
                    result = result + remainder;
                else if (nth_digit > 0 && nth_digit < baseN)
                    result = result + remainder * (int) Math.Pow(baseN + 1, nth_digit);
                else
                    result = result + remainder * (int) Math.Pow(baseN + 1, hereditary_Increment(baseN, nth_digit) + 1);
            }
            nth_digit++;
            base10_num = base10_num / baseN;
        }
        result = result - 1;
        return result;
    }

    private int hereditary_Sum(int baseN, int base10_num)
    {
        int nth_digit = 0;
        int result = 0;
        while (base10_num > 0)
        {
            int remainder = base10_num % baseN;
            if (remainder != 0)
            {
                if (nth_digit == 0)
                    result = result + remainder;
                else if (nth_digit > 0 && nth_digit < baseN)
                    result = result + remainder + baseN + nth_digit;
                else
                    result = result + baseN + remainder + hereditary_Sum(baseN, nth_digit);
            }
            nth_digit++;
            base10_num = base10_num / baseN;
        }
        return result;
    }

    // These variables are to be used with hereditary_Sum2 function
    int left_number = 0;
    int right_number = 0;
    private int hereditary_Sum2(int baseN, int base10_num)
    {
        int nth_digit = 0;
        int result = 1;
        while (base10_num > 0)
        {
            int remainder = base10_num % baseN;
            if (remainder != 0)
            {
                if (nth_digit == 0)
                    right_number = right_number + remainder;
                else if (nth_digit > 0 && nth_digit < baseN)
                {
                    right_number = right_number + nth_digit + remainder;
                    left_number = left_number + baseN;
                }
                else
                {
                    right_number = right_number + remainder;
                    left_number = left_number + baseN;
                    hereditary_Sum2(baseN, nth_digit);
                }    
            }
            nth_digit++;
            base10_num = base10_num / baseN;
        }
        result = left_number * right_number;
        return result;
    }

    private void victory_Screen()
    {
        int index = Rnd.Range(0, solvedText1.Length);
        displayText[0].GetComponent<TextMesh>().text = solvedText1[index];
        index = Rnd.Range(0, solvedText2.Length);
        displayText[1].GetComponent<TextMesh>().text = solvedText2[index];
    }
    //Twitch Plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} submit 46AD to clear the screen and submit 46AD as an answer. Use !{0} type 46AD to type in 46AD without pressing the clear or submit button. The digit string must have a length between 1 to 13. Use !{0} submit will submit anything currently in the top display. Use !{0} clear to clear the top display. The number must be between 0 to 9 or A to F.";
    #pragma warning restore 414

    public IEnumerator TwitchHandleForcedSolve()
    {
        yield return new WaitUntil(() => moduleActivated);
        while (!moduleSolved)
        {
            answerString = answerString.ToUpperInvariant();
            yield return new WaitUntil(() => !animatingFlag[16]);
            keypad[16].OnInteract();
            yield return new WaitForSeconds(0.1F);
            for (int index = 0; index < answerString.Length; index++)
            {
                int buttonToPress;
                if (answerString[index] >= '0' || answerString[index] <= '9')
                {
                    buttonToPress = answerString[index] - '0';
                }
                else
                {
                    buttonToPress = answerString[index] - 'A' + 10;
                }
                yield return new WaitUntil(() => !animatingFlag[buttonToPress]);
                keypad[buttonToPress].OnInteract();
                yield return new WaitForSeconds(0.1F);
            }
            yield return new WaitUntil(() => !animatingFlag[17]);
            keypad[17].OnInteract();
            yield return new WaitForSeconds(0.1F);
        }
    }

    public IEnumerator ProcessTwitchCommand(string command)
    {
        if (!moduleActivated)
        {
            yield return "sendtochaterror The module is not yet ready to be interacted with. Please wait until the module activates.";
            yield break;
        }
 
        string[] parameters = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (Regex.IsMatch(parameters[1], @"^\s*[0-9|A-F]{1,13}\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                {
	                yield return null;
                    parameters[1] = parameters[1].ToUpperInvariant();
                    while (animatingFlag[16])
                        yield return new WaitForSeconds(0.1F);
                    keypad[16].OnInteract();
                    yield return new WaitForSeconds(0.1F);
                    for (int index = 0; index < parameters[1].Length; index++)
                    {
                        int buttonToPress;
                        if (parameters[1][index] >= '0' && parameters[1][index] <= '9')
                        {
                            buttonToPress = parameters[1][index] - '0';
                        }
                        else
                        {
                            buttonToPress = parameters[1][index] - 'A' + 10;
                        }
                        while (animatingFlag[buttonToPress])
                            yield return new WaitForSeconds(0.1F);
                        keypad[buttonToPress].OnInteract();
                        yield return new WaitForSeconds(0.1F);
                    }
                    while (animatingFlag[17])
                        yield return new WaitForSeconds(0.1F);
                    keypad[17].OnInteract();
                    yield return new WaitForSeconds(0.1F);
                }
                else
                {
                    yield return "sendtochaterror Unexpected characters or string length detected. Please only enter any number from 0 to 9 or letter from A to F. The string length must be between 1 to 13.";
                    yield break;
                }
            }
            else if (parameters.Length == 1)
            {
	            yield return null;
                while (animatingFlag[17])
                    yield return new WaitForSeconds(0.1F);
                keypad[17].OnInteract();
                yield return new WaitForSeconds(0.1F);
            }
            else
            {
                yield return "sendtochaterror Invalid command: submit command requires 0 to 1 arguments after it. If it is followed by 1 argument, then that argument must be a string of digits 0 to 9 and letters A to F without any space in between. This string length must be between 1 to 13.";
                yield break;
            }
        }
        else if (Regex.IsMatch(parameters[0], @"^\s*type\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && parameters.Length == 2)
        {
            if (Regex.IsMatch(parameters[1], @"^\s*[0-9|A-F]{1,13}\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
	            yield return null;
                parameters[1] = parameters[1].ToUpperInvariant();
                for (int index = 0; index < parameters[1].Length; index++)
                {
                    int buttonToPress;
                    if (parameters[1][index] >= '0' && parameters[1][index] <= '9')
                    {
                        buttonToPress = parameters[1][index] - '0';
                    }
                    else
                    {
                        buttonToPress = parameters[1][index] - 'A' + 10;
                    }
                    while (animatingFlag[buttonToPress])
                        yield return new WaitForSeconds(0.1F);
                    keypad[buttonToPress].OnInteract();
                    yield return new WaitForSeconds(0.1F);
                }
            }
            else
            {
                yield return "sendtochaterror Invalid command: type command requires exactly 1 argument after it. That argument must be a string of digits 0 to 9 and letters A to F without any space in between. This string length must be between 1 to 13.";
                yield break;
            }
        }
        else if (Regex.IsMatch(parameters[0], @"^\s*clear\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && parameters.Length == 1)
        {
            yield return null;
            while (animatingFlag[16])
                yield return new WaitForSeconds(0.1F);
            keypad[16].OnInteract();
            yield return new WaitForSeconds(0.1F);
        }
        else
        {
            yield return "sendtochaterror Invalid command: The command must start with \"submit\", \"type\", or \"clear\". submit command can have 0 or 1 arguments after it. type command requires exactly 1 argument after it. clear command must not be followed by any argument. If the command requires 1 argument, then it must be a string of digits 0 to 9 and letters A to F without any space in between.";
            yield break;
        }
        yield break;
    }
}