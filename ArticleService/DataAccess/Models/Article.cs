namespace DataAccess.Models;

public class Article
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Continent { get; set; }
    public bool IsGlobal { get; set; }
}