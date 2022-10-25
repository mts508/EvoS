// ROGUES
// SERVER

using System;
using EvoS.Framework.Network.Static;
using EvoS.Framework.Network.Unity;

// server-only, missing in reactor
[Serializable]
public class JoinGameServerRequest : AllianceMessageBase
{
	public int OrigRequestId;
	public LobbySessionInfo SessionInfo;
	public LobbyServerPlayerInfo PlayerInfo;
	public string GameServerProcessCode;

	public override void Deserialize(NetworkReader reader)
	{
		base.Deserialize(reader);
		this.OrigRequestId = reader.ReadInt32();
		DeserializeObject<LobbySessionInfo>(out this.SessionInfo, reader);
		DeserializeObject<LobbyServerPlayerInfo>(out this.PlayerInfo, reader);
		this.GameServerProcessCode = reader.ReadString();
	}

	public override void Serialize(NetworkWriter writer)
	{
		base.Serialize(writer);
		writer.Write(OrigRequestId);
		SerializeObject(SessionInfo, writer);
		SerializeObject(PlayerInfo, writer);
		writer.Write(GameServerProcessCode);
	}
}
