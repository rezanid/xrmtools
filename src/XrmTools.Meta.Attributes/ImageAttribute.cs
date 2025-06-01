namespace XrmTools.Meta.Attributes
{
    using System;

    // Any change in the constructors of this class requires a change in PluginAttributeExtractor.
    /// <summary>
    /// Adds image to a plugin step of a plugin type. This attribute should only be applied after a <see cref="StepAttribute" /> to the class.
    /// </summary>
    /// <param name="type">Defines if this is a pre-image, post-image or both.</param>
    /// <param name="messagePropertyName">Defines which property of the message is captured by this image.</param>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ImageAttribute : Attribute
    {
        public Guid Id { get; set; }
        /// <summary>
        /// Type of image requested.
        /// </summary>
        public ImageTypes ImageType { get; set; }
        /// <summary>
        /// Name of the property on the Request message. E.g. "Target".
        /// </summary>
        public string MessagePropertyName { get; set; }

        /// <summary>
        /// Name of SdkMessage processing step image.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Key name used to access the pre-image or post-image property bags in a step
        /// </summary>
        public string EntityAlias { get; set; }
        public string Attributes { get; set; }
        public string Description { get; set; }

        public ImageAttribute(ImageTypes imageType)
        {
            ImageType = imageType;
            Name = imageType.ToString();
            EntityAlias = imageType.ToString();
        }
        public ImageAttribute(ImageTypes imageType, string attributes) : this(imageType)
            => Attributes = attributes;
    }
}