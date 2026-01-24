using CommunityToolkit.Mvvm.Messaging.Messages;
using ServerAppDesktop.Models;

namespace ServerAppDesktop.Messaging;

// Mensaje que transporta el estado del servidor
public class ServerStateChangedMessage(ServerState value) : ValueChangedMessage<ServerState>(value);