// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.ServiceBus;
using Azure.ResourceManager.ServiceBus.Models;

namespace ServiceBusWithClaimBasedAuthorization
{
    public class Program
    {
        /**
         * Azure Service Bus basic scenario sample.
         * - Create namespace with a queue and a topic
         * - Create 2 subscriptions for topic using different methods.
         * - Create send authorization rule for queue.
         * - Create send and listener authorization rule for Topic.
         * - Get the keys from authorization rule to connect to queue.
         * - Delete namespace
         */
        private static ResourceIdentifier? _resourceGroupId = null;
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                //============================================================
                
                // Create a namespace.
              
                // Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                // Create a resource group in the USWest region
                var rgName = Utilities.CreateRandomName("rgSB03_");
                Utilities.Log("Creating resource group with name : " + rgName);
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.WestUS));
                var resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log("Created resource group with name: " + resourceGroup.Data.Name + "...");

                //create namespace and wait for completion
                var namespaceName = Utilities.CreateRandomName("namespace");
                var queueName = Utilities.CreateRandomName("queue1_");
                var topicName = Utilities.CreateRandomName("topic_");
                Console.WriteLine("Creating name space " + namespaceName + " along with a queue " + queueName + " and a topic " + topicName + " in resource group " + rgName + "...");
                var namespaceCollection = resourceGroup.GetServiceBusNamespaces();
                var data = new ServiceBusNamespaceData(AzureLocation.WestUS)
                {
                    Sku = new ServiceBusSku(ServiceBusSkuName.Standard),
                    Location = AzureLocation.WestUS,
                };
                var serviceBusNamespace = (await namespaceCollection.CreateOrUpdateAsync(WaitUntil.Completed, namespaceName, data)).Value;

                // Create queue in namespace
                Utilities.Log("Creating queue...");
                var queueCollection = serviceBusNamespace.GetServiceBusQueues();
                var queueData = new ServiceBusQueueData()
                {
                    MaxSizeInMegabytes = 1024,
                };
                var queue = (await queueCollection.CreateOrUpdateAsync(WaitUntil.Completed, queueName, queueData)).Value;
                Utilities.Log("Created queue with name : " + queue.Data.Name);

                // Create topic in namespace
                Utilities.Log("Creating topic " + topicName + " in namespace " + namespaceName + "...");
                var topicCollection = serviceBusNamespace.GetServiceBusTopics();
                var topicData = new ServiceBusTopicData()
                {
                    MaxSizeInMegabytes = 1024,
                };
                var topic = (await topicCollection.CreateOrUpdateAsync(WaitUntil.Completed, topicName, topicData)).Value;
                Utilities.Log("Created topic in namespace with name : " + topic.Data.Name);
                Utilities.Log("Created service bus " + serviceBusNamespace.Data.Name + " (with queue and topic)");

                //============================================================

                // Create 2 subscriptions in topic using different methods.
                var subscriptionName = Utilities.CreateRandomName("subs1_");
                Utilities.Log("Creating a subscription in the topic using update on topic");
                var subscriptionCollection = topic.GetServiceBusSubscriptions();
                var subscriptionData = new ServiceBusSubscriptionData()
                {
                    RequiresSession = true,
                };
                var subscription1 = (await subscriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, subscriptionName, subscriptionData)).Value;
                Utilities.Log("Created a subscription in the topic using update on topic with a name :" + subscription1.Data.Name);
                
                var subscription2Name = Utilities.CreateRandomName("subs2_");
                Utilities.Log("Creating another subscription in the topic using direct create method for subscription");
                var subscription2Data = new ServiceBusSubscriptionData()
                {
                    RequiresSession = true,
                };
                var subscription2 = (await subscriptionCollection.CreateOrUpdateAsync(WaitUntil.Completed, subscription2Name, subscription2Data)).Value;
                Utilities.Log("Creating another subscription in the topic using direct create method for subscription with name :" + subscription2.Data.Name);
                
                //=============================================================

                // Create new authorization rule for queue to send message.
                Utilities.Log("Create authorization rule for queue ...");
                var sendRuleCollection = serviceBusNamespace.GetServiceBusNamespaceAuthorizationRules();
                var sendRuleName = Utilities.CreateRandomName("SendRule");
                Utilities.Log("Creating rule : " + sendRuleName + " in queue " + queue.Data.Name + "...");
                var sendRuleData = new ServiceBusAuthorizationRuleData()
                {
                    Rights =
                    {
                      ServiceBusAccessRight.Send
                    }
                };
                var sendRule = (await sendRuleCollection.CreateOrUpdateAsync(WaitUntil.Completed, sendRuleName, sendRuleData)).Value;
                Utilities.Log("Created sendrule : " + sendRule.Data.Name);
                
                //=============================================================

                // Delete a namespace
                Utilities.Log("Deleting namespace + namespaceName + [topic, queues and subscription will delete along with that]...");
                try
                {
                    _ = await serviceBusNamespace.DeleteAsync(WaitUntil.Completed);
                }
                catch (Exception)
                {
                }
                Console.WriteLine("Deleted namespace " + namespaceName + "...");

            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group: {_resourceGroupId}");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId}");
                    }
                }
                catch (NullReferenceException)
                {
                    Utilities.Log("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Utilities.Log(g);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);
                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}
