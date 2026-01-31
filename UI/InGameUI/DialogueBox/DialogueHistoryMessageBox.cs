using Godot;
using System;

public partial class DialogueHistoryMessageBox : Control
{
	[Export] public int MaxLabelWidth { get; set; } = 400;
	[Export] public bool AlignLeft { get; set; } = false;
	private Panel Panel => field ??= GetNode<Panel>("%Panel");
	private Label DummyLabel => field ??= GetNode<Label>("%DummyLabel");
	private Label DisplayLabel => field ??= GetNode<Label>("%DisplayLabel");
	public override void _Process(double delta)
	{
		UpdateWhole();
		UpdateDummyLabel();
		UpdateDisplayLabel();
	}
	private void UpdateWhole()
	{
		Vector2 size = CustomMinimumSize;
		size.Y = Panel.GetSize().Y;
		size.X = MaxLabelWidth;
		CustomMinimumSize = size;
		SizeFlagsHorizontal = AlignLeft ? SizeFlags.ShrinkBegin : SizeFlags.ShrinkEnd;
	}
	private void UpdateDummyLabel()
	{
		Vector2 size = DummyLabel.CustomMinimumSize;
		size.X = MaxLabelWidth;
		DummyLabel.Size = size;
		DummyLabel.HorizontalAlignment = AlignLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
	}
	private void UpdateDisplayLabel()
	{
		Vector2 size = DisplayLabel.CustomMinimumSize;
		size.X = 200f;
		DisplayLabel.Size = size;
		GD.Print(size);
		DisplayLabel.HorizontalAlignment = AlignLeft ? HorizontalAlignment.Left : HorizontalAlignment.Right;
		DisplayLabel.Text = DummyLabel.Text;
	}
}
