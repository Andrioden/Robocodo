using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

public class TechnologyTree : NetworkBehaviour
{
    private PlayerController player;

    private List<Technology> technologies = new List<Technology>();
    public List<Technology> Technologies { get { return technologies; } }
    public Technology activeResearch;

    public event Action OnTechnologyUpdated = delegate { };
    public event Action OnNewRobotResearched = delegate { };

    public void Start()
    {
        technologies.AddRange(TechnologiesDB.Technologies(this));

        //Unlock Harvester from the start
        technologies[0].AddProgress(technologies[0].cost);

        if (isServer)
            WorldTickController.instance.OnTick += ContinueResearch;
    }

    private void OnDestroy()
    {
        WorldTickController.instance.OnTick -= ContinueResearch;
    }

    public void Initialize(PlayerController player)
    {
        this.player = player;
    }

    [Server]
    private void ContinueResearch()
    {
        if (activeResearch == null)
            return;

        int scienceEarned = (int)(Settings.City_Science_PerPopulationPerTick * player.City.PopulationManager.Population);
        AddProgressToActiveResearch(scienceEarned);
    }

    [Server]
    public void AddProgressToActiveResearch(int addedProgress)
    {
        activeResearch.AddProgress(addedProgress);
        TargetSetProgress(player.connectionToClient, activeResearch.id, activeResearch.Progress);
    }

    [TargetRpc]
    private void TargetSetProgress(NetworkConnection target, int techId, int progress)
    {
        Technology tech = GetTechnology(techId);
        tech.SetProgress(progress);
        OnTechnologyUpdated();
    }

    [Client]
    public void SetOrPauseActiveResearch(Technology tech)
    {
        if (tech.IsResearched())
            return;
        else if (activeResearch == tech)
        {
            activeResearch = null;
            CmdClearActiveResearch();
            return;
        }

        activeResearch = tech;
        CmdSetActiveResearch(tech.id);
    }

    [Command]
    private void CmdSetActiveResearch(int techId)
    {
        activeResearch = GetTechnology(techId);
    }

    [Command]
    private void CmdClearActiveResearch()
    {
        activeResearch = null;
    }

    private Technology GetTechnology(int techId)
    {
        Technology tech = technologies.FirstOrDefault(t => t.id == techId);
        if (tech == null)
            throw new Exception("Could not find tech with id " + techId);
        else
            return tech;
    }

    public bool IsRobotTechResearched(Type robotType)
    {
        return technologies.Any(t =>
            (t is Technology_Robot)
            && (((Technology_Robot)t).robotType == robotType)
            && t.IsResearched()
        );
    }

    public Technology GetFinishedVictoryTech()
    {
        return technologies.FirstOrDefault(t => t.IsResearched() && t is Technology_Victory);
    }

    public bool PlayerShouldSelectResearch()
    {
        if (activeResearch == null || activeResearch.IsResearched())
            return technologies.Exists(x => !x.IsResearched());

        return false;
    }

    public void TriggerNewRobotResearchedEvent()
    {
        OnNewRobotResearched();
    }
}