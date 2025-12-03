// Jaden Olvera, CS-1400, Lab 12 Blackjack
using System.Text;

Console.Title = "Console Blackjack";
Random rng = new Random();

// Loads player list
string playerFilePath = "playerFile.csv";
List<List<string>> playerList = loadPlayerFile(playerFilePath);

// Figure out who is playing
var (playerProfile, playerName, playerBankDecimal) = findPlayer(playerList, playerFilePath);

bool quitting = false;
while (quitting == false)
{
    // Builds the deck, inside loop so the deck isn't drained over multiple hands
    List<string> cardDeck = BuildDeck();
    
    List<(char, int)> playerHand = [];
    List<(char, int)> dealerHand = [];
    bool gameOver = false;

    // Collect bet amount
    (decimal playerBet, playerBankDecimal) = betPrompt(playerProfile, playerName, playerBankDecimal);

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
        DisplayTable(playerBankDecimal, playerName, playerHand, playerHandValue, playerBet, dealerHand, dealerHandValue, dealerTurn);
        // Check for blackjack on the first turn
        if (playerBlackjackCheck == 0)
        {
            // If they landed on 21 their first turn, blackjack
            if (playerHandValue == 21)
            {
                playerBlackjackCheck = 2;
                gameOver = true;
                Thread.Sleep(1500);
            }
            else
                playerBlackjackCheck = 1;
        }
        // Check if the player has busted, if they have, game over
        if (playerHandValue > 21)
        {
            gameOver = true;
            Thread.Sleep(1500);
        }
        else if (playerHandValue == 21)
            dealerTurn = true;

        // Player Choices
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
                        if (playerHand.Count == 2 && playerBankDecimal >= playerBet)
                        {
                        playerBankDecimal = playerBankDecimal - playerBet;
                        playerBet += playerBet;
                        playerHand.Add(DrawCard(cardDeck, rng));
                        dealerTurn = true;
                        selectionMade = true;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        // Dealer Choices
        else if (gameOver == false && dealerTurn == true)
        {
            Thread.Sleep(1500);
            // Strict dealer rules, pretty much ignores what the player has
            if (dealerHandValue >= 17)
                gameOver = true;
            else if (dealerHandValue <= 16)
                dealerHand.Add(DrawCard(cardDeck, rng));
            else
                gameOver = true;
        }
    }
    // Conclusion of the round handling
    Console.SetCursorPosition(0, Console.CursorTop - 1);
    Console.Write(new string(' ', Console.WindowWidth));
    Console.WriteLine();
    // If the player got blackjack
    if (playerBlackjackCheck == 2)
    {
        decimal winnings = playerBet * 1.5m;
        playerBankDecimal = playerBankDecimal + playerBet + winnings;
        Console.WriteLine($"\nBlackjack! You win: ${winnings:F2}");
    }
    // If the player busted, or the dealer beat their hand without busting
    else if (playerHandValue > 21 || (dealerHandValue > playerHandValue && dealerHandValue <= 21))
    {
        Console.WriteLine($"\nThe house wins, you lost ${playerBet:F2}");
    }
    // If the dealer and the player matched, push
    else if (dealerHandValue == playerHandValue)
    {
        playerBankDecimal = playerBankDecimal + playerBet;
        Console.WriteLine($"\nPush: Your ${playerBet:F2} bet is returned.");
    }
    // Otherwise
    else
    {
        decimal winnings = playerBet;
        playerBankDecimal = playerBankDecimal + playerBet + winnings;
        Console.WriteLine($"\nYou won ${winnings:F2}");
    }
    // If the player is broke, give them 50 more
    if (playerBankDecimal <= 0m)
    {
        playerBankDecimal = 50.00m;
        Console.WriteLine("\nYou deposit $50 into your bank so you can keep playing.\nIf this were real you might have had to stop instead!");
    }

    Console.WriteLine($"{playerName}'s Bank now has: ${playerBankDecimal:F2}");
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey(true);

    drawHeader();
    Console.WriteLine("Play another hand?");
    Console.WriteLine($"{playerName}'s Bank: ${playerBankDecimal:F2}");
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
        playerProfile[1] = Convert.ToString(playerBankDecimal);
        savePlayerFile(playerFilePath, playerList);
        Console.Clear();
        Console.WriteLine("\nGoodbye!");
    }
}



