using System;

namespace ASN1Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ASN.1 Vehicle Inheritance Demo ===\n");

            // Create a car
            var car = new Car("Toyota", "Camry", 2024, "1HGBH41JXMN109186",
                             4, "Gasoline", 2.5, true);

            // Create a motorcycle
            var motorcycle = new Motorcycle("Harley-Davidson", "Street 750", 2023, "1HD1KB4197K456789",
                                          "Cruiser", 750, false, "Class M");
            motorcycle.AddSafetyFeature("ABS Brakes");
            motorcycle.AddSafetyFeature("LED Headlights");
            motorcycle.AddSafetyFeature("Traction Control");

            // Demonstrate inheritance - polymorphism
            Vehicle[] vehicles = { car, motorcycle };

            foreach (var vehicle in vehicles)
            {
                Console.WriteLine("--- Vehicle Information ---");
                vehicle.DisplayInfo();
                vehicle.StartEngine();
                Console.WriteLine();
            }

            // Demonstrate ASN.1 serialization/deserialization
            Console.WriteLine("=== ASN.1 Serialization Demo ===\n");

            // Serialize car to ASN.1
            Console.WriteLine("Serializing car to ASN.1 format...");
            byte[] carAsn1Data = car.SerializeToAsn1();
            Console.WriteLine($"Car serialized to {carAsn1Data.Length} bytes");

            // Deserialize car from ASN.1
            Console.WriteLine("Deserializing car from ASN.1 format...");
            var deserializedCar = new Car();
            deserializedCar.DeserializeFromAsn1(carAsn1Data);
            Console.WriteLine("Deserialized car:");
            deserializedCar.DisplayInfo();
            Console.WriteLine();

            // Serialize motorcycle to ASN.1
            Console.WriteLine("Serializing motorcycle to ASN.1 format...");
            byte[] motorcycleAsn1Data = motorcycle.SerializeToAsn1();
            Console.WriteLine($"Motorcycle serialized to {motorcycleAsn1Data.Length} bytes");

            // Deserialize motorcycle from ASN.1
            Console.WriteLine("Deserializing motorcycle from ASN.1 format...");
            var deserializedMotorcycle = new Motorcycle();
            deserializedMotorcycle.DeserializeFromAsn1(motorcycleAsn1Data);
            Console.WriteLine("Deserialized motorcycle:");
            deserializedMotorcycle.DisplayInfo();

            Console.WriteLine("\n=== Demo Complete ===");
        }
    }
}