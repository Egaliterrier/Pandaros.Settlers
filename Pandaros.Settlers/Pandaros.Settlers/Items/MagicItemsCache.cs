﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.Settlers.Items
{
    public static class MagicItemsCache
    {
        public static Dictionary<string, IPlayerMagicItem> PlayerMagicItems { get; set; } = new Dictionary<string, IPlayerMagicItem>();

    }
}
