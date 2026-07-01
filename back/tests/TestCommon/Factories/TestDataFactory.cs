using Domain.Entities;
using TestCommon.Builders;

namespace TestCommon.Factories;

public static class TestDataFactory
{
    public static Conversation CreateConversationWithMessages(int convId, int user1Id = 1, int user2Id = 2, int messageCount = 3)
    {
        var conv = ConversationBuilder.Create(user1Id, user2Id);
        conv.Id = convId;
        for (int i = 0; i < messageCount; i++)
        {
            var msg = MessageBuilder.CreateText(convId, i % 2 == 0 ? user1Id : user2Id, $"Message {i + 1}");
            msg.Id = i + 1;
            conv.Messages.Add(msg);
        }
        conv.LastMessage = conv.Messages.Last();
        conv.LastMessageId = conv.LastMessage.Id;
        conv.LastMessageContent = conv.LastMessage.Content;
        conv.LastMessageAt = conv.LastMessage.CreatedAt;
        return conv;
    }

    public static List<Conversation> CreateMultipleConversations(int count, int user1Id = 1, int user2Id = 2)
    {
        var conversations = new List<Conversation>();
        for (int i = 0; i < count; i++)
        {
            var conv = CreateConversationWithMessages(i + 1, user1Id, user2Id, 1);
            conversations.Add(conv);
        }
        return conversations;
    }
}
