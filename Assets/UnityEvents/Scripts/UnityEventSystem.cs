using System.Collections.Generic;
using UnityEngine;
using UnityEvents;

namespace UnityEventsInternal
{
	/// <summary>
	/// The base class exists in case we want to generically call a function
	/// without caring about the type.
	/// </summary>
	public abstract class UnityEventSystemBase
	{
		// Global usuage, MonoEventSystem handles per GameObject
		public static System.Action onGlobalReset;
		public static System.Action onResetAll;
		public static System.Action<object> onUnsubscribeTarget;

		public abstract void UnsubscribeTarget(object target);
		public abstract void Reset();
		public abstract void CleanUp();
	}

	public class UnityEventSystem<T> : UnityEventSystemBase where T : struct
	{
		private LinkedList<EventSubscription<T>> _callbacks = new LinkedList<EventSubscription<T>>();
		private bool _reset;
		private int _currentGeneration;
		private bool _externalLocked;
		private LinkedListNode<EventSubscription<T>> _nextNode;
		private bool _sendingEvent;

		private static Stack<LinkedListNode<EventSubscription<T>>> _nodePool;
		private const int INITIAL_NODE_POPULATION = 100;

		public bool debug { get; set; }

		public UnityEventSystem()
		{
			onResetAll += Reset;
		}

		public void SendEvent(T ev)
		{
			// External locked means we might be sending multiple events to the same generation.
			if (!_externalLocked)
			{
				_currentGeneration++;
			}

			LinkedListNode<EventSubscription<T>> node = _callbacks.First;
			_sendingEvent = true;

			while (node != null)
			{
				_nextNode = node.Next;

				if (node.Value.callback.Target != null && node.Value.generation < _currentGeneration)
				{
					if (debug)
					{
						Debug.Log("Sending event: " + node.Value.callback.Target + " " + node.Value);
					}

					node.Value.callback(ev);
				}
				else
				{
					// Remove, must be gone.
					_callbacks.Remove(node);
					_nodePool.Push(node);
				}

				node = _nextNode;
			}

			_sendingEvent = false;

			if (_reset)
			{
				_reset = false;
				Reset();
			}
		}

		/// <summary>
		/// Once Reset all active SusbscriptionHandles are no longer valid.
		/// They are still safe to unsubscribe with but have no effect.
		/// </summary>
		public override void Reset()
		{
			if (_sendingEvent)
			{
				_reset = true;
				return;
			}

			_callbacks = new LinkedList<EventSubscription<T>>();
		}

		public EventHandle<T> Subscribe(System.Action<T> callback)
		{
			EventSubscription<T> sub = new EventSubscription<T>();
			sub.callback = callback;
			sub.generation = _currentGeneration;
			
			EventHandle<T> handle = new EventHandle<T>();
			handle.callbackNode = GetNode(sub);

			if (debug)
			{
				Debug.Log("Subscribe: " + handle.callbackNode.Value.callback.Target + " " + handle.callbackNode.Value.callback);
			}

			_callbacks.AddLast(handle.callbackNode);

			return handle;
		}

		public void Unsubscribe(System.Action<T> callback)
		{
			LinkedListNode<EventSubscription<T>> node = FindLastNode(callback);

			if (node == null)
			{
				return;
			}

			if (debug)
			{
				Debug.Log("Unsubscribe: " + callback.Target + " " + callback);
			}

			Unsubscribe(node);
		}

		private LinkedListNode<EventSubscription<T>> FindLastNode(System.Action<T> callback)
		{
			LinkedListNode<EventSubscription<T>> node = _callbacks.Last;

			while (node != null)
			{
				if (node.Value.callback == callback)
				{
					return node;
				}

				node = node.Previous;
			}

			return null;
		}

		public void Unsubscribe(EventHandle<T> handle)
		{
			if (handle.callbackNode.List != _callbacks)
			{
				return;
			}

			Unsubscribe(handle.callbackNode);
		}

		private void Unsubscribe(LinkedListNode<EventSubscription<T>> node)
		{
			if (node == _nextNode)
			{
				_nextNode = _nextNode.Next;
			}

			if (node.List == _callbacks)
			{
				_callbacks.Remove(node);
			}

			_nodePool.Push(node);
		}

		private static LinkedListNode<EventSubscription<T>> GetNode(EventSubscription<T> callback)
		{
			if (_nodePool == null)
			{
				_nodePool = new Stack<LinkedListNode<EventSubscription<T>>>();

				for (int i = 0; i < INITIAL_NODE_POPULATION; i++)
				{
					_nodePool.Push(new LinkedListNode<EventSubscription<T>>(new EventSubscription<T>()));
				}
			}

			LinkedListNode<EventSubscription<T>> node;

			if (_nodePool.Count > 0)
			{
				node = _nodePool.Pop();
			}
			else
			{
				node = new LinkedListNode<EventSubscription<T>>(new EventSubscription<T>());
			}

			node.Value = callback;

			return node;
		}

		public override void CleanUp()
		{
			onResetAll -= Reset;
			onGlobalReset -= Reset;
		}

		public void LockGeneration()
		{
			_externalLocked = true;
			_currentGeneration++;
		}

		public void UnlockGeneration()
		{
			_externalLocked = false;
		}

		public static UnityEventSystem<T> global
		{
			get
			{
				if (_global == null)
				{
					_global = new UnityEventSystem<T>();
					onGlobalReset += _global.Reset;
					onUnsubscribeTarget += _global.UnsubscribeTarget;
				}

				return _global;
			}
		}

		private static UnityEventSystem<T> _global;

		public override void UnsubscribeTarget(object target)
		{
			LinkedListNode<EventSubscription<T>> node = _callbacks.First;

			while (node != null)
			{
				LinkedListNode<EventSubscription<T>> nextNode = node.Next;

				if (node.Value.callback.Target == target)
				{
					Unsubscribe(node);
				}

				node = nextNode;
			}
		}
	}
}


