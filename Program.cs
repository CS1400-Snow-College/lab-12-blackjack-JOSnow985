// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Diagnostics;
using System.Globalization;
using System.Text;

Console.Title = "Console Blackjack";
Random rng = new Random();

// Loads player list
string playerFilePath = "playerFile.csv";
List<List<string>> playerList = loadPlayerFile(playerFilePath);
// Debug.Assert(File.Exists(playerFilePath));
// Debug.Assert(playerList != null);

// Builds the deck
List<string> cardDeck = BuildDeck();
// Debug.Assert(cardDeck.Count == 52);
// Debug.Assert(cardDeck.Contains("D13"));

// (char suit, int value) card = DrawCard(cardDeck, rng);                    // Draws a card, parses it to a suit and value tuple
// Debug.Assert("HDSC".Contains(card.suit));                                 // Checks that the suit and values are valid
// Debug.Assert(card.value <= 14 && card.value >= 2);
// Debug.Assert(cardDeck.Contains($"{card.suit}{card.value}") == false);     // Checks that the card was removed from the deck list
// Console.WriteLine($"Suit: {card.suit} Value: {card.value}");              // Debug print

// Figure out who is playing
var (playerProfile, playerName) = findPlayer(playerList, playerFilePath);

bool quitting = false;
while (quitting == false)
{
    List<(char, int)> playerHand = [];
    List<(char, int)> dealerHand = [];
    bool gameOver = false;

    // Collect bet and save player file so the bet is taken
    decimal playerBet = betPrompt(playerProfile, playerName);
    savePlayerFile(playerFilePath, playerList);

    // Deal initial cards
    while (dealerHand.Count < 2)
        DealCards(cardDeck, rng, dealerHand, playerHand);

    int playerHandValue = 0;
    int dealerHandValue = 0;
    bool dealerTurn = false;
    int playerBlackjackCheck = 0;
    
    // Main game loop
    while (gameOver == false)
    {
        (playerHandValue, dealerHandValue) = GetHandValues(playerHand, dealerHand);
        if (playerBlackjackCheck == 0)
        {
            if (playerHandValue == 21)
            {
                // pay out 3:2
                playerBlackjackCheck = 2;
                return;
            }
            else
                playerBlackjackCheck = 1;
        }
        if (playerHandValue > 21)
        {
            gameOver = true;
            dealerTurn = true;
        }
        else if (playerHandValue == 21)
            dealerTurn = true;
        DisplayTable(playerProfile, playerName, playerHand, playerHandValue, playerBet, dealerHand, dealerHandValue, dealerTurn);
        if (gameOver == false && dealerTurn == false)
        {
            bool selectionMade = false;
            while (selectionMade == false)
            {
                char optionSelect = Console.ReadKey(true).KeyChar;
                switch (optionSelect)
                {
                    case 'h':
                    case 'H':
                        playerHand.Add(DrawCard(cardDeck, rng));
                        selectionMade = true;
                        break;
                    case 's':
                    case 'S':
                        dealerTurn = true;
                        selectionMade = true;
                        break;
                    case 'd':
                    case 'D':
                        playerHand.Add(DrawCard(cardDeck, rng));
                        dealerTurn = true;
                        selectionMade = true;
                        break;
                    default:
                        break;
                }
            }
        }
        else
        {
            Thread.Sleep(1500);
            if (playerHandValue > 21)
                continue;
            else if (playerHandValue == dealerHandValue)
                gameOver = true;
            else if (dealerHandValue > playerHandValue)
                gameOver = true;
            else if (dealerHandValue < playerHandValue)
                dealerHand.Add(DrawCard(cardDeck, rng));
        }
    }
    // Conclusion of the round handling
    drawHeader();
    Console.WriteLine($"Dealer's Total: {dealerHandValue}   vs   {playerName}'s Total: {playerHandValue}\n");
    // If the player got blackjack
    if (playerBlackjackCheck == 2)
    {
        decimal winnings = playerBet * 1.5m;
        playerProfile[1] = Convert.ToString(decimal.Parse(playerProfile[1]) + playerBet + winnings);
        Console.WriteLine("Dealer: You got blackjack! That's a 3:2 payout at this table!\n");
        Console.WriteLine($"~More chips slide towards you, your bet ${playerBet:F2} and a ${winnings:F2} payout is added to your bank~");
    }
    // If the dealer had worse cards but the player busted
    else if (dealerHandValue < playerHandValue && playerHandValue > 21)
    {
        Console.WriteLine("Dealer: Sorry, the house wins this time.\n");
        Console.WriteLine($"~Your bet is claimed by the Dealer. You've lost ${playerBet:F2}~");
    }
    // If the dealer had better cards and didn't bust
    else if ((dealerHandValue > playerHandValue && dealerHandValue <= 21) || (dealerHandValue < playerHandValue && playerHandValue > 21))
    {
        Console.WriteLine("Dealer: Sorry, the house wins this time.\n");
        Console.WriteLine($"~Your bet is claimed by the Dealer. You've lost ${playerBet:F2}~");
    }
    // If the dealer and the player matched, push
    else if (dealerHandValue == playerHandValue)
    {
        playerProfile[1] = Convert.ToString(decimal.Parse(playerProfile[1]) + playerBet);
        Console.WriteLine("Dealer: A push, at least you didn't lose your bet, right?");
        Console.WriteLine($"~Your bet of ${playerBet:F2} is returned to your bank~");
    }
    // Otherwise
    else
    {
        decimal winnings = playerBet;
        playerProfile[1] = Convert.ToString(decimal.Parse(playerProfile[1]) + playerBet + winnings);
        Console.WriteLine("Dealer: That's a win for you, here are your winnings!");
        Console.WriteLine($"~The dealer stacks up an equal amount of chips to your bet and slides them to you~");
        Console.WriteLine($"~Your bet ${playerBet:F2} and a ${winnings:F2} payout is added to your bank~");
    }
    savePlayerFile(playerFilePath, playerList);
    Console.WriteLine($"{playerName}'s Bank: {decimal.Parse(playerProfile[1]):F2}");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey(true);
    
    // If the player is broke, give them 50 more
    if (decimal.Parse(playerProfile[1]) <= 0m)
    {
        drawHeader();
        playerProfile[1] = "50.00";
        savePlayerFile(playerFilePath, playerList);
        Console.WriteLine("Dealer: Don't worry, we all have runs of bad luck, you can keep playing!");
        Console.WriteLine("~Your bank has been refilled to $50.00~");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(true);
    }

    drawHeader();
    Console.WriteLine("Dealer: Want to play another hand?");
    Console.WriteLine("~The dealer starts shuffling the cards while waiting for your decision~");
    Console.WriteLine("\nOptions: <(Y)es>   <(N)o>");
    bool choiceMade = false;
    while (choiceMade == false)
    {
        char playAgain = Console.ReadKey(true).KeyChar;
        switch (playAgain)
        {
            case 'y':
            case 'Y':
                choiceMade = true;
                break;
            case 'n':
            case 'N':
                choiceMade = true;
                quitting = true;
                break;
            default:
                break;
        }
    }
    if (quitting)
    {
        Console.Clear();
        Console.WriteLine("\nGoodbye!");
    }
}



