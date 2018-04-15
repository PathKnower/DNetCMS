using DNetCMS.Models.DataContract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNetCMS.Interfaces
{
    public interface ICacheStore
    {
        IList<BaseViewOverride> ViewOverrides { get; }
    }
}
