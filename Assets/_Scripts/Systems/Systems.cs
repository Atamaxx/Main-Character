/// <summary>
/// Don't specifically need anything here other than the fact it's persistent.
/// I like to keep one main object which is never killed, with sub-systems as children.
/// </summary>
namespace Systems.Persistence
{
    public class Systems : PersistentSingleton<Systems>
    {

    }
}