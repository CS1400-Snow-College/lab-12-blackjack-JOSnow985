// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Diagnostics;

Console.Clear();
Console.WriteLine("Console Blackjack");

List<string> cardDeck = BuildDeck();

Debug.Assert(cardDeck.Count == 52);
Debug.Assert(cardDeck.Contains("D13"));

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