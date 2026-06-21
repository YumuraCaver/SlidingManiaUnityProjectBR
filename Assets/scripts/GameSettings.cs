using UnityEngine;
using System;
public class GameSettings : MonoBehaviour
{
    // Registra configurações e modos de jogo que o jogador escolher
    // Lembre de sincronizar os modos com os toggles pois eles não ativam/desativam automaticamente

    public int maxSlotAmount = 9;

    public int emptySlotAmount = 1;

    public bool TimedMode;
    public float startingTimer;

    public bool countedMode;
    public int countedMovesAmount;

    public bool blindMode;
    public int blindTileAmount;

    public bool pongMode;
    public int pongableTileAmount;

    public event Action OnTileAmountChange;



    public void setTimeMode(bool b) { TimedMode = b; }
    public void setCountedMode(bool b) { countedMode = b; }
    public void setBlindMode(bool b) { blindMode = b; }
    public void setPongMode(bool b) { pongMode = b; }

    public void SetMaxTileAmount(int value)
    {
        maxSlotAmount = value;

        checkAmounts();
    }

    public void checkAmounts()
    {
        // Garante que sempre terá pelo menos um tile
        if (emptySlotAmount >= maxSlotAmount)
        {
            emptySlotAmount = maxSlotAmount - 1;
        }

        // Garante que a quantidade de tiles afetadas por modos não ultrapassa a quantidade total de tiles
        int tileAmount = maxSlotAmount - emptySlotAmount;

        if (pongableTileAmount > tileAmount)
        {
            pongableTileAmount = tileAmount;
        }
        if (blindTileAmount > tileAmount)
        {
            blindTileAmount = tileAmount;
        }

        OnTileAmountChange?.Invoke();//avisao ao scripts SettingsSelector para atualizar seus valores caso estejam passando dos limites

    }



}