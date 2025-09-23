using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace ASN1Demo
{
    public abstract class Vehicle
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
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
        public abstract byte[] SerializeToAsn1();
        public abstract void DeserializeFromAsn1(byte[] data);

        public virtual void DisplayInfo()
        {
            Console.WriteLine($"Vehicle: {Year} {Manufacturer} {Model}");
            Console.WriteLine($"VIN: {VIN}");
        }
    }

    public class Car : Vehicle
    {
        public int NumberOfDoors { get; set; }
        public string FuelType { get; set; }
        public double EngineSize { get; set; }
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

        public override byte[] SerializeToAsn1()
        {
            var writer = new AsnWriter(AsnEncodingRules.DER);

            using (writer.PushSequence())
            {
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, Manufacturer);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, Model);
                writer.WriteInteger(Year);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, VIN);
                writer.WriteInteger(NumberOfDoors);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, FuelType);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, EngineSize.ToString());
                writer.WriteBoolean(HasSunroof);
            }

            return writer.Encode();
        }

        public override void DeserializeFromAsn1(byte[] data)
        {
            var reader = new AsnReader(data, AsnEncodingRules.DER);

            var sequence = reader.ReadSequence();
            Manufacturer = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            Model = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            Year = (int)sequence.ReadInteger();
            VIN = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            NumberOfDoors = (int)sequence.ReadInteger();
            FuelType = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            EngineSize = double.Parse(sequence.ReadCharacterString(UniversalTagNumber.UTF8String));
            HasSunroof = sequence.ReadBoolean();
        }
    }

    public class Motorcycle : Vehicle
    {
        public string BikeType { get; set; }
        public int EngineCC { get; set; }
        public bool HasSidecar { get; set; }
        public string LicenseClass { get; set; }
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

        public override byte[] SerializeToAsn1()
        {
            var writer = new AsnWriter(AsnEncodingRules.DER);

            using (writer.PushSequence())
            {
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, Manufacturer);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, Model);
                writer.WriteInteger(Year);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, VIN);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, BikeType);
                writer.WriteInteger(EngineCC);
                writer.WriteBoolean(HasSidecar);
                writer.WriteCharacterString(UniversalTagNumber.UTF8String, LicenseClass);

                using (writer.PushSequence())
                {
                    foreach (var feature in SafetyFeatures)
                    {
                        writer.WriteCharacterString(UniversalTagNumber.UTF8String, feature);
                    }
                }
            }

            return writer.Encode();
        }

        public override void DeserializeFromAsn1(byte[] data)
        {
            var reader = new AsnReader(data, AsnEncodingRules.DER);

            var sequence = reader.ReadSequence();
            Manufacturer = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            Model = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            Year = (int)sequence.ReadInteger();
            VIN = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            BikeType = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
            EngineCC = (int)sequence.ReadInteger();
            HasSidecar = sequence.ReadBoolean();
            LicenseClass = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);

            var featuresSequence = sequence.ReadSequence();
            SafetyFeatures = new List<string>();
            while (featuresSequence.HasData)
            {
                SafetyFeatures.Add(featuresSequence.ReadCharacterString(UniversalTagNumber.UTF8String));
            }
        }
    }
}