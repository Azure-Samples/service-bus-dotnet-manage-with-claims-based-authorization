---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
  services: Service-Bus
  platforms: dotnet
---

# Getting started on managing Service Bus with claims based authorization in C# #

 Azure Service Bus basic scenario sample.
 - Create namespace with a queue and a topic
 - Create 2 subscriptions for topic using different methods.
 - Create send authorization rule for queue.
 - Create send and listener authorization rule for Topic.
 - Get the keys from authorization rule to connect to queue.
 - Delete namespace

 For more on how to use Azure Service Bus see the [samples for sending and receiving messages](https://docs.microsoft.com/samples/azure/azure-sdk-for-net/azuremessagingservicebus-samples/).


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-libraries-for-net/blob/master/AUTH.md).

    git clone https://github.com/Azure-Samples/service-bus-dotnet-manage-with-claims-based-authorization.git

    cd service-bus-dotnet-manage-with-claims-based-authorization

    dotnet build

    bin\Debug\net452\ServiceBusWithClaimBasedAuthorization.exe

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.