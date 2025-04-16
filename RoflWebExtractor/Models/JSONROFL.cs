namespace RoflWebExtractor.Models;

public class JSONROFL<T>
{
    public string match { get; set; }
    
    public T data { get; set; }
}