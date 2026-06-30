using Kawwer.Domain.Entities;
using Kawwer.Domain.Enums;
using Kawwer.Domain.Exceptions;

namespace Kawwer.Tests.Domain;

public sealed class ChatMessageAndRatingTests
{
    [Fact]
    public void SystemMessage_CannotBeEdited()
    {
        var message = ChatMessage.CreateSystemMessage(Guid.NewGuid(), "Match cancelled.");
        Assert.Throws<DomainException>(() => message.Edit(Guid.NewGuid(), "changed"));
    }

    [Fact]
    public void UserMessage_CanOnlyBeEditedByAuthor()
    {
        var author = Guid.NewGuid();
        var message = ChatMessage.CreateUserMessage(Guid.NewGuid(), author, "hello");
        Assert.Throws<DomainException>(() => message.Edit(Guid.NewGuid(), "hijack"));

        message.Edit(author, "hello again");
        Assert.True(message.IsEdited);
        Assert.Equal("hello again", message.Content);
    }

    [Fact]
    public void Rating_RejectsOutOfRangeStars()
    {
        Assert.Throws<DomainException>(() =>
            new Rating(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RatingType.Player, 6));
    }

    [Fact]
    public void Rating_RejectsSelfRating()
    {
        var user = Guid.NewGuid();
        Assert.Throws<DomainException>(() =>
            new Rating(Guid.NewGuid(), user, user, RatingType.Organizer, 5));
    }
}
