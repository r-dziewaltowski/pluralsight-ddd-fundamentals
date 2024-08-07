using System;
using PluralsightDdd.SharedKernel;
using UnitTests.Builders;
using Xunit;

namespace UnitTests.Core.AggregatesEntities.AppointmentTests
{
  public class Appointment_UpdateStartTime
  {
    private readonly DateTimeOffset _startTime = new DateTimeOffset(2021, 01, 01, 10, 00, 00, new TimeSpan(-4, 0, 0));
    private readonly DateTimeOffset _endTime = new DateTimeOffset(2021, 01, 01, 12, 00, 00, new TimeSpan(-4, 0, 0));
    private AppointmentBuilder _builder = new AppointmentBuilder();
    private DateTimeOffsetRange _newDateTimeOffsetRange;

    public Appointment_UpdateStartTime()
    {
      _newDateTimeOffsetRange = new DateTimeOffsetRange(_startTime, _endTime);
    }

    [Fact]
    public void UpdatesTimeRange()
    {
      var appointment = _builder
        .WithDefaultValues()
        .WithDateTimeOffsetRange(_newDateTimeOffsetRange)
        .Build();

      var newStartTime = new DateTime(2021, 01, 01, 11, 00, 00);

      appointment.UpdateStartTime(newStartTime);

      Assert.Equal(_newDateTimeOffsetRange.DurationInMinutes(), appointment.TimeRange.DurationInMinutes());
      Assert.Equal(newStartTime, appointment.TimeRange.Start);
    }

    [Fact]
    public void AddsEventWhenSuccessful()
    {
      var appointment = _builder
        .WithDefaultValues()
        .WithDateTimeOffsetRange(_newDateTimeOffsetRange)
        .Build();

      var newStartTime = new DateTime(2021, 01, 01, 11, 00, 00);

      appointment.UpdateStartTime(newStartTime);

      Assert.NotEmpty(appointment.Events);
    }

    [Fact]
    public void DoesNotAddEventWhenNoActualUpdateMade()
    {
      var appointment = _builder
        .WithDefaultValues()
        .WithDateTimeOffsetRange(_newDateTimeOffsetRange)
        .Build();

      var newStartTime = _startTime;

      appointment.UpdateStartTime(newStartTime);

      Assert.Empty(appointment.Events);
    }
  }
}
