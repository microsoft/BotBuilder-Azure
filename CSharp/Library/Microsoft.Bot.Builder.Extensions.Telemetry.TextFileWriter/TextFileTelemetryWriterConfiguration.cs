using System;
using System.IO;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.TextFileWriter
{
    public class TextFileTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        private readonly IShardStrategy _fileShardStrategy;

        public TextFileTelemetryWriterConfiguration(IShardStrategy fileShardStrategy = null)
        {
            if (null == fileShardStrategy)
            {
                fileShardStrategy = new ShardPerDayStrategy();
            }

            SetField.NotNull(out _fileShardStrategy, nameof(fileShardStrategy), fileShardStrategy);
            Filename = BuildDefaultFilename();
        }

        private string BuildDefaultFilename()
        {
            var executingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (string.IsNullOrEmpty(executingDirectory))
            {
                throw new DirectoryNotFoundException("Unable to determine full path to executing assembly as default base path for log file.");
            }

            var fullyQualifiedFilePath = Path.Combine(executingDirectory.Replace("file:\\", string.Empty), $"{_fileShardStrategy.CurrentShardKey}.log");

            return fullyQualifiedFilePath;
        }

        public bool OverwriteFileIfExists { get; set; }
        public string Filename { get; set; }

        public void ValidateSettings()
        {
            if (string.IsNullOrEmpty(Filename))
            {
                throw new ArgumentException(nameof(Filename));
            }
        }
    }
}