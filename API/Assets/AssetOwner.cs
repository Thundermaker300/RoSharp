using RoSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API.Assets
{
    public class AssetOwner
    {
        public AssetOwnerType OwnerType { get; }
        public Group? GroupOwner { get; }
        public User? UserOwner { get; }

        internal AssetOwner(AssetOwnerType ownerType, Group? groupOwner, User? userOwner)
        {
            OwnerType = ownerType;
            GroupOwner = groupOwner;
            UserOwner = userOwner;
        }
    }
}
