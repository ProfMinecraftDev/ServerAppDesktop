
namespace ServerAppDesktop.Messaging;


public class ServerStateChangedMessage(ServerStateType value) : ValueChangedMessage<ServerStateType>(value);
