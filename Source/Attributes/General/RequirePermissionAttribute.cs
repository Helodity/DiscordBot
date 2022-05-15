namespace DiscordBotRewrite.Attributes {
    public class RequirePermissionsAttribute : SlashCheckBaseAttribute {

        public Permissions Permissions { get; }

        public RequirePermissionsAttribute(Permissions permissions) {
            Permissions = permissions;
        }

        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) {

            if(!ctx.Member.Permissions.HasPermission(Permissions)) {
                await ctx.CreateResponseAsync("You don't have the required permissions to use this command!", true);
                return false;
            }

            return true;
        }
    }
}
