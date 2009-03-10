﻿namespace StateMachineToolkit.Tests.Passive
{
	using System;
	using System.Diagnostics;
	using System.Threading;
	using NUnit.Framework;
	using Sanford.StateMachineToolkit;

	/// <summary>
	/// The error-handling behavior of the SM should be as follows:
	/// A standard sequence is:
	///		Event --> Guard* --> BeginTransition --> State.Exit* --> 
	///			Transition.Action* --> State.Enter* --> TransitionCompleted
	/// Another might be:
	///		Event --> Guard* --> TransitionDeclined
	/// </summary>
	[TestFixture]
	public class TestPassiveStateMachine
	{
		private EventTester m_beginDispatchEvent;
		private EventTester m_transitionDeclinedEvent;
		private EventTester m_transitionCompletedEvent;
		private EventTester m_exceptionThrownEvent;

		private void assertMachineEvents(bool beginDispatchCalled, bool transitionDeclinedCalled,
		                                 bool transitionCompletedCalled, bool exceptionThrownCalled)
		{
			if (beginDispatchCalled)
				m_beginDispatchEvent.AssertWasCalled("BeginDispatch was not called.");
			else
				m_beginDispatchEvent.AssertWasNotCalled("BeginDispatch was called.");

			if (transitionDeclinedCalled)
				m_transitionDeclinedEvent.AssertWasCalled("TransitionDeclined was not called.");
			else
				m_transitionDeclinedEvent.AssertWasNotCalled("TransitionDeclined was called.");

			if (transitionCompletedCalled)
				m_transitionCompletedEvent.AssertWasCalled("TransitionCompleted was not called.");
			else
				m_transitionCompletedEvent.AssertWasNotCalled("TransitionCompleted was called.");

			if (exceptionThrownCalled)
				m_exceptionThrownEvent.AssertWasCalled("ExceptionThrown was not called.");
			else
				m_exceptionThrownEvent.AssertWasNotCalled("ExceptionThrown was called.");
		}

		private void registerMachineEvents(IStateMachine<State, Event> machine)
		{
			m_beginDispatchEvent = new EventTester();
			m_transitionDeclinedEvent = new EventTester();
			m_transitionCompletedEvent = new EventTester();
			m_exceptionThrownEvent = new EventTester();
			machine.BeginDispatch += (sender, e) => m_beginDispatchEvent.Set();
			machine.TransitionDeclined += (sender, e) => m_transitionDeclinedEvent.Set();
			machine.TransitionCompleted += (sender, e) => m_transitionCompletedEvent.Set();
			machine.ExceptionThrown += (sender, e) => m_exceptionThrownEvent.Set();
		}

		[Test]
		public void SimpleTransitionTest()
		{
			var machine = new TestMachine();
			registerMachineEvents(machine);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void SimpleTransitionTest2()
		{
			var machine = new TestMachine<State, Event>();
			machine.AddTransition(State.S1, Event.S1_to_S2, State.S2);

			registerMachineEvents(machine);
			machine.Start(State.S1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void GuardException()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, args => { throw new Exception(); }, s2);

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, true, false, true);
			Assert.AreEqual(State.S1, machine.CurrentStateID);
		}

		[Test]
		public void EntryExceptionOnInit()
		{
			TransitionErrorEventArgs<State, Event> args;
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1, () => { throw new Exception(); }, null);
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);
			registerMachineEvents(machine);
			args = null;
			machine.ExceptionThrown += (sender, e) => args = e;
			machine.Start(s1);
			assertMachineEvents(false, false, false, true);
			Assert.AreEqual(State.S1, machine.CurrentStateID);
			Assert.AreEqual(false, args.MachineInitialized);
		}

		[Test]
		public void EntryExceptionOnSend()
		{
			TransitionErrorEventArgs<State, Event> errorEventArgs;
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2, () => { throw new Exception(); }, null);
			s1.Transitions.Add(Event.S1_to_S2, s2);
			errorEventArgs = null;
			machine.ExceptionThrown += (sender, args) => errorEventArgs = args;
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);

			Assert.AreEqual(State.S1, errorEventArgs.SourceStateID);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
			Assert.AreEqual(Event.S1_to_S2, errorEventArgs.EventID);
		}

		[Test]
		public void ExitException()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void TransitionActions_ThrowsException()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			var count = 0;
			s1.Transitions.Add(Event.S1_to_S2, s2, args =>
			                                        	{
			                                        		count++;
			                                        		throw new Exception();
			                                        	});
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
			Assert.AreEqual(1, count);
		}

		[Test]
		public void TransitionActions_ThrowsExceptionTwice()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			var count = 0;
			ActionHandler actionHandler = args =>
			                              	{
			                              		count++;
			                              		throw new Exception();
			                              	};
			s1.Transitions.Add(Event.S1_to_S2, s2, actionHandler, actionHandler);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
			Assert.AreEqual(2, count);
		}

		[Test]
		public void TransitionDeclined()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.S2_to_S1);
			machine.Execute();
			assertMachineEvents(true, true, false, false);
			Assert.AreEqual(State.S1, machine.CurrentStateID);
		}

		[Test]
		public void TransitionDeclined_ThrowsError()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1, null, () => { throw new Exception(); });
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);
			registerMachineEvents(machine);
			machine.TransitionDeclined += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(Event.S2_to_S1);
			machine.Execute();
			assertMachineEvents(true, true, false, true);
			Assert.AreEqual(State.S1, machine.CurrentStateID);
		}

		[Test]
		public void BeginDispatch()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);
		    IPassiveStateMachine<State, Event> m = machine;
            m.BeginDispatch += (sender, e) =>
			                         	{
                                            Assert.AreEqual(State.S1, m.CurrentStateID);
			                         		Assert.AreEqual(Event.S1_to_S2, e.EventID);
			                         		Assert.AreEqual(State.S1, e.SourceStateID);
			                         		Assert.AreEqual(123, e.EventArgs[0]);
			                         	};

			registerMachineEvents(m);
			machine.Start(s1);
            m.Send(Event.S1_to_S2, 123);
            m.Execute();
			assertMachineEvents(true, false, true, false);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void BeginDispatch_ThrowsError()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);

			registerMachineEvents(machine);
			machine.BeginDispatch += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void TransitionCompleted_ThrowsError()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s2 = machine.CreateState(State.S2);
			s1.Transitions.Add(Event.S1_to_S2, s2);

			registerMachineEvents(machine);
			machine.TransitionCompleted += (sender, e) => { throw new Exception(); };
			machine.Start(s1);
			machine.Send(Event.S1_to_S2);
			machine.Execute();
			assertMachineEvents(true, false, true, true);
			Assert.AreEqual(State.S2, machine.CurrentStateID);
		}

		[Test]
		public void Superstate_should_handle_event_when_guard_of_substate_does_not_pass()
		{
			var machine = new TestMachine<State, Event>();
			var s1 = machine.CreateState(State.S1);
			var s1_1 = machine.CreateState(State.S1_1);
			var s1_2 = machine.CreateState(State.S1_2);
			var s2 = machine.CreateState(State.S2);

			s1.Substates.Add(s1_1);
			s1.Substates.Add(s1_2);
			s1.InitialState = s1_1;
			s1_1.Transitions.Add(Event.E1, s1_2);
			s1_2.Transitions.Add(Event.E1, args => false, s1_1);
			s1.Transitions.Add(Event.E1, s2);

			registerMachineEvents(machine);
			machine.Start(s1);
			machine.Send(Event.E1);
			machine.Execute();
			Assert.AreEqual(State.S1_2, machine.CurrentStateID);
			machine.Send(Event.E1);
			machine.Execute();
			Assert.AreEqual(State.S2, machine.CurrentStateID);
			assertMachineEvents(true, false, true, false);
		}
	}

	public class EventTester
	{
		private readonly AutoResetEvent wasCalledEvent = new AutoResetEvent(false);

		public void Set()
		{
			wasCalledEvent.Set();
		}

		private bool wasCalled(TimeSpan timeout)
		{
			return wasCalledEvent.WaitOne(timeout, false);
		}

		private bool wasCalled()
		{
			var timeout = Debugger.IsAttached
			                   	? TimeSpan.FromMinutes(1)
			                   	: TimeSpan.FromMilliseconds(50);
			return wasCalled(timeout);
		}

		public void AssertWasCalled(string message)
		{
			Assert.IsTrue(wasCalled(), message);
		}

		public void AssertWasNotCalled(string message)
		{
			Assert.IsFalse(wasCalled(), message);
		}
	}

	public class TestMachine<TState, TEvent> : PassiveStateMachine<TState, TEvent>
		where TEvent : struct, IComparable, IFormattable
		where TState : struct, IComparable, IFormattable
	{
		public void Start(TState initialState)
		{
			Initialize(initialState);
		}
		public void Start(State initialState)
		{
			Initialize(initialState);
		}
	}

	public class TestMachine : PassiveStateMachine<State, Event>
	{
		public TestMachine()
		{
			AddTransition(Passive.State.S1, Event.S1_to_S2, Passive.State.S2);
			AddTransition(Passive.State.S2, Event.S2_to_S1, Passive.State.S1);
// ReSharper disable DoNotCallOverridableMethodsInConstructor
			Initialize(Passive.State.S1);
// ReSharper restore DoNotCallOverridableMethodsInConstructor
		}
	}

	public enum State
	{
		S1,
		S2,
		S1_1,
		S1_2,
	}

	public enum Event
	{
		S1_to_S2,
		S2_to_S1,
		E1,
	}
}