using Godot;
using System;
[GlobalClass]
public partial class ResponsiveButton : Button
{
	private const float HoverScale = 1.1f;
	private Vector2 _scaleVector = Vector2.Zero;
	private const float Duration = 0.1f;
	public sealed override void _Ready()
	{
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
		_scaleVector = new Vector2(HoverScale, HoverScale);
		PivotOffset = Size / 2;
		ReadyBehavior();
	}
    public override void _Process(double delta)
    {
        PivotOffset = Size / 2;
    }

	protected virtual void ReadyBehavior() {}
	private void OnMouseEntered()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", _scaleVector, Duration)
			.SetTrans(Tween.TransitionType.Quart)
			.SetEase(Tween.EaseType.Out);
	}
	private void OnMouseExited()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One, Duration)
			.SetTrans(Tween.TransitionType.Quart)
			.SetEase(Tween.EaseType.Out);
	}
}
