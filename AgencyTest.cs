using System;

namespace ASN1Demo
{
    class AgencyTest
    {
        public static void Test0()
        {
            // Test 0: Simple loading and saving of intelligence agency
            var intelTest0 = new IntelligenceAgency("CIA", ConfidentialityLevel.Secret, "USA");
            intelTest0.Agents.Add("Agent A");
            intelTest0.Operations.Add("Operation X");

            Console.WriteLine("Saving Intelligence Agency to ASN.1 format...");
            byte[] data = Asn1Serializer.SerializeToAsn1(intelTest0);
            Console.WriteLine($"Serialized to {data.Length} bytes");
            Console.WriteLine($"String (UTF-8): {System.Text.Encoding.UTF8.GetString(data)}");
            Console.WriteLine($"Data (hex): {BitConverter.ToString(data).Replace("-", " ")}");

            Console.WriteLine("Loading Intelligence Agency from ASN.1 format...");
            var intelTest0Loaded = new IntelligenceAgency();
            Asn1Serializer.DeserializeFromAsn1(intelTest0Loaded, data);
            Console.WriteLine($"Loaded Agency Name: {intelTest0Loaded.AgencyName}");
            Console.WriteLine($"Loaded Confidentiality: {intelTest0Loaded.Confidentiality}");
            Console.WriteLine($"Loaded Agents: {string.Join(", ", intelTest0Loaded.Agents)}");
            Console.WriteLine($"Loaded Operations: {string.Join(", ", intelTest0Loaded.Operations)}");
            Console.WriteLine($"Loaded Country: {intelTest0Loaded.Country}");
        }

        // Test 1: Load data from Test 0 (v1.0) into v1.1 class with new variable with default value.
        public static void Test1()
        {
            // These are results from the first test. 
            byte[] data = { 0x30, 0x49, 0x0C, 0x03, 0x31, 0x2E, 0x30, 0x0C, 0x1B, 0x41, 0x53, 0x4E, 0x31, 0x44, 0x65, 0x6D, 0x6F, 0x2E, 0x49, 0x6E, 0x74, 0x65, 0x6C, 0x6C, 0x69, 0x67, 0x65, 0x6E, 0x63, 0x65, 0x41, 0x67, 0x65, 0x6E, 0x63, 0x79, 0x0C, 0x03, 0x43, 0x49, 0x41, 0x02, 0x01, 0x03, 0x30, 0x09, 0x0C, 0x07, 0x41, 0x67, 0x65, 0x6E, 0x74, 0x20, 0x41, 0x30, 0x0D, 0x0C, 0x0B, 0x4F, 0x70, 0x65, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x58, 0x0C, 0x03, 0x55, 0x53, 0x41 };

            // The classes shouldn't matter here. Using the data from Test0,
            // try to load v1.0 class into v1.1 class.
            var intelTest1Loaded = new IntelligenceAgencyV1();
            Asn1Serializer.DeserializeFromAsn1(intelTest1Loaded, data);
            Console.WriteLine($"Loaded Agency Name: {intelTest1Loaded.AgencyName}");
            Console.WriteLine($"Loaded Confidentiality: {intelTest1Loaded.Confidentiality}");
            Console.WriteLine($"Loaded Agents: {string.Join(", ", intelTest1Loaded.Agents)}");
            Console.WriteLine($"Loaded Operations: {string.Join(", ", intelTest1Loaded.Operations)}");
            Console.WriteLine($"Loaded Country: {intelTest1Loaded.Country}");
            Console.WriteLine($"Loaded Budget: {intelTest1Loaded.BudgetInMillions} million (should be default 0)");
        }

        // Test 2: Load data from Test 0 (v1.0) into v1.2 class with new variable with default value.
        public static void Test2()
        {
            // These are results from the first test. 
            byte[] data = { 0x30, 0x49, 0x0C, 0x03, 0x31, 0x2E, 0x30, 0x0C, 0x1B, 0x41, 0x53, 0x4E, 0x31, 0x44, 0x65, 0x6D, 0x6F, 0x2E, 0x49, 0x6E, 0x74, 0x65, 0x6C, 0x6C, 0x69, 0x67, 0x65, 0x6E, 0x63, 0x65, 0x41, 0x67, 0x65, 0x6E, 0x63, 0x79, 0x0C, 0x03, 0x43, 0x49, 0x41, 0x02, 0x01, 0x03, 0x30, 0x09, 0x0C, 0x07, 0x41, 0x67, 0x65, 0x6E, 0x74, 0x20, 0x41, 0x30, 0x0D, 0x0C, 0x0B, 0x4F, 0x70, 0x65, 0x72, 0x61, 0x74, 0x69, 0x6F, 0x6E, 0x20, 0x58, 0x0C, 0x03, 0x55, 0x53, 0x41 };

            // The classes shouldn't matter here. Using the data from Test0,
            // try to load v1.0 class into v1.2 class.
            var intelTest2Loaded = new IntelligenceAgencyV2();
            Asn1Serializer.DeserializeFromAsn1(intelTest2Loaded, data);
            Console.WriteLine($"Loaded Agency Name: {intelTest2Loaded.AgencyName}");
            Console.WriteLine($"Loaded Confidentiality: {intelTest2Loaded.Confidentiality}");
            Console.WriteLine($"Loaded Agents: {string.Join(", ", intelTest2Loaded.Agents)}");
            Console.WriteLine($"Loaded Operations: {string.Join(", ", intelTest2Loaded.Operations)}");
            Console.WriteLine($"Loaded Country: {intelTest2Loaded.Country}");
            Console.WriteLine($"Loaded Budget: {intelTest2Loaded.BudgetInMillions} million (should be default 0)");
        }
    }
}