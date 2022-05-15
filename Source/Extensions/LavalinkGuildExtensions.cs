namespace DiscordBotRewrite.Extensions;

public static class LavalinkGuildExtensions {
    public static int AmountOfMembersInChannel(this LavalinkGuildConnection conn) {
        List<DiscordMember> members = conn.Channel.Users.ToList();
        int totalMembers = 0;
        for(int i = 0; i < members.Count; i++) {
            if(!members[i].IsBot)
                totalMembers++;
        }
        return totalMembers;
    }
}