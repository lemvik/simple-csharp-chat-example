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

- [ ] Manual user input is not mandatory

### Some notes 

- The chat library itself relies on the underlying transport (implementation of `IChatTransport`) to throw an error from `Receive` call on disconnection. 
  Unfortunately, with plain TCP implementation that is bundled in examples one can wait for quite a while until OS closes the socket and `Receive` errors out. 
  One possible workaround is to have a transport-level keepalive but I didn't implement it.
  - In tests this is achieved by closing writing part of the `Channel`-based transport, which is instantaneous.
- The API provided is centered around proxy-like objects (especially on the client - `IRoom` there is a proxy), so some scenarios like "listen to any message in any room"
  will required something like `Task.WhenAny(rooms.Select(room => room.GetMessage()))` polling.
- Polling - the API doesn't use callbacks as it's simpler to use polling on `Task`s and it gives more control. But implementing callback-based API on top of `Task` is entirely possible, just like it's possible to do the other way round.
- ! First build after `dotnet clean` fails: see [non-reproducible issue on GitHub](https://github.com/grpc/grpc/issues/18625) that reproduces all-right on my machine.
