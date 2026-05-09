// Функция для вычисления расстояния Левенштейна
static int DistOfDL(string word_1, string word_2, bool verbose)
{
    int len_1 = word_1.Length, len_2 = word_2.Length;
    int[,] matrix = new int[len_1 + 1, len_2 + 1];
    for (int i = 0; i <= len_1; i++)
    {
        for (int j = 0; j <= len_2; j++)
        {
            if (i == 0) { matrix[i, j] = j; continue; }
            if (j == 0) { matrix[i, j] = i; continue; }

            int check = (word_1.Substring(i - 1, 1) == word_2.Substring(j - 1, 1) ? 0 : 1);

            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + check);

            // Проверка на возможность перестановки
            if (i > 1 && j > 1 &&
                word_1.Substring(i - 1, 1) == word_2.Substring(j - 2, 1) &&
                word_1.Substring(i - 2, 1) == word_2.Substring(j - 1, 1))
            {

                matrix[i, j] = Math.Min(
                    matrix[i, j],
                    matrix[i - 2, j - 2] + check);
            }
        }
    }
    if (verbose)
    {
        for (int i = 0; i <= len_1; i++)
        {
            for (int j = 0; j <= len_2; j++)
            {

                Console.Write(matrix[i, j]);
            }
            Console.Write('\n');
        }
    }
    return matrix[len_1, len_2];
}
Console.WriteLine("Вас приветствует программа по нахождению расстояния Дамерау-Левенштейна");
Console.Write("Показывать промежуточную матрицу вычислений по алгоритму Вагнера-Фишера? (да/нет): ");
bool verbose = (Console.ReadLine() == "да" ? true : false);
while (true)
{
    Console.Write("Введите первое слово или exit: ");
    string word_1 = Console.ReadLine();
    if (word_1 == "") { Console.WriteLine("Слово не может быть пустым!"); continue; }
    if (word_1 == "exit") break;
    Console.Write("Введите второе слово: ");
    string word_2 = Console.ReadLine();
    if (word_2 == "") { Console.WriteLine("Слово не может быть пустым!"); continue; }
    Console.WriteLine($"Расстояние Дамерау-Левенштейна: {DistOfDL(word_1, word_2, verbose)}");
}