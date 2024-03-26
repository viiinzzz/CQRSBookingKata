namespace VinZ.ToolBox;

public abstract record RecordWithValidation
{
    public RecordWithValidation() => Validate();

    protected virtual void Validate() { }
}