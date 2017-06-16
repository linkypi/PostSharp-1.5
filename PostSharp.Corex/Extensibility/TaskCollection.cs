using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using PostSharp.Collections;
using PostSharp.Extensibility.Configuration;

namespace PostSharp.Extensibility
{
    /// <summary>
    /// Collection of tasks (<see cref="Task"/>).
    /// </summary>
    [Serializable]
    public sealed class TaskCollection : MarshalByRefObject, ICollection<Task>, IDisposable
    {
        /// <summary>
        /// Underlying implementation of the collection.
        /// </summary>
        [NonSerialized] private readonly TaskCollectionImpl implementation = new TaskCollectionImpl();

        /// <summary>
        /// Context to which tasks of this collection belong.
        /// </summary>
        private readonly Project project;


        /// <summary>
        /// Initializes a new <see cref="TaskCollection"/>.
        /// </summary>
        /// <param name="context">Context to which tasks of this collection belong.</param>
        internal TaskCollection( Project context )
        {
            this.project = context;
        }

        /// <summary>
        /// Adds to the current collection a new task given its name.
        /// </summary>
        /// <param name="taskTypeName">Name of the task type.</param>
        public void Add( string taskTypeName )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( taskTypeName, "taskTypeName" );

            #endregion

            Trace.ProjectLoader.WriteLine( "Adding the task named {0}.", taskTypeName );

            TaskTypeConfiguration taskType;

            if ( !this.project.TaskTypes.TryGetValue( taskTypeName, out taskType ) )
            {
                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0048",
                                                  new object[] {taskTypeName} );
            }

