using System.IO;
using Newtonsoft.Json;

namespace DiscordBotRewrite.Global {
    public class CustomJsonTextWriter : JsonTextWriter {
        #region Properties
        public int? MaxIndentDepth { get; set; }
        #endregion

        #region Constructors
        public CustomJsonTextWriter(TextWriter writer) : base(writer) {
            Formatting = Formatting.Indented;
        }
        #endregion

        #region Public
        public override void WriteStartArray() {
            base.WriteStartArray();
            if(MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
                Formatting = Formatting.None;
        }

        public override void WriteStartObject() {
            base.WriteStartObject();
            if(MaxIndentDepth.HasValue && Top > MaxIndentDepth.Value)
                Formatting = Formatting.None;
        }

        public override void WriteEndArray() {
            base.WriteEndArray();
            if(MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
                Formatting = Formatting.Indented;
        }

        public override void WriteEndObject() {
            base.WriteEndObject();
            if(MaxIndentDepth.HasValue && Top <= MaxIndentDepth.Value)
                Formatting = Formatting.Indented;
        }
        #endregion
    }
}