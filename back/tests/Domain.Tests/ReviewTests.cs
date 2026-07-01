using Domain.Entities;
using FluentAssertions;

namespace Domain.Tests;

public class ReviewTests
{
    private static Review CreateReview() => new()
    {
        BookingId = 1,
        CustomerId = 1,
        WorkerProfileId = 1,
        Rating = 5,
        Comment = "Great work!"
    };

    public class UpdateRating
    {
        [Fact]
        public void Should_update_rating_and_comment()
        {
            var review = CreateReview();
            review.UpdateRating(3, "Updated comment");
            review.Rating.Should().Be(3);
            review.Comment.Should().Be("Updated comment");
            review.IsEdited.Should().BeTrue();
            review.EditedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        [InlineData(-1)]
        public void Should_throw_for_invalid_rating(int invalidRating)
        {
            var review = CreateReview();
            var act = () => review.UpdateRating(invalidRating, "comment");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Rating must be between 1 and 5");
        }

        [Fact]
        public void Should_accept_boundary_ratings()
        {
            var review = CreateReview();
            review.UpdateRating(1, "Min");
            review.Rating.Should().Be(1);
            review.UpdateRating(5, "Max");
            review.Rating.Should().Be(5);
        }

        [Fact]
        public void Should_set_IsEdited_true()
        {
            var review = CreateReview();
            review.IsEdited.Should().BeFalse();
            review.UpdateRating(4, null);
            review.IsEdited.Should().BeTrue();
        }
    }

    public class Reply
    {
        [Fact]
        public void Should_set_worker_reply()
        {
            var review = CreateReview();
            review.Reply("Thank you!");
            review.WorkerReply.Should().Be("Thank you!");
        }

        [Fact]
        public void Should_update_UpdatedAt()
        {
            var review = CreateReview();
            review.Reply("Thanks");
            review.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Should_allow_empty_reply()
        {
            var review = CreateReview();
            review.Reply("");
            review.WorkerReply.Should().Be("");
        }

        [Fact]
        public void Should_overwrite_previous_reply()
        {
            var review = CreateReview();
            review.Reply("First reply");
            review.Reply("Updated reply");
            review.WorkerReply.Should().Be("Updated reply");
        }
    }
}
