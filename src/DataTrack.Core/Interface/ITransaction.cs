namespace DataTrack.Core.Interface
{
	public interface ITransaction<T>
	{
		void Execute();
	}
}
