using API.Domain.Hero;
using API.Infrastructure.Interface;
using API.Jobs;
using API.Utilities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CSharpAPI.Tests.UnitTests.Jobs
{
    public class RedisUpdateTests
    {
        private readonly Mock<IHeroDao> _heroDaoMock;
        private readonly Mock<IRedisDao> _redisDaoMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly RedisUpdate _job;

        public RedisUpdateTests()
        {
            _heroDaoMock = new Mock<IHeroDao>();
            _redisDaoMock = new Mock<IRedisDao>();
            _configurationMock = new Mock<IConfiguration>();

            _job = new RedisUpdate(
                _heroDaoMock.Object,
                _redisDaoMock.Object,
                _configurationMock.Object);
        }

        [Fact(DisplayName = "Run: Should fetch active heroes and update Redis cache successfully")]
        public async Task Run_WhenCalled_UpdatesRedisWithActiveHeroes()
        {
            var configKey = "Metadata:activeHeroes";
            var cachePath = "cache:heroes:active";

            int mockCount = 2;
            var expectedValue = "2";

            _configurationMock.Setup(c => c[configKey]).Returns(cachePath);

            _heroDaoMock.Setup(h => h.ListActiveHeroes()).ReturnsAsync(mockCount);

            await _job.Run(null!);

            _heroDaoMock.Verify(h => h.ListActiveHeroes(), Times.Once);

            _redisDaoMock.Verify(r => r.setAsync(
                cachePath,
                It.Is<string>(val => val == expectedValue)),
                Times.Once);
        }

        [Fact(DisplayName = "Run: Should handle zero heroes correctly")]
        public async Task Run_WhenZeroHeroesFound_UpdatesRedisWithZeroString()
        {
            var cachePath = "cache:heroes:active";
            _configurationMock.Setup(c => c["Metadata:activeHeroes"]).Returns(cachePath);
            _heroDaoMock.Setup(h => h.ListActiveHeroes()).ReturnsAsync(0);

            await _job.Run(null!);

            _redisDaoMock.Verify(r => r.setAsync(cachePath, "0"), Times.Once);
        }

        [Fact(DisplayName = "Run: Should propagate exception when the database fails")]
        public async Task Run_WhenDaoThrowsException_PropagatesException()
        {
            _configurationMock.Setup(c => c[It.IsAny<string>()]).Returns("any_path");
            _heroDaoMock.Setup(h => h.ListActiveHeroes()).ThrowsAsync(new Exception("DB Error"));

            Func<Task> act = async () => await _job.Run(null!);

            await act.Should().ThrowAsync<Exception>().WithMessage("DB Error");
        }

        [Fact(DisplayName = "Run: Should handle failure when writing to Redis")]
        public async Task Run_WhenRedisFails_ThrowsException()
        {
            _configurationMock.Setup(c => c[It.IsAny<string>()]).Returns("any_path");
            _heroDaoMock.Setup(h => h.ListActiveHeroes()).ReturnsAsync(5);
            _redisDaoMock.Setup(r => r.setAsync(It.IsAny<string>(), It.IsAny<string>()))
                        .ThrowsAsync(new Exception("Redis connection failed"));

            Func<Task> act = async () => await _job.Run(null!);

            await act.Should().ThrowAsync<Exception>();
        }
    }
}