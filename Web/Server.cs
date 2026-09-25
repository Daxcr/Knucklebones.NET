using Rose.Net;

namespace CotLMinigames.Web;

public class Server : GenericServer
{
    public Server()
    {
        OnClientConnect += ClientConntect;
        OnClientRequest += ClientRequest;
        OnClientClose += ClientClose;
    }

    public async Task ClientConntect(Client client) { }

    public async Task ClientRequest(Client client)
    {
        await ApplyRoutes(client);
        if (client.RouteMatched)
            return;
            
        await client.RespondStatic();
    }
    
    public async Task ClientClose(Client client) { }
}