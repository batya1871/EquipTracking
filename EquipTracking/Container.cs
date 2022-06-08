using System;
using System.Collections.Generic;
using System.Text;

namespace EquipTracking
{
    public static class Container
    {
        
        public static LinkedList<AgrMachinery> EventsList;
        public static bool EndFlag { get; set; }
        public static bool NewNotes { get; set; } = false;
        static Container()
        {
            EventsList = new LinkedList<AgrMachinery>();

            for (int i = 0; i < 4; i++)
            {

                EndFlag = false;
            }
        }
    }
}
