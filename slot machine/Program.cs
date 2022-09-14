using System;
using System.Collections.Generic;
using System.Linq;

namespace SlotMachine
{
    class Game
    {
        static void Main(string[] args)
        {
            GameModel model = new GameModel();
            GameView view = new GameView();
            GameController controller = new GameController(model, view);

            while (controller.Play()) ;

            view.Display($"Thank you for playing Slot-0-Matic-3000! you achived {model.Spins} spins! ");
        }
        class GameController
        {
            private GameModel model;
            private GameView view;

            public GameController(GameModel inputModel, GameView inputView)
            {
                model = inputModel;
                view = inputView;
            }

            //makes iterations of the game until balence is exhausted, if the player wishes to try again returns True otherwise false,
            //may also return false if a player opts to quit
            public bool Play()
            {
                model.Balence = view.GetDeposit();

                while (model.Balence > 0)
                {
                    var stake = view.GetStake(model.Balence, model.Spins);
                    if (stake == -1)
                        return false;

                    var payout = model.SpinAll(stake);

                    view.ShowBoard(model.getBoard());
                    view.ShowResult(payout, model.Balence);

                    if (!view.PlayAgain())
                        return false;
                }

                return view.PlayAgain();
            }
        }

        class GameModel
        {
            public decimal Balence;
            public int Spins = 0;

            private List<Row> board = Enumerable.Range(1, 4).Select(i => new Row()).ToList();

            private Random rnd = new Random();

            //spin all returns the payout (winnings) of the round triggered by calling this method. 
            public decimal SpinAll(int stake)
            {
                var winModifier = 0.0m;

                foreach (var row in board)
                {
                    foreach (var tile in row.tiles)
                        tile.Spin(rnd.Next(100));

                    winModifier += row.evaluate();
                }

                Spins += 1;
                var payout = stake * winModifier;
                Balence = Balence - stake + payout;

                return payout;
            }

            //returns faces of each tile as a list of strings
            public List<string> getBoard()
            {
                List<string> boardState = new List<string>();
                foreach (var row in board)
                {
                    var rowState = "";
                    foreach (var tile in row.tiles)
                        rowState += tile.face;

                    boardState.Add(rowState);
                }

                return boardState;
            }

            class Row
            {
                public List<Tile> tiles = Enumerable.Range(1, 3).Select(i => new Tile()).ToList();

                //evaluates the tiles in this row by grouping the unique faces in the list then removing the stars
                //if the count of unique faces is 1 after that operation, we have a winnign row and can then
                //sum up the coefficent of the row
                public decimal evaluate()
                {
                    var groupings = tiles.GroupBy(tile => tile.face).ToList();
                    groupings.RemoveAll(tiles => tiles.Key == "*");

                    if (groupings.Count() == 1)
                        return groupings.First().Sum(tile => tile.coefficent);

                    return 0.0m;
                }
            }

            class Tile
            {
                public string face;
                public decimal coefficent;

                public void Spin(int random)
                {
                    if (random < 45)
                    {
                        face = "A";
                        coefficent = 0.4m;
                    }
                    else if (45 <= random && random < 80)
                    {
                        face = "B";
                        coefficent = 0.6m;
                    }
                    else if (80 <= random && random < 95)
                    {
                        face = "P";
                        coefficent = 0.8m;
                    }
                    else
                    {
                        face = "*";
                        coefficent = 0.0m;
                    }
                }
            }
        }

        class GameView
        {
            private Random rnd = new Random();
            private const int LOW = 10, HIGH = 100;
            string escape = "ESC";

            List<List<string>> Afruit = new List<List<string>>();
            List<List<string>> Bfruit = new List<List<string>>();
            List<List<string>> Pfruit = new List<List<string>>();
            List<List<string>> Sfruit = new List<List<string>>();

