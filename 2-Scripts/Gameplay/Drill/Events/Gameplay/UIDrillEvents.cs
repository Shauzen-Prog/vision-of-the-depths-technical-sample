public struct BarControllerEvent
{
    public bool needToShowBar;
    public BarControllerEvent(bool needToShowBar) => this.needToShowBar = needToShowBar;
}

public struct UpdateMaterialToDrillUI
{
    public bool needToShowMaterial;
    public UpdateMaterialToDrillUI(bool needToShowMaterial) => this.needToShowMaterial = needToShowMaterial;
    
}
