using System.Linq;
using UnityEngine;
using Verse;

namespace Operable_Roof;

public class PlaceWorker_OperatableRoof : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        GenDraw.DrawFieldEdges(CellRect.CenteredOn(center, 5).Cells.ToList());
    }
}