            Task task = taskType.CreateInstance( this.project );
            this.Add( task );
        }

        /// <summary>
        /// Adds a range of tasks, then resolve dependencies and sort.
        /// </summary>
        /// <param name="tasks">A range of tasks.</param>
        public void AddRange( IEnumerable<Task> tasks )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( tasks, "tasks" );

            #endregion

            foreach ( Task task in tasks )
            {
                Trace.ProjectLoader.WriteLine( "Adding task {0} explicitely.", task.TaskName );
                this.InternalAdd( task );
            }

            foreach ( Task task in tasks )
            {
                this.AddRequiredDependencies( task );
            }

            this.AddOptionalDependencies();
            this.Sort();
        }

        /// <summary>
        /// Inspects all tasks in the collection and update the collections of
        /// successors and predecessors based on optional dependencies.
        /// </summary>
        private void AddOptionalDependencies()
        {
            Trace.TaskResolving.WriteLine( "Inspecting optional depedencies." );

            foreach ( Task task in this.implementation )
            {
                Trace.TaskResolving.WriteLine( "Inspecting optional dependencies of the task {0}.", task );

                TaskTypeConfiguration taskType = task.TaskType;

                if ( taskType.Dependencies != null )
                {
                    foreach ( DependencyConfiguration dependency in taskType.Dependencies )
                    {
                        if ( !dependency.Required && !dependency.Processed )
                        {
                            Task dependentTask = this[dependency.TaskType];

                            if ( dependentTask != null )
                            {
                                Trace.TaskResolving.WriteLine( "Adding the dependency {{{0}}}.", dependency );

                                if ( dependency.Position == DependencyPosition.After )
                                {
                                    task.Successors.Add( dependency );
                                    dependentTask.Predecessors.Add( dependency );
                                }
                                else
                                {
                                    dependentTask.Successors.Add( dependency );
                                    task.Predecessors.Add( dependency );
                                }

                                dependency.Processed = true;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively adds all tasks required by a given task.
        /// </summary>
        /// <param name="task">A newly added task.</param>
        private void AddRequiredDependencies( Task task )
        {
            Trace.TaskResolving.WriteLine( "Inspecting the depedencies required for the task {0}.", task );

            TaskTypeConfiguration taskType = task.TaskType;

            if ( taskType.Dependencies != null )
            {
                foreach ( DependencyConfiguration dependency in taskType.Dependencies )
                {
                    dependency.OwnerTaskType = task.TaskType.Name;

                    if ( !dependency.Required )
                        continue;

                    dependency.Processed = true;

                    Task dependentTask = this[dependency.TaskType];

                    // If the dependent task has not yet been added, add it now.
                    if ( dependentTask == null )
                    {
                        Trace.TaskResolving.WriteLine( "Inspecting the dependency {{{0}}}.", dependency );

                        TaskTypeConfiguration dependentTaskType;
                        if ( !this.project.TaskTypes.TryGetValue( dependency.TaskType, out dependentTaskType ) )
                        {
                            CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0041",
                                                              new object[] {task.TaskName, dependency.TaskType} );
                        }

                        Trace.ProjectLoader.WriteLine( "Adding task {0} because it is required by task {1}.",
                                                       dependentTaskType.Name, task.TaskName );

                        dependentTask = dependentTaskType.CreateInstance( this.project );
                        dependentTask.ImplicitelyRequired = true;

                        this.InternalAdd( dependentTask );
                        this.AddRequiredDependencies( dependentTask );
                    }

                    // Update predecessor-successor relationships.
                    if ( dependency.Position == DependencyPosition.After )
                    {
                        task.Successors.Add( dependency );
                        dependentTask.Predecessors.Add( dependency );
                    }
                    else
                    {
                        dependentTask.Successors.Add( dependency );
                        task.Predecessors.Add( dependency );
                    }
                }
            }
        }

        /// <summary>
        /// Sort the current collection so that dependencies are respected.
        /// </summary>
        private void Sort()
        {
            Trace.TaskResolving.WriteLine( "Sorting tasks." );

            Set<Task> tasksWithPredecessors = new Set<Task>( this.Count );
            Set<Task> tasksWithoutPredecessors = new Set<Task>( this.Count );
            int nextOrdinal = 0;


            // Put all pending tasks in the set of tasks with predecessors
            // and compute the next task ordinal.
            foreach ( Task task in this )
            {
                if ( task.State == TaskState.Queued )
                {
                    tasksWithPredecessors.Add( task );
                }
                else
                {
                    if ( task.Ordinal >= nextOrdinal )
                    {
                        nextOrdinal = task.Ordinal + 1;
                    }
                }
            }

            // Reset the ordinal of tasks with predecessors.
            foreach ( Task task in tasksWithPredecessors )
            {
                task.Ordinal = int.MaxValue;
            }

            int iterations = 0;
            while ( tasksWithPredecessors.Count > 0 )
            {
                iterations++;

                if ( iterations > 100 )
                {
                    // Trace the tasks with predecessors.
                    Trace.ProjectLoader.WriteLine( "Trace with cycles in predecessors:" );
                    foreach ( Task task in tasksWithPredecessors )
                    {
                        Trace.ProjectLoader.WriteLine( "Task {0}:", task.TaskName );
                        foreach ( DependencyConfiguration dependency in task.Predecessors )
                        {
                            Trace.ProjectLoader.WriteLine( "   after {0}", dependency.TaskType );
                        }
                    }

                    CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0042", null );
                }

                foreach ( Task task in tasksWithPredecessors )
                {
                    // Evaluate if this task has predecessors in tasksWithPredecessors
                    DependencyConfiguration predecessingDependency = null;
                    foreach ( DependencyConfiguration dependency in task.Predecessors )
                    {
                        Task predecessor = this[dependency.PrecedingTaskType];
                        if ( predecessor != null && predecessor.Ordinal == int.MaxValue )
                        {
                            predecessingDependency = dependency;
                            break;
                        }
                    }

                    if ( predecessingDependency == null )
                    {
                        // No predecessor. Assign it an ordinal and mark it to be removed from the 
                        // set of tasks with predecessors.
                        if ( task.TaskType.Phase != null )
                        {
                            PhaseConfiguration phase = Project.Phases[task.TaskType.Phase];
                            if ( phase == null )
                            {
                                CoreMessageSource.Instance.Write( SeverityType.Fatal, "PS0044",
                                                                  new object[] {task.TaskType.Name, task.TaskType.Phase} );
                            }
                            task.Ordinal = nextOrdinal + phase.Ordinal*1000;
                        }
                        else
                        {
                            // The task is not included in a phase, i.e. it will never be executed.
                            // The execution order is indifferent.
                            task.Ordinal = int.MaxValue - nextOrdinal - 1;
                        }
                        nextOrdinal++;
                        tasksWithoutPredecessors.Add( task );

                        Trace.TaskResolving.WriteLine(
                            "The task {0} has bubbled out, it was assigned the ordinal {1}.", task, task.Ordinal );
                    }
                }

                // Remove tasks without predecessors.
                foreach ( Task task in tasksWithoutPredecessors )
                {
                    tasksWithPredecessors.Remove( task );
                }

                tasksWithoutPredecessors.Clear();
            }
        }


        /// <summary>
        /// Gets a <see cref="Task"/> given its type name.
        /// </summary>
        /// <param name="taskTypeName">Name of the task type.</param>
        /// <returns>A <see cref="Task"/>, or <b>null</b> if the task was not found.</returns>
        [SuppressMessage( "Microsoft.Performance", "CA1822:MarkMembersAsStatic" )]
        [SuppressMessage( "Microsoft.Usage", "CA1801:ReviewUnusedParameters" )]
        public Task this[ string taskTypeName ]
        {
            get
            {
                this.AssertNotDisposed();

                return this.implementation[taskTypeName];
            }
        }

        #region ICollection<Task> Members

        /// <summary>
        /// Adds a <see cref="Task"/> to the collection.
        /// </summary>
        /// <param name="item">A <see cref="Task"/>.</param>
        /// <remarks>
        /// This method shall also add all tasks that are required by <paramref name="item"/>.
        /// </remarks>
        public void Add( Task item )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            if ( item.State != TaskState.Queued )
                throw new ArgumentException( "The state of this item should be Queued." );
            this.AssertNotDisposed();

            #endregion

            Trace.ProjectLoader.WriteLine( "Adding the instantiated task {0}.", item.TaskName );

            this.InternalAdd( item );

            Trace.ProjectLoader.WriteLine( "Adding required dependencies of task {0}.", item.TaskName );
            this.AddRequiredDependencies( item );

            Trace.ProjectLoader.WriteLine( "Processing optional dependencies of task {0}.", item.TaskName );
            this.AddOptionalDependencies();

            this.Sort();
        }

        private void InternalAdd( Task item )
        {
            this.implementation.Add( item );
            item.Parent = this;
            if ( this.TaskAdded != null )
            {
                this.TaskAdded( this, EventArgs.Empty );
            }
        }

        /// <summary>
        /// Remove all items of th current collection.
        /// </summary>
        public void Clear()
        {
            this.AssertNotDisposed();
            this.implementation.Clear();
        }

        /// <summary>
        /// Determines whether the current collection contains a given <see cref="Task"/>.
        /// </summary>
        /// <param name="item">A <see cref="Task"/>.</param>
        /// <returns><b>true</b> if this collection contains <paramref name="item"/>,
        /// otherwise <b>false</b>.</returns>
        public bool Contains( Task item )
        {
            #region Preconditions

            ExceptionHelper.AssertArgumentNotNull( item, "item" );
            this.AssertNotDisposed();

            #endregion

            return this.implementation.Contains( item );
        }

        /// <inheritdoc />
        public void CopyTo( Task[] array, int arrayIndex )
        {
            this.AssertNotDisposed();
            this.implementation.CopyTo( array, arrayIndex );
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                this.AssertNotDisposed();
                return this.implementation.Count;
            }
        }

        /// <inheritdoc />
        bool ICollection<Task>.IsReadOnly
        {
            get { return false; }
        }

        /// <inheritdoc />
        public bool Remove( Task item )
        {
            this.AssertNotDisposed();

            if ( this.Contains( item ) )
            {
                this.InternalRemove( item );

                // Remove tasks requiring the task being removed and dependency links.
                foreach ( DependencyConfiguration predecessor in item.Predecessors )
                {
                    Task dependentTask = this[predecessor.TaskType];

                    if ( predecessor.Required )
                    {
                        this.Remove( dependentTask );
                    }
                    else
                    {
                        dependentTask.Successors.Remove( predecessor );
                    }
                }

                foreach ( DependencyConfiguration successor in item.Successors )
                {
                    Task dependentTask = this[successor.TaskType];


                    if ( successor.Required )
                    {
                        if ( dependentTask != null )
                        {
                            this.Remove( dependentTask );
                        }
                    }
                    else
                    {
                        dependentTask.Predecessors.Remove( successor );
                    }
                }


                return true;
            }
            else
            {
                return false;
            }
        }

        private void InternalRemove( Task item )
        {
            if ( this.implementation.Remove( item ) )
            {
                if ( this.TaskRemoved != null )
                {
                    this.TaskRemoved( this, EventArgs.Empty );
                }
            }
        }

        #endregion

        #region IEnumerable<Task> Members

        /// <inheritdoc />
        public IEnumerator<Task> GetEnumerator()
        {
            this.AssertNotDisposed();
            return new MarshalByRefEnumerator<Task>( this.implementation.GetEnumerator() );
        }

        internal IEnumerator<Task> GetPendingTaskEnumerator( string phase )
        {
            this.AssertNotDisposed();
            return new PendingTaskEnumerator( this, phase );
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Enumerates all tasks implementing a given interface.
        /// </summary>
        /// <typeparam name="T">The interface that tasks have to implement.</typeparam>
        /// <returns>An enumerator returning all tasks implementing <typeparamref name="T"/>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1004" /* GenericMethodsShouldProvideTypeParameter  */ )]
        [SuppressMessage( "Microsoft.Design", "CA1024" /* UsePropertiesWhereAppropriate  */ )]
        public IEnumerator<T> GetInterfaces<T>()
        {
            this.AssertNotDisposed();
            foreach ( Task task in this )
            {
                if ( typeof(T).IsAssignableFrom( task.GetType() ) )
                {
                    yield return (T) (object) task;
                }
            }
        }

        /// <summary>
        /// Event raised after a new task has been added to the current collection.
        /// </summary>
        internal event EventHandler TaskAdded;

        /// <summary>
        /// Event raised after a task has been removed from the current collection.
        /// </summary>
        internal event EventHandler TaskRemoved;

        /// <summary>
        /// Enumerates tasks that have not yet been executed.
        /// </summary>
        [Serializable]
        private sealed class PendingTaskEnumerator : MarshalByRefObject, IEnumerator<Task>
        {
            /// <summary>
            /// Enumerated collection.
            /// </summary>
            private readonly TaskCollection collection;

            /// <summary>
            /// Internal enumerator.
            /// </summary>
            private IEnumerator<Task> innerEnumerator;

            /// <summary>
            /// Name of the phase whose tasks have to be retrieved.
            /// </summary>
            private readonly string phase;

            /// <summary>
            /// Initializes a new <see cref="PendingTaskEnumerator"/>.
            /// </summary>
            /// <param name="parent">The enumerated collection.</param>
            /// <param name="phase">Name of the phase whose tasks have to be retrieved.</param>
            public PendingTaskEnumerator( TaskCollection parent, string phase )
            {
                this.phase = phase;
                this.collection = parent;
                this.collection.TaskAdded += OnTaskAdded;
                this.collection.TaskRemoved += OnTaskRemoved;
                this.innerEnumerator = this.collection.GetEnumerator();
            }

            /// <summary>
            /// Called when a task has been added to the collection.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnTaskAdded( object sender, EventArgs e )
            {
                this.innerEnumerator.Dispose();
                this.innerEnumerator = this.collection.GetEnumerator();
            }

            /// <summary>
            /// Called when a task has been added to the collection.
            /// </summary>
            /// <param name="sender">Sender.</param>
            /// <param name="e">Event arguments.</param>
            private void OnTaskRemoved( object sender, EventArgs e )
            {
                this.innerEnumerator.Dispose();
                this.innerEnumerator = this.collection.GetEnumerator();
            }


            /// <inheritdoc />
            public Task Current
            {
                get { return this.innerEnumerator.Current; }
            }


            /// <inheritdoc />
            public void Dispose()
            {
                this.collection.TaskAdded -= OnTaskAdded;
                this.collection.TaskRemoved -= OnTaskRemoved;
                this.innerEnumerator.Dispose();
            }


            /// <inheritdoc />
            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                while ( this.innerEnumerator.MoveNext() )
                {
                    if ( this.innerEnumerator.Current.State == TaskState.Queued &&
                         this.innerEnumerator.Current.TaskType.Phase == phase )
                    {
                        return true;
                    }
                }

                return false;
            }

            /// <inheritdoc />
            public void Reset()
            {
                this.innerEnumerator.MoveNext();
            }
        }

        #region IDisposable Members

        [Conditional( "ASSERT" )]
        private void AssertNotDisposed()
        {
            if ( this.IsDisposed )
            {
                throw new ObjectDisposedException( "TaskCollection" );
            }
        }

        /// <summary>
        /// Determines whether the collection has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return this.implementation.IsDisposed; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.implementation.Dispose();
        }

        #endregion

        internal void OnItemOrdinalChanged( Task task, int oldOrdinal, int newOrdinal )
        {
            this.implementation.OnItemOrdinalChanged( task, oldOrdinal, newOrdinal );
        }

        private class TaskCollectionImpl : IndexedCollection<Task>
        {
            public TaskCollectionImpl()
                : base( new ICollectionFactory<Task>[]
                            {
                                new OrdinalIndexFactory<Task>( SortedMultiDictionaryFactory<int, Task>.Default ),
                                new NameIndexFactory<Task>( true, StringComparer.InvariantCultureIgnoreCase )
                            }, 16 )
            {
            }

            public Task this[ string taskTypeName ]
            {
                get
                {
                    Task task;
                    this.TryGetFirstValueByKey( 1, taskTypeName, out task );
                    return task;
                }
            }

            public void OnItemOrdinalChanged( Task item, int oldOrdinal, int newOrdinal )
            {
                this.OnItemPropertyChanged( 0, item, oldOrdinal );
            }
        }
    }
}