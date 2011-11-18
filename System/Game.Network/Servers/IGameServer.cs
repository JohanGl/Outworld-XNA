using Game.Network.Servers.Settings;
using Microsoft.Xna.Framework;

namespace Game.Network.Servers
{
	public interface IGameServer
	{
		bool IsStarted { get; }

		void Initialize(GameServerSettings settings);
		void Start();
		void Stop(string message = null);
		void Update(GameTime gameTime);
	}
}