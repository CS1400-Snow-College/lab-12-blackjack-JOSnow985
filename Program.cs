// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Diagnostics;
using System.Text;

drawHeader();
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
Debug.Assert("HDSC".Contains(card.suit));                                     // Checks that the suit and values are valid
Debug.Assert(card.value <= 14 && card.value >= 2);
Debug.Assert(cardDeck.Contains($"{card.suit}{card.value}") == false);     // Checks that the card was removed from the deck list
Console.WriteLine($"Suit: {card.suit} Value: {card.value}");              // Debug print





// Methods
static void drawHeader()
{
    Console.Clear();
    Console.WriteLine("Console Blackjack");
}

// Loads csv into a list of lists of strings, splits by ,
static List<List<string>> loadPlayerFile(string filepath)
{
    List<List<string>> dataList = [];
    if (File.Exists(filepath))
    {
        dataList = File.ReadAllLines(filepath).Select(line => line.Split(',').ToList()).ToList();
        return dataList;
    }
    // If there's no player file, write an empty one and return the empty list
    else
    {
    File.WriteAllLines(filepath, []);
        return dataList;
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