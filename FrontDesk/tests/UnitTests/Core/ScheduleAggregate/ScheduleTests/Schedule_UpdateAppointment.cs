using System;
using FrontDesk.Core.Exceptions;
using FrontDesk.Core.ScheduleAggregate;
using FrontDesk.Core.SyncedAggregates;
using PluralsightDdd.SharedKernel;
using UnitTests.Builders;
using Xunit;

namespace UnitTests.Core.AggregatesEntities.ScheduleTests
{
  public class Schedule_UpdateAppointment
  {
    const int NewRoomId = 100;
    const int NewDoctorId = 101;
    const string NewTitle = "test title";
    const int ApptDuration = 45;
    private readonly AppointmentType _apptType = new(102, "Test AppointmentType", "Test Code", ApptDuration);
    private readonly DateTimeOffset _start = new(new DateTime(2024, 6, 6, 11, 0, 0));
    private readonly DateTimeOffsetRange _range;

    public Schedule_UpdateAppointment()
    {
      _range = new DateTimeOffsetRange(_start, TimeSpan.FromMinutes(ApptDuration));
    }

    [Fact]
    public void ThrowsGivenEmptyAppointmentId()
    {
      // Arrange
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();

      // Act + Assert
      Assert.Throws<ArgumentException>(() => schedule.UpdateAppointment(
        Guid.Empty, _apptType, _start, NewRoomId, NewDoctorId, NewTitle));
    }

    [Fact]
    public void ThrowsGivenNonExistentAppointment()
    {
      // Arrange
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();

      // Act + Assert
      Assert.Throws<AppointmentNotFoundException>(() => schedule.UpdateAppointment(
        Guid.NewGuid(), _apptType, _start, NewRoomId, NewDoctorId, NewTitle));
    }

    [Fact]
    public void UpdatesAndReturnsAppointment()
    {
      // Arrange
      var schedule = CreateSchedule();
      var appointment = new AppointmentBuilder().WithDefaultValues().Build();
      schedule.AddNewAppointment(appointment);

      // Act
      var updatedAppointment = schedule.UpdateAppointment(
        appointment.Id, _apptType, _start, NewRoomId, NewDoctorId, NewTitle);

      // Assert
      Assert.Equal(NewRoomId, appointment.RoomId);
      Assert.Equal(NewDoctorId, appointment.DoctorId);
      Assert.Equal(NewTitle, appointment.Title);
      Assert.Equal(_start, appointment.TimeRange.Start);
      Assert.Equal(_start + TimeSpan.FromMinutes(ApptDuration), appointment.TimeRange.End);
      Assert.Equal(appointment, updatedAppointment);
    }

    [Fact]
    public void MarksConflictingAppointmentsDueToSameRoomId()
    {
      // Arrange
      var schedule = CreateSchedule();
      var appointment1 = CreateAppointment(_range, _apptType.Id, NewRoomId, NewDoctorId);
      var appointment2 = CreateAppointment(_range, _apptType.Id + 1, NewRoomId + 1, NewDoctorId + 1);
      schedule.AddNewAppointment(appointment1);
      schedule.AddNewAppointment(appointment2);

      // Act
      var updatedAppointment = schedule.UpdateAppointment(
        appointment1.Id, _apptType, _start, NewRoomId + 1, NewDoctorId, NewTitle);

      // Assert
      Assert.True(appointment1.IsPotentiallyConflicting);
      Assert.True(appointment2.IsPotentiallyConflicting);
    }

    [Fact]
    public void MarksConflictingAppointmentsDueToSameDoctorId()
    {
      // Arrange
      var schedule = CreateSchedule();
      var appointment1 = CreateAppointment(_range, _apptType.Id, NewRoomId, NewDoctorId);
      var appointment2 = CreateAppointment(_range, _apptType.Id + 1, NewRoomId + 1, NewDoctorId + 1);
      schedule.AddNewAppointment(appointment1);
      schedule.AddNewAppointment(appointment2);

      // Act
      var updatedAppointment = schedule.UpdateAppointment(
        appointment1.Id, _apptType, _start, NewRoomId, NewDoctorId + 1, NewTitle);

      // Assert
      Assert.True(appointment1.IsPotentiallyConflicting);
      Assert.True(appointment2.IsPotentiallyConflicting);
    }

    [Fact]
    public void MarksConflictingAppointmentsDueToOverlappingAfterStartChanged()
    {
      // Arrange
      var schedule = CreateSchedule();
      var appointment1 = CreateAppointment(_range, _apptType.Id, NewRoomId, NewDoctorId);
      var lateStart = new DateTimeOffsetRange(_start.AddHours(3), TimeSpan.FromMinutes(60));
      var appointment2 = CreateAppointment(lateStart, _apptType.Id + 1, NewRoomId + 1, NewDoctorId + 1);
      schedule.AddNewAppointment(appointment1);
      schedule.AddNewAppointment(appointment2);

      // Act
      var updatedAppointment = schedule.UpdateAppointment(
        appointment1.Id, _apptType, _start.AddHours(3), NewRoomId, NewDoctorId, NewTitle);

      // Assert
      Assert.True(appointment1.IsPotentiallyConflicting);
      Assert.True(appointment2.IsPotentiallyConflicting);
    }

    [Fact]
    public void MarksConflictingAppointmentsDueToOverlappingAfterAppointmentTypeChanged()
    {
      // Arrange
      var schedule = CreateSchedule();
      var appointment1 = CreateAppointment(_range, _apptType.Id, NewRoomId, NewDoctorId);
      var lateStart = new DateTimeOffsetRange(_start.AddHours(1), TimeSpan.FromMinutes(60));
      var appointment2 = CreateAppointment(lateStart, _apptType.Id + 1, NewRoomId + 1, NewDoctorId + 1);
      schedule.AddNewAppointment(appointment1);
      schedule.AddNewAppointment(appointment2);

      // Act
      var newApptType = new AppointmentType(103, "Test AppointmentType", "Test Code", 90);
      var updatedAppointment = schedule.UpdateAppointment(
        appointment1.Id, newApptType, _start, NewRoomId, NewDoctorId, NewTitle);

      // Assert
      Assert.True(appointment1.IsPotentiallyConflicting);
      Assert.True(appointment2.IsPotentiallyConflicting);
    }

    private Appointment CreateAppointment(DateTimeOffsetRange dateTimeOffsetRange, int appointmentTypeId,
      int roomId, int doctorId)
    {
      var appointmentBuilder = new AppointmentBuilder();

      return appointmentBuilder.
        WithDefaultValues().
        WithDateTimeOffsetRange(dateTimeOffsetRange).
        WithAppointmentTypeId(appointmentTypeId).
        WithRoomId(roomId).
        WithDoctorId(doctorId).
        Build();
    }

    private Schedule CreateSchedule()
    {
      return new ScheduleBuilder().WithDefaultValues().Build();
    }
  }
}
