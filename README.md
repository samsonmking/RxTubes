# RxTubes
Simple library providing sockets API based on IObservable.

```C#
var anExampleMessage = new FixedHeaderMessage()
	.SetHeaderLength(4)
	.ParseForMessageLength(header => BitConverter.ToInt32(header, 0));

var client = new ReactiveClient("127.0.0.1", 1000, anExampleMessage);

client.WhenMessage
	.Select(bytes => Encoding.ASCII.GetString(bytes))
	.Subscribe(msg => Console.WriteLine(msg));
```