// Methods
static void drawHeader()
{
    Console.Clear();
    Console.WriteLine("====BLACKJACK====");
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

static (List<string> playerProfile, string playerName, decimal playerBankDecimal) findPlayer(List<List<string>> playerList, string filepath)
{
    while (true)
    {
        drawHeader();
        Console.WriteLine("Welcome to Console Blackjack, please enter your name!");
        Console.Write("\nYour name:  ");
        string? playerNameInput = Console.ReadLine();
        if (string.IsNullOrEmpty(playerNameInput) == false)
        {
            // Whitespace trimmed and dropped to lowercase so names aren't case sensitive
            playerNameInput = playerNameInput.Trim().ToLowerInvariant();
            // Try to capitalize their name so it looks nicer when we print it
            string playerNameCapitalized = char.ToUpperInvariant(playerNameInput[0]) + playerNameInput[1..];
            // Fallback bank value
            decimal playerBankDecimal = 50.00m;
            int playerIndex = -1;
            for (int index = 0; index < playerList.Count; index++)
            {
                if (playerNameInput.Equals(playerList[index][0]) == true)
                    playerIndex = index;
            }
            drawHeader();
            if (playerIndex != -1)
            {
                playerBankDecimal = decimal.Parse(playerList[playerIndex][1]);
                // If their money count is zeroed, refill to 50
                if (playerBankDecimal < 0m)
                    playerBankDecimal = 50m;
                Console.WriteLine($"Returning player: {playerNameCapitalized}");
                Console.WriteLine($"{playerNameCapitalized}'s Bank: ${playerBankDecimal:F2}");
            }
            else
            {
                playerList.Add([playerNameInput, "100.00"]);
                savePlayerFile(filepath, playerList);
                playerIndex = playerList.Count - 1;
                playerBankDecimal = 100m;
                Console.WriteLine($"New player: {playerNameCapitalized}");
                Console.WriteLine($"Starting {playerNameCapitalized}'s Bank at: $100.00");
            }
            Console.Write("\nPress any key to continue...");
            Console.ReadKey(true);
            return (playerList[playerIndex], playerNameCapitalized, playerBankDecimal);
        }
        else 
        {
            Console.WriteLine("Sorry, we couldn't make that input work!");
            Console.Write("Press any key to try entering your name again... ");
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

static (decimal playerBet, decimal playerBankDecimal) betPrompt(List<string> playerProfile, string playerName, decimal playerBankDecimal)
{
    while (true)
    {
        drawHeader();
        Console.WriteLine("How much will you bet?\n");
        Console.WriteLine($"{playerName}'s Bank: ${playerBankDecimal:F2}");
        Console.WriteLine($"You can bet from $0.01 to ${playerBankDecimal:F2}");
        Console.Write("Your Bet:  $");
        if (decimal.TryParse(Console.ReadLine(), out decimal playerBet))
        {
            if (playerBet < 0.01m)
                Console.WriteLine("You have to bet at least $0.01.");
            else if (playerBet > playerBankDecimal)
                Console.WriteLine("You don't have that much!");
            else
            {
                playerBankDecimal = playerBankDecimal - playerBet;
                return (playerBet, playerBankDecimal);
            }
        }
        else
        {
            Console.WriteLine("That input won't work for a bet, please try again!");
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
        'H' => "\u2665",
        'D' => "\u2666",
        'S' => "\u2660",
        'C' => "\u2663",
        'X' => "",
        _ => $"{card.suit}",
    };
    string valuePart = card.value switch
    {
        0 => "?",
        11 => "J",
        12 => "Q",
        13 => "K",
        14 => "A",
        _ => $"{card.value}"
    };
    Console.Write($"[{suitPart}{valuePart}]");
}

// Handles printing the table out, checking player money, who has what cards to print
static void DisplayTable(decimal playerBankDecimal, string playerName, List<(char, int)> playerHand, int playerHandValue, decimal playerBet, List<(char, int)> dealerHand, int dealerHandValue, bool dealerTurn)
{
    drawHeader();
    Console.WriteLine($"{playerName}'s Bank: ${playerBankDecimal:F2}     Bet: ${playerBet:F2}");
    Console.WriteLine();
    if (dealerHand.Count != 0)
    {
        Console.WriteLine($"Dealer's Hand:");
        if (dealerTurn == true)
            Console.Write($"Total: {dealerHandValue}   ");
        else
            Console.Write("Total: ?    ");
        foreach ((char, int) card in dealerHand)
        {
            if (card == dealerHand[^1] && dealerTurn == false)
                DisplayCard(('X', 0));
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
        {
            Console.Write("\nOptions: <(H)it>   <(S)tand>   ");
            if (playerHand.Count == 2 && playerBankDecimal >= playerBet)
                Console.WriteLine("<(D)ouble Down>");
            else
                Console.WriteLine();
        }
        else
            Console.WriteLine("\nIt's the dealer's turn...");
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