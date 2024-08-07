using System;
using FrontDesk.Core.Exceptions;
using FrontDesk.Core.SyncedAggregates;
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
    public void ThrowsGivenNullAppointmentType()
    {
      // Arrange
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();

      // Act + Assert
      Assert.Throws<ArgumentException>(() => schedule.UpdateAppointment(
        Guid.NewGuid(), null, _start, NewRoomId, NewDoctorId, NewTitle));
    }

    [Fact]
    public void ThrowsGivenZeroRoomId()
    {
      // Arrange
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();

      // Act + Assert
      Assert.Throws<ArgumentException>(() => schedule.UpdateAppointment(
        Guid.NewGuid(), _apptType, _start, 0, NewDoctorId, NewTitle));
    }

    [Fact]
    public void ThrowsGivenZeroDoctorId()
    {
      // Arrange
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();

      // Act + Assert
      Assert.Throws<ArgumentException>(() => schedule.UpdateAppointment(
        Guid.NewGuid(), _apptType, _start, NewRoomId, 0, NewTitle));
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
      var schedule = new ScheduleBuilder().WithDefaultValues().Build();
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
  }
}
