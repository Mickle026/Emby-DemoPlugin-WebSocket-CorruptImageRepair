using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Emby.CorruptImageRepair
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        public Plugin(IApplicationPaths appPaths, IXmlSerializer xml)
             : base(appPaths, xml) { }

        public override string Name => "Corrupt Image Repair";

        public override Guid Id => Guid.Parse("f2a4a1d3-3c4b-4c12-8f95-67b9f522caaa");

        public override string Description => "Detects and repairs corrupt images in the library";

        public PluginPageInfo[] GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "corruptionrepair",
                    EmbeddedResourcePath = GetType().Namespace + ".Dashboard.corruptionrepair.html"
                },
                new PluginPageInfo
                {
                    Name = "corruptionrepairjs",
                    EmbeddedResourcePath = GetType().Namespace + ".Dashboard.corruptionrepair.js"
                }
            };
        }

        IEnumerable<PluginPageInfo> IHasWebPages.GetPages()
        {
            return GetPages();
        }
        public Stream GetThumbImage()
        {
            var type = this.GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".icon.png");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

    }

    public class PluginConfiguration : BasePluginConfiguration
    {
    }
}
