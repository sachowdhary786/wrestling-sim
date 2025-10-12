using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all actions initiated by a human player.
/// </summary>
public static class PlayerManager
{
    /// <summary>
    /// Simulates a show that has been booked by the player.
    /// </summary>
    public static void SimulatePlayerShow(Company playerCompany, List<Match> matches, GameData gameData)
    {
        Debug.Log($"[Player] Simulating {playerCompany.name}'s show...");
        var show = new Show(playerCompany.name + " Weekly", playerCompany.id);
        show.matches = matches;

        foreach (var match in show.matches)
        {
            // Player matches are always simulated in Advanced mode for full detail
            MatchSimulator.Simulate(match, gameData, MatchSimulationMode.Advanced);
        }

        // Update prestige based on the show's performance
        PrestigeCalculator.UpdatePrestige(playerCompany, show);
    }

    /// <summary>
    /// Allows the player to offer a contract to a free agent.
    /// </summary>
    /// <returns>True if signing was successful, false otherwise.</returns>
    public static bool SignFreeAgent(Company playerCompany, Wrestler freeAgent, int salary, int duration, GameData gameData)
    {
        if (freeAgent.contract != null)
        {
            Debug.LogError($"[Player] Cannot sign {freeAgent.name}. They are already under contract.");
            return false;
        }

        if (playerCompany.finances < salary)
        {
            Debug.LogError($"[Player] Cannot sign {freeAgent.name}. Not enough funds.");
            return false;
        }

        // Create and assign the new contract
        freeAgent.contract = new Contract(playerCompany.id, salary, duration);
        playerCompany.roster.Add(freeAgent.id);
        playerCompany.finances -= salary; // Simplified deduction

        Debug.Log($"[Player] Successfully signed {freeAgent.name} to {playerCompany.name}!");
        return true;
    }

    /// <summary>
    /// Releases a wrestler from their contract, making them a free agent.
    /// </summary>
    public static void ReleaseWrestler(Company playerCompany, Wrestler wrestler, GameData gameData)
    {
        if (wrestler.contract == null || wrestler.contract.companyId != playerCompany.id)
        {
            Debug.LogError($"[Player] Cannot release {wrestler.name}. They are not signed to this company.");
            return;
        }

        // Terminate the contract
        wrestler.contract = null;
        playerCompany.roster.Remove(wrestler.id);

        // Apply a major morale penalty for being fired
        MoraleManager.ApplyEventBonus(wrestler, -30, "Released from contract");

        Debug.Log($"[Player] {wrestler.name} has been released from {playerCompany.name}.");
    }

    /// <summary>
    /// Upgrades a player's company from Indie to Major tier if requirements are met.
    /// </summary>
    public static bool UpgradeCompanyToMajor(Company playerCompany)
    {
        const int PRESTIGE_REQUIREMENT = 70;
        const float FINANCES_REQUIREMENT = 1000000f;

        if (playerCompany.tier == CompanyTier.Major)
        {
            Debug.LogWarning($"[Player] {playerCompany.name} is already a Major league company.");
            return false;
        }

        if (playerCompany.prestige < PRESTIGE_REQUIREMENT)
        {
            Debug.LogError($"[Player] Cannot upgrade. Prestige must be at least {PRESTIGE_REQUIREMENT}.");
            return false;
        }

        if (playerCompany.finances < FINANCES_REQUIREMENT)
        {
            Debug.LogError($"[Player] Cannot upgrade. You must have at least {FINANCES_REQUIREMENT:C} in the bank.");
            return false;
        }

        // Upgrade the company
        playerCompany.tier = CompanyTier.Major;
        Debug.Log($"[Player] Congratulations! {playerCompany.name} has been upgraded to a Major league promotion!");
        return true;
    }
}
