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
- [x] Chat room object stores chat history
  This one was a bit unclear - shall it store internally or persist chat history. There is an interface `IMessageTracker` that allows
  providing an entity that will either store messages in memory (as bundled `InMemoryMessageTracker` does) or persist it somewhere
  (I didn't provide implementation for that). Chat rooms on server use that interface.
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

- [x] Manual user input is not mandatory
  There is a client example that uses super-simple (and not really convenient) interaction loop with following commands:
  - `:i` - output information about client connection (now only client name)
  - `:l` - list rooms (requests list of rooms from server), needs to be run before joining anything
  - `:j <ROOM>` - joins room `<ROOM>`
  - `:s <ROOM> <Message>` - sends message to `<ROOM>`, need to join that one first

  The client is quite rudimentary but works:
  ```
√ published/client > ./chat-client
:l
Barrens-General
LFG
:j LFG
[room=LFG][user=Victor]: HEllo
Joined ChatRoom[Id=2,Name=LFG]
:s LFG Hello from another window
[room=LFG][user=Antti]: Hello from another window
[room=LFG][user=Victor]: Well, hello there

  ```
