using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Text;

namespace ASN1Demo
{
    public enum ConfidentialityLevel
    {
        Public = 0,
        Internal = 1,
        Confidential = 2,
        Secret = 3
    }

    // Test case 0. Saving and loading simple class with basic types.
    [Asn1Serializable(Version = "1.0")]
    public abstract class Agency // Representing a government agency
    {
        [Asn1Property(0)]
        public string AgencyName { get; set; }
        [Asn1Property(1)]
        public ConfidentialityLevel Confidentiality { get; set; }
        [Asn1Property(2)]
        public List<string> Agents { get; set; }

        protected Agency()
        {
            AgencyName = string.Empty;
            Confidentiality = ConfidentialityLevel.Public;
            Agents = new List<string>();
        }

        protected Agency(string agencyName, ConfidentialityLevel confidentiality)
        {
            AgencyName = agencyName;
            Confidentiality = confidentiality;
            Agents = new List<string>();
        }
    }

    [Asn1Serializable(Version = "1.0")]
    public class IntelligenceAgency : Agency
    {
        [Asn1Property(3)]
        public List<string> Operations { get; set; }
        [Asn1Property(4)]
        public string Country { get; set; }

        public IntelligenceAgency() : base()
        {
            Operations = new List<string>();
            Country = string.Empty;
        }

        public IntelligenceAgency(string agencyName, ConfidentialityLevel confidentiality, string country)
            : base(agencyName, confidentiality)
        {
            Operations = new List<string>();
            Country = country;
        }
    }

    // Default and Optional should work together
    [Asn1Serializable(Version = "1.1")]
    public class IntelligenceAgencyV1 : Agency
    {
        [Asn1Property(3)]
        public List<string> Operations { get; set; }
        [Asn1Property(4)]
        public string Country { get; set; }
        [Asn1Property(5, DefaultValue = "0", Optional = true, SinceVersion = "1.1")]
        public int BudgetInMillions { get; set; }

        public IntelligenceAgencyV1() : base()
        {
            Operations = new List<string>();
            Country = string.Empty;
            BudgetInMillions = 0;
        }

        public IntelligenceAgencyV1(string agencyName, ConfidentialityLevel confidentiality, string country, int budgetInMillions = 0)
            : base(agencyName, confidentiality)
        {
            Operations = new List<string>();
            Country = country;
            BudgetInMillions = budgetInMillions;
        }
    }


    // Default and Optional should work together
    [Asn1Serializable(Version = "1.2")]
    public class IntelligenceAgencyV2 : Agency
    {
        [Asn1Property(3)]
        public List<string> Operations { get; set; }
        [Asn1Property(4, DefaultValue = "0", Optional = true, SinceVersion = "1.2")]
        public int BudgetInMillions { get; set; }
        [Asn1Property(5)]
        public string Country { get; set; }

        public IntelligenceAgencyV2() : base()
        {
            Operations = new List<string>();
            Country = string.Empty;
            BudgetInMillions = 0;
        }

        public IntelligenceAgencyV2(string agencyName, ConfidentialityLevel confidentiality, string country, int budgetInMillions = 0)
            : base(agencyName, confidentiality)
        {
            Operations = new List<string>();
            Country = country;
            BudgetInMillions = budgetInMillions;
        }
    }

    // For testing reordered data
    [Asn1Serializable(Version = "1.0")]
    public class LawAgency : Agency
    {
        [Asn1Property(3)]
        public string Country { get; set; }
        [Asn1Property(4)]
        public List<string> Operations { get; set; }

        public LawAgency() : base()
        {
            Operations = new List<string>();
            Country = string.Empty;
        }

        public LawAgency(string agencyName, ConfidentialityLevel confidentiality, string country)
            : base(agencyName, confidentiality)
        {
            Operations = new List<string>();
            Country = country;
        }
    }
}