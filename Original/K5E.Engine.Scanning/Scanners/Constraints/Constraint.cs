namespace K5E.Engine.Scanning.Scanners.Constraints
{
    using K5E.Engine.Common;
    using System;

    public abstract class Constraint
    {
        public Constraint()
        {
        }

        /// <summary>
        /// Sets the element type to which all constraints apply.
        /// </summary>
        /// <param name="elementType">The new element type.</param>
        public abstract void SetElementType(ScannableType elementType);

        public abstract Boolean IsValid();

        /// <summary>
        /// Clones this scan constraint.
        /// </summary>
        /// <returns>The cloned scan constraint.</returns>
        public abstract Constraint Clone();
    }
    //// End class
}
//// End namespace