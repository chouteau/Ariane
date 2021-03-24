# Welcome to Ariane (5.0.1) for Dotnet5 or Dotnet Standard 2.0

Ariane is a simple bus manager for asynchronous messages based on type or dynamic objects for later treatment, Ariane manage queueing system in one place with one thread by queue and multi readers by queue

### List of queue provider

* In Memory
* MSMQ
* Azure Service Bus 
* File System
* Custom...

## Installing

You can download http://nuget.org/packages/Ariane5 from NuGet in Visual Studio, With Package Manager Console type:

```
PM> Install-Package Ariane5
```

#### Example 
* *Client*

##### Get service bus

```c#

serviceCollection.ConfigureAriane(register =>
{
    register.AddMSMQReader<Person>("q1");
});

var serviceProvider = serviceCollection.BuildServiceProvider();

var sb = serviceProvider.GetRequiredService<IServiceBus>();
await sb.StartReadingAsync();

```

# Welcome to Ariane (5.0.59) for Dotnet4.7

## Installing

You can download http://nuget.org/packages/Ariane from NuGet in Visual Studio, With Package Manager Console type:

```
PM> Install-Package Ariane
```

#### Example 
* *Client*

##### Get service bus

```c#

var bus = new Ariane.BusManager(); 

```

##### Register the writer with name or configuration file

```c#

bus.Register.AddMemoryWriter("test"); 

```

##### Create message

```c#

var person = new Person();
person.FirsName = i.ToString();
person.LastName = Guid.NewGuid().ToString();

```

##### Send typed message or dynamic 

```c#

bus.Send("test", person);

```

* *Server*

##### Get service bus

```c#

var bus = new Ariane.BusManager(); 

```

##### Register concrete reader

```c#

bus.Register.AddMemoryReader("test", typeof(PersonMessageReader)); 

```

##### or register anonymous reader

```c#

bus.Register.AddMemoryReader<Person>("test", (message) => {
     Console.WriteLine("{0}:{1}", System.Threading.Thread.CurrentThread.Name, message.FirsName);
}); 

```

##### Start reading

```c#

bus.StartReading(); 

```

##### Receive message from dedicated thread

```c#

public class PersonMessageReader : MessageReaderBase<Person>
{
	public override void ProcessMessage(Person message)
	{
		Console.WriteLine("{0}:{1}", System.Threading.Thread.CurrentThread.Name, message.FirsName);
	}
}

```

### Using dynamic and anonymous reader in memory

```c#

var bus = new Ariane.ServiceBus();

bus.Register.AddMemoryReader<System.Dynamic.ExpandoObject>("queueName", (p) =>
{
     Console.WriteLine("person : {0} {1}", p.FirstName, p.LastName);
}); 

dynamic person = bus.CreateMessage("person");
person.FirstName = "ftest";
person.LastName = "ltest";

bus.Send("queueName", person);

```
