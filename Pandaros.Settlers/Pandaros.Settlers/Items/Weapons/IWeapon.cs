﻿namespace Pandaros.Settlers.Items.Weapons
{
    public interface IWeapon : IMagicEffect
    {
        int WepDurability { get; set; }
    }
}