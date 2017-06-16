using System;
using PostSharp.CodeModel;

namespace PostSharp.CodeWeaver
{
    /// <summary>
    /// An advice is a class that is able to produce some MSIL code.
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="RequiresWeave"/> method determine whether the <see cref="Weave"/>
    /// method should be called at a given join point.
    /// </para>
    /// </remarks>
    public interface IAdvice
    {
        /// <summary>
        /// Gets the advice priority.
        /// </summary>
        /// <remarks>
        /// This influences the order in which advices are applied to join points, in case
        /// that many advices are applied to the same join point. In join point of type
        /// <i>before</i> or <i>instead of</i>, advices are injected with <i>direct</i> order of priority.
        /// In join point of type <i>after</i>, advices are injected with <i>inverse</i> order of priority.
        /// </remarks>
        int Priority { get; }

        /// <summary>
        /// Determines whether the current advice requires to be woven on a given join point.
        /// </summary>
        /// <param name="context">Weaving context.</param>
        /// <returns><b>true</b> if the <see cref="Weave"/> method should be called for
        /// this join point, otherwise <b>false</b>.</returns>
        /// <remarks>
        /// It is theoretically possible for this method to return always <b>false</b>,
        /// because the <see cref="Weave"/> method is not obliged to emit any code. However,
        /// restructuring the method body to make weaving possible is expensive, so the
        /// <see cref="RequiresWeave"/> method should be designed in order to minimize
        /// the number of useless calls to the <see cref="Weave"/> method.
        /// </remarks>
        bool RequiresWeave( WeavingContext context );

        /// <summary>
        /// Weave the current advice into a given join point, i.e. inject code.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="block">Block where the code has to be injected.</param>
        void Weave( WeavingContext context, InstructionBlock block );
    }

    /// <summary>
    /// Exposes a method that allows advices of join points 
    /// <see cref="JoinPointKinds.AfterMethodBodyException"/>
    /// to specify which exceptions should be caught.
    /// </summary>
    /// <remarks>
    /// If an advice is applied on a join point of type <see cref="JoinPointKinds.AfterMethodBodyException"/>
    /// but does not implement this interface, all exceptions
    /// will be caught.
    /// </remarks>
    public interface ITypedExceptionAdvice : IAdvice
    {
        /// <summary>
        /// Gets the type of exceptions that should be caughts for
        /// advices applied on join points of kind <see cref="JoinPointKinds.AfterMethodBodyException"/>.
        /// </summary>
        /// <param name="context">The weaving context.</param>
        /// <returns>A <see cref="Type"/> (derived from <see cref="Exception"/>),
        /// or <b>null</b> if any <see cref="Exception"/> is to be caught.</returns>
        ITypeSignature GetExceptionType( WeavingContext context );
    }

    /// <summary>
    /// Interface optionally implemented by advices on join points <see cref="JoinPointKinds.BeforeStaticConstructor"/>,
    /// allowing to specify if the <b>BeforeFieldInit</b> class-level specifier is supported by this advice. An
    /// advice should return <b>true</b> if it only initializes static fields. The default, if the interface is
    /// not specified, is that this specifier is not specified.
    /// </summary>
    public interface IBeforeStaticConstructorAdvice : IAdvice
    {
        /// <summary>
        /// Determines whether the advice is compatible with the <b>BeforeFieldInit</b>
        /// class-level specifier.
        /// </summary>
        bool IsBeforeFieldInitSupported { get; }
    }

    internal struct AdviceJoinPointKindsPair
    {
        private readonly IAdvice advice;
        private readonly JoinPointKinds kinds;

        public AdviceJoinPointKindsPair( IAdvice advice, JoinPointKinds kinds )
        {
            this.advice = advice;
            this.kinds = kinds;
        }

        public IAdvice Advice
        {
            get { return advice; }
        }

        public JoinPointKinds Kinds
        {
            get { return kinds; }
        }
    }
}