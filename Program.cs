// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.


using System;
using System.Linq;
using System.Text;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ServiceBus.Fluent;

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
        public static void RunSample(IAzure azure)
        {
            // New resources
            var rgName = SdkContext.RandomResourceName("rgSB03_", 24);
            var namespaceName = SdkContext.RandomResourceName("namespace", 20);
            var queueName = SdkContext.RandomResourceName("queue1_", 24);
            var topicName = SdkContext.RandomResourceName("topic_", 24);
            var subscription1Name = SdkContext.RandomResourceName("sub1_", 24);
            var subscription2Name = SdkContext.RandomResourceName("sub2_", 24);

            try
            {
                //============================================================
                // Create a namespace.

                Console.WriteLine("Creating name space " + namespaceName + " along with a queue " + queueName + " and a topic " + topicName + " in resource group " + rgName + "...");

                var serviceBusNamespace = azure.ServiceBusNamespaces
                        .Define(namespaceName)
                        .WithRegion(Region.USWest)
                        .WithNewResourceGroup(rgName)
                        .WithSku(NamespaceSku.Standard)
                        .WithNewQueue(queueName, 1024)
                        .WithNewTopic(topicName, 1024)
                        .Create();

                Console.WriteLine("Created service bus " + serviceBusNamespace.Name + " (with queue and topic)");

                PrintServiceBusNamespace(serviceBusNamespace);

                var queue = serviceBusNamespace.Queues.GetByName(queueName);

                PrintServiceBusQueue(queue);

                var topic = serviceBusNamespace.Topics.GetByName(topicName);

                PrintServiceBusTopic(topic);

                //============================================================
                // Create 2 subscriptions in topic using different methods.
                Console.WriteLine("Creating a subscription in the topic using update on topic");
                topic = topic.Update().WithNewSubscription(subscription1Name).Apply();

                var subscription1 = topic.Subscriptions.GetByName(subscription1Name);

                Console.WriteLine("Creating another subscription in the topic using direct create method for subscription");
                var subscription2 = topic.Subscriptions.Define(subscription2Name).Create();

                PrintSubscriptionsInTopic(subscription1);

                PrintSubscriptionsInTopic(subscription2);

                //=============================================================
                // Create new authorization rule for queue to send message.
                Console.WriteLine("Create authorization rule for queue ...");
                var sendQueueAuthorizationRule = serviceBusNamespace.AuthorizationRules.Define("SendRule").WithSendingEnabled().Create();
                StringBuilder builderSendQueueAuthorizationRule = new StringBuilder()
                    .Append("Service bus queue authorization rule: ").Append(sendQueueAuthorizationRule.Id)
                    .Append("\n\tName: ").Append(sendQueueAuthorizationRule.Name)
                    .Append("\n\tResourceGroupName: ").Append(sendQueueAuthorizationRule.ResourceGroupName)
                    .Append("\n\tNamespace Name: ").Append(sendQueueAuthorizationRule.NamespaceName);

                var rights = sendQueueAuthorizationRule.Rights;
                builderSendQueueAuthorizationRule.Append("\n\tNumber of access rights in queue: ").Append(rights.Count());
                foreach (var right in rights)
                {
                    builderSendQueueAuthorizationRule.Append("\n\t\tAccessRight: ")
                            .Append("\n\t\t\tName :").Append(right.ToString());
                }

                Console.WriteLine(builderSendQueueAuthorizationRule.ToString());

                Console.WriteLine("Getting keys for authorization rule ...");
                var keys = sendQueueAuthorizationRule.GetKeys();
                StringBuilder builderKeys = new StringBuilder()
                    .Append("Authorization keys: ")
                    .Append("\n\tPrimaryKey: ").Append(keys.PrimaryKey)
                    .Append("\n\tPrimaryConnectionString: ").Append(keys.PrimaryConnectionString)
                    .Append("\n\tSecondaryKey: ").Append(keys.SecondaryKey)
                    .Append("\n\tSecondaryConnectionString: ").Append(keys.SecondaryConnectionString);

                Console.WriteLine(builderKeys.ToString());

                //=============================================================
                // Delete a namespace
                Console.WriteLine("Deleting namespace " + namespaceName + " [topic, queues and subscription will delete along with that]...");
                // This will delete the namespace and queue within it.
                try
                {
                    azure.ServiceBusNamespaces.DeleteById(serviceBusNamespace.Id);
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
                    Console.WriteLine("Deleting Resource Group: " + rgName);
                    azure.ResourceGroups.BeginDeleteByName(rgName);
                    Console.WriteLine("Deleted Resource Group: " + rgName);
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine("Did not create any resources in Azure. No clean up is necessary");
                }
                catch (Exception g)
                {
                    Console.WriteLine(g);
                }
            }
        }
        public static void Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate
                var credentials = SdkContext.AzureCredentialsFactory.FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));

                var azure = Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

                // Print selected subscription
                Console.WriteLine("Selected subscription: " + azure.SubscriptionId);

                RunSample(azure);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void PrintServiceBusNamespace(IServiceBusNamespace serviceBusNamespace)
        {
            var builderServiceBusNamespace = new StringBuilder()
                    .Append("Service bus Namespace: ").Append(serviceBusNamespace.Id)
                    .Append("\n\tName: ").Append(serviceBusNamespace.Name)
                    .Append("\n\tRegion: ").Append(serviceBusNamespace.RegionName)
                    .Append("\n\tResourceGroupName: ").Append(serviceBusNamespace.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(serviceBusNamespace.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(serviceBusNamespace.UpdatedAt)
                    .Append("\n\tDnsLabel: ").Append(serviceBusNamespace.DnsLabel)
                    .Append("\n\tFQDN: ").Append(serviceBusNamespace.Fqdn)
                    .Append("\n\tSku: ")
                    .Append("\n\t\tCapacity: ").Append(serviceBusNamespace.Sku.Capacity)
                    .Append("\n\t\tSkuName: ").Append(serviceBusNamespace.Sku.Name)
                    .Append("\n\t\tTier: ").Append(serviceBusNamespace.Sku.Tier);

            Console.WriteLine(builderServiceBusNamespace.ToString());
        }

        static void PrintServiceBusQueue(IQueue queue)
        {
            StringBuilder builderQueueName = new StringBuilder()
                    .Append("Service bus Queue: ").Append(queue.Id)
                    .Append("\n\tName: ").Append(queue.Name)
                    .Append("\n\tResourceGroupName: ").Append(queue.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(queue.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(queue.UpdatedAt)
                    .Append("\n\tAccessedAt: ").Append(queue.AccessedAt)
                    .Append("\n\tActiveMessageCount: ").Append(queue.ActiveMessageCount)
                    .Append("\n\tCurrentSizeInBytes: ").Append(queue.CurrentSizeInBytes)
                    .Append("\n\tDeadLetterMessageCount: ").Append(queue.DeadLetterMessageCount)
                    .Append("\n\tDefaultMessageTtlDuration: ").Append(queue.DefaultMessageTtlDuration)
                    .Append("\n\tDuplicateMessageDetectionHistoryDuration: ").Append(queue.DuplicateMessageDetectionHistoryDuration)
                    .Append("\n\tIsBatchedOperationsEnabled: ").Append(queue.IsBatchedOperationsEnabled)
                    .Append("\n\tIsDeadLetteringEnabledForExpiredMessages: ").Append(queue.IsDeadLetteringEnabledForExpiredMessages)
                    .Append("\n\tIsDuplicateDetectionEnabled: ").Append(queue.IsDuplicateDetectionEnabled)
                    .Append("\n\tIsExpressEnabled: ").Append(queue.IsExpressEnabled)
                    .Append("\n\tIsPartitioningEnabled: ").Append(queue.IsPartitioningEnabled)
                    .Append("\n\tIsSessionEnabled: ").Append(queue.IsSessionEnabled)
                    .Append("\n\tDeleteOnIdleDurationInMinutes: ").Append(queue.DeleteOnIdleDurationInMinutes)
                    .Append("\n\tMaxDeliveryCountBeforeDeadLetteringMessage: ").Append(queue.MaxDeliveryCountBeforeDeadLetteringMessage)
                    .Append("\n\tMaxSizeInMB: ").Append(queue.MaxSizeInMB)
                    .Append("\n\tMessageCount: ").Append(queue.MessageCount)
                    .Append("\n\tScheduledMessageCount: ").Append(queue.ScheduledMessageCount)
                    .Append("\n\tStatus: ").Append(queue.Status)
                    .Append("\n\tTransferMessageCount: ").Append(queue.TransferMessageCount)
                    .Append("\n\tLockDurationInSeconds: ").Append(queue.LockDurationInSeconds)
                    .Append("\n\tTransferDeadLetterMessageCount: ").Append(queue.TransferDeadLetterMessageCount);

            Console.WriteLine(builderQueueName.ToString());
        }

        static void PrintServiceBusTopic(ITopic topic)
        {
            StringBuilder builderTopicName = new StringBuilder()
                    .Append("Service bus topic: ").Append(topic.Id)
                    .Append("\n\tName: ").Append(topic.Name)
                    .Append("\n\tResourceGroupName: ").Append(topic.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(topic.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(topic.UpdatedAt)
                    .Append("\n\tAccessedAt: ").Append(topic.AccessedAt)
                    .Append("\n\tActiveMessageCount: ").Append(topic.ActiveMessageCount)
                    .Append("\n\tCurrentSizeInBytes: ").Append(topic.CurrentSizeInBytes)
                    .Append("\n\tDeadLetterMessageCount: ").Append(topic.DeadLetterMessageCount)
                    .Append("\n\tDefaultMessageTtlDuration: ").Append(topic.DefaultMessageTtlDuration)
                    .Append("\n\tDuplicateMessageDetectionHistoryDuration: ").Append(topic.DuplicateMessageDetectionHistoryDuration)
                    .Append("\n\tIsBatchedOperationsEnabled: ").Append(topic.IsBatchedOperationsEnabled)
                    .Append("\n\tIsDuplicateDetectionEnabled: ").Append(topic.IsDuplicateDetectionEnabled)
                    .Append("\n\tIsExpressEnabled: ").Append(topic.IsExpressEnabled)
                    .Append("\n\tIsPartitioningEnabled: ").Append(topic.IsPartitioningEnabled)
                    .Append("\n\tDeleteOnIdleDurationInMinutes: ").Append(topic.DeleteOnIdleDurationInMinutes)
                    .Append("\n\tMaxSizeInMB: ").Append(topic.MaxSizeInMB)
                    .Append("\n\tScheduledMessageCount: ").Append(topic.ScheduledMessageCount)
                    .Append("\n\tStatus: ").Append(topic.Status)
                    .Append("\n\tTransferMessageCount: ").Append(topic.TransferMessageCount)
                    .Append("\n\tSubscriptionCount: ").Append(topic.SubscriptionCount)
                    .Append("\n\tTransferDeadLetterMessageCount: ").Append(topic.TransferDeadLetterMessageCount);

            Console.WriteLine(builderTopicName.ToString());
        }

        static void PrintSubscriptionsInTopic(Microsoft.Azure.Management.ServiceBus.Fluent.ISubscription serviceBusSubscription)
        {
            StringBuilder builder = new StringBuilder()
                    .Append("Service bus subscription: ").Append(serviceBusSubscription.Id)
                    .Append("\n\tName: ").Append(serviceBusSubscription.Name)
                    .Append("\n\tResourceGroupName: ").Append(serviceBusSubscription.ResourceGroupName)
                    .Append("\n\tCreatedAt: ").Append(serviceBusSubscription.CreatedAt)
                    .Append("\n\tUpdatedAt: ").Append(serviceBusSubscription.UpdatedAt)
                    .Append("\n\tAccessedAt: ").Append(serviceBusSubscription.AccessedAt)
                    .Append("\n\tActiveMessageCount: ").Append(serviceBusSubscription.ActiveMessageCount)
                    .Append("\n\tDeadLetterMessageCount: ").Append(serviceBusSubscription.DeadLetterMessageCount)
                    .Append("\n\tDefaultMessageTtlDuration: ").Append(serviceBusSubscription.DefaultMessageTtlDuration)
                    .Append("\n\tIsBatchedOperationsEnabled: ").Append(serviceBusSubscription.IsBatchedOperationsEnabled)
                    .Append("\n\tDeleteOnIdleDurationInMinutes: ").Append(serviceBusSubscription.DeleteOnIdleDurationInMinutes)
                    .Append("\n\tScheduledMessageCount: ").Append(serviceBusSubscription.ScheduledMessageCount)
                    .Append("\n\tStatus: ").Append(serviceBusSubscription.Status)
                    .Append("\n\tTransferMessageCount: ").Append(serviceBusSubscription.TransferMessageCount)
                    .Append("\n\tIsDeadLetteringEnabledForExpiredMessages: ").Append(serviceBusSubscription.IsDeadLetteringEnabledForExpiredMessages)
                    .Append("\n\tIsSessionEnabled: ").Append(serviceBusSubscription.IsSessionEnabled)
                    .Append("\n\tLockDurationInSeconds: ").Append(serviceBusSubscription.LockDurationInSeconds)
                    .Append("\n\tMaxDeliveryCountBeforeDeadLetteringMessage: ").Append(serviceBusSubscription.MaxDeliveryCountBeforeDeadLetteringMessage)
                    .Append("\n\tIsDeadLetteringEnabledForFilterEvaluationFailedMessages: ").Append(serviceBusSubscription.IsDeadLetteringEnabledForFilterEvaluationFailedMessages)
                    .Append("\n\tTransferMessageCount: ").Append(serviceBusSubscription.TransferMessageCount)
                    .Append("\n\tTransferDeadLetterMessageCount: ").Append(serviceBusSubscription.TransferDeadLetterMessageCount);

            Console.WriteLine(builder.ToString());
        }
    }

}
