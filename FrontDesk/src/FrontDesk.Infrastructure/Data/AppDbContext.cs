using System;
using System.Linq;
using System.Reflection;
using FrontDesk.Core.ScheduleAggregate;
using FrontDesk.Core.SyncedAggregates;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PluralsightDdd.SharedKernel;

namespace FrontDesk.Infrastructure.Data
{
  public class AppDbContext : DbContext
  {
    private readonly IMediator _mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
        : base(options)
    {
      _mediator = mediator;
      SavingChanges += DispatchEvents;
    }

    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentType> AppointmentTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void DispatchEvents(object sender, SavingChangesEventArgs e)
    {
      // ignore events if no dispatcher provided
      if (_mediator == null) return;

      var entitiesWithEvents = ChangeTracker
          .Entries()
          .Select(e => e.Entity as BaseEntity<Guid>)
          .Where(e => e?.Events != null && e.Events.Any())
          .ToArray();

      foreach (var entity in entitiesWithEvents)
      {
        var events = entity.Events.ToArray();
        entity.Events.Clear();
        foreach (var domainEvent in events)
        {
          _mediator.Publish(domainEvent);
        }
      }
    }
  }
}
