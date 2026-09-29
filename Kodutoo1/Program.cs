Random random = new Random();

int secretNumber = random.Next(1, 11);
int guess = 0;

while (guess != secretNumber)
{
    Console.Write("Paku arv 1-10: ");
    guess = int.Parse(Console.ReadLine());

    if (guess == secretNumber)
    {
        Console.WriteLine("Õige vastus!");
    }
    else if (guess < secretNumber)
    {
        Console.WriteLine($"Vale vastus! Õige number oli suurem kui {guess}.");
    }
    else
    {
        Console.WriteLine($"Vale vastus! Õige number oli väiksem kui {guess}.");
    }
}