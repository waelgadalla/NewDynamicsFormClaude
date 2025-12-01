namespace DynamicForms.Core.V4.Schemas;

/// <summary>
/// Represents a condition for conditional rules.
/// Supports simple conditions (field comparison) and complex conditions (AND/OR/NOT logic).
/// Enables cross-module field references using dot notation (ModuleKey.FieldId).
/// </summary>
public record Condition
{
    // ===== Simple Condition (Leaf Node) =====

/// <summary>
    /// Field reference for condition evaluation.
    /// 
    /// Syntax options:
    /// - Simple reference: "age" (field in current module)
  /// - Numeric module reference: "1.age" (field in module with ID 1)
    /// - Named module reference: "PersonalInfo.age" (field in module named "PersonalInfo")
    /// 
    /// Cross-module references allow conditions to depend on field values from other modules in a workflow.
    /// </summary>
    public string? Field { get; init; }

    /// <summary>
    /// Comparison operator for evaluating the field value.
    /// 
    /// Supported operators:
    /// - Equality: "eq" or "==", "neq" or "!="
    /// - Comparison: "lt" or "<", "lte" or "<=", "gt" or ">", "gte" or ">="
    /// - Collection: "in" (value in array), "notIn" (value not in array)
    /// - String: "contains", "notContains", "startsWith", "endsWith"
    /// - Existence: "isEmpty", "isNotEmpty", "isNull", "isNotNull"
    /// </summary>
    public string? Operator { get; init; }

  /// <summary>
    /// Expected value to compare the field value against.
    /// Type depends on field type and operator:
    /// - String comparison: string value
    /// - Numeric comparison: int or double
    /// - Array operators (in/notIn): array of values
    /// - Boolean: true/false
    /// </summary>
    public object? Value { get; init; }

    // ===== Complex Condition (Branch Node) =====

    /// <summary>
    /// Logical operator for combining multiple sub-conditions.
    /// Only used for complex conditions (when Conditions array is populated).
    /// </summary>
    public LogicalOperator? LogicalOp { get; init; }

    /// <summary>
    /// Array of sub-conditions for complex logic.
    /// Used with LogicalOp to create AND/OR/NOT expressions.
    /// Each sub-condition can itself be simple or complex (recursive).
    /// </summary>
    public Condition[]? Conditions { get; init; }

    // ===== Validation Helper =====

    /// <summary>
    /// Checks if this is a simple condition (has Field and Operator).
    /// </summary>
    public bool IsSimpleCondition => Field != null && Operator != null;

    /// <summary>
    /// Checks if this is a complex condition (has LogicalOp and Conditions).
    /// </summary>
    public bool IsComplexCondition => LogicalOp != null && Conditions != null && Conditions.Length > 0;
}

/// <summary>
/// Logical operators for combining conditions in complex logic expressions.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// All sub-conditions must be true (logical AND).
    /// Example: Age < 18 AND Province = ON
    /// </summary>
    And,

    /// <summary>
    /// At least one sub-condition must be true (logical OR).
    /// Example: Province = ON OR Province = QC
    /// </summary>
    Or,

    /// <summary>
    /// Negates the sub-condition (logical NOT).
    /// Example: NOT (Age < 18)
    /// </summary>
    Not
}
