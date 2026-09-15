namespace API_RouteXFlow.Responses;

public class CustomResponse<T>
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; }
    public T? Result { get; set; }
    public int? TotalRows { get; set; }

    public CustomResponse(bool success, List<string> errors, T? result, int? totalRows = null)
    {
        Success = success;
        Errors = errors;
        Result = result;
        TotalRows = totalRows;
    }

    public static CustomResponse<T> SuccessTrade(T? result, int? totalRows = null) =>
        new(true, new List<string>(), result, totalRows);

    public static CustomResponse<T> Fail(params string[] errors) =>
        new(false, errors.ToList(), default);
}
