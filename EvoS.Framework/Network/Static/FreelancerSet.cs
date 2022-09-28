using System;
using System.Collections.Generic;

namespace EvoS.Framework.Network.Static
{
    [Serializable]
    [EvosMessage(138)]
    public class FreelancerSet
    {
        [EvosMessage(139)]
        public List<CharacterType> Types;
        [EvosMessage(141)]
        public List<CharacterRole> Roles;
        [EvosMessage(10)]
        public List<int> FactionGroups;

        public static FreelancerSet None = new FreelancerSet() { Types = new List<CharacterType> { CharacterType.None } };
        public static FreelancerSet AllRoles = new FreelancerSet() { Roles = new List<CharacterRole> { CharacterRole.Assassin, CharacterRole.Tank, CharacterRole.Support} };

        public bool Matches(CharacterType characterType)
        {
            foreach (CharacterType type in Types)
            {
                if (type == characterType) return true;
            }

            foreach (CharacterRole role in Roles)
            {
                if (role == GetRole(characterType)) return true;
            }

            return false;
        }

        public CharacterRole GetRole(CharacterType characterType)
        {
            switch (characterType)
            {
                case CharacterType.Soldier:
                case CharacterType.Thief:
                case CharacterType.Blaster:
                case CharacterType.Gremlins:
                case CharacterType.Tracker:
                case CharacterType.Exo:
                case CharacterType.TeleportingNinja:
                case CharacterType.Fireborg:
                case CharacterType.Scoundrel:
                case CharacterType.Neko:
                case CharacterType.Sniper:
                case CharacterType.Trickster:
                case CharacterType.RobotAnimal:
                case CharacterType.Samurai:
                case CharacterType.Iceborg:
                case CharacterType.BazookaGirl:
                    return CharacterRole.Assassin;
                case CharacterType.BattleMonk:
                case CharacterType.Valkyrie:
                case CharacterType.SpaceMarine:
                case CharacterType.Scamp:
                case CharacterType.Dino:
                case CharacterType.Manta:
                case CharacterType.Rampart:
                case CharacterType.RageBeast:
                case CharacterType.Claymore:
                    return CharacterRole.Tank;
                case CharacterType.DigitalSorceress:
                case CharacterType.FishMan:
                case CharacterType.NanoSmith:
                case CharacterType.Archer:
                case CharacterType.Cleric:
                case CharacterType.Martyr:
                case CharacterType.Spark:
                case CharacterType.Sensei:
                    return CharacterRole.Support;
                default:
                    return CharacterRole.None;
            }
        }
    }
}
