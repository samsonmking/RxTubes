# RxTubes
Simple library providing sockets API based on IObservable.

```
var anExampleMessage = new FixedHeaderMessage()
	.SetHeaderLength(4)
	.ParseForMessageLength(header => BitConverter.ToInt32(header, 0));

var client = new ReactiveClient(_ip, _port, anExampleMessage);

client.WhenMessage
	.Select(bytes => Encoding.ASCII.GetString(bytes))
	.Subscribe(msg => Console.WriteLine(msg));
```
