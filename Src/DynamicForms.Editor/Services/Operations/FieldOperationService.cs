using DynamicForms.Core.V2.Schemas;

namespace DynamicForms.Editor.Services.Operations;

/// <summary>
/// Service providing factory methods for creating form fields and related configurations.
/// Simplifies the creation of properly configured FormFieldSchema instances.
/// </summary>
public class FieldOperationService
{
    // ========================================================================
    // BASIC FIELD TYPES
    // ========================================================================

    /// <summary>
    /// Creates a text box field with common settings.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="maxLength">Maximum text length (optional)</param>
    /// <param name="placeholder">Placeholder text (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a text field</returns>
    public FormFieldSchema CreateTextField(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int? maxLength = null,
        string? placeholder = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "TextBox",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            MaxLength = maxLength,
            PlaceholderEn = placeholder,
            PlaceholderFr = placeholder,
            Order = order
        };
    }

    /// <summary>
    /// Creates a text area field for multi-line text input.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="rows">Number of visible text rows (default: 4)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="maxLength">Maximum text length (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a text area</returns>
    public FormFieldSchema CreateTextArea(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        int rows = 4,
        bool isRequired = false,
        int? maxLength = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "TextArea",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            MaxLength = maxLength,
            Order = order,
            TypeConfig = new TextAreaConfig(rows)
        };
    }

    /// <summary>
    /// Creates a dropdown field with options.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="options">List of dropdown options</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a dropdown</returns>
    public FormFieldSchema CreateDropDown(
        string fieldId,
        string labelEn,
        FieldOption[] options,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "DropDown",
            LabelEn = labelEn,
            LabelFr = labelFr,
            Options = options,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a dropdown field that uses a code set from the database.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="codeSetId">ID of the code set to use</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a dropdown with code set</returns>
    public FormFieldSchema CreateCodeSetDropDown(
        string fieldId,
        string labelEn,
        int codeSetId,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "DropDown",
            LabelEn = labelEn,
            LabelFr = labelFr,
            CodeSetId = codeSetId,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a radio button group field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="options">List of radio button options</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for radio buttons</returns>
    public FormFieldSchema CreateRadioButtons(
        string fieldId,
        string labelEn,
        FieldOption[] options,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "RadioButtons",
            LabelEn = labelEn,
            LabelFr = labelFr,
            Options = options,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a checkbox field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a checkbox</returns>
    public FormFieldSchema CreateCheckbox(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Checkbox",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a checkbox list field (multiple checkboxes).
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="options">List of checkbox options</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether at least one checkbox is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a checkbox list</returns>
    public FormFieldSchema CreateCheckboxList(
        string fieldId,
        string labelEn,
        FieldOption[] options,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "CheckboxList",
            LabelEn = labelEn,
            LabelFr = labelFr,
            Options = options,
            IsRequired = isRequired,
            Order = order
        };
    }

    // ========================================================================
    // DATE AND TIME FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a date picker field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="minDate">Minimum allowed date (optional)</param>
    /// <param name="maxDate">Maximum allowed date (optional)</param>
    /// <param name="allowFutureDates">Whether dates in the future are allowed (default: true)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a date picker</returns>
    public FormFieldSchema CreateDatePicker(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        DateTime? minDate = null,
        DateTime? maxDate = null,
        bool allowFutureDates = true,
        int order = 1)
    {
        var config = minDate != null || maxDate != null || !allowFutureDates
            ? new DateRangeConfig(minDate, maxDate, allowFutureDates)
            : null;

        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "DatePicker",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order,
            TypeConfig = config
        };
    }

    /// <summary>
    /// Creates a date-time picker field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a date-time picker</returns>
    public FormFieldSchema CreateDateTimePicker(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "DateTimePicker",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order
        };
    }

    // ========================================================================
    // NUMERIC FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a numeric input field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="minValue">Minimum allowed value (optional)</param>
    /// <param name="maxValue">Maximum allowed value (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a numeric field</returns>
    public FormFieldSchema CreateNumericField(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        decimal? minValue = null,
        decimal? maxValue = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Numeric",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order,
            ColumnType = "decimal(18,2)"
        };
    }

    /// <summary>
    /// Creates a currency input field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a currency field</returns>
    public FormFieldSchema CreateCurrencyField(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Currency",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order,
            ColumnType = "decimal(18,2)"
        };
    }

    // ========================================================================
    // FILE UPLOAD FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a file upload field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="allowedExtensions">Array of allowed file extensions (e.g., [".pdf", ".docx"])</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="maxFileSizeBytes">Maximum file size in bytes (default: 10 MB)</param>
    /// <param name="maxFiles">Maximum number of files (default: 1)</param>
    /// <param name="requireVirusScan">Whether virus scan is required (default: true)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a file upload</returns>
    public FormFieldSchema CreateFileUpload(
        string fieldId,
        string labelEn,
        string[] allowedExtensions,
        string? labelFr = null,
        bool isRequired = false,
        long maxFileSizeBytes = 10_485_760,
        int maxFiles = 1,
        bool requireVirusScan = true,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "FileUpload",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order,
            TypeConfig = new FileUploadConfig(
                allowedExtensions,
                maxFileSizeBytes,
                maxFiles,
                requireVirusScan)
        };
    }

    // ========================================================================
    // CONTAINER FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a section (container) field.
    /// </summary>
    /// <param name="fieldId">Unique section identifier</param>
    /// <param name="titleEn">English section title</param>
    /// <param name="titleFr">French section title (optional)</param>
    /// <param name="descriptionEn">English section description (optional)</param>
    /// <param name="descriptionFr">French section description (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a section</returns>
    public FormFieldSchema CreateSection(
        string fieldId,
        string titleEn,
        string? titleFr = null,
        string? descriptionEn = null,
        string? descriptionFr = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Section",
            LabelEn = titleEn,
            LabelFr = titleFr,
            DescriptionEn = descriptionEn,
            DescriptionFr = descriptionFr,
            Order = order
        };
    }

    /// <summary>
    /// Creates a fieldset (grouped fields) container.
    /// </summary>
    /// <param name="fieldId">Unique fieldset identifier</param>
    /// <param name="legendEn">English legend text</param>
    /// <param name="legendFr">French legend text (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a fieldset</returns>
    public FormFieldSchema CreateFieldset(
        string fieldId,
        string legendEn,
        string? legendFr = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Fieldset",
            LabelEn = legendEn,
            LabelFr = legendFr,
            Order = order
        };
    }

    // ========================================================================
    // ADVANCED FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a modal table field for multi-row data entry.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="modalFields">Array of fields that appear in the modal dialog</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether at least one row is required (default: false)</param>
    /// <param name="maxRecords">Maximum number of records (optional)</param>
    /// <param name="allowDuplicates">Whether duplicate records are allowed (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a modal table</returns>
    public FormFieldSchema CreateModalTable(
        string fieldId,
        string labelEn,
        FormFieldSchema[] modalFields,
        string? labelFr = null,
        bool isRequired = false,
        int? maxRecords = null,
        bool allowDuplicates = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "ModalTable",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order,
            TypeConfig = new ModalTableConfig(modalFields, maxRecords, allowDuplicates)
        };
    }

    /// <summary>
    /// Creates a rich text editor field (HTML editor).
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a rich text editor</returns>
    public FormFieldSchema CreateRichTextEditor(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "RichTextEditor",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order
        };
    }

    /// <summary>
    /// Creates a signature pad field for digital signatures.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="labelEn">English label</param>
    /// <param name="labelFr">French label (optional)</param>
    /// <param name="isRequired">Whether the field is required (default: false)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a signature pad</returns>
    public FormFieldSchema CreateSignaturePad(
        string fieldId,
        string labelEn,
        string? labelFr = null,
        bool isRequired = false,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "SignaturePad",
            LabelEn = labelEn,
            LabelFr = labelFr,
            IsRequired = isRequired,
            Order = order
        };
    }

    // ========================================================================
    // DISPLAY-ONLY FIELDS
    // ========================================================================

    /// <summary>
    /// Creates a label (display-only text) field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="textEn">English text to display</param>
    /// <param name="textFr">French text to display (optional)</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a label</returns>
    public FormFieldSchema CreateLabel(
        string fieldId,
        string textEn,
        string? textFr = null,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Label",
            LabelEn = textEn,
            LabelFr = textFr,
            IsReadOnly = true,
            Order = order
        };
    }

    /// <summary>
    /// Creates a divider (horizontal line) field.
    /// </summary>
    /// <param name="fieldId">Unique field identifier</param>
    /// <param name="order">Display order (default: 1)</param>
    /// <returns>Configured FormFieldSchema for a divider</returns>
    public FormFieldSchema CreateDivider(
        string fieldId,
        int order = 1)
    {
        return new FormFieldSchema
        {
            Id = fieldId,
            FieldType = "Divider",
            Order = order
        };
    }

    // ========================================================================
    // FIELD OPTIONS
    // ========================================================================

    /// <summary>
    /// Creates a field option for dropdowns, radio buttons, or checkbox lists.
    /// </summary>
    /// <param name="value">The underlying value of the option</param>
    /// <param name="labelEn">English display label</param>
    /// <param name="labelFr">French display label (optional)</param>
    /// <param name="isDefault">Whether this option is selected by default (default: false)</param>
    /// <param name="order">Display order (default: 0)</param>
    /// <returns>Configured FieldOption</returns>
    public FieldOption CreateOption(
        string value,
        string labelEn,
        string? labelFr = null,
        bool isDefault = false,
        int order = 0)
    {
        return new FieldOption(value, labelEn, labelFr, isDefault, order);
    }

    /// <summary>
    /// Creates multiple field options from a dictionary.
    /// </summary>
    /// <param name="options">Dictionary of value -> English label pairs</param>
    /// <param name="defaultValue">Value of the default option (optional)</param>
    /// <returns>Array of FieldOptions</returns>
    public FieldOption[] CreateOptions(
        Dictionary<string, string> options,
        string? defaultValue = null)
    {
        var order = 0;
        return options.Select(kvp => new FieldOption(
            Value: kvp.Key,
            LabelEn: kvp.Value,
            LabelFr: null,
            IsDefault: kvp.Key == defaultValue,
            Order: order++
        )).ToArray();
    }

    // ========================================================================
    // CONDITIONAL RULES
    // ========================================================================

    /// <summary>
    /// Creates a conditional rule for field visibility or behavior.
    /// </summary>
    /// <param name="triggerFieldId">ID of the field that triggers this rule</param>
    /// <param name="operator">Comparison operator (equals, notEquals, contains, greaterThan, lessThan, etc.)</param>
    /// <param name="value">Value to compare against (null for existence checks)</param>
    /// <param name="action">Action to take when condition is met (show, hide, enable, disable, require)</param>
    /// <returns>Configured ConditionalRule</returns>
    public ConditionalRule CreateConditionalRule(
        string triggerFieldId,
        string @operator,
        string? value,
        string action)
    {
        return new ConditionalRule(triggerFieldId, @operator, value, action);
    }

    /// <summary>
    /// Creates a "show when equals" conditional rule.
    /// </summary>
    /// <param name="triggerFieldId">ID of the field that triggers this rule</param>
    /// <param name="value">Value to compare against</param>
    /// <returns>ConditionalRule that shows the field when trigger equals value</returns>
    public ConditionalRule CreateShowWhenEquals(string triggerFieldId, string value)
    {
        return new ConditionalRule(triggerFieldId, "equals", value, "show");
    }

    /// <summary>
    /// Creates a "hide when equals" conditional rule.
    /// </summary>
    /// <param name="triggerFieldId">ID of the field that triggers this rule</param>
    /// <param name="value">Value to compare against</param>
    /// <returns>ConditionalRule that hides the field when trigger equals value</returns>
    public ConditionalRule CreateHideWhenEquals(string triggerFieldId, string value)
    {
        return new ConditionalRule(triggerFieldId, "equals", value, "hide");
    }

    /// <summary>
    /// Creates a "require when equals" conditional rule.
    /// </summary>
    /// <param name="triggerFieldId">ID of the field that triggers this rule</param>
    /// <param name="value">Value to compare against</param>
    /// <returns>ConditionalRule that makes the field required when trigger equals value</returns>
    public ConditionalRule CreateRequireWhenEquals(string triggerFieldId, string value)
    {
        return new ConditionalRule(triggerFieldId, "equals", value, "require");
    }
}
