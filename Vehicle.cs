using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace ASN1Demo
{
    /*
    Serializable is for classes, Property is for variables with getters/setters
    within a class.
    */
    [Asn1Serializable(Version = "1.0")]
    public abstract class Vehicle
    {
        [Asn1Property(0)]
        public string Manufacturer { get; set; }
        [Asn1Property(1)]
        public string Model { get; set; }
        [Asn1Property(2)]
        public int Year { get; set; }
        [Asn1Property(3)]
        public string VIN { get; set; }

        protected Vehicle()
        {
            Manufacturer = string.Empty;
            Model = string.Empty;
            VIN = string.Empty;
        }

        protected Vehicle(string manufacturer, string model, int year, string vin)
        {
            Manufacturer = manufacturer;
            Model = model;
            Year = year;
            VIN = vin;
        }

        public abstract void StartEngine();
        public abstract double CalculateFuelEfficiency();

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Vehicle: {Year} {Manufacturer} {Model}");
            Console.WriteLine($"VIN: {VIN}");
        }
    }

    [Asn1Serializable(Version = "1.2")]
    public class Car : Vehicle
    {
        [Asn1Property(4)]
        public int NumberOfDoors { get; set; }
        [Asn1Property(5, Optional = true, DefaultValue = "Gasoline", SinceVersion = "1.1")]
        public string FuelType { get; set; }
        [Asn1Property(6)]
        public double EngineSize { get; set; }
        [Asn1Property(7)]
        public bool HasSunroof { get; set; }

        public Car() : base()
        {
            FuelType = string.Empty;
        }

        public Car(string manufacturer, string model, int year, string vin,
                   int numberOfDoors, string fuelType, double engineSize, bool hasSunroof)
            : base(manufacturer, model, year, vin)
        {
            NumberOfDoors = numberOfDoors;
            FuelType = fuelType;
            EngineSize = engineSize;
            HasSunroof = hasSunroof;
        }

        public override void StartEngine()
        {
            Console.WriteLine($"Starting {EngineSize}L {FuelType} engine in {Manufacturer} {Model}");
        }

        public override double CalculateFuelEfficiency()
        {
            double baseEfficiency = 25.0;
            double adjustment = EngineSize * -2.5 + (NumberOfDoors == 2 ? 2.0 : -1.0);
            return Math.Max(15.0, baseEfficiency + adjustment);
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine($"Type: Car ({NumberOfDoors} doors)");
            Console.WriteLine($"Engine: {EngineSize}L {FuelType}");
            Console.WriteLine($"Sunroof: {(HasSunroof ? "Yes" : "No")}");
            Console.WriteLine($"Fuel Efficiency: {CalculateFuelEfficiency():F1} MPG");
        }

    }

    [Asn1Serializable(Version = "1.0")]
    public class Motorcycle : Vehicle
    {
        [Asn1Property(4)]
        public string BikeType { get; set; }
        [Asn1Property(5)]
        public int EngineCC { get; set; }
        [Asn1Property(6)]
        public bool HasSidecar { get; set; }
        [Asn1Property(7)]
        public string LicenseClass { get; set; }
        [Asn1Property(8)]
        public List<string> SafetyFeatures { get; set; }

        public Motorcycle() : base()
        {
            BikeType = string.Empty;
            LicenseClass = string.Empty;
            SafetyFeatures = new List<string>();
        }

        public Motorcycle(string manufacturer, string model, int year, string vin,
                         string bikeType, int engineCC, bool hasSidecar, string licenseClass)
            : base(manufacturer, model, year, vin)
        {
            BikeType = bikeType;
            EngineCC = engineCC;
            HasSidecar = hasSidecar;
            LicenseClass = licenseClass;
            SafetyFeatures = new List<string>();
        }

        public override void StartEngine()
        {
            Console.WriteLine($"Kickstarting {EngineCC}cc engine on {BikeType} motorcycle");
        }

        public override double CalculateFuelEfficiency()
        {
            double baseEfficiency = 45.0;
            double adjustment = (EngineCC / 100.0) * -1.5 + (HasSidecar ? -5.0 : 0.0);
            return Math.Max(25.0, baseEfficiency + adjustment);
        }

        public override void DisplayInfo()
        {
            base.DisplayInfo();
            Console.WriteLine($"Type: {BikeType} Motorcycle");
            Console.WriteLine($"Engine: {EngineCC}cc");
            Console.WriteLine($"Sidecar: {(HasSidecar ? "Yes" : "No")}");
            Console.WriteLine($"License Required: {LicenseClass}");
            Console.WriteLine($"Safety Features: {string.Join(", ", SafetyFeatures)}");
            Console.WriteLine($"Fuel Efficiency: {CalculateFuelEfficiency():F1} MPG");
        }

        public void AddSafetyFeature(string feature)
        {
            if (!SafetyFeatures.Contains(feature))
            {
                SafetyFeatures.Add(feature);
            }
        }

    }
}