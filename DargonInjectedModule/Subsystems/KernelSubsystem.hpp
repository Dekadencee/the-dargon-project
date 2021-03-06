#pragma once

#include "../stdafx.h"
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "KernelSubsystemTypedefs.hpp"

namespace dargon { namespace Subsystems {
   class KernelSubsystem : public dargon::Subsystem 
   {
      // - instance -------------------------------------------------------------------------------
   public:
      KernelSubsystem();
      bool Initialize() override;
      bool Uninitialize() override;
      
      // - static ---------------------------------------------------------------------------------
      DIM_DECL_STATIC_DETOUR(KernelSubsystem, CreateProcessA, FunctionCreateProcessA, "CreateProcessA", MyCreateProcessA);

   private:
      static BOOL WINAPI MyCreateProcessA(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                          LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags,
                                          LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo,
                                          LPPROCESS_INFORMATION lpProcessInformation);

      static bool ShouldSuspendProcess(const char* path);
   };
} }