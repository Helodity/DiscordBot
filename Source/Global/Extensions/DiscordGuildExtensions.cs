﻿using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace DiscordBotRewrite.Global.Extensions
{
    public static class DiscordGuildExtensions
    {
        /// <summary>
        /// Guild.GetMemberAsync() with an error catch
        /// </summary>
        public static async Task<DiscordMember> TryGetMember(this DiscordGuild guild, ulong userId)
        {
            try
            {
                DiscordMember member = await guild.GetMemberAsync(userId);
                return member;
            }
            catch (NotFoundException)
            {
                return null;
            }
        }
        /// <summary>
        /// Returns a list of members.
        /// </summary>
        public static async Task<List<DiscordMember>> GetMembersAsync(this DiscordGuild guild, bool includeBots = false)
        {
            IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
            //Convert members to a list that can be written to.
            List<DiscordMember> filteredMembers = new List<DiscordMember>();
            foreach (DiscordMember member in members)
            {
                if (!member.IsBot || includeBots)
                {
                    filteredMembers.Add(member);
                }
            }
            return filteredMembers;
        }
        public static async Task<List<ulong>> GetMembersIdAsync(this DiscordGuild guild, bool includeBots = false)
        {
            IReadOnlyCollection<DiscordMember> members = await guild.GetAllMembersAsync();
            //Convert members to a list that can be written to.
            List<ulong> filteredMembers = new List<ulong>();
            foreach (DiscordMember member in members)
            {
                if (!member.IsBot || includeBots)
                {
                    filteredMembers.Add(member.Id);
                }
            }
            return filteredMembers;
        }

        public static List<DiscordRole> GetAllRoles(this DiscordGuild guild)
        {
            IReadOnlyDictionary<ulong, DiscordRole> roles = guild.Roles;
            List<DiscordRole> roleList = new List<DiscordRole>();
            foreach (DiscordRole role in roles.Values)
            {
                roleList.Add(role);
            }
            return roleList;
        }
    }
}