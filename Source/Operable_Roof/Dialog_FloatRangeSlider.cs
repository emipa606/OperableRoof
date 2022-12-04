using System;
using UnityEngine;
using Verse;

namespace Operable_Roof;

public class Dialog_FloatRangeSlider : Window
{
    private readonly Action<FloatRange> ConfirmAction;
    public readonly string Description;

    public readonly float From;
    public readonly float To;
    private FloatRange curValue;

    public Dialog_FloatRangeSlider(string text, float from, float to, FloatRange currentValue,
        Action<FloatRange> confirmAction)
    {
        Description = text;
        From = (float)Math.Round(from, 0);
        To = (float)Math.Round(to, 0);
        forcePause = true;
        closeOnClickedOutside = true;
        curValue = currentValue;
        ConfirmAction = confirmAction;
    }

    public override Vector2 InitialSize => new Vector2(300f, 150f);

    protected override float Margin => 10f;

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var height = Text.CalcHeight(Description, inRect.width);
        var rect = new Rect(inRect.x, inRect.y, inRect.width, height);
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.Label(rect, Description);
        Text.Anchor = TextAnchor.UpperLeft;
        var rect2 = new Rect(inRect.x, inRect.y + rect.height + 10f, inRect.width, 30f);
        Widgets.FloatRange(rect2, Description.GetHashCode(), ref curValue, From, To, null, ToStringStyle.Temperature);
        GUI.color = ColoredText.SubtleGrayColor;
        Text.Font = GameFont.Tiny;
        Widgets.Label(new Rect(inRect.x, rect2.yMax - 5f, inRect.width / 2f, Text.LineHeight),
            From.ToStringTemperature());
        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(new Rect(inRect.x + (inRect.width / 2f), rect2.yMax - 5f, inRect.width / 2f, Text.LineHeight),
            To.ToStringTemperature());
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

        ConfirmAction(curValue);
        Close();
    }
}