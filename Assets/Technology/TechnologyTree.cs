using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Networking;

public class TechnologyTree : NetworkBehaviour
{
    private PlayerController player;

    private int techIdIterator = 0;

    public List<Technology> technologies = new List<Technology>();
    public Technology activeResearch;

    public event Action OnTechnologyUpdated = delegate { };

    public void Start()
    {
        technologies.Add(new Technology_Robot(this, techIdIterator++, "Harvester", 100, typeof(HarvesterRobotController)));
        technologies.Add(new Technology_Robot(this, techIdIterator++, "Predator", 100, typeof(CombatRobotController)));
        technologies.Add(new Technology_Robot(this, techIdIterator++, "Transporter", 100, typeof(TransporterRobotController)));
        technologies.Add(new Technology_Robot(this, techIdIterator++, "Storage", 100, typeof(StorageRobotController)));
        technologies.Add(new Technology_Robot(this, techIdIterator++, "Purger", 100, typeof(PurgeRobotController)));
        technologies.Add(new Technology_Victory(this, techIdIterator++, "DX Vaccine", 100));

        technologies[0].AddProgress(technologies[0].cost);
        technologies[1].AddProgress(technologies[1].cost);
        technologies[2].AddProgress(technologies[2].cost);
        technologies[3].AddProgress(technologies[3].cost);

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
        int scienceEarned = (int)(Settings.City_Science_PerPopulationPerTick * player.City.PopulationManager.Population);
        AddProgressToActiveResearch(scienceEarned);
    }

    [Server]
    public void AddProgressToActiveResearch(int addedProgress)
    {
        if (activeResearch != null)
        {
            activeResearch.AddProgress(addedProgress);
            TargetSetProgress(player.connectionToClient, activeResearch.id, activeResearch.Progress);
        }
    }

    [TargetRpc]
    private void TargetSetProgress(NetworkConnection target, int techId, int progress)
    {
        Technology tech = GetTechnology(techId);
        tech.SetProgress(progress);
        OnTechnologyUpdated();
    }

    [Client]
    public void SetActiveResearch(Technology tech)
    {
        activeResearch = tech;
        CmdSetActiveResearch(tech.id);
    }

    [Command]
    private void CmdSetActiveResearch(int techId)
    {
        activeResearch = GetTechnology(techId);
    }

    private Technology GetTechnology(int techId)
    {
        Technology tech = technologies.FirstOrDefault(t => t.id == techId);
        if (tech == null)
            throw new Exception("Could not find tech with id " + techId);
        else
            return tech;
    }

    [Server]
    public bool HasRobotTech(Type robotTyoe)
    {
        return technologies.Any(t => 
            (t is Technology_Robot) 
            && (((Technology_Robot)t).robotType == robotTyoe)
            && t.IsResearched()
        );
    }

    public Technology GetFinishedVictoryTech()
    {
        return technologies.FirstOrDefault(t => t.IsResearched() && t is Technology_Victory);
    }

}