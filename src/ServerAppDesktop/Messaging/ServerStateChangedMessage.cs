
namespace ServerAppDesktop.Messaging;


public class ServerStateChangedMessage(ServerState value) : ValueChangedMessage<ServerState>(value);
