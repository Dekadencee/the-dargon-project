#pragma once

#include "stdafx.h"
#include "dargon.hpp"
#include "../DSPEx.hpp"
#include "../IDSPExSession.hpp"
#include "../DSPExRITransactionHandler.hpp"

namespace dargon { namespace IO { namespace DSP { namespace ClientImpl {
   class DSPExRITEchoHandler : public DSPExRITransactionHandler
   {
   public:
      DSPExRITEchoHandler(UINT32 transactionId);
      void ProcessInitialMessage(IDSPExSession& session, DSPExInitialMessage& message);
      void ProcessMessage(IDSPExSession& session, DSPExMessage& message);
   };
} } } }