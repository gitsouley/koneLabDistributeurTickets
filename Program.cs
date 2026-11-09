namespace koneLabDistributeurTickets;

internal static class Program
{
    private static readonly Operation[] Operations =
    [
        new("Versement", 'V'),
        new("Retrait", 'R'),
        new("Informations", 'I')
    ];

    private const string MessageTicket = "Votre numéro est {0}. {1}";

    private static void Main()
    {
        string cheminFichier = Path.Combine(Path.GetTempPath(), "fnumero.txt");
        CreerFichierNumerosSiAbsent(cheminFichier);
        Dictionary<char, int> compteurs = LireCompteurs(cheminFichier);
        List<Client> clients = [];

        while (true)
        {
            Operation? operation = DemanderOperation();
            if (operation is null)
            {
                break;
            }

            string numeroCompte = LireChampObligatoire("Numéro de compte : ");
            string nom = LireChampObligatoire("Nom : ");
            string prenom = LireChampObligatoire("Prénom : ");

            int personnesAvant = compteurs[operation.Prefixe];
            string numeroTicket = $"{operation.Prefixe}-{personnesAvant + 1}";
            compteurs[operation.Prefixe]++;
            EnregistrerCompteurs(cheminFichier, compteurs);

            clients.Add(new Client(operation.Nom, numeroCompte, nom, prenom, numeroTicket));

            string attente = personnesAvant == 0
                ? "Vous êtes le premier dans la file pour cette opération."
                : personnesAvant == 1
                    ? "Une personne ayant choisi cette opération est avant vous."
                    : $"{personnesAvant} personnes ayant choisi cette opération sont avant vous.";
            Console.WriteLine(string.Format(MessageTicket, numeroTicket, attente));

            if (!DemanderNouveauClient())
            {
                break;
            }
        }

        AfficherClients(clients);
    }

    private static void CreerFichierNumerosSiAbsent(string cheminFichier)
    {
        if (!File.Exists(cheminFichier))
        {
            EnregistrerCompteurs(cheminFichier, new Dictionary<char, int>
            {
                ['V'] = 0,
                ['R'] = 0,
                ['I'] = 0
            });
        }
    }

    private static Dictionary<char, int> LireCompteurs(string cheminFichier)
    {
        Dictionary<char, int> compteurs = [];

        foreach (string ligne in File.ReadAllLines(cheminFichier))
        {
            string[] parties = ligne.Split('=');
            if (parties.Length != 2 ||
                parties[0].Length != 1 ||
                !char.TryParse(parties[0], out char prefixe) ||
                !Operations.Any(operation => operation.Prefixe == prefixe) ||
                !int.TryParse(parties[1], out int nombre) ||
                nombre < 0 ||
                !compteurs.TryAdd(prefixe, nombre))
            {
                throw new InvalidDataException(
                    $"Le fichier de numéros contient une donnée invalide : « {ligne} ».");
            }
        }

        if (compteurs.Count != Operations.Length)
        {
            throw new InvalidDataException(
                "Le fichier de numéros ne contient pas les compteurs de toutes les opérations.");
        }

        return compteurs;
    }

    private static void EnregistrerCompteurs(
        string cheminFichier,
        Dictionary<char, int> compteurs)
    {
        string[] lignes = Operations
            .Select(operation => $"{operation.Prefixe}={compteurs[operation.Prefixe]}")
            .ToArray();

        File.WriteAllLines(cheminFichier, lignes);
    }

    private static Operation? DemanderOperation()
    {
        while (true)
        {
            Console.WriteLine("Choisissez le type d'opération :");
            for (int index = 0; index < Operations.Length; index++)
            {
                Console.WriteLine($"{index + 1}. {Operations[index].Nom}");
            }

            Console.WriteLine("0. Quitter");
            Console.Write("Votre choix : ");
            string? choix = Console.ReadLine();
            if (choix is null ||
                (int.TryParse(choix, out int numero) && numero == 0))
            {
                return null;
            }

            if (int.TryParse(choix, out numero) &&
                numero >= 1 &&
                numero <= Operations.Length)
            {
                return Operations[numero - 1];
            }

            Console.WriteLine("Choix invalide. Veuillez sélectionner 0, 1, 2 ou 3.");
        }
    }

    private static string LireChampObligatoire(string invite)
    {
        while (true)
        {
            Console.Write(invite);
            string? valeur = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(valeur))
            {
                return valeur.Trim();
            }

            Console.WriteLine("Ce champ est obligatoire.");
        }
    }

    private static bool DemanderNouveauClient()
    {
        while (true)
        {
            Console.Write("Voulez-vous prendre un autre numéro ? (o/n) : ");
            string? reponse = Console.ReadLine()?.Trim();

            if (string.Equals(reponse, "o", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(reponse, "n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Répondez par « o » ou « n ».");
        }
    }

    private static void AfficherClients(List<Client> clients)
    {
        Console.WriteLine();
        Console.WriteLine("Liste des clients et de leurs tickets :");
        if (clients.Count == 0)
        {
            Console.WriteLine("Aucun client enregistré.");
            return;
        }

        foreach (Client client in clients)
        {
            Console.WriteLine(
                $"{client.Prenom} {client.Nom} | Compte : {client.NumeroCompte} | " +
                $"Opération : {client.Operation} | Ticket : {client.NumeroTicket}");
        }
    }

    private sealed record Operation(string Nom, char Prefixe);

    private sealed record Client(
        string Operation,
        string NumeroCompte,
        string Nom,
        string Prenom,
        string NumeroTicket);
}
