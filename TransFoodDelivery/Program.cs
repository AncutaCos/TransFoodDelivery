using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TransFoodDelivery
{
    // Classe che rappresenta una richiesta di traduzione
    class TranslationRequest
    {
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string TextToTranslate { get; set; }
        public string TranslatedText { get; set; }
    }

    // Classe che rappresenta un traduttore
    class Translator
    {
        public string Name { get; set; }
        public string Language { get; set; }

        public string Translate(TranslationRequest request)
        {
            return $"[{Language}] {request.TextToTranslate} => {request.TargetLanguage}";
        }
    }

    class Order
    {
        public int OrderId { get; set; }
        public List<FoodOption> Cart { get; set; }
        public enum OrderStatus { Received, Preparing, Delivering }
        public OrderStatus Status { get; set; }

        public void UpdateStatus(OrderStatus status)
        {
            Status = status;
            // Aggiorna lo stato dell'ordine e implementa logica correlata
        }

        public Order()
        {
            // Inizializza il carrello come una nuova lista vuota
            Cart = new List<FoodOption>();
        }
        public void AddToCart(FoodOption foodOption, int quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                Cart.Add(foodOption);
            }
        }

    }

    class OrderManager
    {
        private List<Order> orders = new List<Order>();
        private Queue<Order> orderQueue = new Queue<Order>();
        private List<Order> processingOrders = new List<Order>();
        private const int MaxPreparingItems = 4; // il massimo numero di articoli in preparazione


        private async Task PrepareOrderAsync(Order order)
        {
            // Calcola il tempo totale di preparazione sommando il tempo di tutti gli articoli
            TimeSpan totalPreparationTime = order.Cart.Aggregate(TimeSpan.Zero, (total, next) => total + next.PreparationTime);

            // Aspetta il tempo di preparazione
            await Task.Delay(totalPreparationTime);

            // Cambia lo stato in "Delivering"
            UpdateOrderStatus(order, Order.OrderStatus.Delivering);
        }
        public void PlaceOrder(Order order)
        {
            order.OrderId = GenerateUniqueId();
            orders.Add(order);
            orderQueue.Enqueue(order);
            Console.WriteLine($"Ordine {order.OrderId} ricevuto.");
            ProcessNextOrder();
        }

        private int GenerateUniqueId()
        {
            return orders.Count + 1;
        }

        private bool CanAllocateResource(Order newOrder)
        {
            int currentPreparingItems = processingOrders
                .SelectMany(order => order.Cart)
                .Count();

            int newOrderItems = newOrder.Cart.Count;

            return currentPreparingItems + newOrderItems <= MaxPreparingItems;
        }


        private void ProcessNextOrder()
        {
            while (orderQueue.Any())
            {
                var nextOrder = orderQueue.Peek();
                if (CanAllocateResource(nextOrder))
                {
                    orderQueue.Dequeue();
                    processingOrders.Add(nextOrder);
                    nextOrder.UpdateStatus(Order.OrderStatus.Preparing);
                    Console.WriteLine($"Ordine {nextOrder.OrderId} in preparazione.");

                    // Avvia la preparazione dell'ordine
                    PrepareOrderAsync(nextOrder).ContinueWith(t =>
                    {
                        // Gestisci eventuali azioni post-preparazione qui, se necessario
                    });
                }
                else
                {
                    break;
                }
            }
        }

        public void PrintOrderStatus(int? orderId = null)
        {
            if (orderId.HasValue)
            {
                var order = orders.FirstOrDefault(o => o.OrderId == orderId.Value);
                if (order != null)
                {
                    Console.WriteLine($"Ordine {order.OrderId}: Stato: {order.Status}");
                }
                else
                {
                    Console.WriteLine($"Ordine {orderId.Value} non trovato.");
                }
            }
            else
            {
                foreach (var order in orders)
                {
                    Console.WriteLine($"Ordine {order.OrderId}: Stato: {order.Status}");
                }
            }
        }



        public void UpdateOrderStatus(Order order, Order.OrderStatus newStatus)
        {
            order.UpdateStatus(newStatus);
            if (newStatus == Order.OrderStatus.Delivering)
            {
                processingOrders.Remove(order);
                ProcessNextOrder(); // Prova a processare il prossimo ordine nella coda
            }
        }
        private void PrintOrderSummary(Order order)
        {
            Console.WriteLine("\nRiepilogo del tuo ordine:");
            var groupedItems = order.Cart.GroupBy(item => item.Name);
            double total = 0;

            foreach (var group in groupedItems)
            {
                var item = group.First();
                int quantity = group.Count();
                double subtotal = quantity * item.Price;
                total += subtotal;

                Console.WriteLine($"{quantity}x {item.Name} - ${item.Price} ciascuno (Subtotale: ${subtotal})");
            }

            Console.WriteLine($"Totale ordine: ${total}");
        }
    }



    class FoodOption
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Priority { get; set; } // Priorità dell'articolo
        public TimeSpan PreparationTime { get; set; } // Tempo di preparazione
    }


    // Classe che rappresenta un fornitore di cibo
    class FoodProvider
    {
        public string Name { get; set; }
        public string OperatingHours { get; set; }

        public List<FoodOption> FoodOptions { get; set; }


        public FoodProvider()
        {
            // Inizializza la lista di opzioni di cibo come una nuova lista vuota
            FoodOptions = new List<FoodOption>();
        }


    }
    // classe per rappresentare il Translation Store
    class TranslationStore
    {
        private List<Translator> translators = new List<Translator>();

        public TranslationStore()
        {
            // Aggiungi traduttori per le lingue supportate
            translators.Add(new Translator { Name = "English to German Translator", Language = "English" });
            translators.Add(new Translator { Name = "German to English Translator", Language = "German" });
        }

        public Translator FindTranslator(string targetLanguage)
        {
            // Cerca un traduttore per la lingua target
            return translators.FirstOrDefault(translator => translator.Language.ToLower() == targetLanguage.ToLower());
        }
    }

    // Classe che rappresenta l'Office Manager
    class OfficeManager
    {
        private TranslationStore translationStore = new TranslationStore();
        private List<FoodProvider> foodProviders = new List<FoodProvider>();
        private List<Translator> translators = new List<Translator>();
        private List<Order> orders = new List<Order>();
        private OrderManager orderManager = new OrderManager();

        public void PlaceOrder(Order order)
        {
            orderManager.PlaceOrder(order);
        }


        public OfficeManager()
        {
            // Aggiungi traduttori per le lingue supportate
            translators.Add(new Translator { Name = "English to German Translator", Language = "English" });
            translators.Add(new Translator { Name = "German to English Translator", Language = "German" });

            // Aggiungi fornitori di cibo
            foodProviders.Add(new FoodProvider
            {
                Name = "Starbucks (Breakfast)",
                OperatingHours = "07:00-11:00",
                FoodOptions = new List<FoodOption>
    {
        new FoodOption { Name = "Caffè", Price = 1.00, Priority = 5, PreparationTime = TimeSpan.FromSeconds(30) },
        new FoodOption { Name = "Cornetto", Price = 2.00, Priority = 2, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Muffin", Price = 2.50, Priority = 3, PreparationTime = TimeSpan.FromMinutes(2) },
        new FoodOption { Name = "Frappe", Price = 3.50, Priority = 4, PreparationTime = TimeSpan.FromMinutes(1) },
    }
            });

            foodProviders.Add(new FoodProvider
            {
                Name = "McDonald's (Breakfast)",
                OperatingHours = "07:00-11:00",
                FoodOptions = new List<FoodOption>
    {
        new FoodOption { Name = "Egg McMuffin", Price = 3.99, Priority = 1, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Hotcakes", Price = 4.50, Priority = 2, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Sausage McMuffin", Price = 3.49, Priority = 3, PreparationTime = TimeSpan.FromMinutes(1) },
    }
            });

            foodProviders.Add(new FoodProvider
            {
                Name = "Local Bakery (Lunch)",
                OperatingHours = "12:00-15:00",
                FoodOptions = new List<FoodOption>
    {
        new FoodOption { Name = "Panino al Pollo", Price = 5.00, Priority = 1, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Insalata Cesar", Price = 4.50, Priority = 2, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Pizza Margherita", Price = 6.99, Priority = 1, PreparationTime = TimeSpan.FromMinutes(1) },
    }
            });

            foodProviders.Add(new FoodProvider
            {
                Name = "Pizza Hut (Dinner)",
                OperatingHours = "18:00-22:00",
                FoodOptions = new List<FoodOption>
    {
        new FoodOption { Name = "Pizza Pepperoni", Price = 10.99, Priority = 1, PreparationTime = TimeSpan.FromMinutes(2) },
        new FoodOption { Name = "Pasta Alfredo", Price = 8.50, Priority = 2, PreparationTime = TimeSpan.FromMinutes(1) },
        new FoodOption { Name = "Insalata Caprese", Price = 6.99, Priority = 3, PreparationTime = TimeSpan.FromMinutes(2) },
    }
            });



        }

        public void PrintOrderStatus()
        {
            orderManager.PrintOrderStatus();
        }

        private void DisplayFoodOptions(List<FoodOption> foodOptions)
        {
            int optionNumber = 1;
            foreach (var option in foodOptions)
            {
                Console.WriteLine($"{optionNumber}. {option.Name} - ${option.Price}");
                optionNumber++;
            }
        }

        public void HandleTranslations()
        {
            Console.WriteLine("Seleziona la lingua di destinazione:");
            Console.WriteLine("1- Tedesco");
            Console.WriteLine("2- Inglese");
            string languageChoice = Console.ReadLine();

            string targetLanguage = "";

            if (languageChoice == "1")
            {
                targetLanguage = "German";
                Console.WriteLine("Hai scelto una traduzione in Tedesco.");
            }
            else if (languageChoice == "2")
            {
                targetLanguage = "English";
                Console.WriteLine("Hai scelto una traduzione in Inglese.");
            }
            else
            {
                Console.WriteLine("Lingua non valida. Uscita.");
                return;
            }
        }


        
        public void HandleFoodDelivery()
        {
            Console.WriteLine("Hai scelto il servizio di Food Delivery, seleziona quello che preferisci:");
            Console.WriteLine("1- Breakfast  (07:00-11:00)");
            Console.WriteLine("2- Brunch  (07:00-15:00)");
            Console.WriteLine("3- Lunch & Dinner  (12:00-22:00)");
            Console.WriteLine("-------------------------");

            string foodChoice = Console.ReadLine();
            Order currentOrder = new Order(); // Crea un nuovo ordine

            switch (foodChoice)
            {
                case "1":
                    if (IsProviderOpen("07:00-11:00"))
                    {
                        Console.WriteLine("Seleziona il tuo cibo per la colazione:");
                        DisplayFoodOptions(foodProviders[0].FoodOptions);
                        AddItemsToOrder(foodProviders[0].FoodOptions, currentOrder);
                    }
                    else
                    {
                        Console.WriteLine("Siamo spiacenti, il Breakfast Provider non è attualmente aperto.");
                    }
                    break;

                case "2":
                    if (IsProviderOpen("07:00-15:00"))
                    {
                        Console.WriteLine("Seleziona il tuo cibo per il brunch:");
                        DisplayFoodOptions(foodProviders[1].FoodOptions);
                        AddItemsToOrder(foodProviders[1].FoodOptions, currentOrder);
                    }
                    else
                    {
                        Console.WriteLine("Siamo spiacenti, il Brunch Provider non è attualmente aperto.");
                    }
                    break;

                case "3":
                    if (IsProviderOpen("12:00-22:00"))
                    {
                        Console.WriteLine("Seleziona il tuo cibo per il Lunch & Dinner:");
                        DisplayFoodOptions(foodProviders[2].FoodOptions);
                        AddItemsToOrder(foodProviders[2].FoodOptions, currentOrder);
                    }
                    else
                    {
                        Console.WriteLine("Siamo spiacenti, il Lunch & Dinner Provider non è attualmente aperto.");
                    }
                    break;

                default:
                    Console.WriteLine("Scelta non valida. Uscita.");
                    break;
            }

            if (currentOrder.Cart.Any())
            {
                this.PlaceOrder(currentOrder);
            }
        }

        private void AddItemsToOrder(List<FoodOption> foodOptions, Order order)
        {
            while (true)
            {
                Console.WriteLine("Inserisci il numero dell'opzione o 'finito' per completare l'ordine:");
                string input = Console.ReadLine();
                if (input.ToLower() == "finito") break;

                if (int.TryParse(input, out int option) && option >= 1 && option <= foodOptions.Count)
                {
                    Console.WriteLine("Quante porzioni vuoi aggiungere al carrello?");
                    string quantityInput = Console.ReadLine();
                    if (int.TryParse(quantityInput, out int quantity))
                    {
                        order.AddToCart(foodOptions[option - 1], quantity);
                        Console.WriteLine($"{quantity} porzioni di {foodOptions[option - 1].Name} aggiunte al carrello.");
                    }
                    else
                    {
                        Console.WriteLine("Quantità non valida.");
                    }
                }
                else
                {
                    Console.WriteLine("Opzione non valida.");
                }
            }
        }


        private bool IsProviderOpen(string operatingHours)
        {
            // Verifica se il provider è aperto in base all'orario specificato
            // Puoi implementare la logica per confrontare l'orario corrente con l'orario di apertura e chiusura
            // per determinare se il provider è aperto.
            // Ad esempio, puoi utilizzare DateTime per ottenere l'orario corrente e confrontarlo con l'orario di apertura e chiusura.
            // Restituisce true se il provider è aperto, altrimenti restituisce false.
            
            DateTime now = DateTime.Now;
            string[] hours = operatingHours.Split('-');
            if (hours.Length == 2 && DateTime.TryParse(hours[0], out DateTime openTime) && DateTime.TryParse(hours[1], out DateTime closeTime))
            {
                return now >= openTime && now <= closeTime;
            }
            return false;
        }



    }

    
    class Program
    {
        static void Main(string[] args)
        {
            OfficeManager officeManager = new OfficeManager();
            bool continueRunning = true;

            while (continueRunning)
            {
                Console.WriteLine("Benvenuto a TransFoodDelivery!");
                Console.WriteLine("Scegli un servizio:");
                Console.WriteLine("1 - Traduzioni");
                Console.WriteLine("2 - Food");
                Console.WriteLine("3 - Visualizza stato ordini");
                Console.WriteLine("0 - Esci");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        officeManager.HandleTranslations();
                        break;
                    case "2":
                        officeManager.HandleFoodDelivery();
                        break;
                    case "3":
                        officeManager.PrintOrderStatus();
                        break;
                    case "0":
                        continueRunning = false;
                        Console.WriteLine("Uscita dall'applicazione.");
                        break;
                    default:
                        Console.WriteLine("Scelta non valida. Riprova.");
                        break;
                }
            }
        }
    }

}
