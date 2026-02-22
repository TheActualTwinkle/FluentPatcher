namespace FluentPatcher.Context
{
    /// <summary>
    /// Result of applying a patch, containing the updated entity and change context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity that was patched.</typeparam>
    /// <typeparam name="TContext">The type of patch context with change details.</typeparam>
    public sealed class PatchResult<TEntity, TContext> 
        where TEntity : class
        where TContext : IPatchContext
    {
        /// <summary>
        /// The entity after applying the patch.
        /// </summary>
        public TEntity Entity { get; }
        
        /// <summary>
        /// The context containing details about what changed.
        /// </summary>
        public TContext Context { get; }
        
        /// <summary>
        /// Shortcut to check if any changes were made.
        /// </summary>
        public bool HasChanges => Context.HasChanges();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PatchResult{TEntity,TContext}"/> class with the patched entity and context.
        /// </summary>
        /// <param name="entity">Entity after patching.</param>
        /// <param name="context">Context with change details.</param>
        public PatchResult(TEntity entity, TContext context)
        {
            Entity = entity;
            Context = context;
        }
    }
}