            public GameView()
            {
                //initialise the ascii art for apple
                Afruit.Add(new List<string> { " ", " ", " ", "(", ")" });
                Afruit.Add(new List<string> { " ", "/", "¯", "\\", " " });
                Afruit.Add(new List<string> { "/", " ", " ", " ", "\\" });
                Afruit.Add(new List<string> { "\\", " ", " ", " ", "/" });
                Afruit.Add(new List<string> { " ", "\\", "_", "/", " " });

                //initalise the ascii art for banana
                Bfruit.Add(new List<string> { " ", " ", " ", "/", " " });
                Bfruit.Add(new List<string> { " ", " ", "/", "\\", " " });
                Bfruit.Add(new List<string> { " ", "/", " ", "/", " " });
                Bfruit.Add(new List<string> { " ", "\\", "/", " ", " " });
                Bfruit.Add(new List<string> { " ", "/", " ", " ", " " });

                //initialise the ascii art for pineapple
                Pfruit.Add(new List<string> { " ", "<", ">", " ", " " });
                Pfruit.Add(new List<string> { " ", "<", ">", " ", " " });
                Pfruit.Add(new List<string> { "{", "v", "v", "}", " " });
                Pfruit.Add(new List<string> { "{", "v", "v", "}", " " });
                Pfruit.Add(new List<string> { "{", "v", "v", "}", " " });

                //initialise the ascii art for star
                Sfruit.Add(new List<string> { " ", " ", "^", " ", " " });
                Sfruit.Add(new List<string> { " ", "/", " ", "\\", " " });
                Sfruit.Add(new List<string> { "<", " ", " ", " ", ">" });
                Sfruit.Add(new List<string> { " ", "\\", " ", "/", " " });
                Sfruit.Add(new List<string> { " ", " ", "v", " ", " " });
            }


            public int GetDeposit()
            {
                Display("Welcome to Slot-0-Matic-3000! at any time type ESC to exit");
                while (true)
                {
                    Display("\n\nPlease enter your deposit: ");
                    var input = Console.ReadLine();

                    if (string.Equals(input, escape, StringComparison.OrdinalIgnoreCase))
                        return -1;

                    if (int.TryParse(input, out var deposit))
                        if (deposit < 0)
                            Display($"\nyour attempt to cheat has been noted... enter a value grater than 0 or type ESC to quit");
                        else
                            return deposit;
                    else
                        Display("\nplease enter only a whole number, or type ESC to quit \n\nPlease enter your deposit: ");
                }

            }

            public bool PlayAgain()
            {
                Display("\n\nwould you like to play again Y / N? ");

                while (true)
                {
                    var input = Console.ReadLine();

                    if (string.Equals(input, escape, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(input, "N", StringComparison.OrdinalIgnoreCase))

                        return false;

                    if (string.Equals(input, "Y", StringComparison.OrdinalIgnoreCase))
                        return true;
                    else
                        Display("\nplease enter Y or N \n\nWould you like to play again Y / N? ");
                }
            }

            public int GetStake(decimal max, int spins)
            {
                Display($"lucky spin number {spins + 1}");

                while (true)
                {
                    Display("\n\nhow much would you like to bet? ");
                    var input = Console.ReadLine();

                    if (string.Equals(input, escape, StringComparison.OrdinalIgnoreCase))
                        return -1;

                    if (int.TryParse(input, out var stake))
                        if (stake < 0)
                            Display($"\nyour attempt to cheat has been noted... enter a value grater than 0 or type ESC to quit");
                        else if (stake > max)
                            Display($"\nyou cannot bet more than your balence!\n\nYour balence is {max}");
                        else
                            return stake;
                    else
                        Display("\nplease enter only a whole number, or type ESC to quit");
                }
            }

            public void ShowResult(decimal payout, decimal balence)
            {
                Display($"\nyou have won: {payout} \nYour balence is now: {balence}");
            }

            public void ShowBoard(List<string> state)
            {
                foreach (var row in state)
                {
                    var displayString = Beautify(row);

                    Display(displayString, 10, 15);
                }
            }

            private string Beautify(string row)
            {
                var fruitSize = 5;
                var beautifulString = "";
                List<List<List<string>>> fruits = new List<List<List<string>>>();

                foreach (var face in row)
                {
                    if (string.Equals(face, 'A'))
                        fruits.Add(Afruit);

                    else if (string.Equals(face, 'B'))
                        fruits.Add(Bfruit);

                    else if (string.Equals(face, 'P'))
                        fruits.Add(Pfruit);

                    else if (string.Equals(face, '*'))
                        fruits.Add(Sfruit);
                }

                for (var i = 0; i < fruitSize; i++)
                {
                    foreach (var fruit in fruits)
                        beautifulString += "| " + string.Join("", fruit[i]) + " |";

                    beautifulString += "\n";
                }

                return beautifulString += "\n";
            }

            public void Display(string value, int low = LOW, int high = HIGH)
            {
                foreach (var character in value)
                {
                    System.Threading.Thread.Sleep(rnd.Next(low, high));
                    Console.Write(character);
                }
            }
        }
    }
}
