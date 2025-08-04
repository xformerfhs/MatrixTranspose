using MatrixTranspose;
using TranspositionNS;

namespace SubstitutionAlphabetTest {
   [TestClass]
   public sealed class TranspositionTest {
      private const int PASSWORD_LOOP_COUNT = 100;
      private const int TRANSPOSITION_LOOP_COUNT = 21;
      private const int MIN_SIZE = 2;
      private const int MAX_SIZE = 100;
      private const int MIN_PASSWORDS = 1;
      private const int MAX_PASSWORDS = 10;
      private const int MIN_PASSWORD_LENGTH = 2;
      private const int MAX_PASSWORD_LENGTH = 20;

      private static readonly Random _random = new();

      [TestMethod]
      public void RandomTest() {
         for (int i = 0; i < PASSWORD_LOOP_COUNT; i++) {
            string[] passwords = MakeRandomPasswords();

            using (Transposition transposer = new(passwords))
               for (int j = 0; j < TRANSPOSITION_LOOP_COUNT; j++) {
                  char[] data = GenerateRandomData();

                  // Save data because it will be overwritten by Transpose.
                  char[] original = new char[data.Length];
                  Array.Copy(data, original, data.Length);

                  char[] transposed = transposer.Transpose(data, data.Length);
                  char[] untransposed = transposer.Untranspose(transposed, data.Length);
                  CollectionAssert.AreEqual(original, untransposed);
               }
         }
      }

      [TestMethod]
      public void KnownTranspositionTest() {
         using (Transposition transposer = new([@"quark"])) {
            char[] data = @"Bieberburzel".ToCharArray();
            char[] expected = @"euezBrebribl".ToCharArray();
            char[] transposed = transposer.Transpose(data, data.Length);
            CollectionAssert.AreEqual(expected, transposed);
         }
      }

      [TestMethod]
      public void NullPasswordsTest() {
         Assert.ThrowsException<ArgumentNullException>(() => new Transposition(null));
      }

      [TestMethod]
      public void EmptyPasswordsTest() {
         Assert.ThrowsException<ArgumentException>(() => new Transposition([]));
      }

      [TestMethod]
      public void ShortPasswordTest() {
         try {
            new Transposition([@"P1P2", @"A", @"Bla"]);
         } catch (ArgumentException ex) {
            Assert.IsTrue(ex.Message.StartsWith(@"2. "));
         } catch (Exception ex) {
            Assert.Fail($"Unexpected exception type: {ex.GetType().Name}");
         }
      }

      private static char[] GenerateRandomData() {
         int count = _random.Next(MIN_SIZE, MAX_SIZE+1);
         char[] data = new char[count];
         for (int i = 0; i < count; i++)
            data[i] = (char)_random.Next(32, 127); // Printable ASCII characters

         return data;
      }

      private static string[] MakeRandomPasswords() {
         int numPasswords = _random.Next(MIN_PASSWORDS, MAX_PASSWORDS + 1);
         string[] result = new string[numPasswords];

         for (int i = 0; i < numPasswords; i++) {
            int passwordLength = _random.Next(MIN_PASSWORD_LENGTH, MAX_PASSWORD_LENGTH + 1);
            result[i] = new string(GenerateRandomPassword(passwordLength));
         }

         return result;
      }

      private static char[] GenerateRandomPassword(int length) {
         char[] data = new char[length];
         for (int i = 0; i < length; i++)
            data[i] = (char)_random.Next(97, 123); // Lower case characters

         return data;
      }
   }
}
