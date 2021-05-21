# Simple chat 

[![Build Status](https://lemvic.visualstudio.com/websocket-chat/_apis/build/status/lemvik.simple-csharp-chat-example?branchName=master)](https://lemvic.visualstudio.com/websocket-chat/_build/latest?definitionId=1&branchName=master)

Implements test assignment from a company I was interviewing for.

## Running

1. Build, test and publish
   ```zsh
   dotnet build
   dotnet test
   dotnet publish
   ```
2. Navigate to published server
   ```zsh
   pushd ./examples/published/server
   ./chat-server
   # Something happening, Ctrl-C when finished
   ```
   same for client (in another terminal, maybe run several of them)
   ```zsh
   pushd ./examples/published/server
   ./chat-server
   # Something happening, Ctrl-C when finished
   ```
3. Some interaction in client terminal
   ```
   Victor entered the chat
   :l
   Barrens-General
   LFG
   :c NewRoom
   :j LFG
   [room=LFG][user=Victor]: HEllo
   Joined ChatRoom[Id=2,Name=LFG]
   :s LFG Hello from another window
   [room=LFG][user=Antti]: Hello from another window
   [room=LFG][user=Victor]: Well, hello there
   ```

## Some notes 

- The chat library itself relies on the underlying transport (implementation of `IChatTransport`) to throw an error from `Receive` call on disconnection. 
  Unfortunately, with plain TCP implementation that is bundled in examples one can wait for quite a while until OS closes the socket and `Receive` errors out. 
  One possible workaround is to have a transport-level keepalive but I didn't implement it.
  - In tests this is achieved by closing writing part of the `Channel`-based transport, which is instantaneous.
- The API provided is centered around proxy-like objects (especially on the client - `IRoom` there is a proxy), so some scenarios like "listen to any message in any room"
  will required something like `Task.WhenAny(rooms.Select(room => room.GetMessage()))` polling.
- Polling - the API doesn't use callbacks as it's simpler to use polling on `Task`s and it gives more control. But implementing callback-based API on top of `Task` is entirely possible, just like it's possible to do the other way round.
- ! First build after `dotnet clean` fails: see [non-reproducible issue on GitHub](https://github.com/grpc/grpc/issues/18625) that reproduces all-right on my machine.
- Short description of client (admittedly poor) commands (found in [`examples/console-interaction`](./examples/console-interaction/ConsoleCommandsReader.cs)):
  - `:l` - list available rooms
  - `:c <RoomNameSingleWord>` - create room
  - `:j <RoomNameSingleWord>` - join room
  - `:s <RoomNameSingleWord> Message any number of words` - send message to the room

