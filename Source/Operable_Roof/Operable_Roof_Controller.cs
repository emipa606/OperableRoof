using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Operable_Roof;

[StaticConstructorOnStartup]
public class Operable_Roof_Controller : Building
{
    public static readonly Texture2D UI_OPEN;

    public static readonly Texture2D UI_CLOSE;

    public bool CLOSESET = true;

    private List<Operable_Roof> connectedBuildings;

    public bool OPENSET = true;

    private CompPowerTrader Power;

    static Operable_Roof_Controller()
    {
        UI_OPEN = ContentFinder<Texture2D>.Get("UI/OPEN");
        UI_CLOSE = ContentFinder<Texture2D>.Get("UI/CLOSE");
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.GetInspectString());
        stringBuilder.AppendLine($"Connected to {connectedBuildings.Count} roofs");
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public override void TickRare()
    {
        base.TickRare();
        updateConnectedBuildings();
    }

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        foreach (var connectedBuilding in connectedBuildings)
        {
            if (connectedBuilding.Power.PowerOn && Power.PowerOn)
            {
                GenDraw.DrawLineBetween(connectedBuilding.TrueCenter(), this.TrueCenter());
            }
            else
            {
                GenDraw.DrawLineBetween(connectedBuilding.TrueCenter(), this.TrueCenter(),
                    CompAffectedByFacilities.InactiveFacilityLineMat);
            }
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (OPENSET)
        {
            yield return new Command_Action
            {
                action = OPEN,
                defaultLabel = "OPEN",
                defaultDesc = "Work",
                icon = UI_OPEN
            };
        }

        if (CLOSESET)
        {
            yield return new Command_Action
            {
                action = CLOSE,
                defaultLabel = "CLOSE",
                defaultDesc = "Work",
                icon = UI_CLOSE
            };
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (def.HasComp(typeof(CompPowerTrader)))
        {
            Power = GetComp<CompPowerTrader>();
        }

        updateConnectedBuildings();
    }

    public void updateConnectedBuildings()
    {
        connectedBuildings = new List<Operable_Roof>();
        foreach (var intVec3 in CellRect.CenteredOn(InteractionCell, 15))
        {
            foreach (var building in intVec3.GetThingList(Find.CurrentMap).ToList())
            {
                if (building is Operable_Roof operable_Roof)
                {
                    connectedBuildings.Add(operable_Roof);
                }
            }
        }
    }

    private void OPEN()
    {
        var power = Power;
        if (power is { PowerOn: false })
        {
            Messages.Message("Can not work", MessageTypeDefOf.RejectInput);
            OPENSET = true;
            CLOSESET = false;
            return;
        }

        OPENSET = false;
        CLOSESET = true;

        updateConnectedBuildings();
        foreach (var operable_Roof in connectedBuildings)
        {
            operable_Roof.OPENSET = false;
        }
    }

    private void CLOSE()
    {
        var power = Power;
        if (power is { PowerOn: false })
        {
            Messages.Message("Can not work", MessageTypeDefOf.RejectInput);
            OPENSET = false;
            CLOSESET = true;
            return;
        }

        OPENSET = true;
        CLOSESET = false;

        updateConnectedBuildings();
        foreach (var operable_Roof in connectedBuildings)
        {
            operable_Roof.CLOSESET = false;
        }
    }
}
