namespace Set_BE.Interfaces
{
	public interface ICodeValidatorService
	{
		Task<bool> IsCodeRealAsync(string code, string category, int authorId);
	}
}
