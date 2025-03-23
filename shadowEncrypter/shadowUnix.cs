using System;
using System.Collections.Generic;
using System.Linq;
using StringBuilder = System.Text.StringBuilder;

namespace ShadowUNIX
{
    public static class ShadowUnix
    {

        public static string LogError(string text)
        {
        return "Error: " + text;
        }

        public static char[] symbols = { '/', ',', '.', '}', '{', '|', '^', '+', ':', '?', '&', '%', '$', ';', '#', '@', '!', '№', '*', '(', ')', '-', '_', '=', '~', '`' };

        /// <summary>
        /// Encrypts the given plain text using the specified key.
        /// </summary>
        /// <param name="plainText">The plain text to encrypt.</param>
        /// <param name="key">The key used for encryption.</param>
        /// <returns>The encrypted text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either plainText or key is null.</exception>
        public static string EncryptText(string plainText, string key)
        {
            // Check if the plain text or key is null
            if (plainText == null)
            {
                throw new ArgumentNullException(nameof(plainText));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Convert each character in the plain text into its corresponding ASCII value
            var encryptedText = new int[plainText.Length];
            for (int i = 0; i < plainText.Length; i++)
            {
                encryptedText[i] = (int)plainText[i];
                // Duplicate line - remove the unnecessary assignment
            }

            // Split the key string into an array of operations
            var operations = key.Split('/');

            // Apply each operation to the encrypted text
            foreach (var operation in operations)
            {
                // Split the operation string into its arguments
                var operationArgs = operation
                    .Split(',', '(', ')', ' ').Where(arg => !string.IsNullOrEmpty(arg)).ToArray();

                // Check if the operation format is valid
                if (operationArgs.Length < 2)
                {
                    return "Invalid operation format";
                }

                // Apply the operation to the encrypted text
                if (operationArgs[1] == "all")
                {
                    for (int i = 0; i < encryptedText.Length; i++)
                    {
                        int localOperand = operationArgs.Length > 2 ? int.Parse(operationArgs[2]) : 0;
                        encryptedText[i] = CalculateResult(operationArgs[0], encryptedText[i], localOperand);
                    }
                    continue;
                }

                // Check if the index is valid
                if (!int.TryParse(operationArgs[1], out int index) || index < 0 || index >= plainText.Length)
                {
                    throw new ArgumentException("Invalid index");
                }

                // Apply the operation to the specific character in the encrypted text
                int operand = operationArgs.Length > 2 ? int.Parse(operationArgs[2]) : 0;
                encryptedText[index] = CalculateResult(operationArgs[0], encryptedText[index], operand);
            }

            // Convert the encrypted text back into a string
            var encryptedTextBuild = new StringBuilder();
            int lastRandomChar = -1;
            foreach (int encryptedChar in encryptedText)
            {
                int randomChar;
                do
                {
                    randomChar = GetRandomCharacter();
                } while (randomChar == lastRandomChar);
                lastRandomChar = randomChar;
                encryptedTextBuild.Append(encryptedChar).Append((char)randomChar);
            }

            return encryptedTextBuild.ToString();
        }

        /// <summary>
        /// Decrypts the given encrypted text using the specified key.
        /// </summary>
        /// <param name="encryptedText">The encrypted text to decrypt.</param>
        /// <param name="key">The key used for decryption.</param>
        /// <returns>The decrypted text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either encryptedText or key is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if the encrypted text is not in the correct format or if the operation format or index is invalid.</exception>
        public static string DecryptText(string encryptedText, string key)
        {
            // Check if the encrypted text or key is null or empty
            if (string.IsNullOrEmpty(encryptedText))
            {
                throw new ArgumentNullException(nameof(encryptedText));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Split the encrypted text into an array of integers
            string[] encryptedTextArray = encryptedText.Split(symbols).Where(arg => !string.IsNullOrEmpty(arg)).ToArray();
            int[] encryptedIntArray = new int[encryptedTextArray.Length];

            for (int i = 0; i < encryptedTextArray.Length; i++)
            {
                // Check if the encrypted text is in the correct format
                if (!int.TryParse(encryptedTextArray[i], out int encryptedChar))
                {
                    throw new ArgumentException("Invalid encrypted text");
                }
                encryptedIntArray[i] = int.Parse(encryptedTextArray[i]);
            }

            // Split the key string into an array of operations
            string[] operations = key.Split('/').Where(arg => !string.IsNullOrEmpty(arg)).ToArray();

            // Apply each operation in reverse order to the encrypted text
            for (int i = operations.Length - 1; i >= 0; i--)
            {
                // Split the operation string into its arguments
                string[] operationArgs = operations[i].Split(new[] { ',', '(', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                // Check if the operation format is valid
                if (operationArgs.Length < 2)
                {
                    throw new ArgumentException("Invalid operation format");
                }

                // Apply the operation to the encrypted text
                if (operationArgs[1] == "all")
                {
                    for (int k = 0; k < encryptedIntArray.Length; k++)
                    {
                        int localOperand = operationArgs.Length > 2 ? int.Parse(operationArgs[2]) : 0;
                        encryptedIntArray[k] = CalculateResult(ReverseOperation(operationArgs[0]), encryptedIntArray[k], localOperand);
                    }
                    continue;
                }

                // Check if the index is valid
                if (!int.TryParse(operationArgs[1], out int index) || index < 0 || index >= encryptedIntArray.Length)
                {
                    throw new ArgumentException("Invalid index");
                }

                int operand = operationArgs.Length > 2 ? int.Parse(operationArgs[2]) : 0;

                encryptedIntArray[index] = CalculateResult(ReverseOperation(operationArgs[0]), encryptedIntArray[index], operand);
            }

            // Convert the encrypted text back into a string
            string decryptedText = string.Empty;
            foreach (int encryptedChar in encryptedIntArray)
            {
                decryptedText += (char)encryptedChar;
            }

            return decryptedText;
        }
        
        /// <summary>
        /// Calculates the result of a mathematical operation on two operands.
        /// </summary>
        /// <param name="operation">The mathematical operation to perform.
        /// Valid options are "sum", "subtract", "multiply", "divide", "power", and "squareroot".</param>
        /// <param name="operand1">The first operand.</param>
        /// <param name="operand2">The second operand.</param>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="ArgumentException">Thrown if the operation is null or empty, or if the
        /// operation is not one of the valid options.</exception>
        /// <exception cref="DivideByZeroException">Thrown if the operation is "divide" and the second
        /// operand is zero.</exception>
        /// <exception cref="ArgumentException">Thrown if the operation is "squareroot" and the first
        /// operand is negative.</exception>
        private static int CalculateResult(string operation, int operand1, int operand2)
        {
            // Check if the operation is null or empty
            if (string.IsNullOrEmpty(operation))
            {
                throw new ArgumentException("Operation cannot be null or empty", nameof(operation));
            }

            // Perform the specified mathematical operation on the operands
            switch (operation.ToLowerInvariant())
            {
                // Sum
                case "sum":
                    return operand1 + operand2;

                // Subtraction
                case "subtract":
                    return operand1 - operand2;

                // Multiplication
                case "multiply":
                    return operand1 * operand2;

                // Division
                case "divide":
                    // Check if the second operand is zero
                    if (operand2 == 0)
                    {
                        throw new DivideByZeroException("Cannot divide by zero");
                    }
                    return operand1 / operand2;

                // Exponentiation
                case "power":
                    return (int)Math.Pow(operand1, operand2);

                // Square root
                case "squareroot":
                    // Check if the first operand is negative
                    if (operand1 < 0)
                    {
                        throw new ArgumentException("Cannot calculate square root of a negative number");
                    }
                    return (int)Math.Sqrt(operand1);

                // Unknown operation
                default:
                    throw new ArgumentException($"Unknown operation: {operation}", nameof(operation));
            }
        }

        /// <summary>
        /// Reverses the specified operation.
        /// </summary>
        /// <param name="operationName">The name of the operation to reverse.</param>
        /// <returns>The reversed operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the operationName is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the operationName is an unknown operation.</exception>
        private static string ReverseOperation(string operationName)
        {
            // Check if the operationName is null
            if (operationName is null)
            {
                throw new ArgumentNullException(nameof(operationName));
            }

            // Reverse the operationName using a switch statement
            // and return the reversed operation
            switch(operationName)
            {
                // Sum -> Decrement
                case "sum":
                    operationName = "subtract";
                    
                    break;
                // Decrement -> Sum
                case "subtract":
                    operationName = "sum";
                    break;
                // Multiply -> Divide
                case "multiply":
                    operationName = "divide";
                    break;
                // Divide -> Multiply
                case "divide":
                    operationName = "multiply";
                    break;
                // Power -> Square Root
                case "power":
                    operationName = "squareroot";
                    break;
                // Square Root -> Power
                case "squareroot":
                    operationName = "power";
                    break;
                // Unknown operation
                default:
                    throw new ArgumentException($"Unknown operation: {operationName}");
            }

            return operationName;
        }

        /// <summary>
        /// Generates a random character from the symbols array.
        /// </summary>
        /// <returns>A randomly generated character.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the symbols array is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the randomly generated index is out of bounds.</exception>
        public static char GetRandomCharacter()
        {
            // Validate that the symbols array is not null
            if (symbols == null)
            {
                throw new InvalidOperationException("The symbols array is null.");
            }
            

            // Generate a random index within the range of the symbols array
            Random random = new Random();
            int randomIndex = random.Next(0, symbols.Length);

            // Validate that the randomIndex is within the bounds of the symbols array
            if (randomIndex < 0 || randomIndex >= symbols.Length)
            {
                throw new InvalidOperationException($"The random index {randomIndex} is out of bounds. The symbols array has a length of {symbols.Length}.");
            }

            // Retrieve the character at the randomly generated index
            char randomCharacter = symbols[randomIndex];

            // Return the randomly generated character
            return randomCharacter;
        }

        /// <summary>
        /// Generates a random key by applying a series of random mathematical operations.
        /// </summary>
        /// <returns>A randomly generated key.</returns>
        public static string GenerateRandomKey(string text)
        {
            // Constant values for key generation
            const int MaxOperationCount = 30;
            int MaxOperand = text.Length - 1;
            const int MinOperand = 1;

            // Initialize the Random object
            var random = new Random();

            // Initialize the list to store the operations
            var operations = new List<string>();

            // Validate the Random object
            if (random == null)
            {
                throw new InvalidOperationException("The Random object is null.");
            }

            Console.WriteLine($"Number of operations: {MaxOperationCount}");

            // Generate the number of operations
            var operationCount = random.Next(1, MaxOperationCount + 1);

            // Log the actual number of operations
            Console.WriteLine($"Actual number of operations: {operationCount}");

            // Generate the operations
            for (var i = 0; i < operationCount; i++)
            {
                // Define the available operations
                var availableOperations = new List<string>
                {
                    "sum",
                    "multiply",
                    "subtract",
                    "divide",
                    "power",
                    "squareroot"
                };

                // Validate the availableOperations list
                if (availableOperations == null)
                {
                    throw new InvalidOperationException("The availableOperations list is null.");
                }

                // Choose a random operation
                var chosenOperation = availableOperations[random.Next(0, availableOperations.Count)];

                // Validate the chosenOperation
                if (chosenOperation == null)
                {
                    throw new InvalidOperationException("The chosenOperation is null.");
                }

                // Generate the operands
                var firstOperand = random.Next(MinOperand, MaxOperand + 1);
                var secondOperand = random.Next(MinOperand, MaxOperand + 1);

                // Create the operation string
                var operationString = $"{chosenOperation}({firstOperand},{secondOperand})";

                // Log the operation string
                Console.WriteLine($"Operation: {operationString}");

                // Validate the operationString
                if (operationString == null)
                {
                    throw new InvalidOperationException("The operationString is null.");
                }

                // Add the operation to the list
                operations.Add(operationString);
            }

            // Log the generated key
            Console.WriteLine($"Generated key: {string.Join("/", operations)}");

            // Validate the operations list
            if (operations == null)
            {
                throw new InvalidOperationException("The operations list is null.");
            }

            // Return the generated key
            return string.Join("/", operations);
        }
    }
}