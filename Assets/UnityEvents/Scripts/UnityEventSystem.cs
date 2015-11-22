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
		public bool lockSubscriptions;

		private Queue<Subscription> _queuedSubscriptions = new Queue<Subscription>();
		private LinkedList<System.Action<T>> _callbacks = new LinkedList<System.Action<T>>();
		private bool _reset;
		private bool _internalLockSubscriptions;

		private static Stack<LinkedListNode<System.Action<T>>> _nodePool;
		private const int INITIAL_NODE_POPULATION = 100;

		private bool locked { get { return lockSubscriptions || _internalLockSubscriptions; } }

		public bool debug { get; set; }

		public UnityEventSystem()
		{
			onResetAll += Reset;
		}

		private struct Subscription
		{
			public LinkedListNode<System.Action<T>> node;

			public Subscription(LinkedListNode<System.Action<T>> node)
			{
				this.node = node;
			}
		}

		public void SendEvent(T ev)
		{
			_internalLockSubscriptions = true;
			SendSingleEvent(ev);
			_internalLockSubscriptions = false;
			HandlePostEvent();
		}

		private LinkedListNode<System.Action<T>> _nextNode;

		private void SendSingleEvent(T ev)
		{
			LinkedListNode<System.Action<T>> node = _callbacks.First;

			while (node != null)
			{
				_nextNode = node.Next;

				if (node.Value.Target != null)
				{
					if (debug)
					{
						Debug.Log("Sending event: " + node.Value.Target + " " + node.Value);
					}

					node.Value(ev);
				}
				else
				{
					// Remove, must be gone.
					_callbacks.Remove(node);
					_nodePool.Push(node);
				}

				node = _nextNode;
			}
		}

		private void HandlePostEvent()
		{
			if (_reset)
			{
				_reset = false;
				Reset();
			}
			else
			{
				while (_queuedSubscriptions.Count > 0)
				{
					Subscription sub = _queuedSubscriptions.Dequeue();

					if (debug)
					{
						Debug.Log("Handling Queued Subscribe: " + sub.node.Value.Target + " " + sub.node.Value);
					}

					Subscribe(sub.node);
				}
			}
		}

		/// <summary>
		/// Once Reset all active SusbscriptionHandles are no longer valid.
		/// They are still safe to unsubscribe with but have no effect.
		/// </summary>
		public override void Reset()
		{
			if (locked)
			{
				_reset = true;
				return;
			}

			_queuedSubscriptions = new Queue<Subscription>();
			_callbacks = new LinkedList<System.Action<T>>();
		}

		public EventHandle<T> Subscribe(System.Action<T> callback)
		{
			EventHandle<T> handle = new EventHandle<T>();
			handle.callbackNode = GetNode(callback);

			if (locked)
			{
				if (debug)
				{
					Debug.Log("Queued Subscribe: " + handle.callbackNode.Value.Target + " " + handle.callbackNode.Value);
				}

				_queuedSubscriptions.Enqueue(new Subscription(handle.callbackNode));
			}
			else
			{
				if (debug)
				{
					Debug.Log("Subscribe: " + handle.callbackNode.Value.Target + " " + handle.callbackNode.Value);
				}

				Subscribe(handle.callbackNode);
			}

			return handle;
		}

		private void Subscribe(LinkedListNode<System.Action<T>> node)
		{
			if (node.List == null)
			{
				_callbacks.AddLast(node);
			}
		}

		public void Unsubscribe(System.Action<T> callback)
		{
			LinkedListNode<System.Action<T>> node = _callbacks.FindLast(callback);

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

		public void Unsubscribe(EventHandle<T> handle)
		{
			if (handle.callbackNode.List != _callbacks)
			{
				return;
			}

			Unsubscribe(handle.callbackNode);
		}

		private void Unsubscribe(LinkedListNode<System.Action<T>> node)
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

		private static LinkedListNode<System.Action<T>> GetNode(System.Action<T> callback)
		{
			if (_nodePool == null)
			{
				_nodePool = new Stack<LinkedListNode<System.Action<T>>>();

				for (int i = 0; i < INITIAL_NODE_POPULATION; i++)
				{
					_nodePool.Push(new LinkedListNode<System.Action<T>>(null));
				}
			}

			LinkedListNode<System.Action<T>> node;

			if (_nodePool.Count > 0)
			{
				node = _nodePool.Pop();
			}
			else
			{
				node = new LinkedListNode<System.Action<T>>(null);
			}

			node.Value = callback;

			return node;
		}

		public override void CleanUp()
		{
			onResetAll -= Reset;
			onGlobalReset -= Reset;
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
			LinkedListNode<System.Action<T>> node = _callbacks.First;

			while (node != null)
			{
				LinkedListNode<System.Action<T>> nextNode = node.Next;

				if (node.Value.Target == target)
				{
					Unsubscribe(node);
				}

				node = nextNode;
			}
		}
	}
}


