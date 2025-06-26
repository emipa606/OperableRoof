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
    private static readonly Texture2D openGizmoTexture2D;

    private static readonly Texture2D closeGizmoTexture2D;

    private static readonly Texture2D autoOpenTimeGizmoTexture2D;

    private static readonly Texture2D autoCloseTimeGizmoTexture2D;

    private static readonly Texture2D autoTempGizmoTexture2D;

    private bool canClose = true;

    private bool canOpen = true;
    private int closeOnTimer;
    private FloatRange closeTempFloatRange;

    private List<Operable_Roof> connectedBuildings;
    private int openOnTimer;

    private FloatRange openTempFloatRange;

    private CompPowerTrader power;

    static Operable_Roof_Controller()
    {
        openGizmoTexture2D = ContentFinder<Texture2D>.Get("UI/OPEN");
        closeGizmoTexture2D = ContentFinder<Texture2D>.Get("UI/CLOSE");
        autoOpenTimeGizmoTexture2D = ContentFinder<Texture2D>.Get("UI/autoopentimer");
        autoCloseTimeGizmoTexture2D = ContentFinder<Texture2D>.Get("UI/autoclosetimer");
        autoTempGizmoTexture2D = ContentFinder<Texture2D>.Get("UI/autotemp");
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(base.GetInspectString());
        stringBuilder.AppendLine("OpRo.ConnectedTo".Translate(connectedBuildings.Count));

        if (openTempFloatRange != FloatRange.Zero)
        {
            stringBuilder.AppendLine("OpRo.OpensTemp".Translate(openTempFloatRange.min.ToStringTemperature(),
                openTempFloatRange.max.ToStringTemperature()));
        }

        if (closeTempFloatRange != FloatRange.Zero)
        {
            stringBuilder.AppendLine("OpRo.ClosesTemp".Translate(closeTempFloatRange.min.ToStringTemperature(),
                closeTempFloatRange.max.ToStringTemperature()));
        }

        if (openOnTimer > -1)
        {
            stringBuilder.AppendLine("OpRo.OpensTimer".Translate(openOnTimer));
        }

        if (closeOnTimer > -1)
        {
            stringBuilder.AppendLine("OpRo.ClosesTimer".Translate(closeOnTimer));
        }

        return stringBuilder.ToString().TrimEndNewlines();
    }

    protected override void Tick()
    {
        base.Tick();
        if (GenTicks.TicksGame % GenTicks.TickRareInterval != 0)
        {
            return;
        }

        UpdateConnectedBuildings();
        checkAutomaticRules();
    }


    public override void DrawExtraSelectionOverlays()
    {
        base.DrawExtraSelectionOverlays();
        foreach (var connectedBuilding in connectedBuildings)
        {
            if (connectedBuilding.Power.PowerOn && power.PowerOn)
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


    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref openOnTimer, "openOnTimer", -1);
        Scribe_Values.Look(ref closeOnTimer, "closeOnTimer", -1);
        Scribe_Values.Look(ref openTempFloatRange, "openTempFloatRange", FloatRange.Zero);
        Scribe_Values.Look(ref closeTempFloatRange, "closeTempFloatRange", FloatRange.Zero);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (canOpen)
        {
            yield return new Command_Action
            {
                action = open,
                defaultLabel = "OpRo.OpenRoofs".Translate(),
                defaultDesc = "OpRo.OpenRoofsTT".Translate(),
                icon = openGizmoTexture2D
            };
        }

        if (canClose)
        {
            yield return new Command_Action
            {
                action = close,
                defaultLabel = "OpRo.CloseRoofs".Translate(),
                defaultDesc = "OpRo.CloseRoofsTT".Translate(),
                icon = closeGizmoTexture2D
            };
        }

        yield return new Command_Action
        {
            action = delegate
            {
                var list = new List<FloatMenuOption>
                {
                    new("OpRo.SelectOpenTemp".Translate(),
                        delegate
                        {
                            var dialogFloatRangeSlider = new Dialog_FloatRangeSlider("OpRo.SelectOpenTemp".Translate(),
                                GenTemperature.MinTemperatureAtTile(Map.Tile) * 2,
                                GenTemperature.MaxTemperatureAtTile(Map.Tile) * 2, openTempFloatRange,
                                delegate(FloatRange range)
                                {
                                    if (range == FloatRange.Zero)
                                    {
                                        return;
                                    }

                                    openTempFloatRange = range;
                                    closeTempFloatRange = FloatRange.Zero;
                                    openOnTimer = -1;
                                    closeOnTimer = -1;
                                });
                            Find.WindowStack.Add(dialogFloatRangeSlider);
                        }),
                    new("OpRo.SelectCloseTemp".Translate(),
                        delegate
                        {
                            var dialogFloatRangeSlider = new Dialog_FloatRangeSlider("OpRo.SelectCloseTemp".Translate(),
                                GenTemperature.MinTemperatureAtTile(Map.Tile) * 2,
                                GenTemperature.MaxTemperatureAtTile(Map.Tile) * 2, closeTempFloatRange,
                                delegate(FloatRange range)
                                {
                                    if (range == FloatRange.Zero)
                                    {
                                        return;
                                    }

                                    closeTempFloatRange = range;
                                    openTempFloatRange = FloatRange.Zero;
                                    openOnTimer = -1;
                                    closeOnTimer = -1;
                                });
                            Find.WindowStack.Add(dialogFloatRangeSlider);
                        })
                };
                Find.WindowStack.Add(new FloatMenu(list));
            },
            defaultLabel = "OpRo.TempButton".Translate(getTimeString(openOnTimer)),
            defaultDesc = "OpRo.TempButtonTT".Translate(),
            icon = autoTempGizmoTexture2D
        };

        yield return new Command_Action
        {
            action = delegate { getTimerDropDown(true); },
            defaultLabel = "OpRo.OpenTimerButton".Translate(getTimeString(openOnTimer)),
            defaultDesc = "OpRo.OpenTimerButtonTT".Translate(),
            icon = autoOpenTimeGizmoTexture2D
        };

        yield return new Command_Action
        {
            action = delegate { getTimerDropDown(false); },
            defaultLabel = "OpRo.CloseTimerButton".Translate(getTimeString(closeOnTimer)),
            defaultDesc = "OpRo.CloseTimerButtonTT".Translate(),
            icon = autoCloseTimeGizmoTexture2D
        };
    }


    private static string getTimeString(int timeInt)
    {
        if (timeInt == -1)
        {
            return "OpRo.Never".Translate();
        }

        return $"{timeInt}H";
    }


    private void getTimerDropDown(bool open)
    {
        var list = new List<FloatMenuOption>();
        for (var i = 0; i < 24; i++)
        {
            if (open && i == closeOnTimer ||
                !open && i == openOnTimer)
            {
                continue;
            }

            var timer = i;
            list.Add(new FloatMenuOption($"{i}H", delegate
            {
                if (open)
                {
                    openOnTimer = timer;
                }
                else
                {
                    closeOnTimer = timer;
                }

                closeTempFloatRange = FloatRange.Zero;
                openTempFloatRange = FloatRange.Zero;
            }));
        }

        list.Add(new FloatMenuOption("OpRo.Never".Translate(), delegate
        {
            if (open)
            {
                openOnTimer = -1;
            }
            else
            {
                closeOnTimer = -1;
            }
        }));

        Find.WindowStack.Add(new FloatMenu(list));
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (def.HasComp(typeof(CompPowerTrader)))
        {
            power = GetComp<CompPowerTrader>();
        }

        openOnTimer = -1;
        closeOnTimer = -1;
        UpdateConnectedBuildings();
    }

    public void UpdateConnectedBuildings()
    {
        connectedBuildings = [];
        foreach (var intVec3 in CellRect.CenteredOn(Position, 15))
        {
            if (!intVec3.InBounds(Find.CurrentMap))
            {
                continue;
            }

            foreach (var building in intVec3.GetThingList(Find.CurrentMap).ToList())
            {
                if (building is Operable_Roof operable_Roof)
                {
                    connectedBuildings.Add(operable_Roof);
                }
            }
        }
    }

    private void checkAutomaticRules()
    {
        if (openTempFloatRange != FloatRange.Zero)
        {
            if (openTempFloatRange.Includes(Map.mapTemperature.OutdoorTemp))
            {
                if (canOpen)
                {
                    open();
                }

                return;
            }

            if (canClose)
            {
                close();
            }

            return;
        }

        if (closeTempFloatRange != FloatRange.Zero)
        {
            if (closeTempFloatRange.Includes(Map.mapTemperature.OutdoorTemp))
            {
                if (canClose)
                {
                    close();
                }

                return;
            }

            if (canOpen)
            {
                open();
            }

            return;
        }

        if (openOnTimer != -1 && canOpen && GenLocalDate.HourOfDay(Map) == openOnTimer)
        {
            open();
            return;
        }

        if (closeOnTimer != -1 && canClose && GenLocalDate.HourOfDay(Map) == closeOnTimer)
        {
            close();
        }
    }

    private void open()
    {
        var powerTrader = power;
        if (powerTrader is { PowerOn: false })
        {
            Messages.Message("CannotUseNoPower".Translate(), MessageTypeDefOf.RejectInput);
            canOpen = true;
            canClose = false;
            return;
        }

        canOpen = false;
        canClose = true;

        UpdateConnectedBuildings();
        foreach (var operableRoof in connectedBuildings)
        {
            operableRoof.OpenSet = false;
        }
    }

    private void close()
    {
        var powerTrader = power;
        if (powerTrader is { PowerOn: false })
        {
            Messages.Message("CannotUseNoPower".Translate(), MessageTypeDefOf.RejectInput);
            canOpen = false;
            canClose = true;
            return;
        }

        canOpen = true;
        canClose = false;

        UpdateConnectedBuildings();
        foreach (var operableRoof in connectedBuildings)
        {
            operableRoof.Closed = false;
        }
    }
}