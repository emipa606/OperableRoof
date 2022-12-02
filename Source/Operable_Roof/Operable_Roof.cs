using System.Linq;
using RimWorld;
using Verse;

namespace Operable_Roof;

[StaticConstructorOnStartup]
public class Operable_Roof : Building
{
    private const int updateEveryXTicks = 50;

    public bool CLOSESET = true;

    public bool OPENSET = true;

    public CompPowerTrader Power;

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
        }


        if (def.HasComp(typeof(CompPowerTrader)))
        {
            Power = GetComp<CompPowerTrader>();
        }

        foreach (var intVec3 in CellRect.CenteredOn(InteractionCell, 15))
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

    private void OPEN()
    {
        OPENSET = !OPENSET;
    }

    private void CLOSE()
    {
        CLOSESET = !CLOSESET;
    }

    private void Animation()
    {
        var radius = timer / 5;
        foreach (var item in CellRect.CenteredOn(InteractionCell, radius))
        {
            Find.CurrentMap.roofGrid.SetRoof(item, null);
            FloodFillerFog.FloodUnfog(item, Find.CurrentMap);
        }
    }

    private void Animation2()
    {
        var num = timer / 5;
        var num2 = 5 - num;
        foreach (var item in CellRect.CenteredOn(InteractionCell, num2))
        {
            Find.CurrentMap.roofGrid.SetRoof(item, roofToSet);
        }

        foreach (var item2 in CellRect.CenteredOn(InteractionCell, num2 - 1))
        {
            Find.CurrentMap.roofGrid.SetRoof(item2, null);
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (!CLOSESET)
        {
            var power = Power;
            if (power is { PowerOn: false })
            {
                Messages.Message("Can not work", MessageTypeDefOf.RejectInput);
                CLOSESET = true;
                return;
            }

            Animation2();
            timer++;
            if (timer >= 30)
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
            Messages.Message("Can not work", MessageTypeDefOf.RejectInput);
            OPENSET = true;
            return;
        }

        Animation();
        timer++;
        if (timer < 30)
        {
            return;
        }

        OPENSET = true;
        timer = 0;
    }
}
