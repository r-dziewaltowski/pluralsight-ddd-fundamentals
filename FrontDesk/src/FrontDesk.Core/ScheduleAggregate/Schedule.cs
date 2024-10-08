﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using FrontDesk.Core.Events;
using FrontDesk.Core.SyncedAggregates;
using MediatR;
using PluralsightDdd.SharedKernel;
using PluralsightDdd.SharedKernel.Interfaces;

namespace FrontDesk.Core.ScheduleAggregate
{
  public class Schedule : BaseEntity<Guid>, IAggregateRoot
  {
    public Schedule(Guid id,
      DateTimeOffsetRange dateRange,
      int clinicId)
    {
      Id = Guard.Against.Default(id, nameof(id));
      DateRange = dateRange;
      ClinicId = Guard.Against.NegativeOrZero(clinicId, nameof(clinicId));
    }

    private Schedule(Guid id, int clinicId) // used by EF
    {
      Id = id;
      ClinicId = clinicId;
    }

    public int ClinicId { get; private set; }
    private readonly List<Appointment> _appointments = new List<Appointment>();
    public IEnumerable<Appointment> Appointments => _appointments.AsReadOnly();

    public DateTimeOffsetRange DateRange { get; private set; }

    public void AddNewAppointment(Appointment appointment)
    {
      Guard.Against.Null(appointment, nameof(appointment));
      Guard.Against.Default(appointment.Id, nameof(appointment.Id));
      Guard.Against.DuplicateAppointment(_appointments, appointment, nameof(appointment));

      _appointments.Add(appointment);

      MarkConflictingAppointments();

      var appointmentScheduledEvent = new AppointmentScheduledEvent(appointment);
      Events.Add(appointmentScheduledEvent);
    }

    public void DeleteAppointment(Appointment appointment)
    {
      Guard.Against.Null(appointment, nameof(appointment));
      var appointmentToDelete = _appointments
                                .Where(a => a.Id == appointment.Id)
                                .FirstOrDefault();

      if (appointmentToDelete != null)
      {
        _appointments.Remove(appointmentToDelete);
      }

      MarkConflictingAppointments();

      var appointmentDeletedEvent = new AppointmentDeletedEvent(appointment);
      Events.Add(appointmentDeletedEvent);
    }

    public Appointment UpdateAppointment(
      Guid apptId, 
      AppointmentType apptType,  
      DateTimeOffset start,
      int roomId,
      int doctorId,
      string title)
    {
      Guard.Against.Default(apptId, nameof(apptId));
      var apptToUpdate = Guard.Against.NonExistentAppointment(_appointments, apptId);

      apptToUpdate.UpdateAppointmentType(apptType);
      apptToUpdate.UpdateStartTime(start);
      apptToUpdate.UpdateRoom(roomId);
      apptToUpdate.UpdateDoctor(doctorId);
      apptToUpdate.UpdateTitle(title);

      MarkConflictingAppointments();

      return apptToUpdate;
    }

    private void MarkConflictingAppointments()
    {
      foreach (var appointment in _appointments)
      {
        // mark overlapping appointments as conflicting if same patient/room/doctor
        var potentiallyConflictingAppointments = _appointments
            .Where(a => 
              (a.PatientId == appointment.PatientId || a.RoomId == appointment.RoomId || a.DoctorId == appointment.DoctorId) &&
              a.TimeRange.Overlaps(appointment.TimeRange) &&
              a != appointment)
            .ToList();

        potentiallyConflictingAppointments.ForEach(a => a.IsPotentiallyConflicting = true);

        appointment.IsPotentiallyConflicting = potentiallyConflictingAppointments.Any();
      }
    }
  }
}
