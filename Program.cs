// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Diagnostics;
using System.Text;

Console.Title = "Console Blackjack";
Random rng = new Random();

// Loads player list
string playerFilePath = "playerFile.csv";
List<List<string>> playerList = loadPlayerFile(playerFilePath);
Debug.Assert(File.Exists(playerFilePath));
Debug.Assert(playerList != null);

// Builds the deck
List<string> cardDeck = BuildDeck();
Debug.Assert(cardDeck.Count == 52);
Debug.Assert(cardDeck.Contains("D13"));

(char suit, int value) card = DrawCard(cardDeck, rng);                    // Draws a card, parses it to a suit and value tuple
Debug.Assert("HDSC".Contains(card.suit));                                 // Checks that the suit and values are valid
Debug.Assert(card.value <= 14 && card.value >= 2);
Debug.Assert(cardDeck.Contains($"{card.suit}{card.value}") == false);     // Checks that the card was removed from the deck list
Console.WriteLine($"Suit: {card.suit} Value: {card.value}");              // Debug print

var (playerProfile, playerName) = findPlayer(playerList, playerFilePath);




// Methods
static void drawHeader()
{
    Console.Clear();
    Console.WriteLine("Console Blackjack");
}

// Loads csv into a list of lists of strings, splits by ,
static List<List<string>> loadPlayerFile(string filepath)
{
    List<List<string>> playerList = [];
    if (File.Exists(filepath))
    {
        // Name, money, busts, hands played
        playerList = File.ReadAllLines(filepath).Select(line => line.Split(',').ToList()).ToList();
        return playerList;
    }
    // If there's no player file, write an empty one and return the empty list
    else
    {
    File.WriteAllLines(filepath, []);
        return playerList;
    }
}

static (List<string>, string) findPlayer(List<List<string>> playerList, string filepath)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine("~The dealer at the blackjack table flawlessly springs the deck from one hand to the other as you approach~");
        Console.WriteLine("Dealer: Want to play a hand? What's your name?");
        Console.Write("\nYour name:  ");
        string? playerNameInput = Console.ReadLine();
        if (string.IsNullOrEmpty(playerNameInput) == false)
        {
            playerNameInput = playerNameInput.Trim().ToLowerInvariant();
            string playerNameCapitalized;
            for (int index = 0; index < playerList.Count; index++)
            {
                if (playerNameInput.Equals(playerList[index][0]) == true)
                {
                    playerNameCapitalized = char.ToUpperInvariant(playerList[index][0][0]) + playerList[index][0][1..];
                    Console.WriteLine($"Dealer: Ah, right! {playerNameCapitalized}!");
                    Console.WriteLine($"Dealer: I think you've got ${playerList[index][1]}, right?");
                    Console.WriteLine($"~several chips glide across the felt, they add up to ${playerList[index][1]}~");
                    Console.Write("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return (playerList[index], playerNameCapitalized);
                }
            }
            Console.Clear();
            List<string> newPlayerProfile = [playerNameInput, "100.00", "0", "0"];
            playerList.Add(newPlayerProfile);
            playerNameCapitalized = char.ToUpperInvariant(playerList[^1][0][0]) + playerList[^1][0][1..];
            savePlayerFile(filepath, playerList);
            Console.WriteLine($"Dealer: Hm, I don't think you've played at my table before, {playerNameCapitalized}.");
            Console.WriteLine("Dealer: How about we start you off with a hundred?");
            Console.WriteLine("~several chips glide across the felt, they add up to $100.00~");
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
            return (newPlayerProfile, playerNameCapitalized);
        }
        else 
        {
            Console.WriteLine("\n~The dealer loudly shuffles the deck over your invalid input~");
            Console.WriteLine("Dealer: Oh, sorry, what was that?");
            Console.Write("Press any key to try telling him your name again... ");
            Console.ReadKey(true);
        }
    }
}

// Builds a new string with every list of list of strings, creates a list of strings that becomes an array that is saved
static void savePlayerFile(string filepath, List<List<string>> listToSave)
{
    List<string> writeList = [];
    foreach (List<string> line in listToSave)
    {
        StringBuilder lineBuilder = new();
        foreach (string field in line)
            lineBuilder.Append(field + ",");
        writeList.Add(lineBuilder.ToString().TrimEnd(','));
    }
    string[] dataArray = writeList.ToArray();
    File.WriteAllLines(filepath, dataArray);
}

static List<string> BuildDeck()
{
    List<string> deck = [];
    
    // Suits
    char[] cardSuits = ['H', 'D', 'S', 'C'];

    // For every suit, we add a string that combines the suit char and a number from 2 to 14 for the value
    foreach (char suit in cardSuits)
    {
        for (int value = 2; value <= 14; value++)
        {
            deck.Add($"{suit}{value}");
        }
    }

    return deck;
}

// Draws a card, parses it to a (char, int) tuple, removes it from the deck, returns the tuple
static (char, int) DrawCard(List<string> deck, Random rng)
{
    // Pick a random card
    int index = rng.Next(deck.Count);
    string card = deck[index];

    // remove the card from the deck, can't draw it again until shuffled
    deck.RemoveAt(index);

    // first index is a char to indicate suit
    char suit = card[0];
    // Retrieves the rest of the string after the suit character, tries to parse it to an int
    if (int.TryParse(card[1..], out int cardValue))
        return (suit, cardValue);
    // If it breaks somehow, give them an ace for fun
    else
        return ('S', 14);
}

// Figures out where the card we drew should be dealt
static void DealCard((char,int) cardToDeal, List<(char, int)> dealerHand, List<(char, int)> playerHand)
{
    // If the player has no cards, start there
    if (playerHand.Count == 0)
        playerHand.Add(cardToDeal);
    // If the player has cards, do they have more or less than the dealer?
    else if (playerHand.Count != 0)
        if (playerHand.Count > dealerHand.Count)
            dealerHand.Add(cardToDeal);
        else
            playerHand.Add(cardToDeal);
}