﻿using System;

namespace Dargon.Client {
   public static class Program {
      [STAThread]
      public static void Main(string[] args) {
         new DargonClientApplication().Start(null);
      }
   }
}
