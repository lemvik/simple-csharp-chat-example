# Simple chat 

Implements test assignment from a company I was interviewing for.

## Assignment 

Design and implement a chat service. Make it as clean as you can.

### Required features

- [x] Has a client class library
  See [client library](./src/client-library)
- [x] Has a server class library
  See [server library](./src/server-library)
  Both server and client libraries are transport-agnostic `netstandard2.0` libraries, so both 
  can be used even in Unity.
- [x] Clients can send chat messages
  Client can send chat message to particular room via `IClientChatRoom` instance
  that can be obtained via `JoinRoom` call on connected client.
- [x] Clients can receive chat messages
  Client receives messages by polling `IClientChatRoom.GetMessage()` method of the rooms
  client is connected to.
- [x] Server application has a chat room object
  Server operates on `IServerChatRoom` implementations, those can have any 
  behavior in addition to explicit interface implementation.
- [ ] Chat room object stores chat history
- [x] Both applications have debug logging
  This is somewhat hazy as there is indeed some debug logging here and there, 
  but I'm not sure if it's sufficient.
- [x] As a developer I can inject different type of logger implementation
  The code uses `Microsoft.Extensions.Logging.Abstractions` everywhere, so
  injecting own logger boils down to writing an `ILogger` implementation and injecting
  it via DI (which is `Microsoft.Extensions.DependencyInjection` in examples).
  Tests actualy inject custom logger as otherwise it won't capture the output.
- [x] Has tests
  See [tests project](./tests/chat-tests)

### Constraints

- [x] Must be .Net or .Net Core
  Libraries are `netstandard2.0`, examples are `net5.0`.

### Other considerations

- [ ] Manual user input is not mandatory

