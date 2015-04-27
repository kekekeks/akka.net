using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor.Internal;

namespace Akka.Actor
{
	public partial class ActorCell
	{
		
		class ActorCellSynchronizationContext : SynchronizationContext
		{
			private readonly ActorCell _actorCell;
			private SynchronizationContext _parent;
			public ActorCellSynchronizationContext(ActorCell actorCell)
			{
				_actorCell = actorCell;
			}

			public void SetParent(SynchronizationContext context)
			{
				while (true)
				{
					var fake = context as ActorCellSynchronizationContext;
					if (fake == null)
					{
						_parent = context;
						break;
					}
					context = fake._parent;
				}
			}
			
			public override void Post(SendOrPostCallback d, object state)
			{
				if (_parent != null)
					_parent.Post(_ => InvokeWithState(d, state), null);
				else
					ThreadPool.QueueUserWorkItem(_ => InvokeWithState(d, state), null);
			}

			void InvokeWithState(SendOrPostCallback d, object state)
			{
				var oldCell = InternalCurrentActorCellKeeper.Current;
				InternalCurrentActorCellKeeper.Current = _actorCell;
				try
				{
					d(state);
				}
				finally
				{
					InternalCurrentActorCellKeeper.Current = oldCell;
				}
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				if (_parent != null)
					_parent.Send(_ => InvokeWithState(d, state), null);
				else
				{
					var tcs = new TaskCompletionSource<int>();
					Post(_ =>
					{
						try
						{
							d(state);
							tcs.SetResult(0);
						}
						catch (Exception e)
						{
							tcs.SetException(e);
						}
					}, null);
					tcs.Task.Wait();
				}
			}
		}
	}
}
