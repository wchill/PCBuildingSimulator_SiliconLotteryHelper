using BepInEx;
using System;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using PCBS;

namespace PCBS_SL;

[BepInPlugin(Plugin.pluginGuid, Plugin.pluginName, Plugin.pluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public const string pluginGuid = "com.wchill.pcbs.sl";
    public const string pluginName = "PCBS Silicon Lottery Helper";
    public const string pluginVersion = "1.0";

    public static ConfigEntry<KeyboardShortcut> PrintSiliconLottery1;
    public static ConfigEntry<KeyboardShortcut> PrintSiliconLottery2;
    public static ConfigEntry<KeyboardShortcut> PrintSiliconLottery3;

    public void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {pluginGuid} is loaded!");
        PrintSiliconLottery1 = Config.Bind<KeyboardShortcut>("Hotkeys", "Print silicon lottery for PC 1", new KeyboardShortcut(KeyCode.F4, new KeyCode[1] {KeyCode.LeftShift}), "config description");
        PrintSiliconLottery2 = Config.Bind<KeyboardShortcut>("Hotkeys", "Print silicon lottery for PC 2", new KeyboardShortcut(KeyCode.F5, new KeyCode[1] {KeyCode.LeftShift}), "config description");
        PrintSiliconLottery3 = Config.Bind<KeyboardShortcut>("Hotkeys", "Print silicon lottery for PC 3", new KeyboardShortcut(KeyCode.F6, new KeyCode[1] {KeyCode.LeftShift}), "config description");
    }

    public void Update()
    {
        int benchSlot = -1;
        if (PrintSiliconLottery1.Value.IsDown())
        {
            benchSlot = 0;
        }
        else if (PrintSiliconLottery2.Value.IsDown())
        {
            benchSlot = 1;
        }
        else if (PrintSiliconLottery3.Value.IsDown())
        {
            benchSlot = 2;
        }

        if (benchSlot < 0) return;
        BenchSlot[] benchSlots = WorkshopController.Get().slsys.benchSlots;

        if (benchSlots[benchSlot].m_type != BenchSlot.Type.WORKBENCH || benchSlots[benchSlot] == null)
        {
            ShowMessageBox("Silicon Lottery", "No computer there");
            return;
        }

        PartInstance caseID = benchSlots[benchSlot].GetComputerButDontDoAnythingElseWithIt().caseID;
        if (caseID == null)
        {
            ShowMessageBox("Silicon Lottery", "No computer there");
            return;
        }

        StringBuilder sb1 = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        ComputerSave computer = benchSlots[benchSlot].GetComputer();
        sb1.Append($"PC {benchSlot} ({computer.caseID.GetPart().m_uiName}):\n");
        sb2.Append($"PC {benchSlot} ({computer.caseID.GetPart().m_uiName}):\n");
        PrintPartInfo(sb1, sb2, "CPU", computer.cpuID);
        PrintPartsInfo(sb1, sb2, "GPU", computer.pciSlots);
        PrintPartsInfo(sb1, sb2, "RAM", computer.ramSlots);

        Logger.LogInfo(sb1.ToString().Trim());
        ShowMessageBox("Silicon Lottery", sb2.ToString().Trim());
    }

    public void ShowMessageBox(string title, string body)
    {
        MessageBox messageBox = CommonUI.messageBox;
        UIStateB currentState = new UIStateB(messageBox.gameObject);
        GameController.Get().SetCurrentState(currentState);
        messageBox.m_title.text = title;
        messageBox.m_body.text = body;
        messageBox.m_apply.gameObject.SetActive(false);
        messageBox.m_no.gameObject.SetActive(false);
        messageBox.m_ok.gameObject.SetActive(true);
        TextGenerator textGenerator = new TextGenerator(messageBox.m_body.text.Length);
        Vector2 size = messageBox.m_body.gameObject.GetComponent<RectTransform>().rect.size;
        textGenerator.Populate(messageBox.m_body.text, messageBox.m_body.GetGenerationSettings(size));
    }
    
    public void OnDestroy()
    {
    }

    private string FloatToHex(float val)
    {
        var bytes = BitConverter.GetBytes(val);
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
    }

    private void PrintPartInfo(StringBuilder sb1, StringBuilder sb2, string partName, PartInstance part)
    {
        float siliconLottery = part.GetSiliconLottery();
        string hexSiliconLottery = FloatToHex(siliconLottery);

        sb1.Append($"- {partName} ({part.GetPart().m_uiName}): {siliconLottery} ({hexSiliconLottery})\n");
        sb2.Append($"- {partName}: {siliconLottery}\n");
    }

    private void PrintPartsInfo(StringBuilder sb1, StringBuilder sb2, string partName, PartInstance[] parts)
    {
        for (int j = 0; j < parts.Length; j++)
        {
            PartInstance part = parts[j];
            if (part == null) continue;

            PrintPartInfo(sb1, sb2, $"{partName} {j}", part);
        }
    }
}

class UIStateB : State
{
	protected GameObject m_ui;

	protected bool m_showBackButton = false;

	public UIStateB(GameObject ui)
	{
		m_ui = ui;
	}

	public override GameState GetState()
	{
		return GameState.UI;
	}

	public GameObject GetUIGameObject()
	{
		return m_ui;
	}

	public override bool ShowExitButton()
	{
		return m_showBackButton;
	}

	public override bool HandleExit()
	{
		return false;
	}

	public override void Tick()
	{
		InputModule.SetCursorVisible(visible: true);
	}

	public override void OnStateEnter()
	{
		RealIllumination.ShowKeyCategories("Default");
		ctrl.PauseGame();
		m_ui.SetActive(value: true);
	}

	public override void OnStateExit()
	{
		ctrl.UnpauseGame();
		m_ui.SetActive(value: false);
	}
}