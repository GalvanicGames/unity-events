using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEvents.Example
{
	public class ExampleEntityTargetReservations
	{
		public void ReserveEntityTargets()
		{
			// See ExampleCustomEventSystem for a description of what EventTarget are and how they could be
			// used. Also see EventTarget.cs
			
			// Reservations allow us to reserve a block of event targets for us to use however way we see fit. Will
			// prevent EventTarget from giving out targets within that reservation on EventTarget creation or other'
			// reservations.
			EventTargetReservation reservation = EventTarget.ReserveTargets(100);

			// Grabs an EventTarget by index
			EventTarget target0 = reservation.GetEntityTarget(0);
			EventTarget target50 = reservation.GetEntityTarget(50);
			
			// Throws an error if outside the reservation (if checks are enabled)
			EventTarget target200 = reservation.GetEntityTarget(200);
		}
	}
}
