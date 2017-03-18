using System;
using System.Collections.Generic;

public static partial class ioDriver 
{
	/// Interface for managed events.
	public interface IEvent
	{
		/// Event ID
		string ID { get; }

		/// User description
		string Description { get; set; }
		/// Is this event enabled?  Will not fire if false, even if the condition returns true.
		bool Enabled { get; set; }
		/// How many times will this event fire before being disposed?
		int FireCount { get; set; }
		/// Event Priority.  During each pump cycle.  The lowest priority values will have their conditions checked first.
		uint Priority { get; set; }
		/// Target object
		object Target { get; }

		/// Fire this event.  FireCount will be reduced by 1.
		void Fire();

		/// Run the condition function and get its result.
		bool TestCond();

		/// Fluency method.  <see cref="Priority"/>
		IEvent SetPriority(uint _priority);
		/// Fluency method.  <see cref="FireCount"/>
		IEvent SetFireCount(int _fireCount);
		/// Fluency method.  <see cref="Enabled"/>
		IEvent SetEnabled(bool _enabled);
	}

	/// Event related data
	public static class Event
	{
	    static Event()
	    {
	        ioDriver.Init();
	    }

		/// Constant representing infinite event fire count.
		public const int FIRE_COUNT_INFINITE = -1;
		private const string DESC_USER_EVENT = "User Event";

		/// <summary>
		/// ioDriver event handler delegate.
		/// </summary>
		public delegate void Handler();

		/// <summary>
		/// ioDriver event handler delegate with a target parameter.
		/// </summary>
		/// <typeparam name="T">Target type</typeparam>
		/// <param name="_target">Target object</param>
		public delegate void Handler<in T>(T _target);

		/// <summary>
		/// ioDriver event condition delegate.
		/// </summary>
		/// <returns>Condition function result</returns>
		public delegate bool Condition();

		/// <summary>
		/// ioDriver event condition delegate with a target parameter.
		/// </summary>
		/// <typeparam name="T">Target type</typeparam>
		/// <param name="_obj">Target object</param>
		/// <returns>Condition function result</returns>
		public delegate bool Condition<in T>(T _obj);

		/// Managed event info
		private class Managed<TTarget> : IEvent
		{
			public string ID { get; private set; }
			public string Description { get; set; }

            private Condition m_Condition;
            private Handler<TTarget> m_Handler;
			public bool Enabled { get; set; }
			public int FireCount { get; set; }
			private TTarget m_Target;

			public uint Priority
			{
				get { return Manager.GetPriority(ID); }
				set { Manager.SetPriority(ID, value); }
			}

			public static IEvent Create(Condition _condition, Handler<IEvent> _action, int _fireCount, string _description, string _id)
			{
				var evt = new Managed<IEvent>
				{
					m_Condition = _condition,
					m_Handler = _action,
					FireCount = _fireCount,
					Description = _description,
					ID = _id,
					Enabled = true
				};
				evt.m_Target = evt;
				return evt;
			}

			public static IEvent Create<T>(Condition _condition, Handler<T> _action, T _target, int _fireCount,
				string _description, string _id)
			{
				var evt = new Managed<T>
				{
					m_Condition = _condition,
					m_Handler = _action,
					m_Target = _target,
					FireCount = _fireCount,
					Description = _description,
					ID = _id,
					Enabled = true
				};
				return evt;
			}

			/// Remove this managed event and dispose it.
			public void Remove()
			{
				Manager.Remove(ID);
			}

			/// Fire this event's handler.  Decreases fire count by 1.
			void IEvent.Fire()
			{
				FireCount--;
				this.m_Handler(m_Target);
			}

			bool IEvent.TestCond()
			{
				return m_Condition();
			}

			/// Fluency method.  See <see cref="Priority"/>
			IEvent IEvent.SetPriority(uint _priority)
			{
				Priority = _priority;
				return this;
			}

			/// Fluency method.  See <see cref="FireCount"/>
			IEvent IEvent.SetFireCount(int _fireCount)
			{
				FireCount = _fireCount;
				return this;
			}

			/// Fluency method.  See <see cref="Enabled"/>
			IEvent IEvent.SetEnabled(bool _enabled)
			{
				Enabled = _enabled;
				return this;
			}

			object IEvent.Target { get { return m_Target; } }
		}

