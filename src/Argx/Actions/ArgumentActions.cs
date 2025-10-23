namespace Argx.Actions;

/// <summary>
/// Holds constants for built-in argument actions.
/// </summary>
public static class ArgumentActions
{
    /// <summary>
    /// Store the value of the argument, this is the default action.
    /// </summary>
    public const string Store = "store";

    /// <summary>
    /// Store a constant value, specified by the <c>constValue</c> parameter when the argument is encountered.
    /// </summary>
    public const string StoreConst = "store_const";

    /// <summary>
    /// Store the boolean value True when the argument is encountered.
    /// </summary>
    public const string StoreTrue = "store_true";

    /// <summary>
    /// Store the boolean value False when the argument is encountered.
    /// </summary>
    public const string StoreFalse = "store_false";

    /// <summary>
    /// Count the number of times the argument is encountered.
    /// </summary>
    public const string Count = "count";

    /// <summary>
    /// Append each argument value to a list, it can be specified multiple times.
    /// </summary>
    public const string Append = "append";

    /// <summary>
    /// No action is performed when the argument is encountered.
    /// </summary>
    internal const string NoAction = "no_action";
}