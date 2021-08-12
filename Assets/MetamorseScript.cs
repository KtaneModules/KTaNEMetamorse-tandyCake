using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class MetamorseScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public MeshRenderer LED;
    public Material unlit, lit;
    public KMSelectable up, down, submit;
    public TextMesh letter;
    public MeshRenderer[] allMeshes;

    private Coroutine flash;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    
    private string transmittedMessage;
    private char greaterLetter;
    private char solution;
    private int currentIndex = 0;

    void Awake () {
        moduleId = moduleIdCounter++;
        
        up.OnInteract += delegate () { Up(); return false; };
        down.OnInteract += delegate () { Down(); return false; };
        submit.OnInteract += delegate () { Submit(); return false; };
        Module.OnActivate += delegate () { if (!moduleSolved) flash = StartCoroutine(Transmit()); }; //If the defuser somehow solves the mod before the lights come on, this would throw an error.
    }

    void Start ()
    {
        List<char> validChars;
        greaterLetter = "ABCDFGHIJKLMNOPQRSUVWXYZ".PickRandom();
        int attempts = 0;
        do
        {
            validChars = new List<char>();
            attempts++;
            transmittedMessage = Data.GenerateSubLetters(greaterLetter);
            foreach (char ch in transmittedMessage)
                if (Data.Rules[greaterLetter](transmittedMessage, ch))
                    validChars.Add(ch);
        } while (validChars.Count != 1);
        solution = validChars.Single();
        currentIndex = UnityEngine.Random.Range(0, 26);
        letter.text = ((char)(currentIndex + 'A')).ToString();
        Debug.LogFormat("[Metamorse #{0}] Generated puzzle in {1} attempt{2}.", moduleId, attempts, attempts == 1 ? "" : "s");
        Debug.LogFormat("[Metamorse #{0}] The transmitted sequence is {1}.", moduleId, transmittedMessage);
        Debug.LogFormat("[Metamorse #{0}] The Morse character obtained from this sequence is {1}({2}).", moduleId, greaterLetter, Data.MorseTranslation[greaterLetter]);
        Debug.LogFormat("[Metamorse #{0}] The only valid letter is {1}.", moduleId, solution);
    }
    void Up()
    {
        up.AddInteractionPunch(0.2f);
        Audio.PlaySoundAtTransform("ArrowPress", up.transform);
        if (moduleSolved)
            return;
        currentIndex++;
        currentIndex %= 26;
        letter.text = ((char)(currentIndex + 'A')).ToString();
    }
    void Down()
    {
        down.AddInteractionPunch(0.2f);
        Audio.PlaySoundAtTransform("ArrowPress", down.transform);
        if (moduleSolved)
            return;
        currentIndex += 25; //Same as decrementing.
        currentIndex %= 26;
        letter.text = ((char)(currentIndex + 'A')).ToString();
    }
    void Submit()
    {
        submit.AddInteractionPunch(0.5f);
        if (moduleSolved)
            return;
        if (currentIndex + 'A' == solution)
        {
            moduleSolved = true;
            Audio.PlaySoundAtTransform("Shimmer", transform);
            Debug.LogFormat("[Metamorse #{0}] Submitted {1}, module solved!", moduleId, solution);
            if (flash != null)
                StopCoroutine(flash);
            LED.material = unlit;
            foreach (MeshRenderer renderer in allMeshes)
                StartCoroutine(SolveFade(renderer));
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Metamorse #{0}] Submitted {1}, strike!", moduleId, (char)(currentIndex + 'A'));
        }
    }
    IEnumerator Transmit()
    {
        string transmission = Data.GenerateSequence(transmittedMessage);
        int pointer = 0;
        while (true)
        {
            LED.material = transmission[pointer++] == 'x' ? lit : unlit;
            pointer %= transmission.Length;
            yield return new WaitForSeconds(0.25f);
        }
    }
    IEnumerator SolveFade(MeshRenderer rend)
    {
        float delta = 0;
        Color start = rend.material.color;
        while (delta < 1)
        {
            delta += Time.deltaTime;
            yield return null;
            rend.material.color = Color.Lerp(start, "00ff64".Color(), Mathf.Pow(delta, 3.5f));
        }
        Module.HandlePass();
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} submit X> to submit X into the module.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command)
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:SUBMIT\s+)?([A-Z])$");
        if (m.Success)
        {
            yield return null;
            int answer = m.Groups[1].Value[0] - 'A';
            KMSelectable whichButton = ShortestPath(currentIndex, answer);
            while (currentIndex != answer)
            {
                whichButton.OnInteract();
                yield return new WaitForSeconds(0.15f);
            }
            submit.OnInteract();
        }
    }
    KMSelectable ShortestPath(int start, int end)
    {
        if (start < end ^ Math.Abs(start - end) > 13)
            return up;
        else return down;
    }

    IEnumerator TwitchHandleForcedSolve ()
    {
        KMSelectable whichButton = ShortestPath(currentIndex, solution - 'A');
        while (currentIndex != solution - 'A')
        {
            whichButton.OnInteract();
            yield return new WaitForSeconds(0.15f);
        }
        submit.OnInteract();
    }
}
