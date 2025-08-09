namespace XrmTools.Meta.Attributes
{
    using System;

    /// <summary>
    /// Adds image to a plugin step of a plugin type. This attribute should only be applied after a <see cref="StepAttribute" /> to the class.
    /// <summary>
    /// Defines an image for a plugin step, specifying the image type, attributes, and other image configuration.
    /// This attribute should only be applied after a <see cref="StepAttribute" /> to the class.
    /// Use this to add pre-image, post-image, or both to a plugin step for capturing entity data before or after the operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ImageAttribute : Attribute
    {
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Type of image requested.
        /// </summary>
        public ImageTypes ImageType { get; set; }
        /// <summary>
        /// Name of the property on the Request message. E.g. "Target".
        /// </summary>
        public string MessagePropertyName { get; set; } = string.Empty;

        /// <summary>
        /// Name of SdkMessage processing step image.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Key name used to access the pre-image or post-image property bags in a step
        /// </summary>
        public string EntityAlias { get; set; } = string.Empty;
        public string Attributes { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ImageAttribute(ImageTypes imageType)
        {
            ImageType = imageType;
            Name = imageType.ToString();
            EntityAlias = imageType.ToString();
        }
        public ImageAttribute(ImageTypes imageType, string attributes) : this(imageType)
        {
            Attributes = attributes;
        }
    }
}