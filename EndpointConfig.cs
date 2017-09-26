using System;
using System.IO;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence;
using NServiceBus.Utils;
using Raven.Client.Document;
using Raven.Client.Document.DTC;

namespace Case00030984
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            var endpointName = "Case00030984"; //Hard coded

            var resourceManagerId = DeterministicGuid.Create(endpointName + "-NServiceBus@" + Environment.MachineName); //replaced with copy of version used by NServiceBus

            var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            var txRecoveryPath = Path.Combine(programDataPath, "NServiceBus.RavenDB", resourceManagerId.ToString());

            var store = new DocumentStore()
            {
                ConnectionStringName = "ravendb", //hard coded
                DefaultDatabase = endpointName,
                ResourceManagerId = resourceManagerId,
                TransactionRecoveryStorage = new LocalDirectoryTransactionRecoveryStorage(txRecoveryPath)
            };
            store.Initialize();

            var logger = LogManager.GetLogger(endpointName);
            logger.InfoFormat("Recovery path for NServiceBBus raven store: '{0}' set to: '{1}'", store.ResourceManagerId, txRecoveryPath);

            configuration.EndpointName(endpointName);
            configuration.UseTransport<MsmqTransport>();
            configuration.UseSerialization<XmlSerializer>();

            var persistence = configuration.UsePersistence<RavenDBPersistence>().DoNotSetupDatabasePermissions();
            persistence.SetDefaultDocumentStore(store);
        }
    }
}
