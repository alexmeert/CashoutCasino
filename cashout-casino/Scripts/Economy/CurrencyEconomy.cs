using Godot;
using System;
using CashoutCasino.Characters;

namespace CashoutCasino.Economy
{
    /// <summary>
    /// Centralized currency rules and helper methods. Keep constants here so designers can tune in one place.
    /// Use CurrencyEconomy.ApplyCurrencyGain / ApplyCurrencyCost to modify player currency.
    /// </summary>
    public static class CurrencyEconomy
    {
        public enum ElimType { Body, Head, Grenade, Bounty }
        public enum CostType { Reroll, Heal, Shoot, Grenade, Other }

        public const int INITIAL_SPAWN = 100;
        public const int BODY_ELIM = 10;
        public const int HEAD_ELIM = 20;
        public const int GRENADE_ELIM = 25;
        public const int REROLL_COST = 15;
        public const int HEAL_COST = 20;
        public const int SHOOT_COST = 1;
        public const int DEATH_PENALTY = 30;
        public const int BOUNTY_ELIM = 40;

        public static void ApplyCurrencyGain(Character player, ElimType type)
        {
            switch (type)
            {
                case ElimType.Body: player.ModifyCurrency(BODY_ELIM); break;
                case ElimType.Head: player.ModifyCurrency(HEAD_ELIM); break;
                case ElimType.Grenade: player.ModifyCurrency(GRENADE_ELIM); break;
                case ElimType.Bounty: player.ModifyCurrency(BOUNTY_ELIM); break;
            }
        }

        public static void ApplyCurrencyCost(Character player, CostType costType)
        {
            switch (costType)
            {
                case CostType.Reroll: player.ModifyCurrency(-REROLL_COST); break;
                case CostType.Heal: player.ModifyCurrency(-HEAL_COST); break;
                case CostType.Shoot: player.ModifyCurrency(-SHOOT_COST); break;
                case CostType.Grenade: player.ModifyCurrency(-REROLL_COST); break;
                default: break;
            }
        }

        public static bool CanAffordAction(Character player, CostType costType)
        {
            int cost = 0;
            switch (costType)
            {
                case CostType.Reroll: cost = REROLL_COST; break;
                case CostType.Heal: cost = HEAL_COST; break;
                case CostType.Shoot: cost = SHOOT_COST; break;
                case CostType.Grenade: cost = REROLL_COST; break;
            }
            return player.GetCurrency() >= cost;
        }
    }
}
