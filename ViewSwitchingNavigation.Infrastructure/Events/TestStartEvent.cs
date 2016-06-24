using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewSwitchingNavigation.Infrastructure.Events
{
    public class TestStartEvent : PubSubEvent<TestInfo>
    {
    }
    public class TestRestartTestEvent : PubSubEvent<PropertyProvider.WindowsMethod>
    {
    }
}
