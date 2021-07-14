using System;
using System.IO;
using System.Media;
using System.Collections.Generic;
namespace Snake
{
    class Program
    {
        static string playersName = "";
        static string message = "";     //for writing a reason of a fail

        static int[,] snake = new int[3090, 3090];  //program uses amount of elements in the array that equals current bodyLength while playing
        static int currentX = 20, currentY = 20;        //coords of snake's head
        static int bodyLength = 0;
        static int initCurrentX = 20;   //snake's head
        static int initCurrentY = 20;
        static int initX = 20;          //snake's body
        static int initY = 21;
        static ConsoleColor initStandardSnakeColor = ConsoleColor.Green;    //standard color for the snake
        static int initStandardSnakePauseTime = 180;                        //standard pause time for the snake
        static ConsoleColor snakeColor;    
        static int snakePauseTime;

        static ConsoleKeyInfo currentKey, prevKey;  //keys for snake directing, showing menu

        //for snake moving:
        static int prevX = 0, prevY = 0;
        static int moveX = 0, moveY = 0;
        static int saveLastX, saveLastY;   //for keeping coords of a tail in case snake starts to grow

        static bool isGameContinue = true;

        static bool isCloseSettings = false;
        static bool isQuitMenu = false;
        static bool isQuitTheProgram = false;
        static bool isSnakeCannotTurn;

        static int appleX = 0, appleY = 0;
        static int Points = 0;
        static bool isAppleEaten = false;

        static bool isSnakeBumpedInto = false;
        static bool isSnakeAteItself = false;

        static string saveGameFile = "snake_game.sav";
        static bool isSaveGameFileExist;
        static string recordsTableFile = "table_records.log";
        static bool isRecordsTableFileExist;
        static int amountOfFieldsInRecordsTable;    //zero will be in this variable before to start counting fields in record's table

        static SoundPlayer playMenuMusic = new SoundPlayer(@"music\MenuMusic.wav");
        static SoundPlayer playEatingAppleMusic = new SoundPlayer(@"music\EatingApple.wav");
        static SoundPlayer playGameOverMusic = new SoundPlayer(@"music\Gameover.wav");
        static SoundPlayer playQuitGameMusic = new SoundPlayer(@"music\QuitGame.wav");
        static SoundPlayer playNewRecordMusic = new SoundPlayer(@"music\NewRecord.wav");
        static bool isSoundsOn = true;

        //for writing in record's table
        static List<string> listNames;
        static List<int> listPoints;
        static List<string> listDates;
        static List<string> listTime;

        //for getting data from record's table
        static string[] arrayNames, arrayDates, arrayTime;
        static int[] arrayPoints;

