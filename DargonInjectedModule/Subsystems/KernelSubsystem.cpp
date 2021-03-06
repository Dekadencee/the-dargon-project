#include "stdafx.h"
#include "util.hpp"
#include "../Subsystem.hpp"
#include "../Subsystem.Detours.hpp"
#include "KernelSubsystem.hpp"
#include "KernelSubsystemTypedefs.hpp"

using namespace dargon::Subsystems;

// - instance -------------------------------------------------------------------------------------
KernelSubsystem::KernelSubsystem() {}

bool KernelSubsystem::Initialize() {
   if (Subsystem::Initialize()) {
      HMODULE hModuleKernel32 = WaitForModuleHandle("Kernel32.dll");
      InstallCreateProcessADetour(hModuleKernel32);
      return true;
   }
}

bool KernelSubsystem::Uninitialize() {
   if (Subsystem::Uninitialize()) {
      UninstallCreateProcessADetour();
      return false;
   } else {
      return true;
   }
}

// - static ---------------------------------------------------------------------------------------
DIM_IMPL_STATIC_DETOUR(KernelSubsystem, CreateProcessA, FunctionCreateProcessA, "CreateProcessA", MyCreateProcessA);

BOOL WINAPI KernelSubsystem::MyCreateProcessA(LPCSTR lpApplicationName, LPSTR lpCommandLine, LPSECURITY_ATTRIBUTES lpProcessAttributes,
                                              LPSECURITY_ATTRIBUTES lpThreadAttributes, BOOL bInheritHandles, DWORD dwCreationFlags,
                                              LPVOID lpEnvironment, LPCSTR lpCurrentDirectory, LPSTARTUPINFOA lpStartupInfo,
                                              LPPROCESS_INFORMATION lpProcessInformation)
{
   s_logger->Log(LL_ALWAYS, [&](std::ostream& os){ 
         os << "Detour CreateProcessA:" 
            << " lpApplicationName: " << lpApplicationName << " lpCommandLine: " << lpCommandLine
            << " lpProcessAttributes: " << lpProcessAttributes << " lpThreadAttributes: " << lpThreadAttributes
            << " bInheritHandles: " << bInheritHandles << " dwCreationFlags: " << dwCreationFlags
            << " lpEnvironment: " << lpEnvironment << " lpCurrentDirectory: " << lpCurrentDirectory
            << " lpStartupInfo: " << lpStartupInfo << " lpProcessInformation: " << lpProcessInformation << std::endl; 
      }
   );

   if (ShouldSuspendProcess(lpApplicationName)) {
      dwCreationFlags |= CREATE_SUSPENDED;
   }

   auto result = m_trampCreateProcessA(
      lpApplicationName, lpCommandLine, lpProcessAttributes,
      lpThreadAttributes, bInheritHandles, dwCreationFlags, 
      lpEnvironment, lpCurrentDirectory, lpStartupInfo, 
      lpProcessInformation
   );
   return result;
}

bool KernelSubsystem::ShouldSuspendProcess(const char* path)
{
   auto processName = GetFileName(std::string(path));
   auto launchSuspendedValue = s_configuration->GetProperty(Configuration::LaunchSuspendedKey);
   if (launchSuspendedValue.size() == 0) {
      return false;
   }

   std::cout << "KernelSubsystem::ShouldSuspendProcess have " << Configuration::LaunchSuspendedKey << " property with value " << launchSuspendedValue << std::endl;

   auto fileNames = split(launchSuspendedValue, ',');
   for (auto fileName : fileNames) {
      if (dargon::iequals(processName, fileName)) {
         return true;
      }
   }
   return false;
}