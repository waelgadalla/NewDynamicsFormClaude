using DynamicForms.Core.V2.Schemas;

namespace DynamicForms.Core.V2.Services;

/// <summary>
/// Sample CodeSets for testing and demonstration purposes.
/// Shows various CodeSet patterns and use cases.
/// </summary>
public static class SampleCodeSets
{
    /// <summary>
    /// Gets a collection of common sample CodeSets
    /// </summary>
    public static CodeSetSchema[] GetSampleCodeSets()
    {
    return new[]
        {
            CanadianProvinces,
    OrganizationTypes,
     YesNoOptions,
            ProjectStatuses,
            FundingRanges
    };
    }

    /// <summary>
    /// Canadian Provinces and Territories
  /// </summary>
    public static CodeSetSchema CanadianProvinces
    {
        get
        {
    var schema = CodeSetSchema.Create(
      id: 1,
         code: "PROVINCES_CA",
     nameEn: "Canadian Provinces and Territories",
             items: new[]
          {
   new CodeSetItem { Value = "", TextEn = "-- Select Province --", TextFr = "-- Sélectionner une province --", Order = 0 },
        new CodeSetItem { Value = "AB", TextEn = "Alberta", TextFr = "Alberta", Order = 1 },
             new CodeSetItem { Value = "BC", TextEn = "British Columbia", TextFr = "Colombie-Britannique", Order = 2 },
        new CodeSetItem { Value = "MB", TextEn = "Manitoba", TextFr = "Manitoba", Order = 3 },
    new CodeSetItem { Value = "NB", TextEn = "New Brunswick", TextFr = "Nouveau-Brunswick", Order = 4 },
            new CodeSetItem { Value = "NL", TextEn = "Newfoundland and Labrador", TextFr = "Terre-Neuve-et-Labrador", Order = 5 },
   new CodeSetItem { Value = "NS", TextEn = "Nova Scotia", TextFr = "Nouvelle-Écosse", Order = 6 },
 new CodeSetItem { Value = "ON", TextEn = "Ontario", TextFr = "Ontario", Order = 7 },
 new CodeSetItem { Value = "PE", TextEn = "Prince Edward Island", TextFr = "Île-du-Prince-Édouard", Order = 8 },
   new CodeSetItem { Value = "QC", TextEn = "Quebec", TextFr = "Québec", Order = 9 },
        new CodeSetItem { Value = "SK", TextEn = "Saskatchewan", TextFr = "Saskatchewan", Order = 10 },
            new CodeSetItem { Value = "NT", TextEn = "Northwest Territories", TextFr = "Territoires du Nord-Ouest", Order = 11 },
new CodeSetItem { Value = "NU", TextEn = "Nunavut", TextFr = "Nunavut", Order = 12 },
 new CodeSetItem { Value = "YT", TextEn = "Yukon", TextFr = "Yukon", Order = 13 }
        });

    return schema with
            {
        NameFr = "Provinces et territoires canadiens",
    Category = "Geography",
        IsSystemManaged = true,
     Tags = new[] { "canada", "geography", "provinces" }
            };
        }
    }

    /// <summary>
    /// Organization Types (matches your schema.json example)
    /// </summary>
    public static CodeSetSchema OrganizationTypes
    {
      get
        {
            var schema = CodeSetSchema.Create(
       id: 99,
      code: "ORG_TYPES",
    nameEn: "Organization Types",
           items: new[]
                {
       new CodeSetItem { Value = "", TextEn = "-- Select Type --", TextFr = "-- Sélectionner le type --", Order = 0 },
         new CodeSetItem { Value = "individual", TextEn = "Individual", TextFr = "Individuel", Order = 1 },
  new CodeSetItem { Value = "non_profit", TextEn = "Non-Profit Organization", TextFr = "Organisation à but non lucratif", Order = 2 },
      new CodeSetItem { Value = "business", TextEn = "Private Business", TextFr = "Entreprise privée", Order = 3 },
  new CodeSetItem { Value = "government", TextEn = "Government Agency", TextFr = "Agence gouvernementale", Order = 4, IsActive = true }
           });

       return schema with
            {
          NameFr = "Types d'organisation",
                Category = "Organizations",
    IsSystemManaged = true,
 Tags = new[] { "organization", "entity-type" }
            };
        }
    }

    /// <summary>
    /// Simple Yes/No Options
    /// </summary>
public static CodeSetSchema YesNoOptions
    {
        get
     {
            var schema = CodeSetSchema.Create(
                id: 2,
        code: "YES_NO",
    nameEn: "Yes/No Options",
       items: new[]
  {
             new CodeSetItem { Value = "yes", TextEn = "Yes", TextFr = "Oui", Order = 1 },
      new CodeSetItem { Value = "no", TextEn = "No", TextFr = "Non", Order = 2 }
      });

   return schema with
     {
       NameFr = "Options Oui/Non",
          Category = "Common",
      IsSystemManaged = true,
      Tags = new[] { "boolean", "common" }
            };
   }
    }

    /// <summary>
    /// Project Status Options
    /// </summary>
    public static CodeSetSchema ProjectStatuses
    {
        get
        {
 var schema = CodeSetSchema.Create(
      id: 10,
      code: "PROJECT_STATUS",
    nameEn: "Project Statuses",
                items: new[]
      {
         new CodeSetItem { Value = "draft", TextEn = "Draft", TextFr = "Brouillon", Order = 1, CssClass = "status-draft" },
         new CodeSetItem { Value = "submitted", TextEn = "Submitted", TextFr = "Soumis", Order = 2, CssClass = "status-submitted" },
         new CodeSetItem { Value = "under_review", TextEn = "Under Review", TextFr = "En cours d'examen", Order = 3, CssClass = "status-review" },
    new CodeSetItem { Value = "approved", TextEn = "Approved", TextFr = "Approuvé", Order = 4, CssClass = "status-approved" },
   new CodeSetItem { Value = "rejected", TextEn = "Rejected", TextFr = "Rejeté", Order = 5, CssClass = "status-rejected" },
        new CodeSetItem { Value = "completed", TextEn = "Completed", TextFr = "Terminé", Order = 6, CssClass = "status-completed" }
       });

            return schema with
            {
                NameFr = "Statuts de projet",
     Category = "Workflow",
          Tags = new[] { "status", "workflow" }
       };
        }
    }

    /// <summary>
    /// Funding Amount Ranges
    /// </summary>
    public static CodeSetSchema FundingRanges
    {
 get
        {
            var schema = CodeSetSchema.Create(
        id: 20,
       code: "FUNDING_RANGES",
    nameEn: "Funding Amount Ranges",
     items: new[]
            {
  new CodeSetItem { Value = "0-5000", TextEn = "Up to $5,000", TextFr = "Jusqu'à 5 000 $", Order = 1 },
       new CodeSetItem { Value = "5000-25000", TextEn = "$5,000 - $25,000", TextFr = "5 000 $ - 25 000 $", Order = 2 },
       new CodeSetItem { Value = "25000-50000", TextEn = "$25,000 - $50,000", TextFr = "25 000 $ - 50 000 $", Order = 3 },
 new CodeSetItem { Value = "50000-100000", TextEn = "$50,000 - $100,000", TextFr = "50 000 $ - 100 000 $", Order = 4 },
      new CodeSetItem { Value = "100000+", TextEn = "Over $100,000", TextFr = "Plus de 100 000 $", Order = 5 }
   });

            return schema with
            {
  NameFr = "Fourchettes de financement",
       Category = "Funding",
 Tags = new[] { "funding", "amounts" }
  };
        }
    }
}
