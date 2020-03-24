using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Chino.Jupiter.Extensions.JupyterNotebook
{
    public partial class Notebook
    {
        public static Notebook FromJson(string json) => JsonConvert.DeserializeObject<Notebook>(json);

        [JsonProperty("cells")]
        public Cell[] Cells { get; set; }

        [JsonProperty("metadata")]
        public NotebookMetadata Metadata { get; set; }

        [JsonProperty("nbformat")]
        public long Nbformat { get; set; }

        [JsonProperty("nbformat_minor")]
        public long NbformatMinor { get; set; }
    }

    public partial class Cell
    {
        [JsonProperty("cell_type")]
        public CellType CellType { get; set; }

        [JsonProperty("source")]
        public string[] Source { get; set; }
    }



    public partial class Data
    {
        [JsonProperty("text/html", NullValueHandling = NullValueHandling.Ignore)]
        public string[] TextHtml { get; set; }

        [JsonProperty("text/plain", NullValueHandling = NullValueHandling.Ignore)]
        public string[] TextPlain { get; set; }

        [JsonProperty("text/markdown", NullValueHandling = NullValueHandling.Ignore)]
        public string[] TextMarkdown { get; set; }
    }

    public partial class NotebookMetadata
    {
        [JsonProperty("kernelspec")]
        public Kernelspec Kernelspec { get; set; }

        [JsonProperty("language_info")]
        public LanguageInfo LanguageInfo { get; set; }
    }

    public partial class Kernelspec
    {
        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class LanguageInfo
    {
        [JsonProperty("file_extension")]
        public string FileExtension { get; set; }

        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pygments_lexer")]
        public string PygmentsLexer { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public enum CellType { Code, Other };

    public enum OutputType { DisplayData, Error };


    internal class CellTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(CellType) || t == typeof(CellType?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "code":
                    return CellType.Code;
                default:
                    return CellType.Other;
            }
            throw new Exception("Cannot unmarshal type CellType");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            
        }

        public static readonly CellTypeConverter Singleton = new CellTypeConverter();
    }
}
