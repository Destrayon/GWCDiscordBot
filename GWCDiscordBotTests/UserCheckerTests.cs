using Discord;
using Discord.WebSocket;
using GWCDiscordBot;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBotTests
{
    [TestClass]
    public class UserCheckerTests
    {
        private Mock<IGuild> _mockGuild;
        private Mock<ITextChannel> _mockChannel;
        private Mock<IConfiguration> _mockConfig;
        private UserChecker _userChecker;

        [TestInitialize]
        public void Setup()
        {
            _mockGuild = new Mock<IGuild>();
            _mockChannel = new Mock<ITextChannel>();
            _mockConfig = new Mock<IConfiguration>();

            _mockConfig.Setup(config => config["DiscordSettings:NewestUsersFilterInSeconds"])
                       .Returns("3600"); // 1 hour for testing

            _userChecker = new UserChecker(_mockGuild.Object, _mockChannel.Object, _mockConfig.Object);
        }

        [TestMethod]
        public async Task GetOffendingUsers_ReturnsCorrectUsers()
        {
            var now = DateTime.Now;
                var joinedUsers = new List<IGuildUser>
            {
                CreateMockUser(1, now.AddMinutes(-10)), // joined within the filter time
                CreateMockUser(2, now.AddMinutes(-30)), // joined within the filter time
                CreateMockUser(3, now.AddHours(-2)) // joined outside the filter time
            };

            _mockGuild.Setup(g => g.GetUsersAsync(It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                      .ReturnsAsync(joinedUsers);

            var messages = new List<IMessage>
            {
                CreateMockMessage(2, now.AddMinutes(-5)), // user who sent a message within the filter time
                CreateMockMessage(3, now.AddHours(-3)) // user who sent a message outside the filter time
            };

            // Setup initial batch of messages
            var initialMessageBatch = new List<IReadOnlyCollection<IMessage>> { messages.ToImmutableArray() };
            var emptyMessageBatch = new List<IReadOnlyCollection<IMessage>> { ImmutableArray<IMessage>.Empty };


            _mockChannel.Setup(c => c.GetMessagesAsync(It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                        .Returns(initialMessageBatch.ToAsyncEnumerable());

            _mockChannel.Setup(c => c.GetMessagesAsync(It.IsAny<IMessage>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>()))
                        .Returns(emptyMessageBatch.ToAsyncEnumerable());

            var offendingUsers = (await _userChecker.GetOffendingUsers()).ToList();

            Assert.IsTrue(offendingUsers.Contains(1), "User 1 should be in the list of offending users.");
            Assert.IsFalse(offendingUsers.Contains(2), "User 2 should not be in the list of offending users.");
            Assert.IsFalse(offendingUsers.Contains(3), "User 3 should not be in the list of offending users as they joined outside the filter time.");
        }

        private IGuildUser CreateMockUser(ulong id, DateTimeOffset joinedAt)
        {
            var mockUser = new Mock<IGuildUser>();
            mockUser.Setup(u => u.Id).Returns(id);
            mockUser.Setup(u => u.JoinedAt).Returns(joinedAt);
            mockUser.Setup(u => u.IsBot).Returns(false);
            return mockUser.Object;
        }

        private IMessage CreateMockMessage(ulong authorId, DateTimeOffset timestamp)
        {
            var mockMessage = new Mock<IMessage>();
            mockMessage.Setup(m => m.Author.Id).Returns(authorId);
            mockMessage.Setup(m => m.Timestamp).Returns(timestamp);
            return mockMessage.Object;
        }
    }
}
