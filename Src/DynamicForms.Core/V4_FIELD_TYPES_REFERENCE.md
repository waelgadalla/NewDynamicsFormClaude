# DynamicForms.Core.V4 - Field Types Reference

## ?? Complete List of Supported Field Types

Based on the DynamicForms.Core.V4 architecture (inherited from V3), here are all supported field types:

---

## ?? Text Input Fields

### TextBox
- **Purpose**: Single-line text input
- **Example**: First name, Last name, Job title
- **Validation**: MinLength, MaxLength, Pattern
- **Supports**: Required, Placeholder, Help text

### TextArea
- **Purpose**: Multi-line text input
- **Example**: Comments, Description, Address
- **Validation**: MinLength, MaxLength
- **Supports**: Row count, Column count

### EmailTextBox
- **Purpose**: Email address input with validation
- **Example**: Contact email
- **Validation**: Email pattern validation
- **Supports**: Multiple email addresses

### TelephoneTextBox
- **Purpose**: Phone number input
- **Example**: Phone, Mobile, Fax
- **Validation**: Phone format validation
- **Supports**: International formats

### URLTextBox
- **Purpose**: Website URL input
- **Example**: Company website, Social media links
- **Validation**: URL format validation

### NumberTextBox
- **Purpose**: Numeric input only
- **Example**: Age, Quantity, Amount
- **Validation**: Min, Max, Step
- **Supports**: Integer or decimal

### PasswordTextBox
- **Purpose**: Password input (masked)
- **Example**: Password, PIN
- **Validation**: MinLength, Pattern (complexity rules)

---

## ?? Date & Time Fields

### DateBox
- **Purpose**: Date selection
- **Example**: Birth date, Start date, End date
- **Validation**: MinDate, MaxDate
- **Supports**: Date picker UI
- **Config**: `DateConfig` with AllowFuture, AllowPast

### DateRangePicker
- **Purpose**: Select a date range (from/to)
- **Example**: Project duration, Vacation dates
- **Validation**: StartDate < EndDate
- **Supports**: Linked date pickers

### TimeBox
- **Purpose**: Time selection
- **Example**: Meeting time, Opening hours
- **Validation**: Time format (HH:mm)
- **Supports**: 12-hour or 24-hour format

### DateTimeBox
- **Purpose**: Combined date and time selection
- **Example**: Appointment datetime, Event start
- **Validation**: DateTime range
- **Supports**: Timezone handling

---

## ?? Selection Fields

### DropDown (DropDownList)
- **Purpose**: Single selection from a list
- **Example**: Country, Province, Status
- **Data Source**: Static options OR CodeSet
- **Supports**: Search/filter, Default value

### RadioButtonList
- **Purpose**: Single selection displayed as radio buttons
- **Example**: Yes/No, Gender, Agreement
- **Data Source**: Static options OR CodeSet
- **Supports**: Inline or vertical layout

### CheckBox
- **Purpose**: Single boolean value
- **Example**: Agree to terms, Subscribe to newsletter
- **Data Source**: Boolean (true/false)
- **Supports**: Default checked state

### CheckBoxList
- **Purpose**: Multiple selections
- **Example**: Interests, Languages spoken, Features
- **Data Source**: Static options OR CodeSet
- **Supports**: Select all/none

---

## ?? Advanced Input Fields

### AutoComplete
- **Purpose**: Search and select from large datasets
- **Example**: Species name, Organization, Person
- **Data Source**: API endpoint
- **Config**: `AutoCompleteConfig`
  - DataSourceUrl
  - MinCharacters
  - ValueField / DisplayField
  - ItemTemplate (Handlebars)
- **Supports**: Typeahead, Fuzzy search

### AddressLookup
- **Purpose**: Structured address input with validation
- **Example**: Mailing address, Business location
- **Data Source**: Address API (e.g., Canada Post)
- **Supports**: Postal code lookup, Autocomplete

### CurrencyTextBox
- **Purpose**: Monetary value input
- **Example**: Budget amount, Grant amount, Fee
- **Validation**: Min, Max, Currency format
- **Supports**: Currency symbol, Thousands separator

---

## ?? File & Media Fields

### FileUpload
- **Purpose**: Upload single or multiple files
- **Example**: Resume, Supporting documents, Photos
- **Config**: `FileUploadConfig`
  - AllowedExtensions
  - MaxFileSizeBytes
  - AllowMultiple
  - ScanRequired (virus scanning)
- **Supports**: Drag-and-drop, Progress indicator

### ImageUpload
- **Purpose**: Image-specific upload with preview
- **Example**: Profile photo, Logo, Gallery
- **Validation**: Image format, Dimensions, File size
- **Supports**: Crop, Resize, Thumbnail preview

---

## ??? Structural Fields

### Section
- **Purpose**: Grouping container for related fields
- **Example**: Personal Information, Contact Details
- **Supports**: Collapsible, Icon, Border
- **Can Contain**: Any field types (children)

### Group (Panel)
- **Purpose**: Visual grouping without section header
- **Example**: Address fields, Name fields
- **Supports**: Border, Padding, Background
- **Can Contain**: Any field types

