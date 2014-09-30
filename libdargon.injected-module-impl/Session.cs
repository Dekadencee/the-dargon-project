using System;
using System.Collections.Generic;
using System.IO;
using Dargon.Transport;
using ItzWarty;
using NLog;

namespace Dargon.InjectedModule
{
   public class Session : ISession
   {
      private const string DIM_PIPE_NAME_PREFIX = "DargonInjectedModule_";
      private readonly int processId;
      private readonly InjectedModuleConfiguration configuration;
      private readonly DtpNode node;

      public Session(int processId, InjectedModuleConfiguration configuration, IDtpNodeFactory nodeFactory)
      {
         this.processId = processId;
         this.configuration = configuration;

         var pipeName = DIM_PIPE_NAME_PREFIX + processId;

         this.node = nodeFactory.CreateNode(true, pipeName, new List<IInstructionSet> { new SessionInstructionSet(this) });
      }

      public int ProcessId { get { return processId; } }
      public InjectedModuleConfiguration Configuration { get { return configuration; } }

      public void HandleBootstrapComplete(IDSPExSession session)
      {

      }

      private class SessionInstructionSet : IInstructionSet
      {
         private readonly Session session;

         public SessionInstructionSet(Session session) {
            this.session = session;
         }

         public bool UseConstructionContext { get { return true; } }
         public object ConstructionContext { get { return session; } }
         public bool TryCreateRemotelyInitializedTransactionHandler(byte opcode, uint transactionId, out RemotelyInitializedTransactionHandler handler)
         {
            handler = null;
            switch ((DTP_DIM)opcode) {
               case DTP_DIM.C2S_GET_BOOTSTRAP_ARGS:
                  handler = new RITGetBootstrapArgsHandler(transactionId, session);
                  break;
               case DTP_DIM.C2S_REMOTE_LOG:
                  handler = new RITRemoteLogHandler(transactionId);
                  break;
            }
            return handler != null;
         }
      }

      private class RITGetBootstrapArgsHandler : RemotelyInitializedTransactionHandler
      {
         private static readonly Logger logger = LogManager.GetCurrentClassLogger();

         private readonly Session dimSession;

         public RITGetBootstrapArgsHandler(uint transactionId, Session dimSession)
            : base(transactionId) { this.dimSession = dimSession; }

         public override void ProcessInitialMessage(IDSPExSession session, TransactionInitialMessage message) 
         {
            logger.Info("Processing Initial Message");

            // GetBootstrapArguments initiator used to send its pid, is no longer required in dargon rewrite.
            if (message.DataLength != 4) {
               logger.Warn("Expected " + GetType().Name + " initial message to have at least 4 bytes");
            } else {
               var pid = BitConverter.ToUInt32(message.DataBuffer, message.DataOffset);
               if (pid != dimSession.ProcessId) {
                  logger.Warn("Expected " + dimSession.processId + " but got " + pid);
               }
            }

            // Send response data - properties and flags
            using (var ms = new MemoryStream()) {
               using (var writer = new BinaryWriter(ms)) {
                  var configuration = dimSession.Configuration.GetBootstrapConfiguration();
                  var properties = configuration.Properties;
                  writer.Write((uint)properties.Count);
                  foreach (var property in properties) {
                     writer.WriteLongText(property.Key);
                     writer.WriteLongText(property.Value);
                  }

                  var flags = configuration.Flags;
                  writer.Write((uint)flags.Count);
                  foreach (var flag in flags) {
                     writer.WriteLongText(flag);
                  }
               }
               var data = ms.ToArray();
               session.SendMessage(new TransactionMessage(
                  message.TransactionId,
                  data,
                  0,
                  data.Length
               ));
            }
            session.DeregisterRITransactionHandler(this);
         }

         public override void ProcessMessage(IDSPExSession session, TransactionMessage message) 
         { 
            logger.Warn("Unexpected ProcessMessage invocation.");
         }
      }

      private class RITRemoteLogHandler : RemotelyInitializedTransactionHandler
      {
         private static readonly Logger logger = LogManager.GetCurrentClassLogger();

         public RITRemoteLogHandler(uint transactionId) : base(transactionId) {
         }

         public override void ProcessInitialMessage(IDSPExSession session, TransactionInitialMessage message) {
            using (var ms = new MemoryStream(message.DataBuffer, message.DataOffset, message.DataLength))
            using (var reader = new BinaryReader(ms)) {
               var loggerLevel = reader.ReadUInt32(); // TODO
               var messageLength = reader.ReadUInt32();
               var messageContent = reader.ReadStringOfLength((int)messageLength);
               logger.Info("REMOTE MESSAGE: L" + loggerLevel + ": " + messageContent);
            }

            session.DeregisterRITransactionHandler(this); }

         public override void ProcessMessage(IDSPExSession session, TransactionMessage message) { throw new NotImplementedException(); }
      }
   }
}