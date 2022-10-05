using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routing
{
    public class TopicOptions
    {
        /// <summary>
        /// Gets or Sets the topic name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets the name of the pubsub component to use.
        /// </summary>
        public string PubsubName { get; set; }

        /// <summary>
        /// Gets or Sets a value which indicates whether to enable or disable processing raw messages.
        /// </summary>
        public bool EnableRawPayload { get; set; }

        /// <summary>
        /// Gets or Sets the CEL expression to use to match events for this handler.
        /// </summary>
        public string Match { get; set; }

        /// <summary>
        /// Gets or Sets the priority in which this rule should be evaluated (lower to higher).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or Sets the <see cref="IOriginalTopicMetadata.Id"/> owned by topic.
        /// </summary>
        public string[] OwnedMetadatas { get; set; }

        /// <summary>
        /// Get or Sets the separator to use for metadata.
        /// </summary>
        public string MetadataSeparator { get; set; }

        /// <summary>
        /// Gets or Sets the dead letter topic.
        /// </summary>
        public string DeadLetterTopic { get; set; }

        /// <summary>
        /// Gets or Sets the original topic metadata.
        /// </summary>
        public IDictionary<string, string> Metadata;
    }
}
