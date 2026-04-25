namespace SubscriberService.DataAccess.Interfaces;

public interface ISubscriberDbContextFactory
{ 
    SubscriberDbContext Create(bool isGlobal);
}