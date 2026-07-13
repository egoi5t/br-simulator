using System.Collections.Generic;

[System.Serializable]
public class CustomerData
{
    public string CustomerId;
    public string Name;

    public int CustomerDay;

    public int ScoopCount;

    public string ContainerType;

    public List<string> FlavorIds;

    public string OrderLine;

    public string SatisfiedLine;

    public string UnhappyLine;
}