// Methods
static void drawHeader()
{
    Console.Clear();
    Console.WriteLine("\uD83C\uDCA0 \uD83C\uDCA0 \uD83C\uDCA0 BLACKJACK\uD83C\uDCA0 \uD83C\uDCA0 \uD83C\uDCA0");
    Console.WriteLine();
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
        drawHeader();
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
                    drawHeader();
                    Console.WriteLine($"Dealer: Ah, right! {playerNameCapitalized}!");
                    Console.WriteLine($"Dealer: I think you've got ${playerList[index][1]}, right?");
                    Console.WriteLine($"~several chips glide across the felt, they add up to ${playerList[index][1]}~");
                    Console.Write("\nPress any key to continue...");
                    Console.ReadKey(true);
                    return (playerList[index], playerNameCapitalized);
                }
            }
            drawHeader();
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

static decimal betPrompt(List<string> playerProfile, string playerName)
{
    decimal playerCurrentMoney = decimal.Parse(playerProfile[1]);
    while (true)
    {
        drawHeader();
        Console.WriteLine("Dealer: How much are you betting this round?\n");
        Console.WriteLine($"{playerName}'s Bank: ${playerCurrentMoney:F2}");
        Console.WriteLine($"You can bet from $0.01 to ${playerCurrentMoney:F2}");
        Console.Write("Your Bet:  $");
        if (decimal.TryParse(Console.ReadLine(), out decimal playerBet))
        {
            if (playerBet < 0.01m)
                Console.WriteLine("You have to bet something at least $0.01.");
            else if (playerBet > playerCurrentMoney)
                Console.WriteLine("You don't have that much!");
            else
            {
                playerProfile[1] = Convert.ToString(playerCurrentMoney - playerBet);
                return playerBet;
            }
        }
        else
        {
            Console.WriteLine("That's not a proper bet, please try again.");
        }
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }
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

// Calls DrawCard and deals the card
static void DealCards(List<string> deck, Random rng, List<(char, int)> dealerHand, List<(char, int)> playerHand)
{
    (char, int) cardToDeal = DrawCard(deck, rng);
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

// Builds a hex to draw a card character for the card on the table
static void DisplayCard((char suit,int value) card)
{
    string suitPart = card.suit switch
    {
        'H' => "B",
        'D' => "C",
        'S' => "A",
        'C' => "D",
        _ => "?",
    };
    string valuePart = card.value switch
    {
        10 => "A",
        11 => "B",
        12 => "D",
        13 => "E",
        14 => "1",
        _ => $"{card.value}"
    };
    string stringConvert = $"1F0{suitPart}{valuePart}";
    string cardPrint = char.ConvertFromUtf32(int.Parse(stringConvert, NumberStyles.HexNumber));
    Console.Write($"{cardPrint} ");
}

// Handles printing the table out, checking player money, who has what cards to print
static void DisplayTable(List<string> playerProfile, string playerName, List<(char, int)> playerHand, int playerHandValue, decimal playerBet, List<(char, int)> dealerHand, int dealerHandValue, bool dealerTurn)
{
    drawHeader();
    Console.WriteLine($"{playerName}'s Bank: ${playerProfile[1]}     Bet: ${playerBet:F2}");
    Console.WriteLine();
    if (dealerHand.Count != 0)
    {
        Console.WriteLine($"Dealer's Hand:");
        Console.Write($"Total: {dealerHandValue}   ");
        foreach ((char, int) card in dealerHand)
        {
            if (card == dealerHand[^1] && dealerTurn == false)
                DisplayCard(('S', 0));
            else
                DisplayCard(card);
        }
        Console.WriteLine();
    }
    else
        Console.WriteLine("\n");
    if (playerHand.Count != 0)
    {
        Console.WriteLine($"{playerName}'s Hand:");
        Console.Write($"Total: {playerHandValue}   ");
        foreach ((char, int) card in playerHand)
            DisplayCard(card);
        if (dealerTurn == false)
            Console.WriteLine("\nOptions: <(H)it>   <(S)tand>   <(D)ouble Down>");
        else
            Console.WriteLine("It's the dealer's turn...");
    }
    else
        Console.WriteLine("\n");
}

static (int playerTotal, int dealerTotal) GetHandValues(List<(char suit, int value)> playerHand, List<(char suit, int value)> dealerHand)
{
    static int SumHand(List<(char suit, int value)> hand)
    {
        int handValue = 0;
        int holdingAces = 0;

        foreach (var card in hand)
        {
            int worth = card.value;

            // J (11) / Q (12) / K (13) are worth 10
            if (worth >= 11 && worth <= 13) 
                handValue += 10;   
            
            // Treat Aces as worth 11 for now
            else if (worth == 14) 
            { 
                handValue += 11; 
                holdingAces++; 
            }
            // Everything else is treated as it's identifying value
            else handValue += worth;
        }

        // If we're over 21 and have at least one ace, drop their value until we're out of aces
        while (handValue > 21 && holdingAces > 0)
        {
            handValue -= 10;
            holdingAces--;
        }

        return handValue;
    }
    return (SumHand(playerHand), SumHand(dealerHand));
}