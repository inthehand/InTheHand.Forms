// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Stretch.cs" company="In The Hand Ltd">
//   Copyright (c) 2017 In The Hand Ltd, All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace InTheHand.Forms
{
    public enum Stretch
    {
        /// <summary>
        /// The content preserves its original size.
        /// </summary>
        None = 0,

        /// <summary>
        /// The content is resized to fill the destination dimensions. 
        /// The aspect ratio is not preserved.
        /// </summary>
        Fill = 1,

        /// <summary>
        /// The content is resized to fit in the destination dimensions while it preserves its native aspect ratio.
        /// </summary>
        Uniform = 2,

        /// <summary>
        /// The content is resized to fill the destination dimensions while it preserves its native aspect ratio. 
        /// If the aspect ratio of the destination rectangle differs from the source, the source content is clipped to fit in the destination dimensions.
        /// </summary>
        UniformToFill = 3,
    }
}
