using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BookingManager
{
    public static Show BookShow(Company company, Show show, GameData data, int targetMatchCount = 7)
    {
        var availableWrestlers = GetAvailableWrestlers(company, data);
        var availableManagers = data
            .wrestlers.Values.Where(w => w.isManager && w.contract?.companyId == company.id)
            .ToList();
        var availableRoadAgents = company
            .corporateStaff.Where(s => s.role == StaffRole.RoadAgent)
            .ToList();

        if (availableWrestlers.Count < 2)
            return show;

        var usedWrestlers = new HashSet<Guid>();

        // Main Event is always booked first
        BookMatch(
            show,
            company,
            data,
            availableWrestlers,
            availableManagers,
            availableRoadAgents,
            usedWrestlers,
            true
        );

        // Book the undercard
        for (int i = 0; i < targetMatchCount - 1; i++)
        {
            BookMatch(
                show,
                company,
                data,
                availableWrestlers,
                availableManagers,
                availableRoadAgents,
                usedWrestlers,
                false
            );
        }

        Debug.Log(
            $"[BOOKING] {company.name}'s show booked with {show.matches.Count} matches based on {company.bookingPhilosophy} philosophy."
        );
        return show;
    }

    private static void BookMatch(
        Show show,
        Company company,
        GameData data,
        List<Wrestler> available,
        List<Wrestler> availableManagers,
        List<CorporateStaff> availableRoadAgents,
        HashSet<Guid> used,
        bool isMainEvent
    )
    {
        List<Wrestler> participants = null;
        // Use a weighted decision based on philosophy
        if (
            company.bookingPhilosophy == BookingPhilosophy.StarPower
            || (company.bookingPhilosophy == BookingPhilosophy.Balanced && UnityEngine.Random.value < 0.4f)
        )
        {
            participants = FindMostPopular(available, used);
        }
        else if (
            company.bookingPhilosophy == BookingPhilosophy.WorkRate
            || (company.bookingPhilosophy == BookingPhilosophy.Balanced && UnityEngine.Random.value < 0.7f)
        )
        {
            participants = FindBestWorkers(available, used);
        }
        else // Fallback to random booking
        {
            participants = FindRandom(available, used);
        }

        if (participants != null && participants.Count == 2)
        {
            var match = CreateMatch(participants, company, "Singles");
            match.isMainEvent = isMainEvent;

            // Assign a Road Agent if available
            if (availableRoadAgents.Any())
            {
                var agent = availableRoadAgents[UnityEngine.Random.Range(0, availableRoadAgents.Count)];
                match.roadAgentId = agent.staffId;
            }

            // Assign a manager if available and not already used
            if (availableManagers.Any() && UnityEngine.Random.value < 0.33f) // 33% chance to get a manager
            {
                var manager = availableManagers[UnityEngine.Random.Range(0, availableManagers.Count)];
                var wrestlerToManage = participants.First(p => !p.isManager);
                if (!match.managers.ContainsValue(manager.id))
                {
                    match.managers[wrestlerToManage.id] = manager.id;
                    Debug.Log($"[Booking] {manager.name} is managing {wrestlerToManage.name}.");
                }
            }

            show.matches.Add(match);
            MarkWrestlersAsUsed(match, used);
        }
    }

    private static List<Wrestler> FindMostPopular(List<Wrestler> available, HashSet<Guid> used)
    {
        return available
            .Where(w => !used.Contains(w.id))
            .OrderByDescending(w => w.popularity)
            .Take(2)
            .ToList();
    }

    private static List<Wrestler> FindBestWorkers(List<Wrestler> available, HashSet<Guid> used)
    {
        return available
            .Where(w => !used.Contains(w.id))
            .OrderByDescending(w => w.technical + w.brawling + w.aerial)
            .Take(2)
            .ToList();
    }

    private static List<Wrestler> FindRandom(List<Wrestler> available, HashSet<Guid> used)
    {
        return available
            .Where(w => !used.Contains(w.id))
            .OrderBy(w => Guid.NewGuid())
            .Take(2)
            .ToList();
    }

    private static List<Wrestler> GetAvailableWrestlers(Company company, GameData data)
    {
        return data
            .wrestlers.Values.Where(w =>
                w.contract != null && w.contract.companyId == company.id && IsAvailable(w)
            )
            .ToList();
    }

    private static bool IsAvailable(Wrestler w)
    {
        return !w.isRetired && !w.injured && w.fatigue < 90;
    }

    private static Match CreateMatch(List<Wrestler> wrestlers, Company company, string matchType)
    {
        return new Match
        {
            id = Guid.NewGuid(),
            participants = wrestlers.Select(w => w.id).ToList(),
            matchType = matchType,
        };
    }

    private static void MarkWrestlersAsUsed(Match match, HashSet<Guid> used)
    {
        foreach (var id in match.participants)
            used.Add(id);
    }
}