		private static class Manager
		{
			private static Dictionary<string, IEvent> m_Events = new Dictionary<string, IEvent>();
			private static Dictionary<IEvent, string> m_EventIDs = new Dictionary<IEvent, string>(); 
			private static Dictionary<object, HashSet<string>> m_EventsByObject = new Dictionary<object, HashSet<string>>();
			private static SortedList<uint, HashSet<string>> m_EventByPriority = new SortedList<uint, HashSet<string>>(Comparer<uint>.Default);
			private static Dictionary<string, uint> m_PriorityByEvent = new Dictionary<string, uint>();

			public static void Init()
			{
				Hooks.ProcessEvents = Process;
				Hooks.DisposeEvents = Dispose;
			}
			

			public static void Process()
			{
				var toRemove = new List<string>();
				if (m_Events.Count == 0) return;
					foreach(var set in m_EventByPriority)
						foreach(var id in set.Value)
						{

							var mEvt = m_Events[id];
							
							var obj = mEvt.Target;
							if (!mEvt.Enabled) continue;
							if (!mEvt.TestCond()) continue;
							if (mEvt.FireCount != FIRE_COUNT_INFINITE && mEvt.FireCount <= 0)
							{
								toRemove.Add(id);
								continue;
							}
							mEvt.Fire();
							Log.Info("Fired event: " + mEvt.Description + " from " + obj.ToString() , Debug.ReportEvents);
							if (mEvt.FireCount == Event.FIRE_COUNT_INFINITE) continue;
							
						}
				foreach (var id in toRemove)
					Remove(id);
			}

			public static IEvent Add(Condition _cond, Handler<IEvent> _handler, int _fireCount, uint _priority, string _desc,
				string _id = null)
			{
			    var id = _id ?? GenID();
				var mEvt = Managed<IEvent>.Create(_cond, _handler, _fireCount, _desc, id);

				Manage(mEvt, _priority);
				return mEvt;
			}

			public static IEvent Add<T>(T _target, Condition _cond, Handler<T> _handler, int _fireCount, uint _priority, string _desc, string _id = null)
			{

                var id = _id ?? GenID();
				var mEvt = Managed<T>.Create(_cond, _handler, _target, _fireCount, _desc, id);

				Manage(mEvt, _priority);
				return mEvt;
			}

			private static void Manage(IEvent _event, uint _priority)
			{
			    if (m_Events.ContainsKey(_event.ID))
			    {
			        Log.Err("Event ID: '" + _event.ID + "' already exists.  IDs must be unique.  Ignoring Event.");
			        return;
			    }
				Log.Info("Adding event for target " + _event.Target + " with event '" + _event.Description + "' - count: " + _event.FireCount +
					" - Priority: " + _priority + " - ID: '" + _event.ID + "'", Debug.ReportEvents);

				m_Events.Add(_event.ID, _event);
				m_EventIDs.Add(_event, _event.ID);
				if (!m_EventsByObject.ContainsKey(_event.Target))
					m_EventsByObject.Add(_event.Target, new HashSet<string>());
				m_EventsByObject[_event.Target].Add(_event.ID);

				_event.Priority = _priority;

			}

			public static Dictionary<string, IEvent> GetAllEvents()
			{
				return new Dictionary<string, IEvent>(m_Events);
			}

			public static IEvent GetEvent(string _id)
			{
				if (!m_Events.ContainsKey(_id)) return null;
				return m_Events[_id];
			}

			public static void Remove(string _id)
			{
				EventExistCheck(_id, "Remove");
				var targ = m_Events[_id].Target;
				m_EventsByObject[targ].Remove(_id);
				if (m_EventsByObject[targ].Count == 0)
					m_EventsByObject.Remove(targ);
				m_EventIDs.Remove(m_Events[_id]);
				m_Events.Remove(_id);
				var pri = m_PriorityByEvent[_id];
				m_EventByPriority[pri].Remove(_id);
				if (m_EventByPriority[pri].Count == 0)
					m_EventByPriority.Remove(pri);
				m_PriorityByEvent.Remove(_id);
			}

			public static void Dispose(object _target)
			{
			    if (!m_EventsByObject.ContainsKey(_target)) return;
				
				var evtIDs = new List<string>(m_EventsByObject[_target]);
				foreach (var id in evtIDs)
					Remove(id);

			}

			public static HashSet<string> GetEventIDsOn(object _target)
			{
				return !m_EventsByObject.ContainsKey(_target) ? 
					new HashSet<string>() : 
					new HashSet<string>(m_EventsByObject[_target]);
			}

