﻿using System;
using Dargon.InjectedModule.Components;
using Dargon.Processes.Injection;
using ItzWarty;
using ItzWarty.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NMockito;

namespace Dargon.InjectedModule
{
   [TestClass]
   public class InjectedModuleServiceImplTest : NMockitoInstance
   {
      private InjectedModuleServiceImpl testObj;

      [Mock] private readonly IServiceLocator serviceLocator = null;
      [Mock] private readonly ProcessInjectionService processInjectionService = null;
      [Mock] private readonly ISessionFactory sessionFactory = null;
      [Mock] private readonly IInjectedModuleServiceConfiguration injectedModuleServiceConfiguration = null;

      [TestInitialize]
      public void Setup()
      {
         InitializeMocks();

         testObj = new InjectedModuleServiceImpl(serviceLocator, processInjectionService, sessionFactory, injectedModuleServiceConfiguration);
      }

      [TestMethod]
      public void InitializeRegistersToServiceLocatorTest()
      {
         testObj.Initialize();

         Verify(serviceLocator).RegisterService(typeof(InjectedModuleService), testObj);
      }

      [TestMethod]
      public void InjectToProcessCreatesSessionAndDelegatesToProcess()
      {
         const int processId = 193;
         const string dllPath = "H:/erp/Derp";
         var configuration = CreateMock<IInjectedModuleConfiguration>();
         var session = CreateMock<ISession>();

         When(sessionFactory.CreateSession(processId, configuration)).ThenReturn(session);
         When(injectedModuleServiceConfiguration.GetInjectedDllPath()).ThenReturn(dllPath);
         When(processInjectionService.InjectToProcess(Eq(processId), Any<string>())).ThenReturn(true);

         testObj.InjectToProcess(processId, configuration);

         Verify(sessionFactory).CreateSession(processId, configuration);
         Verify(injectedModuleServiceConfiguration).GetInjectedDllPath();
         Verify(processInjectionService).InjectToProcess(processId, dllPath);
         ClearInteractions(session, 1); // Event subscription
         VerifyNoMoreInteractions();
      }

      [TestMethod]
      public void HandleSessionEndedUnsubscribesFromSession()
      {
         var session = CreateMock<ISession>();

         testObj.HandleSessionEnded(session, new SessionEndedEventArgs());

         Verify(session).ProcessId.Wrap(); // For removal from internal list
         ClearInteractions(session, 1); // Event unsubscription
         VerifyNoMoreInteractions();
      }
   }
}