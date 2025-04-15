# Event Grid Events

In order to decouple external services, such as the site-wide search, from the Admin website and Publisher function app, a number of events are published to signal changes into Azure's Event Grid. These events are subscribed for and handled accordingly by Azure Functions.

The following classes have been created to make this mechanism simple to use.

## Event Raisers

_IAdminEventRaiserService_ and _IPublisherEventRaiserService_ encapsulate the creation of events specific to the Admin and Publisher respectively.

For example, consumers of _IAdminEventRaiserService_ delegate all knowledge of raising an On Theme Updated Event.

```csharp
public interface IAdminEventRaiser
{
    Task OnThemeUpdated(Theme theme);
    ...
}
```

A consumer invokes the appropriate method to specify that an event is to be raised, and passes in some minimal context information without any knowledge of the events, event grid etc.


# How to Add a New Event

There are many parts to raising and consuming an event in the EES system.

| Part              | Description                                                   |
| --                | --                                                            |
| Event Domain Model      | Our representation of the event. It also contains the code to create the EventGridEvent | 
| Method on Event Raiser | Consumers wishing to raise the event do so by calling the Event Raiser e.g. IAdminEventRaiser.OnThemeChanged |
| Topic             | a grouping of event types in Azure's Event Grid. In Azure terminology, we use _Custom Topics_. See [Azure Documentation on Custom Topics](https://learn.microsoft.com/en-us/azure/event-grid/custom-topics) |
| Topic Configuration | The topic details are read from configuation. This is setup in bicep. |
| Subscription      | when an event is raised, it is handled by zero or more subscriptions. Typically, the event is copied into a handler queue to await processing |
| Queue             | Holds a queue of event messages ready to be processed by the Event Handler |
| Event Handler     | Some code that performs a task in response to an event being raised. For example, there are Azure functions that consume the events that are queued by the subscription. |

## Creating the Domain Event Model

Azure's Event Grid deals with generic _Event Grid Events_. Therefore, we need a nice domain representation of our event that can encapsulate the Event Grid Event concepts (see below) and creation.

### Event Grid Concepts

| Concept      | Definition                                                                                 |
| --           | --                                                                                         |
| Topic        | The group of events into which the event will be published. Subscriptions are made against a Topic |
| Event Type   | Each event has an event type. Subscribers can add a filter to recieve only specified event types |
| Subject      | Typically, the Id of the resource to which the event relates. e.g. For an On Theme Updated event, the Subject is the Theme Id |
| Payload      | Accompanying information related to the event. |
| DataVersion  | The payload is a serialised stream. The DataVersion can be used to determine the contract and appropriate shaped dto to deserialise to. |

### Example Event

Here is a breakdown of the Theme Changed event in the Admin area.

Pass in the theme into the constructor, and extract the information we want to attach to the event.
The subject of the event is the Theme, so assign its Id to the Subject property.
In this case, we've decided to also attach the theme's title, summary and slug values to the event.

```csharp
public record ThemeChangedEvent : IEvent
{
    public ThemeChangedEvent(Theme theme)
    {
        Subject = theme.Id.ToString();
        Payload = new EventPayload
        {
            Title = theme.Title,
            Summary = theme.Summary,
            Slug = theme.Slug
        };
    }
```
Subject is just an auto property. It is used in the creation of the Event Grid Event later.
```csharp
    /// <summary>
    /// The ThemeId is the subject
    /// </summary>
    public string Subject { get; }
```

EventPayload is a nested class defined inside the event.
```csharp
    /// <summary>
    /// The event payload
    /// </summary>
    public EventPayload Payload { get; }
    
    public record EventPayload
    {
        public string Title { get; init; }
        public string Summary { get; init; }
        public string Slug { get; init; }
    }
```
Other properties of the Event Grid Event are defined explicitly for clarity. The event type can be used to filter instances of this event in or out of a subscription. The DataVersion declares the version of the contract to allow consumers of the event to deserialise the payload. It is important that if any changes to the payload occur, especially where a breaking change can not be avoided, that the DataVersion reflects this, and that all consumers are written to take into consideration the version of the payload they are processing.
```csharp
    // Changes to this event should also increment the version accordingly.
    private const string DataVersion = "1.0";
    private const string EventType = "theme-changed";
```
The details of each of the Topics is declared in Configuration under a well known configuration key. Each event exposes to which Topic it belongs via this key.
```csharp
    // Which Topic endpoint to use from the appsettings
    public static string EventTopicOptionsKey => "ThemeChangedEvent";
```
Finally, the creation of the EventGridEvent simply takes in the information declared above.
```csharp
    public EventGridEvent ToEventGridEvent() => new(Subject, EventType, DataVersion, Payload);
}

```

## Adding an event to Event Raiser
To insulate the consuming code from any event knowledge, the consumer needs only to inject the IAdminEventRaiser (IPublisherEventRaiser in the publisher component) and call the corresponding method at the appropriate time.

To introduce a new event raising method, simply add a new method to IAdminEventRaiser/IPublisherEventRaiser.

The heavy lifting is already done by the somain model and the IEventRaiser.

```csharp
    /// <summary>
    /// On Theme Updated
    /// </summary>
    public async Task OnThemeUpdated(Theme theme) => 
        await eventRaiser.RaiseEvent(new ThemeChangedEvent(theme));
```

## Configuring the Event Topic
Each event is published to a specific Topic. If a new Topic is being introduced, then there some configuration and infrastructure steps that need to be added.

### Configuration
Each Topic endpoint must be configured so that the Event Topic client knows where to send the event request. The EES Azure environments utilse role based security, so the topic access keys remain blank. Further more, EES Topics can not be accessed from outside of the Azure subscription, and the use of access keys is disabled. The feature remains to enable testing against a separate Azure subscription.

The shape of the Topic configuration is as follows.
```json
"EventGrid":
  {
    "EventTopics":
    [
      {
        "Key":"PublicationChangedEvent",
        "TopicEndpoint":"",
        "TopicAccessKey":""
      },
      {
        "Key":"ReleaseChangedEvent",
        "TopicEndpoint":"",
        "TopicAccessKey":""
      },
      {
        "Key":"ThemeChangedEvent",
        "TopicEndpoint":"",
        "TopicAccessKey":""
      }
    ]
  }
```



### Infrastructure

#### Declaring the new Topic

#### Declaring Topic Subscriptions


# Further Details

Events can be raised without any further knowledge but for completeness, here are details of the other classes that complete the mechansim.

## IEventRaiser

The first layer of abstraction is the Event Raiser. Its responsibility is to request the client appropriately configured to the event being raised from the IConfiguredEventGridClientFactory, and to invoke it with the event.


## IConfiguredEventGridClientFactory

The ConfiguredEventGridClientFactory obtains the configuration for an IEventGridClient using a specified configuration key.

The actual configuration, such as the Topic endpoint URL, is obtained from IOptions&lt;EventGridOptions&gt;. This information is used to create a configured client via the IEventGridClientFactory.

## IEventGridClientFactory

EventGridClientFactory returns a IEventGridClient using the specified topic endpoint and access key information.
In practice, it instantiates a SafeEventGridClient, passing in a configured Azure EventGridPublisherClient.

## SafeEventGridClient: IEventGridClient

A wrapper around a configured Azure EventGridPublisherClient, which publishes events to Event Grid.
The SafeEventGridClient will not throw any exceptions, instead choosing the log them.
Consumers can be unit tested using Mocked versions of IEventGridClient.

