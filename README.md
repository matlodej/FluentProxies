# FluentProxies

A library that allows for creation of dynamic proxies that provide additional functionality to existing objects, such as implementing the ```INotifyPropertyChanged``` interface or adding custom properties and interfaces.

##Installation

Via [NuGet](https://www.nuget.org/packages/FluentProxies/1.0.0):
```
PM> Install-Package FluentProxies
```

##Usage

To create a proxy, call the ```CreateProxy``` method on the ```ProxyBuilder``` class. Add any desired functionality to a proxy using method chaining on the builder object. When you are done, call the ```Build``` method to build the proxy using the provided configuration.

```
Person person = new Person
{
  Name = "Jack",
  Age = 26,
};

Person proxy = ProxyBuilder.CreateProxy(person)
  .SyncWithReference()
  .Implement(Implementations.INotifyPropertyChanged)
  .Build();
```

###Syncing with reference

Calling the ```SyncWithReference``` method on a builder object will make it so the properties on the proxy object itself will have no value of their own, instead they will be linked to the getters and setters of properties on the source object the proxy was created from.

```
Person proxy = ProxyBuilder.CreateProxy(person)
  .SyncWithReference()
  .Build();
  
proxy.Name = "Nick";
Console.WriteLine(person.Name);

person.Age = 30;
Console.WriteLine(proxy.Age);

Output:
Nick
30
```

If you do not choose to sync with reference, the proxy object will retain the property values at the moment of proxy creation, but will not influence or be influenced by the original object from that point onwards - the proxy will be an entirely different instance with no connection to the original object it was created from.

###Implementations

You can add one of the predefined implementations to a proxy using the ```Implement``` method. The snippet below will create a proxy implementing the ```INotifyPropertyChanged``` interface on all of its public, non-static properties.

```
Person proxy = ProxyBuilder.CreateProxy(person)
  .Implement(Implementations.INotifyPropertyChanged)
  .Build();
```

###Custom properties

You can add a simple public property to a proxy by calling the ```AddProperty``` method.

```
Person proxy = ProxyBuilder.CreateProxy(person)
  .AddProperty<bool>("IsSpecialUser")
  .Build();
```

###Custom interfaces

You can also make the proxy implement a custom interface. The proxy must provide all of the necessary interface members, otherwise an exception will be thrown when calling the ```Build``` method.

```
Person proxy = ProxyBuilder.CreateProxy(person)
  .AddInterface<IPerson>()
  .Build();
```

###Proxy wrappers

If you chose the option to sync with reference, a property of type ```ProxyWrapper<T>``` will be added to a proxy. The wrapper contains the reference to the original object the proxy was created from. You can access the wrapper by calling the ```GetWrapper<T>``` method on the ```ProxyBuilder``` class.

```
ProxyWrapper<Person> wrapper = ProxyBuilder.GetWrapper<Person>(proxy);
wrapper.SourceReference.Name = "Ryan";
```

##Prerequisites

To create a proxy, the object must define at least one public, non-static property. Additionally, all public, non-static properties must be marked as ```virtual```. You can check for any configuration errors before building a proxy by checking the ```IsValid``` property. However, the build method will still throw an exception is some cases (for example, if you define a custom interface on a proxy without providing all of the necessary interface members).
