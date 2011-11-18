using Framework.Core.Contexts;

namespace Framework.Core.Packages
{
	public interface IGamePackage
	{
		GamePackageInfo Info { get; }

		void Initialize(GameContext gameContext);
		void Shutdown();
	}
}