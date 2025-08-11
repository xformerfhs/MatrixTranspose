
namespace LineEndingHandling {
   public static class LineEndingHandler {
      #region Public constants
      /// <summary>
      /// "Carriage Return" (CR) control character.
      /// </summary>
      public const char CarriageReturn = '\r';

      /// <summary>
      /// "Line Feed" (LF) control character.
      /// </summary>
      public const char LineFeed = '\n';

      /// <summary>
      /// "Next Line" (NL) control character. Used in EBCDIC texts.
      /// </summary>
      public const char NextLine = '\u0085';

      /// <summary>
      /// Enumeration of the different line ending options.
      /// </summary>
      public enum Option {
         Windows,
         Unix,
         OldMac,
         Ebcdic
      }
      #endregion
   }
}
