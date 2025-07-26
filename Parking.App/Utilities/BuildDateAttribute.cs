

[AttributeUsage(AttributeTargets.Assembly)]
public class BuildDateAttribute : Attribute
{
    public string Date { get; }
    public BuildDateAttribute(string date)
    {
        Date = date;
    }
}