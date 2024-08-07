using System;

namespace FrontDesk.Core.Exceptions
{
  public class AppointmentNotFoundException : Exception
  {
    public AppointmentNotFoundException(string message) : base(message)
    {
    }

    public AppointmentNotFoundException(Guid appointmentId) : base($"No appointment with id {appointmentId} found.")
    {
    }
  }
}
