// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Diagnostics;
using System.Text;

drawHeader();

List<string> cardDeck = BuildDeck();

Debug.Assert(cardDeck.Count == 52);
Debug.Assert(cardDeck.Contains("D13"));

Random rng = new Random();

// string[] cardFaces = ["2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"];
// Loads player
string playerFilePath = "playerFile.csv";
List<List<string>> playerList = loadPlayerFile(playerFilePath);
Debug.Assert(File.Exists(playerFilePath));
Debug.Assert(playerList != null);

// Check if card is returned and if returned card is properly removed from the deck
string card = DrawCard(cardDeck, rng);
Debug.Assert(card != null);
Console.WriteLine(card);
Debug.Assert(cardDeck.Contains(card) == false);

var (currentCardSuit, currentCardVal) = parseCard(card);
Debug.Assert("HDSC".Contains(currentCardSuit));
Debug.Assert(currentCardVal <= 14 || currentCardVal >= 2);
Console.WriteLine($"Suit: {currentCardSuit} card value: {currentCardVal}");



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
    
    // Card values with Aces high at 14
    int[] cardValues = [2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];
    char[] cardSuits = ['H', 'D', 'S', 'C'];

    foreach (char suit in cardSuits)
    {
        for (int i = 0; i < cardValues.Length; i++)
        {
            deck.Add($"{suit}{cardValues[i]}");
        }
    }

    return deck;
}

static string DrawCard(List<string> deck, Random rng)
{
    // Pick a random card
    int index = rng.Next(deck.Count);
    string card = deck[index];

    // return the card and remove it from the list
    deck.RemoveAt(index);
    return card;
}

static (char, int) parseCard(string card)
{
    char suit = card[0];
    // Retrieves the rest of the string after the suit character, tries to parse it to an int
    if (int.TryParse(card[1..], out int cardValue))
        return (suit, cardValue);
    else
        return ('S', 14);
}