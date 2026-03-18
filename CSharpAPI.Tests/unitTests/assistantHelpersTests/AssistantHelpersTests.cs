using API.Utilities;
using FluentAssertions;

namespace CSharpAPI.Tests.UnitTests.Utilities
{
    public class AssistantHelpersTests
    {
        #region Serialization & Validation

        [Theory(DisplayName = "ToJson: Handles various object types")]
        [InlineData(null, "null")]
        [InlineData(new string[] { }, "[]")]
        public void ToJson_ReturnsExpectedJson(object input, string expected)
        {
            input.ToJson().Should().Be(expected);
        }

        [Fact]
        public void ToJson_WithSimpleObject_ReturnsValidJson()
        {
            var obj = new { Name = "Test", Value = 123 };
            obj.ToJson().Should().ContainAll("Name", "Test", "Value", "123");
        }

        [Theory(DisplayName = "IsValidPassword: Validates constraints and boundaries")]
        [InlineData(null, "Password cannot be null")]
        [InlineData("Short1!", "Password must be between 10 characters and 100 characters")]
        [InlineData("NoDigitHere!", "Password must have at least one number")]
        [InlineData("nouppercase123!", "Password must have at least one uppercase letter")]
        [InlineData("NoSpecialChar123", "Password must have at least one special character")]
        public void IsValidPassword_ReturnsFalseWithError(string password, string expectedError)
        {
            password.IsValidPassword(out var error).Should().BeFalse();
            error.Should().Be(expectedError);
        }

        #endregion

        #region Encryption & Hashing Round-Trips

        [Theory(DisplayName = "Encryption: Round-trip preserves data")]
        [InlineData("test@example.com")]
        [InlineData("SecretPassword123")]
        public void Encryption_RoundTrip_ReturnsOriginalValue(string original)
        {
            original.EncryptCookie().DecryptCookie().Should().Be(original);

            original.Encrypt().Decrypt().Should().Be(original);
        }

        [Fact]
        public void PasswordHashing_ValidatesCorrectly_AndIsUnique()
        {
            var pwd = "ValidPassword123!";
            var hash1 = pwd.PasswordEncryption();
            var hash2 = pwd.PasswordEncryption();

            hash1.Should().NotBe(hash2);
            pwd.PasswordValidation(hash1).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void EncryptCookie_InvalidInput_Throws(string input)
        {
            FluentActions.Invoking(() => input.EncryptCookie())
                .Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Decrypt_InvalidInput_Throws(string input)
        {
            FluentActions.Invoking(() => input.Decrypt())
                .Should().Throw<Exception>();
        }
        #endregion

        #region Random Generation

        [Theory(DisplayName = "Random Generation: Returns correct format and length")]
        [InlineData(6, true)]   
        [InlineData(10, false)]
        [InlineData(50, false)] 
        public void GenerateRandomCode_ReturnsExpectedFormat(int length, bool isNumeric)
        {
            var result = isNumeric
                ? AssistantHelpers.GenerateRandomCodeNumeric()
                : AssistantHelpers.GenerateRandomCodeAlphanumeric(length);

            result.Should().HaveLength(length);
            if (isNumeric) result.Should().MatchRegex(@"^\d{6}$");
        }

        [Fact]
        public void GenerateRandomCode_InvalidLength_Throws()
        {
            FluentActions.Invoking(() => AssistantHelpers.GenerateRandomCodeAlphanumeric(-1))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        #endregion
    }
}