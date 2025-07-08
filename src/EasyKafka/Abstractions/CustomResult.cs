using System.Runtime.InteropServices;

namespace EasyKafka.Abstractions;
public class CustomResult
{
    public List<string>? Errors { get; set; }

    public bool IsSuccess => Errors == null || !Errors.Any();

    public bool IsFail => !IsSuccess;

    public static CustomResult Success()
    {
        return new CustomResult();
    }

    public static CustomResult Error(string error)
    {
        CustomResult serviceResult = new CustomResult();
        int num = 1;
        List<string> list = new List<string>(num);
        CollectionsMarshal.SetCount(list, num);
        Span<string> span = CollectionsMarshal.AsSpan(list);
        int index = 0;
        span[index] = error;
        serviceResult.Errors = list;
        return serviceResult;
    }

    public static CustomResult Error(IEnumerable<string> errors)
    {
        return new CustomResult
        {
            Errors = errors.ToList()
        };
    }
}
