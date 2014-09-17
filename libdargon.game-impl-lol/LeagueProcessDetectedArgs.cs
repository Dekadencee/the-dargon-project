﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dargon.Processes.Watching;

namespace Dargon.Game.LeagueOfLegends
{
   public delegate void LeagueProcessDetectedHandler(LeagueProcessDetectedArgs args);
   public class LeagueProcessDetectedArgs
   {
      public LeagueProcessType ProcessType { get; private set; }
      public CreatedProcessDescriptor ProcessDescriptor { get; private set; }

      public LeagueProcessDetectedArgs(
         LeagueProcessType leagueProcessType,
         CreatedProcessDescriptor createdProcessDescriptor)
      {
         ProcessType = leagueProcessType;
         ProcessDescriptor = createdProcessDescriptor;
      }
   }
}
