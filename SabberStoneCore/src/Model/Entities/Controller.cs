﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Model.Zones;

namespace SabberStoneCore.Model.Entities
{
	/// <summary>
	/// Instance that represents a player in SabberStone game instances.
	/// <seealso cref="Entity"/>
	/// </summary>
	public partial class Controller : Entity
	{
		/// <summary>
		/// Name of the player.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Initial cards that are in the deck of the controller.
		/// </summary>
		public List<Card> DeckCards { get; internal set; } = new List<Card>();

		/// <summary>
		/// Base class of the controller.
		/// </summary>
		public CardClass BaseClass { get; internal set; }

		/// <summary>
		/// Available zones for this player.
		/// </summary>
		public ControlledZones ControlledZones { get; }

		/// <summary>
		/// The deck of this player.
		/// This zone contains cards which are not yet drawn. Can be empty.
		/// </summary>
		public DeckZone DeckZone => ControlledZones[Enums.Zone.DECK] as DeckZone;

		/// <summary>
		/// The hand of this player.
		/// This zone contains cards which were drawn from deck or generated 
		/// during the game. Can be empty.
		/// </summary>
		public HandZone HandZone => ControlledZones[Enums.Zone.HAND] as HandZone;

		/// <summary>
		/// The minions friendly to this player.
		/// This zone contains all minion entities generated by cards from the hand.
		/// Can be empty.
		/// </summary>
		public BoardZone BoardZone => ControlledZones[Enums.Zone.PLAY] as BoardZone;

		/// <summary>
		/// The zone containing all entities which were once in play, but now destroyed. Can be empty.
		/// </summary>
		public GraveyardZone GraveyardZone => ControlledZones[Enums.Zone.GRAVEYARD] as GraveyardZone;

		/// <summary>
		/// The zone containing all played secrets by this player. Can be empty.
		/// </summary>
		public SecretZone SecretZone => ControlledZones[Enums.Zone.SECRET] as SecretZone;

		/// <summary>
		/// The zone containing all entities that need to be chosen by the player.
		/// Before an option set is created, it's entities are built and stored in the this zone.
		/// The picked entity will move from that zone into the hand zone.
		/// 
		/// Unpicked entities will remain in the setaside zone.
		/// </summary>
		public SetasideZone SetasideZone => ControlledZones[Enums.Zone.SETASIDE] as SetasideZone;

		/// <summary>
		/// The hero entity representing this player.
		/// </summary>
		public Hero Hero { get; set; }

		/// <summary>
		/// The cardclass of the deck.
		/// Only neutral cards and cards specific to this class are allowed in the starting deck
		/// of the player.
		/// </summary>
		public CardClass HeroClass => Hero.Card.Class;

		/// <summary>
		/// All standard cards which can be put into a deck of this class.
		/// </summary>
		public IEnumerable<Card> Standard => Cards.Standard[HeroClass];

		/// <summary>
		/// All wild cards which can be put into a deck of this class.
		/// </summary>
		public IEnumerable<Card> Wild => Cards.Wild[HeroClass];

		/// <summary>
		/// The amount of mana available to actually use after calculating all resource factors.
		/// </summary>
		public int RemainingMana => BaseMana + TemporaryMana - (UsedMana + OverloadLocked);

		/// <summary>
		/// Returns true if this player has a dragon in his hand.
		/// </summary>
		public bool DragonInHand => HandZone.GetAll.Exists(p => p.Card.Race == Race.DRAGON);

		/// <summary>
		/// The last choice set proposed to this player.
		/// The actual chosen entity is also stored in the Choice object.
		/// </summary>
		public Choice Choice { get; set; } = null;

		/// <summary>
		/// The opponent player instance.
		/// </summary>
		public Controller Opponent => Game.Player1 == this ? Game.Player2 : Game.Player1;

