using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class SettingSelector : MonoBehaviour
{
    // Este script altera os valores do script settings e atualiza a um textmeshpro
    // pode-se escolher entre os diferentes modos que este script irá alterar, tirando a nescessidade de um script para cada um

    public enum SettingType
    {
        emptySlots,
        StartingTimer,
        MaxMoves,
        BlindTiles,
        PongableTiles,

    }

    [Header("Dependencies")]
    [SerializeField] private GameSettings settings;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private Animator textAnimator;

    [Header("Config")]
    [Tooltip("tipo de configuração que este script estará alterando")]
    [SerializeField] private SettingType settingType;
    [SerializeField] private float minValue = 1;
    [SerializeField] private float maxValue=600;
    [Tooltip("texto que irá aparecer junto ao valor mostrado na UI")]
    [SerializeField] private string extraText = " tile(s)";
    [Tooltip("é o quanto diminui ou aumenta, quanto maior, maior será os pulos de alteração do usuário")]
    [SerializeField] private float step = 1;


    [Header("Configurações se for Timer (caso contrário não altere)")]
    [Tooltip("Converte em segundos e minutos, no formato de relógio digital")]
    [SerializeField] private bool displayAsTime = false;


    private float currentValue;

  

    private void Start()
    {
        LoadValue();
        RefreshText();
    }

    private void OnEnable()
    {
        // Garante que sempre que o jogador alterar a quantidade de tiles, a UI se atualiza se adapta junto
        settings.OnTileAmountChange += RefreshText;
        LoadValue();
        RefreshText();
    }
    private void OnDisable()
    {
        settings.OnTileAmountChange -= RefreshText;

    }

    public void Increase()
    {
        currentValue += step;
        clampValue();


        ApplyValue();
        RefreshText();
        textAnimator.Play("grow", 0, 0f);
    }

    public void Decrease()
    {
        currentValue -= step;
        clampValue();

        ApplyValue();
        RefreshText();
        textAnimator.Play("shrink", 0, 0f);

    }

    // Se atualiza dos valores dos settings
    private void LoadValue()
    {
        switch (settingType)
        {
            case SettingType.emptySlots:
                currentValue = settings.emptySlotAmount;
                break;
            case SettingType.StartingTimer:
                currentValue = settings.startingTimer;
                break;

            case SettingType.MaxMoves:
                currentValue = settings.countedMovesAmount;
                break;

            case SettingType.BlindTiles:
                currentValue = settings.blindTileAmount;
                break;
            case SettingType.PongableTiles:
                currentValue = settings.pongableTileAmount;
                break;
        }
    }

    // Aplica o valor aos settings
    private void ApplyValue()
    {
        switch (settingType)
        {
            case SettingType.emptySlots:
                settings.emptySlotAmount = Mathf.RoundToInt(currentValue);
                settings.checkAmounts();
                break;

            case SettingType.StartingTimer:
                settings.startingTimer = currentValue;
                break;
            case SettingType.MaxMoves:
                settings.countedMovesAmount = Mathf.RoundToInt(currentValue);
                break;

            case SettingType.BlindTiles:
                settings.blindTileAmount = Mathf.RoundToInt(currentValue);
                break;
            case SettingType.PongableTiles:
                settings.pongableTileAmount = Mathf.RoundToInt(currentValue);
                break;
        }
    }

    // Atualiza o textmeshpro de seus valores
    private void RefreshText()
    {
        clampValue();

        // Mostra o valor à UI
        if (displayAsTime)
        {
            valueText.text = FormatTime(currentValue) + extraText;
        }
        else
        {
            valueText.text = Mathf.RoundToInt(currentValue).ToString() + extraText;
        }
    }
    string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);

        return $"{mins:00}:{secs:00}";
    }

    // Garante que o valor não ultrapassa o limite da grid
    private void clampValue()
    {
        if (settingType != SettingType.StartingTimer && settingType != SettingType.MaxMoves)
        {
            maxValue = settings.maxSlotAmount - settings.emptySlotAmount;
        }
        if (settingType == SettingType.emptySlots)
        {
            maxValue = settings.maxSlotAmount - 1;
        }
        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);

    }


}