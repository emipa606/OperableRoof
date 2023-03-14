using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Operable_Roof;

[StaticConstructorOnStartup]
public class Operable_Roof : Building
{
    private const int updateEveryXTicks = 50;

    public bool CLOSESET = true;

    private bool elonger;

    public bool OPENSET = true;

    public CompPowerTrader Power;

    private int range = 5;

    private RoofDef roofToSet;

    private int timer;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        var lastChar = def.defName.Last();
        switch (lastChar)
        {
            case 'a':
                roofToSet = RoofDefOf.RoofConstructed;
                break;
            case 'b':
                roofToSet = RoofDefOf.RoofRockThin;
                break;
            case 'c':
                roofToSet = RoofDefOf.RoofRockThick;
                break;
            case 'd':
                roofToSet = DefDatabase<RoofDef>.GetNamedSilentFail("RoofShip");
                break;
            case 'e':
                roofToSet = DefDatabase<RoofDef>.GetNamedSilentFail("RTR_RoofSteel");
                break;
            case 'f':
                roofToSet = DefDatabase<RoofDef>.GetNamedSilentFail("RTR_RoofTransparent");
                break;
        }


        if (def.HasComp(typeof(CompPowerTrader)))
        {
            Power = GetComp<CompPowerTrader>();
        }

        foreach (var intVec3 in CellRect.CenteredOn(Position, range))
        {
            foreach (var building in intVec3.GetThingList(Find.CurrentMap).ToList())
            {
                if (building is Operable_Roof_Controller operable_Roof_Controller)
                {
                    operable_Roof_Controller.updateConnectedBuildings();
                }
            }
        }
    }

    private void changeRange(bool increase)
    {
        var isOpen = Find.CurrentMap.roofGrid.RoofAt(Position) != null;
        if (isOpen)
        {
            foreach (var intVec3 in CellRect.CenteredOn(Position, range))
            {
                Find.CurrentMap.roofGrid.SetRoof(intVec3, null);
            }
        }

        if (increase)
        {
            range++;
        }
        else
        {
            range--;
        }

        if (!isOpen)
        {
            return;
        }

        foreach (var intVec3 in CellRect.CenteredOn(Position, range))
        {
            Find.CurrentMap.roofGrid.SetRoof(intVec3, roofToSet);
        }
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (range > 1)
        {
            yield return new Command_Action
            {
                action = delegate { changeRange(false); },
                defaultLabel = "OpRo.SmallerRadius".Translate(),
                defaultDesc = "OpRo.SmallerRadiusTT".Translate(),
                hotKey = KeyBindingDefOf.Misc5,
                icon = ContentFinder<Texture2D>.Get("UI/smaller")
            };
        }

        if (range < 5)
        {
            yield return new Command_Action
            {
                action = delegate { changeRange(true); },
                defaultLabel = "OpRo.LargerRadius".Translate(),
                defaultDesc = "OpRo.LargerRadiusTT".Translate(),
                hotKey = KeyBindingDefOf.Misc4,
                icon = ContentFinder<Texture2D>.Get("UI/larger")
            };
        }
    }

    private void OPEN()
    {
        OPENSET = !OPENSET;
    }

    private void CLOSE()
    {
        CLOSESET = !CLOSESET;
    }

    private void OpenAnimation()
    {
        var radius = timer;
        foreach (var intVec3 in CellRect.CenteredOn(Position, radius))
        {
            Find.CurrentMap.roofGrid.SetRoof(intVec3, null);
            FloodFillerFog.FloodUnfog(intVec3, Find.CurrentMap);
        }
    }

    private void CloseAnimation()
    {
        var num = timer;
        var num2 = range - num;
        foreach (var intVec3 in CellRect.CenteredOn(Position, num2))
        {
            Find.CurrentMap.roofGrid.SetRoof(intVec3, roofToSet);
        }

        foreach (var intVec3 in CellRect.CenteredOn(Position, num2 - 1))
        {
            Find.CurrentMap.roofGrid.SetRoof(intVec3, null);
        }
    }

    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        GenDraw.DrawFieldEdges(CellRect.CenteredOn(Position, range).Cells.ToList());
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref range, "range", 5);
    }

    public override void Tick()
    {
        base.Tick();
        elonger = !elonger;
        if (elonger)
        {
            return;
        }

        if (!CLOSESET)
        {
            var power = Power;
            if (power is { PowerOn: false })
            {
                Messages.Message("CannotUseNoPower".Translate(), MessageTypeDefOf.RejectInput);
                CLOSESET = true;
                return;
            }

            CloseAnimation();
            timer++;
            if (timer > range)
            {
                CLOSESET = true;
                timer = 0;
            }
        }

        if (OPENSET)
        {
            return;
        }

        var power2 = Power;
        if (power2 is { PowerOn: false })
        {
            Messages.Message("CannotUseNoPower".Translate(), MessageTypeDefOf.RejectInput);
            OPENSET = true;
            return;
        }

        OpenAnimation();
        timer++;
        if (timer <= range)
        {
            return;
        }

        OPENSET = true;
        timer = 0;
    }
}