using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectedThings
{
    public static class InjectedEvents {
       public static EventData<int> OnPlanetUnlock = new EventData<int>("OnPlanetUnlock");
    }
}