		/// <summary>
		/// Create a new controller instance.
		/// </summary>
		/// <param name="game">The game to which it registers.</param>
		/// <param name="name">The name of the player.</param>
		/// <param name="playerId">The player index; The first player will get assigned 1.</param>
		/// <param name="id">Entity ID of this controller.</param>
		public Controller(Game game, string name, int playerId, int id)
			: base(game, Card.CardPlayer,
			new Dictionary<GameTag, int>
			{
				//[GameTag.HERO_ENTITY] = heroId,
				[GameTag.MAXHANDSIZE] = 10,
				[GameTag.STARTHANDSIZE] = 4,
				[GameTag.PLAYER_ID] = playerId,
				[GameTag.TEAM_ID] = playerId,
				[GameTag.ZONE] = (int)Enums.Zone.PLAY,
				[GameTag.CONTROLLER] = playerId,
				[GameTag.ENTITY_ID] = id,
				[GameTag.MAXRESOURCES] = 10,
				[GameTag.CARDTYPE] = (int)CardType.PLAYER

			})
		{
			Name = name;
			ControlledZones = new ControlledZones(game, this);
			Game.Log(LogLevel.INFO, BlockType.PLAY, "Controller", !Game.Logging? "":$"Created Controller '{name}'");
		}

		/// <summary>
		/// Adds a new Hero entity and HeroPower entity to the game instance.
		/// </summary>
		/// <param name="heroCard">The card to derive the hero entity from.</param>
		/// <param name="powerCard">The heropower card to derive the hero power entity from.</param>
		/// <param name="tags">The inherited tags</param>
		/// <param name="id">The entity id to assign to the generated HERO entity</param>
		public void AddHeroAndPower(Card heroCard, Card powerCard = null, Dictionary<GameTag, int> tags = null, int id = -1)
		{
			// remove hero and place it to the setaside zone
			Weapon weapon = null;
			if  (Hero != null)
			{
				SetasideZone.MoveTo(Hero, SetasideZone.Count);
				SetasideZone.MoveTo(Hero.Power, SetasideZone.Count);
				//Hero[GameTag.EXHAUSTED] = 0;
				//Hero[GameTag.NUM_ATTACKS_THIS_TURN ] = 0;
				//Hero[GameTag.DAMAGE] = 0;
				//Hero[GameTag.REVEALED] = 1;
				if (Hero.Weapon != null)
				{
					weapon = Hero.Weapon;
				}
			}


			Hero = FromCard(this, heroCard, tags, null, id) as Hero;
			HeroId = Hero.Id;
			Hero.Power = FromCard(this, powerCard ?? Cards.FromAssetId(Hero[GameTag.HERO_POWER]),
				new Dictionary<GameTag, int> { [GameTag.CREATOR] = Hero.Id }) as HeroPower;
			Hero.Weapon = weapon;
		}

		/// <summary>
		/// Copy data from the provided argument into this object.
		/// </summary>
		/// <param name="controller"></param>
		public void Stamp(Controller controller)
		{
			ControlledZones.Stamp(controller.ControlledZones);
			DeckCards.AddRange(controller.DeckCards);
			BaseClass = controller.BaseClass;
			base.Stamp(controller);
			Hero = FromCard(this, controller.Hero.Card, null, null, controller.Hero.Id) as Hero;
			Hero.Stamp(controller.Hero);
			Hero.Power = FromCard(this, controller.Hero.Power.Card, null, null, controller.Hero.Power.Id) as HeroPower;
			Hero.Power.Stamp(controller.Hero.Power);
			if (controller.Hero.Weapon != null)
			{
				Hero.Weapon =
					FromCard(this, controller.Hero.Weapon.Card, null, null, controller.Hero.Weapon.Id) as Weapon;
				Hero.Weapon.Stamp(controller.Hero.Weapon);
			}

			if (controller.Choice != null)
			{
				Choice = new Choice(this);
				Choice.Stamp(controller.Choice);
			}
		}

		/// <summary>
		/// Build a unique string describing this object.
		/// </summary>
		/// <param name="ignore">All GameTags which have to be ignored during hash generation.</param>
		/// <returns>The unique hash string.</returns>
		public override string Hash(params GameTag[] ignore)
		{
			var str = new StringBuilder();
			str.Append("][C:");
			str.Append($"{Name}");
			str.Append("]");
			str.Append(base.Hash(ignore));
			str.Append(Hero.Hash(ignore));
			str.Append(Hero.Power.Hash(ignore));
			if (Hero.Weapon != null)
				str.Append(Hero.Weapon.Hash(ignore));
			str.Append(ControlledZones.Hash(ignore));
			return str.ToString();
		}

