using DNA.IO;
using DNA.Net;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.IO;

namespace DNA.CastleMinerZ.Net
{
	public class PlayerUpdateMessage : CastleMinerZMessage
	{
		public Vector3 LocalPosition;

		public Quaternion LocalRotation;

		public Vector3 WorldVelocity;

		public Vector2 Movement;

		public Angle TorsoPitch;

		public bool Using;

		public bool Dead;

		public bool Shouldering;

		public bool Reloading;

		public PlayerMode PlayerMode;

		public bool ThrowingGrenade;

		public bool ReadyToThrowGrenade;

		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				if (Using || Reloading)
				{
					return SendDataOptions.ReliableInOrder;
				}
				return SendDataOptions.InOrder;
			}
		}

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.PlayerUpdate;
			}
		}

		private PlayerUpdateMessage()
		{
		}

		public void Apply(Player player)
		{
			player.LocalPosition = LocalPosition;
			player.PlayerPhysics.WorldVelocity = WorldVelocity;
			player.LocalRotation = LocalRotation;
			if (!player.IsLocal)
			{
				player.Shouldering = Shouldering;
				player.Reloading = Reloading;
				player.UsingTool = Using;
				player.Dead = Dead;
				player._playerMode = PlayerMode;
				player.PlayGrenadeAnim = ThrowingGrenade;
				player.ReadyToThrowGrenade = ReadyToThrowGrenade;
			}
			if (player.Dead)
			{
				player.UpdateAnimation(0f, 0f, Angle.Zero, PlayerMode, Using);
			}
			else
			{
				player.UpdateAnimation(Movement.Y, Movement.X, TorsoPitch, PlayerMode, Using);
			}
		}

		public static void Send(LocalNetworkGamer from, Player player, CastleMinerZControllerMapping input)
		{
			PlayerUpdateMessage sendInstance = Message.GetSendInstance<PlayerUpdateMessage>();
			sendInstance.LocalPosition = player.LocalPosition;
			sendInstance.WorldVelocity = player.PlayerPhysics.WorldVelocity;
			sendInstance.LocalRotation = player.LocalRotation;
			sendInstance.Movement = input.Movement;
			sendInstance.TorsoPitch = player.TorsoPitch;
			sendInstance.Using = player.UsingTool;
			sendInstance.Shouldering = player.Shouldering;
			sendInstance.Reloading = player.Reloading;
			sendInstance.PlayerMode = player._playerMode;
			sendInstance.Dead = player.Dead;
			sendInstance.ThrowingGrenade = player.PlayGrenadeAnim;
			sendInstance.ReadyToThrowGrenade = player.ReadyToThrowGrenade;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			LocalPosition = reader.ReadVector3();
			byte b = reader.ReadByte();
			byte b2 = reader.ReadByte();
			LocalRotation = Quaternion.CreateFromYawPitchRoll(Angle.FromRevolutions((float)(int)b2 / 255f).Radians, 0f, 0f);
			TorsoPitch = Angle.FromRevolutions((float)(int)b / 510f) - Angle.FromDegrees(90f);
			byte b3 = reader.ReadByte();
			PlayerMode = (PlayerMode)reader.ReadByte();
			Using = ((b3 & 1) != 0);
			Dead = ((b3 & 2) != 0);
			Shouldering = ((b3 & 4) != 0);
			Reloading = ((b3 & 8) != 0);
			ThrowingGrenade = ((b3 & 0x10) != 0);
			ReadyToThrowGrenade = ((b3 & 0x20) != 0);
			byte b4 = reader.ReadByte();
			byte b5 = (byte)(b4 & 0xF);
			byte b6 = (byte)(b4 >> 4);
			Movement.X = (float)(int)b5 / 14f * 2f - 1f;
			Movement.Y = (float)(int)b6 / 14f * 2f - 1f;
			HalfSingle halfSingle = default(HalfSingle);
			halfSingle.PackedValue = reader.ReadUInt16();
			WorldVelocity.X = halfSingle.ToSingle();
			halfSingle.PackedValue = reader.ReadUInt16();
			WorldVelocity.Y = halfSingle.ToSingle();
			halfSingle.PackedValue = reader.ReadUInt16();
			WorldVelocity.Z = halfSingle.ToSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(LocalPosition);
			float revolutions = new EulerAngle(LocalRotation).Yaw.Revolutions;
			revolutions -= (float)Math.Floor(revolutions);
			byte value = (byte)Math.Round(255f * revolutions);
			float num = (TorsoPitch + Angle.FromDegrees(90f)).Degrees / 180f;
			num -= (float)Math.Floor(revolutions);
			byte value2 = (byte)Math.Round(255f * num);
			byte b = 0;
			if (Using)
			{
				b = (byte)(b | 1);
			}
			if (Dead)
			{
				b = (byte)(b | 2);
			}
			if (Shouldering)
			{
				b = (byte)(b | 4);
			}
			if (Reloading)
			{
				b = (byte)(b | 8);
			}
			if (ThrowingGrenade)
			{
				b = (byte)(b | 0x10);
			}
			if (ReadyToThrowGrenade)
			{
				b = (byte)(b | 0x20);
			}
			writer.Write(value2);
			writer.Write(value);
			writer.Write(b);
			writer.Write((byte)PlayerMode);
			byte b2 = (byte)((Movement.X + 1f) / 2f * 14f);
			byte b3 = (byte)((Movement.Y + 1f) / 2f * 14f);
			byte value3 = (byte)((b3 << 4) | b2);
			writer.Write(value3);
			writer.Write(new HalfSingle(WorldVelocity.X).PackedValue);
			writer.Write(new HalfSingle(WorldVelocity.Y).PackedValue);
			writer.Write(new HalfSingle(WorldVelocity.Z).PackedValue);
		}
	}
}