### Container
- **Purpose**: Generic layout container
- **Example**: Column layouts, Card layouts
- **Supports**: Custom CSS classes
- **Can Contain**: Any field types

---

## ?? Conditional & Dynamic Fields

### Conditional (ConditionalGroup)
- **Purpose**: Show/hide based on conditions
- **Example**: Show "Business Number" if org type = "Business"
- **Uses**: ConditionalRules from V4
- **Can Contain**: Any field types

---

## ?? Complex Data Fields

### DataGrid (Table, Modal)
- **Purpose**: Repeating data entry (rows/columns)
- **Example**: Line items, Budget breakdown, Team members
- **Config**: `DataGridConfig`
  - AllowAdd, AllowEdit, AllowDelete
  - MaxRows
  - EditorMode ("Modal" or "Inline")
  - Columns (FormFieldSchema[])
- **Supports**: Add/Edit/Delete rows, Pagination

### ModalTable
- **Purpose**: Table with popup editor
- **Example**: Experience history, Education records
- **Supports**: Modal dialog for editing
- **Can Contain**: FormFieldSchema[] as columns

---

## ?? Informational Fields

### Label
- **Purpose**: Display-only text
- **Example**: Instructions, Section description
- **Supports**: HTML formatting, Icons

### Heading
- **Purpose**: Title or heading text
- **Example**: Section titles, Page headers
- **Supports**: H1-H6 levels, Styling

### Paragraph
- **Purpose**: Block of informational text
- **Example**: Help text, Guidelines, Disclaimers
- **Supports**: HTML formatting, Links

### Divider (Separator)
- **Purpose**: Visual separation between sections
- **Example**: Horizontal rule
- **Supports**: Thickness, Color, Margin

---

## ?? Specialized Fields

### RichTextEditor
- **Purpose**: Formatted text input (WYSIWYG)
- **Example**: Long descriptions, Proposals, Narratives
- **Supports**: Bold, Italic, Lists, Links, Tables
- **Config**: Toolbar options

### Signature
- **Purpose**: Digital signature capture
- **Example**: Agreement signature, Attestation
- **Supports**: Touch/mouse drawing
- **Validation**: Required signature

### Rating
- **Purpose**: Star or numeric rating
- **Example**: Satisfaction rating, Skill level
- **Validation**: Min, Max
- **Supports**: Star icons, Number scale

### Slider
- **Purpose**: Numeric input via slider
- **Example**: Budget allocation, Priority level
- **Validation**: Min, Max, Step
- **Supports**: Labels, Ticks

### ColorPicker
- **Purpose**: Color selection
- **Example**: Theme color, Brand color
- **Validation**: Hex or RGB format
- **Supports**: Color palette, Hex input

---

## ??? Geospatial Fields

### MapPicker
- **Purpose**: Select a location on a map
- **Example**: Project location, Service area
- **Supports**: Lat/Long coordinates, Address lookup
- **Config**: Map provider (Google, Bing, OpenStreetMap)

### CoordinateInput
- **Purpose**: Manual lat/long entry
- **Example**: GPS coordinates
- **Validation**: Latitude (-90 to 90), Longitude (-180 to 180)

---

## ?? Security & Compliance Fields

### CaptchaField
- **Purpose**: Bot prevention
- **Example**: Form submission verification
- **Supports**: reCAPTCHA, hCaptcha

### ConsentCheckbox
- **Purpose**: Explicit consent capture
- **Example**: GDPR consent, Terms acceptance
- **Validation**: Must be checked (required)
- **Supports**: Link to terms/policy

---

## ?? Field Type Categories Summary

| Category | Field Types | Count |
|----------|-------------|-------|
| **Text Input** | TextBox, TextArea, Email, Tel, URL, Number, Password | 7 |
| **Date & Time** | DateBox, DateRangePicker, TimeBox, DateTimeBox | 4 |
| **Selection** | DropDown, RadioButtonList, CheckBox, CheckBoxList | 4 |
| **Advanced Input** | AutoComplete, AddressLookup, CurrencyTextBox | 3 |
| **File & Media** | FileUpload, ImageUpload | 2 |
| **Structural** | Section, Group, Container | 3 |
| **Conditional** | Conditional, ConditionalGroup | 2 |
| **Complex Data** | DataGrid, ModalTable | 2 |
| **Informational** | Label, Heading, Paragraph, Divider | 4 |
| **Specialized** | RichTextEditor, Signature, Rating, Slider, ColorPicker | 5 |
| **Geospatial** | MapPicker, CoordinateInput | 2 |
| **Security** | CaptchaField, ConsentCheckbox | 2 |
| **TOTAL** | | **40+ types** |

---

## ??? Field Type Configuration

### Type-Specific Configurations in V4

```csharp
// Base class
public abstract record FieldTypeConfig { }

// Specific configurations
public record AutoCompleteConfig : FieldTypeConfig { ... }
public record DataGridConfig : FieldTypeConfig { ... }
public record FileUploadConfig : FieldTypeConfig { ... }
public record DateConfig : FieldTypeConfig { ... }
```