			public static void SetPriority(string _mEvtID, uint _priority)
			{
				EventExistCheck(_mEvtID, "SetPriority");
				if (m_PriorityByEvent.ContainsKey(_mEvtID))
				{
					var curPri = m_PriorityByEvent[_mEvtID];
					m_PriorityByEvent.Remove(_mEvtID);
					m_EventByPriority[curPri].Remove(_mEvtID);
				}
				

				m_PriorityByEvent.Add(_mEvtID, _priority);
				if (!m_EventByPriority.ContainsKey(_priority))
					m_EventByPriority.Add(_priority, new HashSet<string>());
				m_EventByPriority[_priority].Add(_mEvtID);
			}

			public static uint GetPriority(string _mEvtID)
			{
				EventExistCheck(_mEvtID, "GetPriority");
				return m_PriorityByEvent[_mEvtID];
			}

			private static void EventExistCheck(string _id, string _method)
			{
				if(!m_Events.ContainsKey(_id))
					throw new Exception(_method + " : Attempted operation on event ID: '" + _id + "' event does not exist!");
			}
		}

		/// <summary>
		/// Add new Managed event
		/// </summary>
		/// <typeparam name="T">Target Object Type</typeparam>
		/// <param name="_target">Target Object</param>
		/// <param name="_condition">Condition function, event will fire when condition returns true</param>
		/// <param name="_eventAction">Event Handler Action with target type parameter</param>
		/// <param name="_id">Optional event ID</param>
		/// <returns>Created event</returns>
		public static IEvent Add<T>(T _target, Event.Condition _condition, Event.Handler<T> _eventAction, string _id = null)
		{
			return Manager.Add(_target, _condition, _eventAction, 1, Defaults.EventPriority, DESC_USER_EVENT, _id);
		}

		/// <summary>
		/// Add new Managed event
		/// </summary>
		/// <param name="_condition">Condition function, event will fire when condition returns true</param>
		/// <param name="_eventAction">Event Handler Action with target type IEvent</param>
		/// <param name="_id">Optional event ID</param>
		/// <returns>Created event</returns>
		public static IEvent Add(Event.Condition _condition, Event.Handler<IEvent> _eventAction, string _id = null)
		{
			return Manager.Add(_condition, _eventAction, 1, Defaults.EventPriority, DESC_USER_EVENT, _id);
		}

		/// <summary>
		/// Add new Managed event.  No target (target assigned will be the event itself)
		/// </summary>
		/// <param name="_condition">Condition function, event will fire when condition returns true</param>
		/// <param name="_eventAction">Event Handler Action</param>
		/// <param name="_id">Optional event ID</param>
		/// <returns>Created event</returns>
		public static IEvent Add(Event.Condition _condition, Action _eventAction, string _id = null)
		{
			return Manager.Add(_condition, _evt => _eventAction(), 1, Defaults.EventPriority, DESC_USER_EVENT, _id);
		}

	    public static void Remove(string _id)
	    {
	        var evt = Manager.GetEvent(_id);
	        if (evt == null)
	        {
	            Log.Warn("ioDriver.Event.Remove : Event ID '" + _id + "' not found. Ignoring.");
	            return;
	        }
	        Manager.Remove(_id);
	    }
		
		// Dispose this object (removes all events)
		private static void Dispose(object _target)
		{
			Manager.Dispose(_target);
		}

		/// <summary>
		/// Get managed event with specified ID
		/// </summary>
		/// <param name="_id">Event ID</param>
		/// <returns>Managed event with ID (null if not found)</returns>
		public static IEvent GetEvent(string _id)
		{
			return Manager.GetEvent(_id);
		}


		/// <summary>
		/// Get all managed events keyed by ID.
		/// </summary>
		/// <returns>Dictionary of all managed events keyed by ID.</returns>
		public static Dictionary<string, IEvent> GetAllManagedEvents()
		{
			return new Dictionary<string, IEvent>(Manager.GetAllEvents());
		}

		/// <summary>
		/// Get set of IDs for all managed events with specified target object.
		/// </summary>
		/// <param name="_target">Target object</param>
		/// <returns>HashSet of managed event IDs found with target object</returns>
		public static HashSet<string> GetEventIDsOn(object _target)
		{
			return Manager.GetEventIDsOn(_target);
		}

		private static void Init()
		{
			Manager.Init();
		}
	}

	
}
