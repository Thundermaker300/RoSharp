using RoSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.Structures
{
    public struct AssetReview
    {
        public string ReviewId { get; init; }
        public GenericId<User> Poster { get; init; }
        public string Text { get; init; }
        public bool? IsRecommended { get; init; }
    }
}
