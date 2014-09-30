﻿using Dargon.Game;
using Dargon.Modifications;
using System.Collections.Generic;

namespace Dargon.ModificationRepositories
{
   public interface ModificationRepositoryService
   {
      void ClearModifications();
      void AddModification(IModification modification);
      void RemoveModification(IModification modification);
      IEnumerable<IModification> EnumerateModifications(GameType gameType = GameType.Any);
   }
}