#pragma once

#include "stdafx.h"
#include "dargon.hpp"
#include "util.hpp"
#include "../DSPEx.hpp"
#include "../DSPExMessage.hpp"
#include "../DSPExInitialMessage.hpp"
#include "../IDSPExSession.hpp"
#include "DSPExRITEchoHandler.hpp"
using dargon::file_logger;
using namespace dargon::IO::DSP;
using namespace dargon::IO::DSP::ClientImpl;

DSPExRITEchoHandler::DSPExRITEchoHandler(UINT32 transactionId)
   : DSPExRITransactionHandler(transactionId)
{
}
void DSPExRITEchoHandler::ProcessInitialMessage(IDSPExSession& session, DSPExInitialMessage& message)
{
   auto response = DSPExMessage(TransactionId, message.DataBuffer, message.DataLength);
   file_logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Created Echo Response of payload pointer " << (void*)message.DataBuffer << " length " << message.DataLength << std::endl; });
   session.SendMessage(response);
   file_logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Sent Echo Response" << std::endl; });
   session.DeregisterRITransactionHandler(this); // Disposes of this thing as well
   file_logger::SNL(LL_VERBOSE, [=](std::ostream& os){ os << "Deregistered Echo Response" << std::endl; });
}
void DSPExRITEchoHandler::ProcessMessage(IDSPExSession& session, DSPExMessage& message)
{
   // Just eat the packet (This method call doesn't make sense).
}