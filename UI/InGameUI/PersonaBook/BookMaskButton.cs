using Godot;
using System;
using System.Collections.Generic;

public partial class BookMaskButton : Button
{
	[Export] public Control MaskDescription;
	[Export] public Label Description;

	Dictionary<string, string> MaskDescriptionDic = new Dictionary<string, string>()
	{
		{"Button1", "谈判家: 善于在规则与情感间权衡利益，精于算计的沟通者。通常表现出利益导向、规则意识和共情表达。"},
		{"Button2", "投机者: 表面守规则，实则寻找漏洞谋利，言行常矛盾。通常表现出利益导向、规则意识和矛盾言行。"},
		{"Button3", "独行客: 以利益为导向、共情为手段，但排斥合作的实用主义者。通常表现出利益导向、共情表达和排斥合作。"},
		{"Button4", "操控者: 以“保护”之名行控制之实，言行不一，核心仍是利己。通常表现出利益导向、矛盾言行和保护倾向。"},
		{"Button5", "守序者: 严格遵循规则，排斥合作，有强烈领地与责任意识。通常表现出规则意识、排斥合作和保护倾向。"},
		{"Button6", "内耗者: 高共情但言行矛盾，排斥深入合作，常陷入自我纠结。通常表现出共情表达、矛盾言行和排斥合作。"},
		{"Button7", "共鸣者: 充满保护欲与共情心，但情绪化和矛盾使其保护效率低下。通常表现出共情表达、矛盾言行和保护倾向。"},
		{"Button8", "执法者: 恪守规则且具共情力，但宁愿独自行动维护自认的“正义“。通常表现出规则意识、共情表达和保护倾向。"},
	};

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (MaskDescription != null)
			MaskDescription.Visible = false;
		
		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (MaskDescription != null && MaskDescription.Visible)
		{
			Vector2 mousePos = GetGlobalMousePosition();
			MaskDescription.Position = mousePos + new Vector2(25, 25);
		}
	}

	public void OnMouseEntered()
	{
		if (MaskDescription == null) return;
		if (MaskDescriptionDic.ContainsKey(Name))
		{
			Description.Text = MaskDescriptionDic[Name];
		}

		MaskDescription.Visible = true;
	}

	public void OnMouseExited()
	{
		if (MaskDescription != null)
			MaskDescription.Visible = false;
	}
	
}
