namespace VinZ.Common;

public abstract record RecordWithValidation
{
    public RecordWithValidation() => Validate();

    protected virtual void Validate() { }
}