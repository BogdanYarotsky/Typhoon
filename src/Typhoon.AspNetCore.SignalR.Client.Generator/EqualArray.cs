namespace Typhoon.AspNetCore.SignalR.Client.Generator;

public readonly struct EqualArray<T>
{
    public T[] Values { get; }
    
    public EqualArray(T[] values) => Values = values;
    
    public override bool Equals(object obj)
    {
        if (obj is not EqualArray<T> other) 
            return false;
        
        if (Values.Length != other.Values.Length)
            return false;
        
        for (var i = 0; i < Values.Length; i++)
            if (!Values[i].Equals(other.Values[i]))
                return false;
                
        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = 17;
        
        for (var i = 0; i < Values.Length; i++)
            hash = hash * 31 + Values[i].GetHashCode();
        
        return hash;
    }
}