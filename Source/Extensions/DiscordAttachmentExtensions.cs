using DSharpPlus.Entities;
using System.Collections.Generic;

namespace DiscordBotRewrite.Extensions {
    public static class DiscordAttachmentExtensions {

        static List<string> ImageFileExtensions = new List<string> {
        ".jpg",
        ".jpeg",
        ".gif",
        ".png",
        ".webp"
    };

        public static bool IsImage(this DiscordAttachment attachment) {
            string url = attachment.Url.ToLower();
            for(int i = 0; i < ImageFileExtensions.Count; i++) {
                if(url.EndsWith(ImageFileExtensions[i]))
                    return true;
            }
            return false;
        }
    }
}