### Usage Example

```csharp
var autoCompleteField = new FormFieldSchema
{
Id = "species_name",
    FieldType = "AutoComplete",
    LabelEn = "Species Name",
    TypeConfig = new AutoCompleteConfig
    {
DataSourceUrl = "/api/species/search",
      QueryParameter = "q",
        MinCharacters = 3,
        ValueField = "Id",
      DisplayField = "CommonName",
        ItemTemplate = "{{CommonName}} <span class='text-muted'>({{ScientificName}})</span>"
    }
};
```

---

## ?? Key Properties for All Field Types

Every `FormFieldSchema` has these core properties:

```csharp
public record FormFieldSchema
{
    // Identity
    public required string Id { get; init; }
public required string FieldType { get; init; }  // ? Field type goes here
    public int Order { get; init; } = 1;
    
    // Hierarchy
    public string? ParentId { get; init; }
    public RelationshipType Relationship { get; init; }
    
    // Text
    public string? LabelEn { get; init; }
    public string? LabelFr { get; init; }
    public string? DescriptionEn { get; init; }
    public string? DescriptionFr { get; init; }
    
    // Validation
    public FieldValidationConfig? Validation { get; init; }
    
    // ?? V4: Conditional Rules
    public ConditionalRule[]? ConditionalRules { get; init; }
    
  // Data Source
    public int? CodeSetId { get; init; }
    public FieldOption[]? Options { get; init; }
    
    // Type-Specific Config
    public FieldTypeConfig? TypeConfig { get; init; }  // ? Configuration goes here

    // Layout
    public int? WidthClass { get; init; }
    public string? CssClasses { get; init; }
    public bool IsVisible { get; init; } = true;
    public bool IsReadOnly { get; init; }
}
```

---

## ?? Common Field Type Patterns

### Pattern 1: Simple Input Field
```csharp
new FormFieldSchema
{
    Id = "first_name",
    FieldType = "TextBox",
    LabelEn = "First Name",
    Validation = new FieldValidationConfig { IsRequired = true }
}
```

### Pattern 2: Selection Field with CodeSet
```csharp
new FormFieldSchema
{
  Id = "province",
    FieldType = "DropDown",
    LabelEn = "Province",
    CodeSetId = 1 // Provinces CodeSet
}
```

### Pattern 3: Conditional Field (V4)
```csharp
new FormFieldSchema
{
    Id = "business_number",
    FieldType = "TextBox",
    LabelEn = "Business Number",
    ConditionalRules = new[]
  {
        new ConditionalRule
      {
      Id = "show_if_business",
        Action = "show",
            Condition = new Condition
            {
       Field = "org_type",
        Operator = "eq",
      Value = "Business"
         }
        }
    }
}
```

### Pattern 4: Complex Field with TypeConfig
```csharp
new FormFieldSchema
{
    Id = "team_members",
 FieldType = "DataGrid",
    LabelEn = "Team Members",
  TypeConfig = new DataGridConfig
    {
        AllowAdd = true,
   AllowEdit = true,
     AllowDelete = true,
        MaxRows = 10,
        EditorMode = "Modal",
 Columns = new[]
        {
     FormFieldSchema.CreateTextField("name", "Name", isRequired: true),
            FormFieldSchema.CreateTextField("role", "Role"),
    FormFieldSchema.CreateTextField("email", "Email")
        }
    }
}
```

---

## ?? Factory Methods in V4

DynamicForms.Core.V4 provides factory methods for common field types:

```csharp
// Text field
FormFieldSchema.CreateTextField(
    id: "first_name",
    labelEn: "First Name",
    labelFr: "Prénom",
    isRequired: true,
    order: 1
);

// Section
FormFieldSchema.CreateSection(
    id: "personal_info",
    titleEn: "Personal Information",
    titleFr: "Informations personnelles",
    order: 1
);

// Dropdown
FormFieldSchema.CreateDropDown(
    id: "province",
    labelEn: "Province",
    options: new[]
    {
        new FieldOption("ON", "Ontario", "Ontario"),
        new FieldOption("QC", "Quebec", "Québec")
    },
    isRequired: true,
    order: 2
);
```

---

## ?? Best Practices

1. **Use Semantic Field Types**: Choose the most specific type (e.g., `EmailTextBox` instead of `TextBox`)
2. **Leverage CodeSets**: Use CodeSets for frequently updated options instead of hardcoded Options
3. **Add Conditional Rules**: Use V4's ConditionalRules for dynamic field behavior
4. **Configure TypeConfig**: Provide type-specific configuration for advanced fields
5. **Set Proper Validation**: Use `FieldValidationConfig` for data integrity
6. **Organize with Sections**: Group related fields using Section fields
7. **Consider Mobile**: Use appropriate WidthClass for responsive layouts

---

**Version**: 4.0.0  
**Last Updated**: January 2025  
**Documentation**: Complete field type reference for DynamicForms.Core.V4
