using System;
using UnityEngine;
using Verse;

namespace Operable_Roof;

public class Dialog_FloatRangeSlider : Window
{
    private readonly string description;
    private readonly Action<FloatRange> localConfirmAction;

    private readonly float localFrom;
    private readonly float localTo;
    private FloatRange curValue;

    public Dialog_FloatRangeSlider(string text, float from, float to, FloatRange currentValue,
        Action<FloatRange> confirmAction)
    {
        description = text;
        localFrom = (float)Math.Round(from, 0);
        localTo = (float)Math.Round(to, 0);
        forcePause = true;
        closeOnClickedOutside = true;
        curValue = currentValue;
        localConfirmAction = confirmAction;
    }

    public override Vector2 InitialSize => new(300f, 150f);

    protected override float Margin => 10f;

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var height = Text.CalcHeight(description, inRect.width);
        var rect = new Rect(inRect.x, inRect.y, inRect.width, height);
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(rect, description);
        Text.Anchor = TextAnchor.UpperLeft;
        var rect2 = new Rect(inRect.x, inRect.y + rect.height + 10f, inRect.width, 30f);
        Widgets.FloatRange(rect2, description.GetHashCode(), ref curValue, localFrom, localTo, null,
            ToStringStyle.Temperature);
        GUI.color = ColoredText.SubtleGrayColor;
        Text.Font = GameFont.Tiny;
        Widgets.Label(new Rect(inRect.x, rect2.yMax - 5f, inRect.width / 2f, Text.LineHeight),
            localFrom.ToStringTemperature());
        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(new Rect(inRect.x + (inRect.width / 2f), rect2.yMax - 5f, inRect.width / 2f, Text.LineHeight),
            localTo.ToStringTemperature());
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        var num = (inRect.width - 10f) / 2f;
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - 30f, num, 30f), "CancelButton".Translate()))
        {
            Close();
        }

        if (!Widgets.ButtonText(new Rect(inRect.x + num + 10f, inRect.yMax - 30f, num, 30f), "OK".Translate()))
        {
            return;
        }

        localConfirmAction(curValue);
        Close();
    }
}