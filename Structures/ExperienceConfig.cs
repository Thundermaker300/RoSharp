using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    public readonly struct ExperienceConfig
    {
        public string Key { get; init; }
        public string Value { get; init; }
        public string Description { get; init; }

        public string DraftHash { get; init; }
    }
}
