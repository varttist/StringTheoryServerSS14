namespace Content.Server._ST14.PrinterInsert
{
    [RegisterComponent]
    public sealed partial class PrinterComponent : Component
    {
        [DataField]
        public string UserName = "";

        [DataField]
        public string UserJob = "";
    }
}
