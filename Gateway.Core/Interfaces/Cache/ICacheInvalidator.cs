using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.Core.Interfaces.Cache
{
    public interface ICacheInvalidator
    {
        Task PublishInvalidationAsync(string key);
    }
}
