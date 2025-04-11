using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using ca.whittaker.Maui.Controls;

namespace ca.whittaker.UnitTesting.Maui.Controls;

[TestFixture]
public class InputValidatorTests
{
    #region Email Tests

    [Test]
    public void Test_IsValidEmail_ValidEmail()
    {
        // ARRANGE
        var assertions = new List<string>();
        string email = "test@example.com";

        try
        {
            // ACT
            bool result = InputValidator.IsValidEmail(email);

            // ASSERT
            Assert.That(result, Is.True, "Valid email should return true.");
            assertions.Add("> PASS: Valid email 'test@example.com' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add("> FAIL: Valid email 'test@example.com' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidEmail_InvalidMissingAt()
    {
        // ARRANGE
        var assertions = new List<string>();
        string email = "testexample.com";

        try
        {
            // ACT
            bool result = InputValidator.IsValidEmail(email);

            // ASSERT
            Assert.That(result, Is.False, "Email missing '@' should return false.");
            assertions.Add("> PASS: Email 'testexample.com' (missing '@') returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add("> FAIL: Email 'testexample.com' (missing '@') did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidEmail_TooShort()
    {
        // ARRANGE
        var assertions = new List<string>();
        string email = "a@b";

        try
        {
            // ACT
            bool result = InputValidator.IsValidEmail(email);

            // ASSERT
            Assert.That(result, Is.False, "Too short email should return false.");
            assertions.Add("> PASS: Too short email 'a@b' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add("> FAIL: Too short email 'a@b' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidEmail_EmailWithEmoji()
    {
        // ARRANGE
        var assertions = new List<string>();
        string email = "t😀est@example.com";

        try
        {
            // ACT
            bool result = InputValidator.IsValidEmail(email);

            // ASSERT
            Assert.That(result, Is.False, "Email with emoji should be filtered and return true.");
            assertions.Add("> Email with emoji 't😀est@example.com' was filtered and returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add("> FAIL: Email with emoji 't😀est@example.com' did not return true as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterEmailFilter_RemovesEmoji()
    {
        // ARRANGE
        var assertions = new List<string>();
        string emailWithEmoji = "t😀est@example.com";
        string expectedFiltered = "test@example.com"; // Emoji should be removed

        try
        {
            // ACT
            string filtered = InputValidator.FilterEmailFilter(true, emailWithEmoji);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterEmailFilter should remove emoji.");
            assertions.Add($"> PASS: FilterEmailFilter correctly transformed '{emailWithEmoji}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterEmailFilter did not correctly transform '{emailWithEmoji}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    #endregion

    #region URL Tests
    [Test]
    public void Test_IsValidUrl_ValidUrl()
    {
        // ARRANGE
        var assertions = new List<string>();
        string url = "http://example.com";

        try
        {
            // ACT
            bool result = InputValidator.IsValidUrl(url);

            // ASSERT
            Assert.That(result, Is.True, "Valid URL should return true.");
            assertions.Add($"> PASS: Valid URL '{url}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid URL '{url}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidUrl_InvalidUrl()
    {
        // ARRANGE
        var assertions = new List<string>();
        string url = "not a url";

        try
        {
            // ACT
            bool result = InputValidator.IsValidUrl(url);

            // ASSERT
            Assert.That(result, Is.False, "Invalid URL should return false.");
            assertions.Add($"> PASS: Invalid URL '{url}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Invalid URL '{url}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterUrlFilter_RemovesInvalidChars()
    {
        // ARRANGE
        var assertions = new List<string>();
        string urlWithInvalid = "http://example.com/path?query=string";
        string expectedFiltered = "http://example.com/pathquerystring";

        try
        {
            // ACT
            string filtered = InputValidator.FilterUrlFilter(true, urlWithInvalid);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterUrlFilter should remove invalid URL characters.");
            assertions.Add($"> PASS: FilterUrlFilter correctly transformed '{urlWithInvalid}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterUrlFilter did not correctly transform '{urlWithInvalid}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Plaintext Tests
    [Test]
    public void Test_IsValidPlaintext_ValidText()
    {
        // ARRANGE
        var assertions = new List<string>();
        string text = "Hello World";

        try
        {
            // ACT
            bool result = InputValidator.IsValidPlaintext(text);

            // ASSERT
            Assert.That(result, Is.True, "Valid plaintext should return true.");
            assertions.Add($"> PASS: Valid plaintext '{text}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid plaintext '{text}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidPlaintext_InvalidText()
    {
        // ARRANGE
        var assertions = new List<string>();
        string text = "Hi";

        try
        {
            // ACT
            bool result = InputValidator.IsValidPlaintext(text);

            // ASSERT
            Assert.That(result, Is.False, "Plaintext too short should return false.");
            assertions.Add($"> PASS: Plaintext '{text}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Plaintext '{text}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterPlaintext_RemovesNonAscii()
    {
        // ARRANGE
        var assertions = new List<string>();
        string textWithNonAscii = "Hello€World";
        string expectedFiltered = "HelloWorld";

        try
        {
            // ACT
            string filtered = InputValidator.FilterPlaintext(true, textWithNonAscii);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterPlaintext should remove non-ASCII characters.");
            assertions.Add($"> PASS: FilterPlaintext correctly transformed '{textWithNonAscii}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterPlaintext did not correctly transform '{textWithNonAscii}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Richtext Tests
    [Test]
    public void Test_IsValidRichtext_ValidText()
    {
        // ARRANGE
        var assertions = new List<string>();
        string text = "Hello 😀";

        try
        {
            // ACT
            bool result = InputValidator.IsValidRichtext(text);

            // ASSERT
            Assert.That(result, Is.True, "Valid richtext should return true.");
            assertions.Add($"> PASS: Valid richtext '{text}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid richtext '{text}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterRichtext_RemovesControlCharacters()
    {
        // ARRANGE
        var assertions = new List<string>();
        string textWithControl = "Hello\u0001World";
        string expectedFiltered = "HelloWorld";

        try
        {
            // ACT
            string filtered = InputValidator.FilterRichtext(true, textWithControl);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterRichtext should remove control characters.");
            assertions.Add($"> PASS: FilterRichtext correctly transformed '{textWithControl}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterRichtext did not correctly transform '{textWithControl}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterSingleLine_RemovesLineBreaks()
    {
        // ARRANGE
        var assertions = new List<string>();
        string multiLineText = "Hello\r\nWorld";
        string expectedFiltered = "HelloWorld";

        try
        {
            // ACT
            string filtered = InputValidator.FilterSingleLine(true, multiLineText);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterSingleLine should remove line breaks.");
            assertions.Add($"> PASS: FilterSingleLine correctly transformed line breaks in '{multiLineText}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterSingleLine did not correctly remove line breaks in '{multiLineText}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterAllLowercase_ConvertsToLowercase()
    {
        // ARRANGE
        var assertions = new List<string>();
        string mixedCase = "HelloWorld";
        string expectedFiltered = "helloworld";

        try
        {
            // ACT
            string filtered = InputValidator.FilterAllLowercase(true, mixedCase);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterAllLowercase should convert text to lowercase.");
            assertions.Add($"> PASS: FilterAllLowercase correctly converted '{mixedCase}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterAllLowercase did not correctly convert '{mixedCase}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterAllowWhiteSpace_RemovesWhiteSpace()
    {
        // ARRANGE
        var assertions = new List<string>();
        string textWithSpaces = "Hello World";
        string expectedFiltered = "HelloWorld";

        try
        {
            // ACT
            string filtered = InputValidator.FilterAllowWhiteSpace(true, textWithSpaces);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterAllowWhiteSpace should remove white spaces.");
            assertions.Add($"> PASS: FilterAllowWhiteSpace correctly transformed '{textWithSpaces}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterAllowWhiteSpace did not correctly transform '{textWithSpaces}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Username Tests
    [Test]
    public void Test_IsValidUsername_ValidUsername()
    {
        // ARRANGE
        var assertions = new List<string>();
        string username = "user.name-123";

        try
        {
            // ACT
            bool result = InputValidator.IsValidUsername(username);

            // ASSERT
            Assert.That(result, Is.True, "Valid username should return true.");
            assertions.Add($"> PASS: Valid username '{username}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid username '{username}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidUsername_InvalidUsername()
    {
        // ARRANGE
        var assertions = new List<string>();
        string username = "us!"; // Too short and contains invalid character

        try
        {
            // ACT
            bool result = InputValidator.IsValidUsername(username);

            // ASSERT
            Assert.That(result, Is.False, "Invalid username should return false.");
            assertions.Add($"> PASS: Invalid username '{username}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Invalid username '{username}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterUsernameFilter_RemovesInvalidChars()
    {
        // ARRANGE
        var assertions = new List<string>();
        string usernameWithInvalid = "user!name";
        string expectedFiltered = "username";

        try
        {
            // ACT
            string filtered = InputValidator.FilterUsernameFilter(true, usernameWithInvalid);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterUsernameFilter should remove invalid characters.");
            assertions.Add($"> PASS: FilterUsernameFilter correctly transformed '{usernameWithInvalid}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterUsernameFilter did not correctly transform '{usernameWithInvalid}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Numeric Tests
    [Test]
    public void Test_IsValidNumeric_ValidNumeric()
    {
        // ARRANGE
        var assertions = new List<string>();
        string numericString = "123.45";

        try
        {
            // ACT
            bool result = InputValidator.IsValidNumeric(numericString);

            // ASSERT
            Assert.That(result, Is.True, "Valid numeric string should return true.");
            assertions.Add($"> PASS: Valid numeric string '{numericString}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid numeric string '{numericString}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidNumeric_InvalidNumeric()
    {
        // ARRANGE
        var assertions = new List<string>();
        string numericString = "12a34";

        try
        {
            // ACT
            bool result = InputValidator.IsValidNumeric(numericString);

            // ASSERT
            Assert.That(result, Is.False, "Invalid numeric string should return false.");
            assertions.Add($"> PASS: Invalid numeric string '{numericString}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Invalid numeric string '{numericString}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterNumeric_RemovesInvalidChars()
    {
        // ARRANGE
        var assertions = new List<string>();
        string input = "1a2b3.4-5";
        string expectedFiltered = "123.4-5";

        try
        {
            // ACT
            string filtered = InputValidator.FilterNumeric(true, input);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterNumeric should remove invalid characters.");
            assertions.Add($"> PASS: FilterNumeric correctly transformed '{input}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterNumeric did not correctly transform '{input}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Integer Tests
    [Test]
    public void Test_IsValidInteger_ValidInteger()
    {
        // ARRANGE
        var assertions = new List<string>();
        string integerString = "123";

        try
        {
            // ACT
            bool result = InputValidator.IsValidInteger(integerString);

            // ASSERT
            Assert.That(result, Is.True, "Valid integer string should return true.");
            assertions.Add($"> PASS: Valid integer string '{integerString}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid integer string '{integerString}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidInteger_InvalidInteger()
    {
        // ARRANGE
        var assertions = new List<string>();
        string integerString = "12-3"; // Invalid due to misplaced dash

        try
        {
            // ACT
            bool result = InputValidator.IsValidInteger(integerString);

            // ASSERT
            Assert.That(result, Is.False, "Invalid integer string should return false.");
            assertions.Add($"> PASS: Invalid integer string '{integerString}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Invalid integer string '{integerString}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterInteger_RemovesInvalidCharsAndPlus()
    {
        // ARRANGE
        var assertions = new List<string>();
        string input = "+123";
        string expectedFiltered = "123"; // Plus should be removed by filter

        try
        {
            // ACT
            string filtered = InputValidator.FilterInteger(true, input);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterInteger should remove plus sign and invalid characters.");
            assertions.Add($"> PASS: FilterInteger correctly transformed '{input}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterInteger did not correctly transform '{input}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion

    #region Currency Tests
    [Test]
    public void Test_IsValidCurrency_ValidCurrency()
    {
        // ARRANGE
        var assertions = new List<string>();
        string currency = "$1,234.56";

        try
        {
            // ACT
            bool result = InputValidator.IsValidCurrency(currency);

            // ASSERT
            Assert.That(result, Is.True, "Valid currency string should return true.");
            assertions.Add($"> PASS: Valid currency string '{currency}' returned true.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Valid currency string '{currency}' did not return true.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_IsValidCurrency_InvalidCurrency()
    {
        // ARRANGE
        var assertions = new List<string>();
        // This currency string is invalid because it lacks proper grouping (expects 1-3 digits then comma groups)
        string currency = "1234.56";

        try
        {
            // ACT
            bool result = InputValidator.IsValidCurrency(currency);

            // ASSERT
            Assert.That(result, Is.False, "Invalid currency string should return false.");
            assertions.Add($"> PASS: Invalid currency string '{currency}' returned false.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: Invalid currency string '{currency}' did not return false as expected.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }

    [Test]
    public void Test_FilterCurrency_RemovesInvalidChars()
    {
        // ARRANGE
        var assertions = new List<string>();
        string input = "abc$1,234.56xyz";
        string expectedFiltered = "$1,234.56";

        try
        {
            // ACT
            string filtered = InputValidator.FilterCurrency(true, input);

            // ASSERT
            Assert.That(filtered, Is.EqualTo(expectedFiltered), "FilterCurrency should remove invalid characters.");
            assertions.Add($"> PASS: FilterCurrency correctly transformed '{input}' to '{filtered}'.");
            Assert.Pass($"Test passed\r\nResult List:\r\n{string.Join("\r\n", assertions)}");
        }
        catch (AssertionException ex)
        {
            assertions.Add($"> FAIL: FilterCurrency did not correctly transform '{input}'.");
            Assert.Fail($"Test failed\r\nResult List:\r\n{string.Join("\r\n", assertions)}\r\nException: {ex.Message}");
        }
    }
    #endregion
}
