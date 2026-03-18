using API.Domain.Jobs;
using API.Jobs;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;

namespace CSharpAPI.Tests.UnitTests.Jobs
{
    public class SendEmailTests
    {
        private readonly Mock<API.Infrastructure.Interfaces.IMailer> _mailerMock;
        private readonly Mock<ILogger<SendEmail>> _loggerMock;
        private readonly SendEmail _job;

        public SendEmailTests()
        {
            _mailerMock = new Mock<API.Infrastructure.Interfaces.IMailer>();
            _loggerMock = new Mock<ILogger<SendEmail>>();
            _job = new SendEmail(_mailerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Send_ForgotPasswordEmail_ShouldLogInformationAtStart()
        {
            var data = new ForgotPasswordEmail { Name = "User", Email = "test@test.com" };

            await _job.Send(null, data);

            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("SendEmail.Send ForgotPasswordEmail")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task Send_WhenAllAttemptsFail_ShouldLogErrorsThreeTimes()
        {
            var data = new ForgotPasswordEmail { Name = "User", Email = "fail@test.com" };
            _mailerMock.Setup(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MimeEntity>()))
                .ThrowsAsync(new Exception("SMTP Error"));


            Func<Task> act = async () => await _job.Send(null, data);

            await act.Should().NotThrowAsync();

            _mailerMock.Verify(m => m.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MimeEntity>()),
                Times.Exactly(3));

            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));
        }
    }
}