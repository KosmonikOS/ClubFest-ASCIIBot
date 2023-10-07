namespace ClubFestBotCSharp
{
    internal static class Messages
    {
        public const string StartMessage = "Hello, this is an ASCII converter bot.s \n" +
        "To convert your photo send it with a caption in the following format: \n" +
        "Font size Resolution\n" +
        "Note that both arguments should be positive integers and" +
        "font size * resolution <= 1000";
        public const string InvalidArgumentsMessage = "Invalid input values";
        public const string TooLargeInputMessage = "Try another font size or resolution";
    }

}