		/// <summary>
		/// Returns a set of all options this player can perform execute at the moment.
		/// From this set one option is picked and executed by the game.
		/// </summary>
		/// <param name="playCards"></param>
		/// <returns></returns>
		public List<PlayerTask> Options(bool playCards = true)
		{
			var result = new List<PlayerTask>();

			if (this != Game.CurrentPlayer)
				return result;

			if (Choice != null)
			{
				switch (Choice.ChoiceType)
				{
					case ChoiceType.GENERAL:
						Choice.Choices.ToList().ForEach(p => result.Add(ChooseTask.Pick(this, p)));
						return result;

					case ChoiceType.MULLIGAN:
						IEnumerable<IEnumerable<int>> choices = Util.GetPowerSet(Choice.Choices);
						choices.ToList().ForEach(p => result.Add(ChooseTask.Mulligan(this, p.ToList())));
						return result;

					default:
						throw new NotImplementedException();
				}
			}

			// no options till mulligan is done for both players
			if (Game.Step != Step.MAIN_ACTION)
				return result;

			// add end turn task ...
			result.Add(EndTurnTask.Any(this));

			if (playCards)
			{
				foreach (IPlayable playableCard in HandZone)
				{
					var minion = playableCard as Minion;

					if (!playableCard.IsPlayableByPlayer)
						continue;

					List<IPlayable> playables = playableCard.ChooseOne && !Game.CurrentPlayer.ChooseBoth
						? playableCard.ChooseOnePlayables.ToList()
						: new List<IPlayable> { playableCard };

					foreach (IPlayable t in playables)
					{
						if (!t.IsPlayableByCardReq)
							continue;

						IEnumerable<ICharacter> targets = t.ValidPlayTargets;
						var subResult = new List<PlayCardTask>();
						if (!targets.Any())
						{
							subResult.Add(PlayCardTask.Any(this, playableCard, null, -1,
								playables.Count == 1 ? 0 : playables.IndexOf(t) + 1));
						}

						//subResult.AddRange(
						//	targets.Select(
						//		target =>
						//			PlayCardTask.Any(this, playableCard, target, -1,
						//				playables.Count == 1 ? 0 : playables.IndexOf(t) + 1)));
						foreach (ICharacter target in targets)
						{
							subResult.Add(PlayCardTask.Any(this, playableCard, target, -1,
								playables.Count == 1 ? 0 : playables.IndexOf(t) + 1));
						}

						if (minion != null)
						{
							var tempSubResult = new List<PlayCardTask>();
							int positions = BoardZone.Count + 1;
							for (int j = 0; j < positions; j++)
							{
								subResult.ForEach(p =>
								{
									PlayCardTask task = p.Copy();
									task.ZonePosition = j;
									tempSubResult.Add(task);
								});
							}
							subResult = tempSubResult;
						}
						result.AddRange(subResult);
					}
				}
			}

			foreach (Minion minion in BoardZone)
			{
				if (!minion.CanAttack)
					continue;

				IEnumerable<ICharacter> targets = minion.ValidAttackTargets;
				foreach (ICharacter target in targets)
					result.Add(MinionAttackTask.Any(this, minion, target));
			}

			if (Hero.CanAttack)
			{
				IEnumerable<ICharacter> targets = Hero.ValidAttackTargets;
				foreach (ICharacter target in targets)
					result.Add(HeroAttackTask.Any(this, target));
			}

			if (Hero.Power.IsPlayable)
			{
				IEnumerable<ICharacter> targets = Hero.Power.GetValidPlayTargets();
				if (targets.Any())
				{
					foreach (ICharacter target in targets)
						result.Add(HeroPowerTask.Any(this, target));
				}
				else
				{
					result.Add(HeroPowerTask.Any(this));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a string which dumps information about this player.
		/// </summary>
		/// <returns></returns>
		public string FullPrint()
		{
			var str = new StringBuilder();
			str.Append($"{Name}[Mana:{RemainingMana}/{OverloadOwed}/{BaseMana}][{OverloadLocked}]");
			str.Append($"[ENCH {Enchants.Count}]");
			str.Append($"[TRIG {Triggers.Count}]");
			return str.ToString();
		}
	}

	public partial class Controller
	{
		/// <summary>
		/// Maximum amount of cards in the player's hand
		/// </summary>
		public int MaxHandSize => this[GameTag.MAXHANDSIZE];

		/// <summary>
		/// Maximum amount of mana this player is allowed to spend.
		/// </summary>
		public int MaxResources => this[GameTag.MAXRESOURCES];

		/// <summary>
		/// Duration of seconds of this player's turn.
		/// </summary>
		public int TimeOut
		{
			get { return this[GameTag.TIMEOUT]; }
			set { this[GameTag.TIMEOUT] = value; }
		}

		/// <summary>
		/// ID of the player, which is a monotone ranking order starting from 1.
		/// The first player gets PlayerID == 1
		/// </summary>
		public int PlayerId
		{
			get { return this[GameTag.PLAYER_ID]; }
			set { this[GameTag.PLAYER_ID] = value; }
		}

		/// <summary>
		/// The EntityID of the selected Hero.
		/// </summary>
		public int HeroId
		{
			get { return this[GameTag.HERO_ENTITY]; }
			set { this[GameTag.HERO_ENTITY] = value; }
		}


		/// <summary>
		/// Context in which the controller is performing.
		/// </summary>
		public PlayState PlayState
		{
			get { return (PlayState)this[GameTag.PLAYSTATE]; }
			set { this[GameTag.PLAYSTATE] = (int)value; }
		}

		/// <summary>
		/// Progress this player is making during Mulligan Phase.
		/// <see cref="Mulligan"/>
		/// </summary>
		public Mulligan MulliganState
		{
			get { return (Mulligan)this[GameTag.MULLIGAN_STATE]; }
			set { this[GameTag.MULLIGAN_STATE] = (int)value; }
		}

		/// <summary>
		/// Total amount of mana available to this player.
		/// This value DOES NOT contain temporary mana!
		/// 
		/// This value is limited to 1 turnand should be reset in the next turn.
		/// </summary>
		public int BaseMana
		{
			//get { return this[GameTag.RESOURCES]; }
			//set { this[GameTag.RESOURCES] = value; }
			get { return GetNativeGameTag(GameTag.RESOURCES); }
			set { SetNativeGameTag(GameTag.RESOURCES, value); }
		}

		/// <summary>
		/// Amount of mana used by this player.
		/// 
		/// This value is limited to 1 turnand should be reset in the next turn.
		/// </summary>
		public int UsedMana
		{
			//get { return this[GameTag.RESOURCES_USED]; }
			//set { this[GameTag.RESOURCES_USED] = value; }
			get { return GetNativeGameTag(GameTag.RESOURCES_USED); }
			set { SetNativeGameTag(GameTag.RESOURCES_USED, value); }
		}

		/// <summary>
		/// Additionall mana gained during this turn.
		/// </summary>
		public int TemporaryMana
		{
			//get { return this[GameTag.TEMP_RESOURCES]; }
			//set { this[GameTag.TEMP_RESOURCES] = value; }
			get { return GetNativeGameTag(GameTag.TEMP_RESOURCES); }
			set { SetNativeGameTag(GameTag.TEMP_RESOURCES, value); }
		}

		/// <summary>
		/// Indicates if combo enchantment effects of next cards should be executed or not.
		/// 
		/// Combo is active if at least one card has been played this turn.
		/// </summary>
		public bool IsComboActive
		{
			//get { return this[GameTag.COMBO_ACTIVE] == 1; }
			//set { this[GameTag.COMBO_ACTIVE] = value ? 1 : 0; }
			get { return GetNativeGameTag(GameTag.COMBO_ACTIVE) == 1; }
			set { SetNativeGameTag(GameTag.COMBO_ACTIVE, value ? 1 : 0); }
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public int NumCardsDrawnThisTurn
		{
			get { return this[GameTag.NUM_CARDS_DRAWN_THIS_TURN]; }
			set { this[GameTag.NUM_CARDS_DRAWN_THIS_TURN] = value; }
		}

		public int NumCardsToDraw
		{
			get { return this[GameTag.NUM_CARDS_TO_DRAW]; }
			set { this[GameTag.NUM_CARDS_TO_DRAW] = value; }
		}

		public int NumAttacksThisTurn
		{
			get { return this[GameTag.NUM_ATTACKS_THIS_TURN]; }
			set { this[GameTag.NUM_ATTACKS_THIS_TURN] = value; }
		}

		public int NumCardsPlayedThisTurn
		{
			get { return this[GameTag.NUM_CARDS_PLAYED_THIS_TURN]; }
			set { this[GameTag.NUM_CARDS_PLAYED_THIS_TURN] = value; }
		}

		public int NumMinionsPlayedThisTurn
		{
			get { return this[GameTag.NUM_MINIONS_PLAYED_THIS_TURN]; }
			set { this[GameTag.NUM_MINIONS_PLAYED_THIS_TURN] = value; }
		}

		public int NumElementalsPlayedThisTurn
		{
			get { return this[GameTag.NUM_ELEMENTAL_PLAYED_THIS_TURN]; }
			set { this[GameTag.NUM_ELEMENTAL_PLAYED_THIS_TURN] = value; }
		}

		public int NumElementalsPlayedLastTurn
		{
			get { return this[GameTag.NUM_ELEMENTAL_PLAYED_LAST_TURN]; }
			set { this[GameTag.NUM_ELEMENTAL_PLAYED_LAST_TURN] = value; }
		}

		public int NumOptionsPlayedThisTurn
		{
			get { return this[GameTag.NUM_OPTIONS_PLAYED_THIS_TURN]; }
			set { this[GameTag.NUM_OPTIONS_PLAYED_THIS_TURN] = value; }
		}

		public int NumFriendlyMinionsThatAttackedThisTurn
		{
			get { return this[GameTag.NUM_FRIENDLY_MINIONS_THAT_ATTACKED_THIS_TURN]; }
			set { this[GameTag.NUM_FRIENDLY_MINIONS_THAT_ATTACKED_THIS_TURN] = value; }
		}

		public int NumFriendlyMinionsThatDiedThisTurn
		{
			get { return this[GameTag.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_TURN]; }
			set { this[GameTag.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_TURN] = value; }
		}

		public int NumFriendlyMinionsThatDiedThisGame
		{
			get { return this[GameTag.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_GAME]; }
			set { this[GameTag.NUM_FRIENDLY_MINIONS_THAT_DIED_THIS_GAME] = value; }
		}

		public int NumMinionsPlayerKilledThisTurn
		{
			get { return this[GameTag.NUM_MINIONS_PLAYER_KILLED_THIS_TURN]; }
			set { this[GameTag.NUM_MINIONS_PLAYER_KILLED_THIS_TURN] = value; }
		}

		public int TotalManaSpentThisGame
		{
			get { return this[GameTag.NUM_RESOURCES_SPENT_THIS_GAME]; }
			set { this[GameTag.NUM_RESOURCES_SPENT_THIS_GAME] = value; }
		}

		public int HeroPowerActivationsThisTurn
		{
			get { return this[GameTag.HEROPOWER_ACTIVATIONS_THIS_TURN]; }
			set { this[GameTag.HEROPOWER_ACTIVATIONS_THIS_TURN] = value; }
		}

		public int NumTimesHeroPowerUsedThisGame
		{
			get { return this[GameTag.NUM_TIMES_HERO_POWER_USED_THIS_GAME]; }
			set { this[GameTag.NUM_TIMES_HERO_POWER_USED_THIS_GAME] = value; }
		}

		public int NumSecretsPlayedThisGame
		{
			get { return this[GameTag.NUM_SECRETS_PLAYED_THIS_GAME]; }
			set { this[GameTag.NUM_SECRETS_PLAYED_THIS_GAME] = value; }
		}

		public int NumSpellsPlayedThisGame
		{
			get { return this[GameTag.NUM_SPELLS_PLAYED_THIS_GAME]; }
			set { this[GameTag.NUM_SPELLS_PLAYED_THIS_GAME] = value; }
		}

		public int NumWeaponsPlayedThisGame
		{
			get { return this[GameTag.NUM_WEAPONS_PLAYED_THIS_GAME]; }
			set { this[GameTag.NUM_WEAPONS_PLAYED_THIS_GAME] = value; }
		}

		public int NumMurlocsPlayedThisGame
		{
			get { return this[GameTag.NUM_MURLOCS_PLAYED_THIS_GAME]; }
			set { this[GameTag.NUM_MURLOCS_PLAYED_THIS_GAME] = value; }
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


		/// <summary>
		/// Amount of turns left for this player.
		/// 
		/// This is a special tag used for the spell Time Warp, which grants 
		/// the caster an additional turn.
		/// </summary>
		public int NumTurnsLeft
		{
			get { return this[GameTag.NUM_TURNS_LEFT]; }
			set { this[GameTag.NUM_TURNS_LEFT] = value; }
		}

		/// <summary>
		/// Amount of mana crystals which will be locked during the next turn.
		/// </summary>
		public int OverloadOwed
		{
			get { return this[GameTag.OVERLOAD_OWED]; }
			set { this[GameTag.OVERLOAD_OWED] = value; }
		}

		/// <summary>
		/// Amount of mana crystals locked this turn.
		/// 
		/// The subtraction of BASE_MANA and this value gives the available
		/// resources during this turn.
		/// </summary>
		public int OverloadLocked
		{
			//get { return this[GameTag.OVERLOAD_LOCKED]; }
			//set { this[GameTag.OVERLOAD_LOCKED] = value; }
			get { return GetNativeGameTag(GameTag.OVERLOAD_LOCKED); }
			set { SetNativeGameTag(GameTag.OVERLOAD_LOCKED, value); }
		}

		/// <summary>
		/// Sum of locked mana crystals during the entire game.
		/// </summary>
		public int OverloadThisGame
		{
			get { return this[GameTag.OVERLOAD_THIS_GAME]; }
			set { this[GameTag.OVERLOAD_THIS_GAME] = value; }
		}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

		public int LastCardPlayed
		{
			get { return this[GameTag.LAST_CARD_PLAYED]; }
			set { this[GameTag.LAST_CARD_PLAYED] = value; }
		}

		public int LastCardDrawn
		{
			get { return this[GameTag.LAST_CARD_DRAWN]; }
			set { this[GameTag.LAST_CARD_DRAWN] = value; }
		}

		public int LastCardDiscarded
		{
			get { return this[GameTag.LAST_CARD_DISCARDED]; }
			set { this[GameTag.LAST_CARD_DISCARDED] = value; }
		}

		public bool SeenCthun
		{
			get { return this[GameTag.SEEN_CTHUN] == 1; }
			set { this[GameTag.SEEN_CTHUN] = value ? 1 : 0; }
		}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

		/// <summary>
		/// The entity which is a copy of the real C'Thun entity in deck.
		/// This proxy is used to display and store all buffs from rituals.
		/// The real C'Thun will mirror the proxy C'Thun.
		/// </summary>
		public int ProxyCthun
		{
			get { return this[GameTag.PROXY_CTHUN]; }
			set { this[GameTag.PROXY_CTHUN] = value; }
		}

		/// <summary>
		/// Returns true if for this player all battlecries should be executed another time.
		/// This is applicable when Brann BronzeBeard is in play.
		/// </summary>
		public bool ExtraBattlecry
		{
			get { return this[GameTag.EXTRA_BATTLECRY] == 1; }
			set { this[GameTag.EXTRA_BATTLECRY] = value ? 1 : 0; }
		}

		/// <summary>
		/// Returns true if for this player all end turn effects should be executed another time.
		/// This is applicable when Drakkari Enchanter is in play.
		/// </summary>
		public bool ExtraEndTurnEffect
		{
			get { return this[GameTag.EXTRA_END_TURN_EFFECT] == 1; }
			set { this[GameTag.EXTRA_END_TURN_EFFECT] = value ? 1 : 0; }
		}

		/// <summary>
		/// Returns true if for this player hero power is disabled.
		/// </summary>
		public bool HeroPowerDisabled
		{
			get { return this[GameTag.HERO_POWER_DISABLED] == 1; }
			set { this[GameTag.HERO_POWER_DISABLED] = value ? 1 : 0; }
		}

		/// <summary>
		/// Returns true if this player automatically gets both options instead of having to
		/// choose one.
		/// This is applicable when Fandral Staghelm is in play.
		/// </summary>
		public bool ChooseBoth
		{
			get { return this[GameTag.CHOOSE_BOTH] == 1; }
			set { this[GameTag.CHOOSE_BOTH] = value ? 1 : 0; }
		}
	}
}
