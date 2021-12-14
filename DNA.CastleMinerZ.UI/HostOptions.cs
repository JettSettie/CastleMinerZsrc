using DNA.CastleMinerZ.Globalization;
using DNA.Drawing.UI;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class HostOptions : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement pvpItem;

		private MenuItemElement banListItem;

		private MenuItemElement joiningPolicyItem;

		public HostOptions(CastleMinerZGame game)
			: base(game._largeFont, false)
		{
			TextColor = CMZColors.MenuGreen;
			SelectedColor = Color.White;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			AddMenuItem(Strings.Return_To_Game, HostOptionItems.Return);
			AddMenuItem(Strings.Kick_Player, HostOptionItems.KickPlayer);
			AddMenuItem(Strings.Ban_Player, HostOptionItems.BanPlayer);
			AddMenuItem(Strings.Restart, HostOptionItems.Restart);
			pvpItem = AddMenuItem("PVP:", HostOptionItems.PVP);
			AddMenuItem(Strings.Set_Password, HostOptionItems.Password);
			AddMenuItem(Strings.Server_Message, HostOptionItems.ServerMessage);
			banListItem = AddMenuItem(Strings.Clear_Ban_List, HostOptionItems.ClearBanList);
			joiningPolicyItem = AddMenuItem(Strings.Who_can_join, HostOptionItems.ChangeJoinPolicy);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			switch (_game.PVPState)
			{
			case CastleMinerZGame.PVPEnum.Everyone:
				pvpItem.Text = "PVP: " + Strings.Everyone;
				break;
			case CastleMinerZGame.PVPEnum.NotFriends:
				pvpItem.Text = "PVP: " + Strings.Non_Friends_Only;
				break;
			case CastleMinerZGame.PVPEnum.Off:
				pvpItem.Text = "PVP: " + Strings.Off;
				break;
			}
			joiningPolicyItem.Text = Strings.Who_can_join + " ";
			switch (_game.JoinGamePolicy)
			{
			case JoinGamePolicy.Anyone:
				joiningPolicyItem.Text += Strings.Anyone;
				break;
			case JoinGamePolicy.FriendsOnly:
				joiningPolicyItem.Text += Strings.Friends_only;
				break;
			case JoinGamePolicy.InviteOnly:
				joiningPolicyItem.Text += Strings.Invitation_only;
				break;
			}
			banListItem.Visible = (_game.PlayerStats.BanList.Count > 0);
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			Rectangle destinationRectangle = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
			spriteBatch.Draw(_game.DummyTexture, destinationRectangle, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
