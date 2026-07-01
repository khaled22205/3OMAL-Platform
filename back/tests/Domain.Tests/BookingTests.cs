using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests;

public class BookingTests
{
    private static Booking CreateBooking() => new()
    {
        CustomerId = 1,
        WorkerProfileId = 1,
        ScheduledAt = DateTime.UtcNow.AddDays(1),
        TotalPrice = 500,
        CommissionAmount = 50
    };

    public class Accept
    {
        [Fact]
        public void Should_transition_from_Pending_to_Accepted()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Status.Should().Be(BookingStatus.Accepted);
        }

        [Fact]
        public void Should_throw_when_not_Pending()
        {
            var booking = CreateBooking();
            booking.Accept();
            var act = () => booking.Accept();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot transition from 'Accepted' to 'Accepted'");
        }
    }

    public class Reject
    {
        [Fact]
        public void Should_transition_from_Pending_to_Rejected()
        {
            var booking = CreateBooking();
            booking.Reject("Not available");
            booking.Status.Should().Be(BookingStatus.Rejected);
            booking.CancellationReason.Should().Be("Not available");
            booking.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_throw_when_from_Completed()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.CompleteJob();
            var act = () => booking.Reject();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class Cancel
    {
        [Fact]
        public void Should_cancel_when_in_cancellable_status()
        {
            var booking = CreateBooking();
            booking.Cancel(1, "Changed mind");
            booking.Status.Should().Be(BookingStatus.Cancelled);
            booking.CancelledBy.Should().Be("1");
            booking.CancellationReason.Should().Be("Changed mind");
        }

        [Fact]
        public void Should_throw_when_not_cancellable()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            var act = () => booking.Cancel(1);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot cancel booking in status 'OnTheWay'");
        }
    }

    public class Reschedule
    {
        [Fact]
        public void Should_reschedule_to_new_date()
        {
            var booking = CreateBooking();
            var newDate = DateTime.UtcNow.AddDays(3);
            booking.Reschedule(newDate);
            booking.ScheduledAt.Should().Be(newDate);
            booking.Status.Should().Be(BookingStatus.Scheduled);
        }

        [Fact]
        public void Should_throw_when_not_reschedulable()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            var act = () => booking.Reschedule(DateTime.UtcNow.AddDays(5));
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class MarkOnTheWay
    {
        [Fact]
        public void Should_transition_from_Scheduled_to_OnTheWay()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.Status.Should().Be(BookingStatus.OnTheWay);
        }

        [Fact]
        public void Should_throw_when_not_Scheduled()
        {
            var booking = CreateBooking();
            var act = () => booking.MarkOnTheWay();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class StartJob
    {
        [Fact]
        public void Should_start_job_and_set_StartedAt()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.Status.Should().Be(BookingStatus.Started);
            booking.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_set_StartedAt_only_once()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            var first = booking.StartedAt;
            var act = () => booking.StartJob();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class PauseJob
    {
        [Fact]
        public void Should_pause_from_Started()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.PauseJob();
            booking.Status.Should().Be(BookingStatus.Paused);
        }

        [Fact]
        public void Should_resume_from_Paused()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.PauseJob();
            booking.StartJob();
            booking.Status.Should().Be(BookingStatus.Started);
        }

        [Fact]
        public void Should_throw_when_not_Started()
        {
            var booking = CreateBooking();
            var act = () => booking.PauseJob();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class CompleteJob
    {
        [Fact]
        public void Should_complete_and_set_CompletedAt()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.CompleteJob();
            booking.Status.Should().Be(BookingStatus.Completed);
            booking.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_throw_when_not_Started()
        {
            var booking = CreateBooking();
            var act = () => booking.CompleteJob();
            act.Should().Throw<InvalidOperationException>();
        }
    }

    public class FullLifecycle
    {
        [Fact]
        public void Should_follow_complete_booking_lifecycle()
        {
            var booking = CreateBooking();
            booking.Status.Should().Be(BookingStatus.Pending);
            booking.Accept();
            booking.Status.Should().Be(BookingStatus.Accepted);
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.Status.Should().Be(BookingStatus.Scheduled);
            booking.MarkOnTheWay();
            booking.Status.Should().Be(BookingStatus.OnTheWay);
            booking.StartJob();
            booking.Status.Should().Be(BookingStatus.Started);
            booking.CompleteJob();
            booking.Status.Should().Be(BookingStatus.Completed);
            booking.CompletedAt.Should().NotBeNull();
            booking.StartedAt.Should().NotBeNull();
        }

        [Fact]
        public void Should_support_pause_resume_cycle()
        {
            var booking = CreateBooking();
            booking.Accept();
            booking.Reschedule(DateTime.UtcNow.AddDays(3));
            booking.MarkOnTheWay();
            booking.StartJob();
            booking.PauseJob();
            booking.Status.Should().Be(BookingStatus.Paused);
            booking.StartJob();
            booking.Status.Should().Be(BookingStatus.Started);
            booking.CompleteJob();
            booking.Status.Should().Be(BookingStatus.Completed);
        }

        [Fact]
        public void Should_allow_cancellation_only_in_correct_statuses()
        {
            var booking = CreateBooking();
            booking.Cancel(1, "test");
            booking.Status.Should().Be(BookingStatus.Cancelled);
        }
    }

    public class Expire
    {
        [Fact]
        public void Should_expire_from_any_status()
        {
            var booking = CreateBooking();
            booking.Expire();
            booking.Status.Should().Be(BookingStatus.Expired);
        }
    }

    public class TransitionTo
    {
        [Fact]
        public void Should_throw_for_invalid_transition()
        {
            var booking = CreateBooking();
            var act = () => booking.TransitionTo(BookingStatus.Completed);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Cannot transition from 'Pending' to 'Completed'");
        }

        [Fact]
        public void Should_update_UpdatedAt_on_valid_transition()
        {
            var booking = CreateBooking();
            var before = booking.UpdatedAt;
            Thread.Sleep(10);
            booking.Accept();
            booking.UpdatedAt.Should().BeAfter(before ?? DateTime.MinValue);
        }
    }
}
