using System;

public class Game
{
    public string Title {get; set;}

    public Game(string title)
    {
        Title = title;
    }
    public virtual void Play()
    {
        Console.WriteLine($"Playing {Title}.");
    }
}
public class BoardGame : Game
{
    public int Maxplayers {get; set;}
    public int Minplayers {get; set;}

    public BoardGame(string title, int minplayers, int maxplayers) : base(title)
    {
        Minplayers = minplayers;
        Maxplayers = maxplayers;
    }
    public override void Play()
    {
        Console.WriteLine($"Playing {Title} with {Minplayers} to {Maxplayers} players.");
    }

}
public class VideoGame : Game
{
    public string Platform {get; set;}

    public VideoGame(string title, string platform) : base(title)
    {
        Platform = platform;
    }
    public override void Play()
    {
        Console.WriteLine($"Playing {Title} on {Platform}.");
    }
}
public class CardGame : Game
{
    public string CardType {get; set;}

    public CardGame(string title, string cardType) : base(title)
    {
        CardType = cardType;
    }
    public override void Play()
    {
        Console.WriteLine($"Playing {Title} with {CardType} cards.");
    }
}


class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Game[] games = new Game[3];
        games[0] = new BoardGame("Catan", 3, 4);
        games[1] = new VideoGame("The Legend of Zelda: Breath of the Wild", "Nintendo Switch");
        games[2] = new CardGame("Poker", "Standard");

        foreach (var game in games)
        {
            game.Play();
        }
    }
}