namespace DataAccess.Interfaces;

public interface IProfanityDbContextFactory
{
    ProfanityDbContext Create(bool isGlobal);
}