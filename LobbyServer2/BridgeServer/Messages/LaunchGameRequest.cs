using System.Collections.Generic;
using EvoS.Framework.Misc;
using EvoS.Framework.Network.Static;
using EvoS.Framework.Network.Unity;

public class LaunchGameRequest : AllianceMessageBase
{
	internal LobbyGameInfo GameInfo;
	internal LobbyServerTeamInfo TeamInfo;
	internal Dictionary<int, LobbySessionInfo> SessionInfo;
	internal LobbyGameplayOverrides GameplayOverrides;

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		SerializeObject(GameInfo, writer);
		SerializeObject(TeamInfo, writer);
		writer.Write(SessionInfo.Count);
		foreach ((int key, LobbySessionInfo session) in SessionInfo)
		{
			writer.Write(key);
			session.Serialize(writer);
		}
		SerializeObject(GameplayOverrides, writer);
	}

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		DeserializeObject(out GameInfo, reader);
		DeserializeObject(out TeamInfo, reader);
		int num = reader.ReadInt32();
		SessionInfo = new Dictionary<int, LobbySessionInfo>(num);
		for (int i = 0; i < num; i++)
		{
			int key = reader.ReadInt32();
			LobbySessionInfo lobbySessionInfo = new LobbySessionInfo();
			lobbySessionInfo.Deserialize(reader);
			SessionInfo[key] = lobbySessionInfo;
		}
		DeserializeObject(out GameplayOverrides, reader);
	}
}