        static void Main(string[] args)
        {
            Console.Title = "Snake";
            Console.CursorVisible = false;
            playMenuMusic.PlayLooping();
            Intro();
            getCoordsOfApple(ref appleX, ref appleY);
            if (checkFileExisting(saveGameFile))
                getSnakeColorAndTimePauseFromSaveFile();
            showMenu();
            if (!isQuitTheProgram)
                protectGameFromTheDurak();
            while (!isQuitTheProgram)
            {
                while (isGameContinue)
                {
                    DirectSnakeOrQuitGame();
                    if (currentKey.Key == ConsoleKey.F5)
                    {
                        if (!isSnakeAteItself && !isSnakeBumpedInto)
                            saveGame();                
                        if (isSoundsOn)
                            playMenuMusic.PlayLooping();
                        showMenu();
                        if (isQuitTheProgram)   //if I pressed Escape button when I was in menu mode
                            break;
                        protectGameFromTheDurak();
                        DirectSnakeOrQuitGame();
                    }
                    prevKey = currentKey;
                    isSnakeCannotTurn = true;
                    do
                    {
                        while (!Console.KeyAvailable && isGameContinue)
                        {
                            Console.ForegroundColor = snakeColor;
                            if (currentX >= 0 && currentX < 120)
                                Console.SetCursorPosition(currentX, currentY);
                            Console.Write("o");
                            if (moveY != 0) //without this condition snake "accelerates" while moving up or down
                                System.Threading.Thread.Sleep(snakePauseTime + 60); //it's because of a text format in console apps in Windows
                            else
                                System.Threading.Thread.Sleep(snakePauseTime);
                            if (currentX >= 0 && currentX < 120)
                                Console.SetCursorPosition(currentX, currentY);
                            Console.Write(" ");
                            prevX = currentX;
                            prevY = currentY;
                            Console.SetCursorPosition(snake[bodyLength - 1, 0], snake[bodyLength - 1, 1]);
                            Console.Write(" ");
                            saveLastX = snake[bodyLength - 1, 0];
                            saveLastY = snake[bodyLength - 1, 1];
                            currentX += moveX;
                            currentY += moveY;
                            for (int i = 0; i <= bodyLength - 1; i++)               //we check either snake eats itself or not.
                                if (currentX == snake[i, 0] && currentY == snake[i, 1])
                                {
                                    isGameContinue = false;
                                    isSnakeAteItself = true;
                                    message = "ЗМЕЙКА СЕБЯ УКУСИЛА";
                                    break;
                                }
                                else
                                    isSnakeAteItself = false;       
                            if (currentX < 0 || currentX > 119 || currentY <= 1 || currentY >= 27)       //we check either snake 
                            {                                                                       //bumps into border or not
                                isGameContinue = false;
                                isSnakeBumpedInto = true;
                                message = "ЗМЕЙКА ВРЕЗАЛАСЬ В СТЕНУ";
                            }
                            else
                                isSnakeBumpedInto = false;
                            for (int i = bodyLength - 1; i > 0; i--)
                            {
                                for (int j = 0; j <= 1; j++)
                                {
                                    snake[i, j] = snake[i - 1, j];

                                }
                                Console.SetCursorPosition(snake[i, 0], snake[i, 1]);
                                Console.Write("*");
                            }
                            snake[0, 0] = prevX;
                            snake[0, 1] = prevY;

                            if (currentX >= 0 && currentX < 120)
                                Console.SetCursorPosition(prevX, prevY);
                            Console.Write("*");


                            if (isAppleEaten)
                            {
                                getCoordsOfApple(ref appleX, ref appleY);
                                isAppleEaten = false;
                            }
                            drawApple();
                            if ((currentX == appleX) && (currentY == appleY))
                            {
                                isAppleEaten = true;
                                Points++;
                                if (isSoundsOn)
                                    playEatingAppleMusic.Play();
                                Console.SetCursorPosition(0, 0);
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write("Количество очков: ");
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.Write(Points);
                                bodyLength++;
                                snake[bodyLength - 1, 0] = saveLastX;
                                snake[bodyLength - 1, 1] = saveLastY;
                            }
                        }
                        if (isGameContinue)
                            currentKey = Console.ReadKey(true);

                        LetOrForbidSnakeToTurn();
                        if (currentKey.Key == ConsoleKey.Escape)
                            isSnakeCannotTurn = false;              //to break this cycle
                        if (currentKey.Key == ConsoleKey.F5)
                            isSnakeCannotTurn = false;              //to break this cycle
                    } while (isSnakeCannotTurn && isGameContinue);
                    
                }
                if (isQuitTheProgram)   //if I pressed Escape button when I was in menu mode
                    break;
                Console.Clear();
                if (isSoundsOn)
                {
                    if (Points > arrayPoints[0])
                        playNewRecordMusic.Play();
                    else if (isSnakeAteItself || isSnakeBumpedInto)
                        playGameOverMusic.Play();
                    else
                        playQuitGameMusic.Play();
                }
                if (Points > arrayPoints[0])
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\t\t\t\t\t      ВЫ УСТАНОВИЛИ НОВЫЙ РЕКОРД!");
                }
                if (isSnakeAteItself || isSnakeBumpedInto)
                {
                    File.Delete(saveGameFile); //we delete save game file if we want to create save game file with standard data
                    GameOver();                 //and set new game mode
                    snakeColor = initStandardSnakeColor;
                    snakePauseTime = initStandardSnakePauseTime;
                    if (Points > arrayPoints[0])
                    {
                        saveRecordInTable();
                        writeDataFromTableToArray();
                        sortDataFromRecordsTable();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(52, 2);
                    Console.WriteLine("Набрано очков: " + Points);
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nНажмите клавишу F5 для выхода в меню или любую клавишу для выхода (или просто закройте игру)");
                currentKey = Console.ReadKey(true);
                if (currentKey.Key != ConsoleKey.F5)
                {
                    isQuitTheProgram = true;
                }
                else
                {
                    isGameContinue = true;
                    isQuitTheProgram = false;
                }
            }
        }
        public static void protectGameFromTheDurak()   //Game starts only when player presses next keys: w, s, a, d, arrows and
        {                                               //numbers on NumPad when Num Lock is on
            do
            {
                currentKey = Console.ReadKey(true);
            } while (currentKey.Key != ConsoleKey.W && currentKey.Key != ConsoleKey.A && currentKey.Key != ConsoleKey.S &&
                    currentKey.Key != ConsoleKey.D && currentKey.Key != ConsoleKey.UpArrow && currentKey.Key != ConsoleKey.DownArrow
                    && currentKey.Key != ConsoleKey.LeftArrow && currentKey.Key != ConsoleKey.RightArrow &&
                        currentKey.Key != ConsoleKey.Escape);
        }
        public static void DirectSnakeOrQuitGame()
        {
            switch (currentKey.Key)
            {
                case ConsoleKey.W:
                    moveX = 0;
                    moveY = -1;
                    break;
                case ConsoleKey.A:
                    moveX = -1;
                    moveY = 0;
                    break;
                case ConsoleKey.S:
                    moveX = 0;
                    moveY = 1;
                    break;
                case ConsoleKey.D:
                    moveX = 1;
                    moveY = 0;
                    break;
                case ConsoleKey.Escape:
                    isGameContinue = false;
                    saveGame();
                    break;
                case ConsoleKey.UpArrow:
                    moveX = 0;
                    moveY = -1;
                    break;
                case ConsoleKey.LeftArrow:
                    moveX = -1;
                    moveY = 0;
                    break;
                case ConsoleKey.DownArrow:
                    moveX = 0;
                    moveY = 1;
                    break;
                case ConsoleKey.RightArrow:
                    moveX = 1;
                    moveY = 0;
                    break;
            }
        }
        public static void LetOrForbidSnakeToTurn() //i don't let snake go in backward direction
        {
            if ((prevKey.Key == ConsoleKey.W && (currentKey.Key == ConsoleKey.A || currentKey.Key == ConsoleKey.D)) ||
                        (prevKey.Key == ConsoleKey.UpArrow && (currentKey.Key == ConsoleKey.LeftArrow || currentKey.Key == ConsoleKey.RightArrow)) ||
                        (prevKey.Key == ConsoleKey.W && (currentKey.Key == ConsoleKey.LeftArrow || currentKey.Key == ConsoleKey.RightArrow)) ||
                        (prevKey.Key == ConsoleKey.UpArrow && (currentKey.Key == ConsoleKey.A || currentKey.Key == ConsoleKey.D)))
                isSnakeCannotTurn = false;
            if ((prevKey.Key == ConsoleKey.S && (currentKey.Key == ConsoleKey.A || currentKey.Key == ConsoleKey.D)) ||
                (prevKey.Key == ConsoleKey.DownArrow && (currentKey.Key == ConsoleKey.LeftArrow || currentKey.Key == ConsoleKey.RightArrow)) ||
                (prevKey.Key == ConsoleKey.S && (currentKey.Key == ConsoleKey.LeftArrow || currentKey.Key == ConsoleKey.RightArrow)) ||
                (prevKey.Key == ConsoleKey.DownArrow && (currentKey.Key == ConsoleKey.A || currentKey.Key == ConsoleKey.D)))
                isSnakeCannotTurn = false;
            if ((prevKey.Key == ConsoleKey.A && (currentKey.Key == ConsoleKey.W || currentKey.Key == ConsoleKey.S)) ||
                (prevKey.Key == ConsoleKey.LeftArrow && (currentKey.Key == ConsoleKey.UpArrow || currentKey.Key == ConsoleKey.DownArrow)) ||
                (prevKey.Key == ConsoleKey.LeftArrow && (currentKey.Key == ConsoleKey.W || currentKey.Key == ConsoleKey.S)) ||
                (prevKey.Key == ConsoleKey.A && (currentKey.Key == ConsoleKey.UpArrow || currentKey.Key == ConsoleKey.DownArrow)))
                isSnakeCannotTurn = false;
            if ((prevKey.Key == ConsoleKey.D && (currentKey.Key == ConsoleKey.W || currentKey.Key == ConsoleKey.S)) ||
                (prevKey.Key == ConsoleKey.RightArrow && (currentKey.Key == ConsoleKey.UpArrow || currentKey.Key == ConsoleKey.DownArrow)) ||
                (prevKey.Key == ConsoleKey.D && (currentKey.Key == ConsoleKey.UpArrow || currentKey.Key == ConsoleKey.DownArrow)) ||
                (prevKey.Key == ConsoleKey.RightArrow && (currentKey.Key == ConsoleKey.W || currentKey.Key == ConsoleKey.S)))
                isSnakeCannotTurn = false;
        }
        public static void saveRecordInTable()
        {
            StreamWriter fileRecordTable = new StreamWriter(recordsTableFile, true);
            fileRecordTable.WriteLine(playersName + ' ' + Points + ' ' + DateTime.Now);
            fileRecordTable.Close();
        }
        public static void getCoordsOfApple(ref int x, ref int y)
        {
            Random rndX = new Random();
            Random rndY = new Random();
            int i = 0;
            x = rndX.Next(1, 120);
            y = rndY.Next(3, 26);
            do
            {
                if ((x != currentX) && (y != currentY) && (x != snake[i, 0]) && (y != snake[i, 1]))
                    i++;
                else     
                {
                    i = 0;              //it guarantees that apple won't be drawn on the snake
                    x = rndX.Next(1, 120);
                    y = rndY.Next(3, 26);
                }
            } while (i < bodyLength - 1);
        }
        public static void repeatSymbol(char symbol, int amount)
        {
            for (int i = 1; i <= amount; i++)
                Console.Write(symbol);
        }
        public static void changeSnakeColor()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1. Зеленый (стандартный)");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("2. Красный");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("3. Голубой");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("4. Синий");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("5. Желтый");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("6. Темно-желтый");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("7. Пурпурный");
            Console.ResetColor();
            Console.WriteLine("8. Вернуться в настройки");
            Console.WriteLine("9. Вернуться в меню");
            ConsoleKeyInfo option;
            option = Console.ReadKey(true);
            switch (option.Key)
            {
                case ConsoleKey.D1:
                    snakeColor = ConsoleColor.Green;
                    break;
                case ConsoleKey.D2:
                    snakeColor = ConsoleColor.Red;
                    break;
                case ConsoleKey.D3:
                    snakeColor = ConsoleColor.Cyan;
                    break;
                case ConsoleKey.D4:
                    snakeColor = ConsoleColor.Blue;
                    break;
                case ConsoleKey.D5:
                    snakeColor = ConsoleColor.Yellow;
                    break;
                case ConsoleKey.D6:
                    snakeColor = ConsoleColor.DarkYellow;
                    break;
                case ConsoleKey.D7:
                    snakeColor = ConsoleColor.Magenta;
                    break;
                case ConsoleKey.D8:
                    break;
                case ConsoleKey.D9:
                    isCloseSettings = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Указанный вами номер цвета не найден");
                    Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
                    Console.ResetColor();
                    Console.ReadKey(true);
                    isCloseSettings = true;
                    break;
            }
        }
        public static void changeSnakeSpeed()
        {
            Console.Clear();
            Console.WriteLine("1. Очень медленно");
            Console.WriteLine("2. Медленно");
            Console.WriteLine("3. Нормально");
            Console.WriteLine("4. Быстро");
            Console.WriteLine("5. Очень быстро");
            Console.WriteLine("6. Вернуться в настройки");
            Console.WriteLine("7. Вернуться в меню");
            ConsoleKeyInfo option;
            option = Console.ReadKey(true);
            switch (option.Key)
            {
                case ConsoleKey.D1:
                    snakePauseTime = 500;
                    break;
                case ConsoleKey.D2:
                    snakePauseTime = 400;
                    break;
                case ConsoleKey.D3:
                    snakePauseTime = 270;
                    break;
                case ConsoleKey.D4:
                    snakePauseTime = 180;
                    break;
                case ConsoleKey.D5:
                    snakePauseTime = 80;
                    break;
                case ConsoleKey.D6:
                    break;
                case ConsoleKey.D7:
                    isCloseSettings = true;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Указанная вами скорость змейки не найдена");
                    Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
                    Console.ResetColor();
                    Console.ReadKey(true);
                    isCloseSettings = true;
                    break;
            }
        }
        public static bool isPlayersNameConsistsOnlyOfSpaces(in string name)
        {
            int i = 0, countSpaces = 0;
            while (i < name.Length)
            {
                if (name[i] == ' ')
                    countSpaces++;
                i++;
            }
            return (countSpaces == (i));
        }
        public static void writeNewPlayersName()    //program doesn't accept an empty name or name with spaces
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(25, 1);
            bool isPlayersNameIsSpaces = false;
            Console.WriteLine("ВВЕДИТЕ ИМЯ ИГРОКА И НАЖМИТЕ Enter (ПРОБЕЛЫ В ИМЕНИ БУДУТ УДАЛЕНЫ)");
            string tempName = "";
            do
            {
                Console.Write(">");
                tempName = Console.ReadLine();
                isPlayersNameIsSpaces = isPlayersNameConsistsOnlyOfSpaces(in tempName);
            } while ((tempName == "") || isPlayersNameIsSpaces);
            int PositionOfSpace = tempName.IndexOf(' '); ;
            while (PositionOfSpace != -1)
            {
                playersName = tempName.Remove(PositionOfSpace, 1);
                tempName = playersName;
                PositionOfSpace = tempName.IndexOf(' ');
            }
            playersName = tempName;
        }
        public static void changeSoundSettings()
        {
            Console.Clear();
            Console.WriteLine("0. Выключить звуки");
            Console.WriteLine("1. Включить звуки");
            Console.WriteLine("2. Вернуться в настройки");
            Console.WriteLine("3. Вернуться в меню");
            ConsoleKeyInfo option = Console.ReadKey(true);
            switch (option.Key)
            {
                case ConsoleKey.D0:
                    playMenuMusic.Stop();
                    isSoundsOn = false;
                    break;
                case ConsoleKey.D1:
                    playMenuMusic.PlayLooping();
                    isSoundsOn = true;
                    break;
                case ConsoleKey.D2:
                    break;
                case ConsoleKey.D3:
                    isCloseSettings = true;
                    break;
                default:
                    Console.WriteLine("Указанная вами опция не найдена");
                    Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
                    Console.ReadKey(true);
                    isCloseSettings = true;
                    break;
            }
        }
        public static void showFAQ()
        {
            Console.Clear();
            Console.WriteLine("В игре змея должна съесть как можно больше яблок, при этом не укусив себя и не врезавшись в стену.");
            Console.WriteLine("Для управления змеей воспользуйтесь стрелками или клавишами w, s, a, d (независимо от раскладки).");
            Console.WriteLine("В игру могут играть, как взрослые и дети, так и домашние питомцы (если сможете их научить играть).");
            Console.WriteLine("Захватывающего экшона и лихого сюжета здесь вы не встретите, хотя игра и имеет некоторую долю саспенса.");
            Console.WriteLine("ИМХО. Игра 'Змейка' отлично подходит для убийства времени.");
            Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
            Console.ReadKey(true);
        }
        public static void showSettings()
        {
            ConsoleKeyInfo furtherChoice;
            while (!isCloseSettings)
            {
                Console.Clear();
                Console.WriteLine("1. Выбрать цвет змейки");
                Console.WriteLine("2. Выбрать скорость змейки");
                Console.WriteLine("3. Настройки звука");
                Console.WriteLine("4. Вернуться в меню");
                furtherChoice = Console.ReadKey(true);
                switch (furtherChoice.Key)
                {
                    case ConsoleKey.D1:
                        changeSnakeColor();
                        break;
                    case ConsoleKey.D2:
                        changeSnakeSpeed();
                        break;
                    case ConsoleKey.D3:
                        changeSoundSettings();
                        break;
                    case ConsoleKey.D4:
                        isCloseSettings = true;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Выбранный вами вариант отсутствует");
                        Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
                        Console.ResetColor();
                        Console.ReadKey(true);
                        isCloseSettings = true;
                        break;
                }
            }
            isCloseSettings = false;
            if (checkFileExisting(saveGameFile))
                rewriteSaveFile();
        }
        public static void showMenu()
        {
            isQuitMenu = false;
            isSaveGameFileExist = checkFileExisting(saveGameFile);
            if (!isSaveGameFileExist)
            {
                snakeColor = initStandardSnakeColor;
                snakePauseTime = initStandardSnakePauseTime;
            }
            isRecordsTableFileExist = checkFileExisting(recordsTableFile);
            if (isRecordsTableFileExist)
            {
                writeDataFromTableToArray();
                sortDataFromRecordsTable();
            }
            else
            {
                StreamWriter sW = new StreamWriter(recordsTableFile, false);
                sW.Close();
                //this is a KOSTYL
                arrayPoints = new int[1] { -1 };    //if there are no records in records table file. 
            }                           //The first record will be written in records table file even amount of points equals to zero
            ConsoleKeyInfo firstChoice;
            while (!isQuitMenu)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                repeatSymbol('-', 120);
                repeatSymbol(' ', 54);
                Console.WriteLine("Главное меню");
                repeatSymbol('-', 120);
                Console.ResetColor();
                Console.WriteLine("1. Новая игра");
                if (isSaveGameFileExist)
                    {
                    Console.WriteLine("2. Продолжить игру");
                    Console.WriteLine("3. Настройки");
                    Console.WriteLine("4. Справка");
                    Console.WriteLine("5. Показать таблицу рекордов");
                }
                else
                {
                    Console.WriteLine("2. Настройки");
                    Console.WriteLine("3. Справка");
                    Console.WriteLine("4. Показать таблицу рекордов");
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Esc - Завершить игру");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Нажмите на соответствующую клавишу");
                Console.ResetColor();
                firstChoice = Console.ReadKey(true);
                if (firstChoice.Key == ConsoleKey.D1)
                {
                    if (isSaveGameFileExist)
                    {
                        getDataFromSaveFile();
                        if (arrayPoints.Length == 0)
                            arrayPoints = new int[1] { -1 };
                        if (Points > arrayPoints[0])
                        {
                            saveRecordInTable();
                            writeDataFromTableToArray();
                            sortDataFromRecordsTable();
                        }
                        File.Delete(saveGameFile);  //there's no reason to keep a save file because we started a new game
                    }
                    writeNewPlayersName();
                    InitNewGame();
                    getDataFromSaveFile();
                    isQuitMenu = true;
                    playMenuMusic.Stop();
                }
                if (isSaveGameFileExist)
                    {
                    switch (firstChoice.Key)
                    {
                        case ConsoleKey.D2:
                            rewriteSaveFile();
                            getDataFromSaveFile();
                            isQuitMenu = true;
                            playMenuMusic.Stop();
                            break;
                        case ConsoleKey.D3:
                            showSettings();
                            break;
                        case ConsoleKey.D4:
                            showFAQ();
                            break;
                        case ConsoleKey.D5:
                            showRecordsTable();
                            break;
                    }
                }
                else
                {
                    switch (firstChoice.Key)
                    {
                        case ConsoleKey.D2:
                            showSettings();
                            break;
                        case ConsoleKey.D3:
                            showFAQ();
                            break;
                        case ConsoleKey.D4:
                            showRecordsTable();
                            break;
                    }
                }
                if (firstChoice.Key == ConsoleKey.Escape)
                {
                    isQuitMenu = true;
                    isQuitTheProgram = true;
                    if (checkFileExisting(saveGameFile))
                        rewriteSaveFile();
                    playMenuMusic.Stop();
                }
            }
            DrawBackground();
            if (!isQuitTheProgram)   //in case snake bumped into a border, user entered menu mode and pressed escape button
                drawSnake();        //without this condition program catches an exception if currentX < 0 or currentX > 119
            drawApple();
        }
        public static void DrawBackground()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Количество очков: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(Points);
            Console.SetCursorPosition(103, 0);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("F5 - выход в меню");
            Console.ResetColor();
            Console.SetCursorPosition(0, 1);
            repeatSymbol('-', 120);
            Console.SetCursorPosition(0, 27);
            repeatSymbol('-', 120);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(0, 28);
            Console.Write("ИГРОК: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(playersName);
            Console.SetCursorPosition(86, 28);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Esc - завершение и сохранение игры");
        }
        public static bool checkFileExisting(in string pathToFile)
        {
            try
            {
                StreamReader sr = new StreamReader(pathToFile);
                sr.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static void getDataFromSaveFile()
        {
            StreamReader saveFile = new StreamReader(saveGameFile);
            string coord = "";
            bodyLength = 0;
            coord = saveFile.ReadLine();
            appleX = Convert.ToInt32(coord);
            coord = saveFile.ReadLine();
            appleY = Convert.ToInt32(coord);
            coord = saveFile.ReadLine();
            Points = Convert.ToInt32(coord);
            playersName = saveFile.ReadLine();
            snakeColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), saveFile.ReadLine()); //it's a very very cool example of type casting!
                                                                                              //I'm not a show-off it's just for me :)
            snakePauseTime = int.Parse(saveFile.ReadLine());                                    
            coord = saveFile.ReadLine();
            currentX = Convert.ToInt32(coord); // x coord of the head
            coord = saveFile.ReadLine();
            currentY = Convert.ToInt32(coord); //y coord of the head
            //coords of the snake's body
            while ((coord = saveFile.ReadLine()) != null)
            {
                snake[bodyLength, 0] = Convert.ToInt32(coord);
                snake[bodyLength, 1] = Convert.ToInt32(saveFile.ReadLine());
                bodyLength++;
            }
            saveFile.Close();
        }
        public static void rewriteSaveFile()    //for saving snake's color and snake's speed if player changed them in settings
        {
            StreamReader saveFile = new StreamReader(saveGameFile);
            int countLinesInSaveFile = 0;
            string line = "";
            while ((line = saveFile.ReadLine()) != null) countLinesInSaveFile++;
            saveFile.Close();
            string[] arrayOfLinesInSaveFile = new string[countLinesInSaveFile];
            saveFile = new StreamReader(saveGameFile);
            for (int i = 0; i < countLinesInSaveFile; i++)
                arrayOfLinesInSaveFile[i] = saveFile.ReadLine();
            saveFile.Close();
            StreamWriter changedSaveFile = new StreamWriter(saveGameFile, false);
            for (int i = 0; i <= 3; i++)
                changedSaveFile.WriteLine(arrayOfLinesInSaveFile[i]);
            changedSaveFile.WriteLine(snakeColor);
            changedSaveFile.WriteLine(snakePauseTime);
            for (int i = 6; i < countLinesInSaveFile; i++)
                changedSaveFile.WriteLine(arrayOfLinesInSaveFile[i]);
            changedSaveFile.Close();
        }
        public static void getSnakeColorAndTimePauseFromSaveFile()  //for coloring snake if player picked snake's color in a previous game
        {
            StreamReader saveFile = new StreamReader(saveGameFile);
            for (int i = 0; i < 4; i++)
                saveFile.ReadLine();
            snakeColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), saveFile.ReadLine());
            snakePauseTime = int.Parse(saveFile.ReadLine());
            saveFile.Close();
        }
        public static void drawSnake()
        {
            Console.ForegroundColor = snakeColor;
            Console.SetCursorPosition(currentX, currentY);
            Console.Write("o");
            for (int i = 0; i <= bodyLength - 1; i++)
            {
                Console.SetCursorPosition(snake[i, 0], snake[i, 1]);
                Console.Write("*");
            }
        }
        public static void drawApple()
        {
            Console.SetCursorPosition(appleX, appleY);  //food for our snake
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("@");
            Console.ResetColor();
        }
        public static void InitNewGame()
        {
            StreamWriter saveFile = new StreamWriter(saveGameFile, false); //to clear a save file
            saveFile.Close();
            saveFile = new StreamWriter(saveGameFile, true);
            saveFile.WriteLine(appleX);
            saveFile.WriteLine(appleY);
            saveFile.WriteLine(0);    //amount of points
            saveFile.WriteLine(playersName);
            saveFile.WriteLine(snakeColor);
            saveFile.WriteLine(snakePauseTime);
            saveFile.WriteLine(initCurrentX); //x and y positions of the head
            saveFile.WriteLine(initCurrentY);
            saveFile.WriteLine(initX);        //x and y positions of the body
            saveFile.WriteLine(initY);
            saveFile.Close();
        }
        public static void saveGame()
        {
            StreamWriter saveFile = new StreamWriter(saveGameFile, false);  //to clear a save file
            saveFile.Close();
            saveFile = new StreamWriter(saveGameFile, true);
            saveFile.WriteLine(appleX);
            saveFile.WriteLine(appleY);
            saveFile.WriteLine(Points);
            saveFile.WriteLine(playersName);
            saveFile.WriteLine(snakeColor);
            saveFile.WriteLine(snakePauseTime);
            saveFile.WriteLine(currentX);
            saveFile.WriteLine(currentY);
            for (int i = 0; i <= bodyLength - 1; i++)
            {
                saveFile.WriteLine(snake[i, 0]);
                saveFile.WriteLine(snake[i, 1]);
            }
            saveFile.Close();
        }
        public static void Intro()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(40, 10);
            Console.WriteLine("  _____   _   _              _  __  ______ ");
            Console.SetCursorPosition(40, 11);
            Console.WriteLine(" / ____| | \\ | |     /\\     | |/ / |  ____|");
            Console.SetCursorPosition(40, 12);
            Console.WriteLine("| (___   |  \\| |    /  \\    | ' /  | |__   ");
            Console.SetCursorPosition(40, 13);
            Console.WriteLine(" \\___ \\  | . ` |   / /\\ \\   |  <   |  __|  ");
            Console.SetCursorPosition(40, 14);
            Console.WriteLine(" ____) | | |\\  |  / ____ \\  | . \\  | |____ ");
            Console.SetCursorPosition(40, 15);
            Console.WriteLine("|_____/  |_| \\_| /_/    \\_\\ |_|\\_\\ |______|");
            System.Threading.Thread.Sleep(100);
            int i, k;
            for (i = 0; i < 120; i++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(i, 16);
                Console.WriteLine("   o");
                Console.SetCursorPosition(i, 17);
                Console.WriteLine("  /|\\/");
                Console.SetCursorPosition(i, 18);
                Console.WriteLine("  \\|");
                Console.SetCursorPosition(i, 19);
                Console.WriteLine("  / \\");
                Console.SetCursorPosition(i, 20);
                Console.WriteLine(" /  /");
                if (i >= 60)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(i - 60, 20);
                    Console.WriteLine("*");
                    if (i > 85)
                    {
                        Console.SetCursorPosition(0, 16);
                        Console.WriteLine("    ");
                        Console.SetCursorPosition(0, 17);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(0, 18);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(0, 19);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(0, 20);
                        Console.WriteLine("     ");
                        Console.SetCursorPosition(0, 21);
                        Console.WriteLine("     ");
                    }
                }
                if (i > 75)
                {
                    Console.SetCursorPosition(i - 76, 20);
                    Console.Write(" ");
                    Console.SetCursorPosition(i - 60, 20);
                    Console.Write("*");
                }
                System.Threading.Thread.Sleep(80);
            }
            for (k = i; k < 180; k++)
            {
                Console.SetCursorPosition(k - 76, 20);
                Console.Write(" ");
                Console.SetCursorPosition(k - 60, 20);
                Console.Write("*");
                System.Threading.Thread.Sleep(80);
            }
            for (int j = k - 76; j < k - 60; j++)
            {
                Console.SetCursorPosition(j, 20);
                Console.Write(" ");
                System.Threading.Thread.Sleep(80);
            }
        }
        public static void GameOver()
        {
            int xPosForMsg = (message.Length) / 2;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(60 - xPosForMsg, 2);
            Console.WriteLine(message);
            Console.SetCursorPosition(52, 3);
            Console.WriteLine("Набрано очков: " + Points);
        }
        public static void deleteSpaces(ref string str)
        {
            string new_str = "";
            int i = 0;
            while (str[i] == ' ') i++;
            for (int k = i; k < str.Length; k++)
                new_str += str[k];
            str = new_str;
        }
        public static void splitString(string str, out string name, out int points, out string date, out string time)
        {       //wanna split the string on 4 parts: name, points, date, time
            string[] array_str = new string[3];
            string temp_str;
            int j;
            for (int i = 0; i < 3; i++)
            {
                temp_str = "";
                j = 0;
                while (str[j] != ' ')
                {
                    temp_str += str[j];
                    j++;
                }
                array_str[i] = temp_str;
                temp_str = "";
                for (int k = j; k < str.Length; k++)
                    temp_str += str[k];
                str = temp_str;
                deleteSpaces(ref str);
            }
            name = array_str[0];
            points = Convert.ToInt32(array_str[1]);
            date = array_str[2];
            time = str;
        }
        public static void writeDataFromTableToArray()
        {
            StreamReader sR = new StreamReader(recordsTableFile);
            string field = "", tempName = "", tempDate = "", tempTime = "";
            listNames = new List<string>();
            listPoints = new List<int>();
            listDates = new List<string>();
            listTime = new List<string>();
            int tempPoints = 0;
            amountOfFieldsInRecordsTable = 0;
            while ((field = sR.ReadLine()) != null)
            {
                splitString(field, out tempName, out tempPoints, out tempDate, out tempTime);
                listNames.Add(tempName);
                listPoints.Add(tempPoints);
                listDates.Add(tempDate);
                listTime.Add(tempTime);
                amountOfFieldsInRecordsTable++;
            }
            sR.Close();
            arrayNames = listNames.ToArray();
            arrayPoints = listPoints.ToArray();
            arrayDates = listDates.ToArray();
            arrayTime = listTime.ToArray();
        }
        public static void Swap<T>(ref T a, ref T b)    //it is needed for working with string type and int type
        {
            T temp = a;
            a = b;
            b = temp;
        }
        public static void sortDataFromRecordsTable()
        {
            for (int i = amountOfFieldsInRecordsTable - 1; i >=1; i--)
            {
                for (int j = amountOfFieldsInRecordsTable -1; j>=1; j--)
                {
                    if (arrayPoints[j] > arrayPoints[j-1])      
                                                                 
                    {
                        Swap(ref arrayNames[j], ref arrayNames[j - 1]);
                        Swap(ref arrayPoints[j], ref arrayPoints[j - 1]);
                        Swap(ref arrayDates[j], ref arrayDates[j - 1]);
                        Swap(ref arrayTime[j], ref arrayTime[j - 1]);
                    }
                }
            }
        }
        public static void showRecordsTable()
        {
            Console.Clear();
            if (amountOfFieldsInRecordsTable > 0)
            {
                Console.WriteLine($"{"Имя игрока",30} {"Количество очков",30} {"Дата",15} {"Время",15}");
                for (int i = 0; i < amountOfFieldsInRecordsTable; i++)
                {
                    Console.WriteLine($"{arrayNames[i],30} {arrayPoints[i],30} {arrayDates[i],15} {arrayTime[i],15}");
                }
            }
            else
                Console.WriteLine("Таблица рекордов пока пуста. Чтобы в ней появились записи, начните играть");
            Console.WriteLine("Чтобы вернуться в меню, нажмите любую клавишу.");
            Console.ReadKey(true);
        }
    }
}