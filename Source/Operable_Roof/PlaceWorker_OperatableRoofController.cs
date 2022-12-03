using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Operable_Roof;

public class PlaceWorker_OperatableRoofController : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        foreach (var intVec3 in CellRect.CenteredOn(center, 15))
        {
            foreach (var building in intVec3.GetThingList(Find.CurrentMap).ToList())
            {
                if (building is Operable_Roof)
                {
                    GenDraw.DrawLineBetween(building.TrueCenter(), center.ToVector3());
                }
            }
        }
    }
}