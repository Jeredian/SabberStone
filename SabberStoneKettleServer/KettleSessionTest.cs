﻿using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SabberStoneCore.Tasks.PlayerTasks;
using SabberStoneCore.Kettle;
using Newtonsoft.Json.Linq;

namespace SabberStoneKettleServer
{
    class KettleSessionTest
    {
        private Socket Client;
        private KettleAdapter Adapter;
        private Game _game;

        public KettleSessionTest(Socket client)
        {
            Client = client;
            Adapter = new KettleAdapter(new NetworkStream(client));
            Adapter.OnCreateGame += OnCreateGame;
            Adapter.OnConcede += OnConcede;
            Adapter.OnChooseEntities += OnChooseEntities;
            Adapter.OnSendOption += OnSendOption;
        }

        public void Enter()
        {
            while (true)
            {
                Console.WriteLine("Handling next packet..");
                if (!Adapter.HandleNextPacket())
                {
                    Console.WriteLine("Kettle session ended.");
                    return;
                }
                Console.WriteLine("Done!");
            }
        }

        public void OnConcede(int entityid)
        {
            Console.WriteLine($"player[{entityid}] concedes");
            var concedingPlayer = _game.ControllerById(entityid);
            _game.Process(ConcedeTask.Any(concedingPlayer));
            SendPowerHistory(_game.PowerHistory.Last);
        }

        public void OnSendOption(KettleSendOption sendOption)
        {
            Console.WriteLine("simulator OnSendOption called");

            var allOptions = _game.AllOptionsMap[sendOption.Index];

            SendPowerHistory(_game.PowerHistory.Last);
            SendChoicesOrOptions();
        }

        public void OnChooseEntities(KettleChooseEntities chooseEntities)
        {
            Console.WriteLine("simulator OnChooseEntities called");

            if (chooseEntities.ID == 1)
            {
                Adapter.SendMessage(TagChangeTest(2, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.DEALING));
                Adapter.SendMessage(BlockStartTest("", 6, 2, 0, (int)BlockType.TRIGGER));

                Adapter.SendMessage(TagChangeTest(43, (int)GameTag.ZONE, (int)Zone.HAND));
                Adapter.SendMessage(TagChangeTest(43, (int)GameTag.ZONE_POSITION, 1));
                Adapter.SendMessage(TagChangeTest(57, (int)GameTag.ZONE, (int)Zone.DECK));
                Adapter.SendMessage(TagChangeTest(57, (int)GameTag.ZONE_POSITION, 0));
                Adapter.SendMessage(TagChangeTest(59, (int)GameTag.ZONE, (int)Zone.HAND));
                Adapter.SendMessage(TagChangeTest(59, (int)GameTag.ZONE_POSITION, 2));
                Adapter.SendMessage(TagChangeTest(34, (int)GameTag.ZONE, (int)Zone.DECK));
                Adapter.SendMessage(TagChangeTest(34, (int)GameTag.ZONE_POSITION, 0));
                Adapter.SendMessage(TagChangeTest(51, (int)GameTag.ZONE, (int)Zone.HAND));
                Adapter.SendMessage(TagChangeTest(51, (int)GameTag.ZONE_POSITION, 3));
                Adapter.SendMessage(TagChangeTest(46, (int)GameTag.ZONE, (int)Zone.DECK));
                Adapter.SendMessage(TagChangeTest(46, (int)GameTag.ZONE_POSITION, 0));
                Adapter.SendMessage(TagChangeTest(2, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.WAITING));

                Adapter.SendMessage(BlockEndTest());

                Adapter.SendMessage(BlockStartTest("", 7, 2, 0, (int)BlockType.TRIGGER));
                    Adapter.SendMessage(TagChangeTest(2, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.DONE));
                Adapter.SendMessage(BlockEndTest());

                Adapter.SendMessage(TagChangeTest(3, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.DEALING));
                Adapter.SendMessage(BlockStartTest("", 6, 3, 0, (int)BlockType.TRIGGER));

                Adapter.SendMessage(ShowEntityTest(22, "OG_162", new Dictionary<int, int>
                {
                    [(int)GameTag.PREMIUM] = 0,
                    [(int)GameTag.DAMAGE] = 0,
                    [(int)GameTag.HEALTH] = 1,
                    [(int)GameTag.ATK] = 2,
                    [(int)GameTag.COST] = 3,
                    [(int)GameTag.ZONE] = (int)Zone.HAND,
                    [(int)GameTag.CONTROLLER] = 1,
                    [(int)GameTag.ENTITY_ID] = 22,
                    //[(int)GameTag.ELITE] = 1,
                    [(int)GameTag.SILENCED] = 0,
                    [(int)GameTag.WINDFURY] = 0,
                    [(int)GameTag.TAUNT] = 0,
                    [(int)GameTag.STEALTH] = 0,
                    [(int)GameTag.DIVINE_SHIELD] = 0,
                    [(int)GameTag.CHARGE] = 0,
                    //[(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                    [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                    [(int)GameTag.RARITY] = (int)Rarity.RARE,
                    [(int)GameTag.BATTLECRY] = 1,
                    [(int)GameTag.FROZEN] = 0,
                    [(int)GameTag.ZONE_POSITION] = 0,
                    [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                    [(int)GameTag.FORCED_PLAY] = 0,
                    [(int)GameTag.TO_BE_DESTROYED] = 0,
                    [(int)GameTag.START_WITH_1_HEALTH] = 0,
                    [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                    [(int)GameTag.RITUAL] = 1,
                    [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                    [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 3,
                    [479] = 2,
                }));
                Adapter.SendMessage(TagChangeTest(22, (int)GameTag.ZONE_POSITION, 2));
                Adapter.SendMessage(new KettleHistoryHideEntity() { EntityID = 15, Zone = (int)Zone.DECK });
                Adapter.SendMessage(TagChangeTest(15, (int)GameTag.ZONE, (int)Zone.DECK));
                Adapter.SendMessage(TagChangeTest(15, (int)GameTag.ZONE_POSITION, 0));

                Adapter.SendMessage(ShowEntityTest(26, "LOE_077", new Dictionary<int, int>
                {
                    [(int)GameTag.PREMIUM] = 0,
                    [(int)GameTag.DAMAGE] = 0,
                    [(int)GameTag.HEALTH] = 4,
                    [(int)GameTag.ATK] = 2,
                    [(int)GameTag.COST] = 3,
                    [(int)GameTag.ZONE] = (int)Zone.HAND,
                    [(int)GameTag.CONTROLLER] = 1,
                    [(int)GameTag.ENTITY_ID] = 26,
                    [(int)GameTag.ELITE] = 1,
                    [(int)GameTag.SILENCED] = 0,
                    [(int)GameTag.WINDFURY] = 0,
                    [(int)GameTag.TAUNT] = 0,
                    [(int)GameTag.STEALTH] = 0,
                    [(int)GameTag.DIVINE_SHIELD] = 0,
                    [(int)GameTag.CHARGE] = 0,
                    //[(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                    [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                    [(int)GameTag.RARITY] = (int)Rarity.LEGENDARY,
                    //[(int)GameTag.BATTLECRY] = 1,
                    [(int)GameTag.FROZEN] = 0,
                    [(int)GameTag.ZONE_POSITION] = 0,
                    [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                    [(int)GameTag.FORCED_PLAY] = 0,
                    [(int)GameTag.TO_BE_DESTROYED] = 0,
                    [(int)GameTag.AURA] = 1,
                    [(int)GameTag.START_WITH_1_HEALTH] = 0,
                    [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                    //[(int)GameTag.RITUAL] = 1,
                    [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                    [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 3,
                    [479] = 2,
                }));
                Adapter.SendMessage(TagChangeTest(26, (int)GameTag.ZONE_POSITION, 3));
                Adapter.SendMessage(new KettleHistoryHideEntity() { EntityID = 18, Zone = (int)Zone.DECK });
                Adapter.SendMessage(TagChangeTest(18, (int)GameTag.ZONE, (int)Zone.DECK));
                Adapter.SendMessage(TagChangeTest(18, (int)GameTag.ZONE_POSITION, 0));

                Adapter.SendMessage(TagChangeTest(2, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.WAITING));

                Adapter.SendMessage(BlockEndTest());

                Adapter.SendMessage(BlockStartTest("", 7, 3, 0, (int)BlockType.TRIGGER));
                    Adapter.SendMessage(TagChangeTest(3, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.DONE));
                    //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.MAIN_READY));
                Adapter.SendMessage(BlockEndTest());

                /* MAIN READY !!! */
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.STEP, (int)Step.MAIN_READY));

                //Adapter.SendMessage(BlockStartTest("", 1, 2, 0, (int)BlockType.TRIGGER));
                //Adapter.SendMessage(TagChangeTest(66, (int)GameTag.NUM_TURNS_IN_PLAY, 1));
                //Adapter.SendMessage(TagChangeTest(67, (int)GameTag.NUM_TURNS_IN_PLAY, 1));
                //Adapter.SendMessage(TagChangeTest(64, (int)GameTag.NUM_TURNS_IN_PLAY, 1));
                //Adapter.SendMessage(TagChangeTest(65, (int)GameTag.NUM_TURNS_IN_PLAY, 1));
                //Adapter.SendMessage(TagChangeTest(2, (int)GameTag.RESOURCES, 1));
                //Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 0));
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.MAIN_START_TRIGGERS));
                //Adapter.SendMessage(BlockEndTest());

                /* MAIN START TRIGGERS !!! */
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.STEP, (int)Step.MAIN_START_TRIGGERS));

                //Adapter.SendMessage(BlockStartTest("", 8, 2, 0, (int)BlockType.TRIGGER));
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.MAIN_START));
                //Adapter.SendMessage(BlockEndTest());

                /* MAIN START !!! */
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.STEP, (int)Step.MAIN_START));

                // start turn block
                //Adapter.SendMessage(BlockStartTest("", 0, 2, 0, (int)BlockType.TRIGGER));
                //Adapter.SendMessage(TagChangeTest(2, 467, 1));
                //Adapter.SendMessage(ShowEntityTest(62, "CS2_182", new Dictionary<int, int>
                //{
                //    [(int)GameTag.PREMIUM] = 0,
                //    [(int)GameTag.DAMAGE] = 0,
                //    [(int)GameTag.HEALTH] = 5,
                //    [(int)GameTag.ATK] = 4,
                //    [(int)GameTag.COST] = 4,
                //    [(int)GameTag.ZONE] = (int)Zone.HAND,
                //    [(int)GameTag.CONTROLLER] = 2,
                //    [(int)GameTag.ENTITY_ID] = 62,
                //    //[(int)GameTag.ELITE] = 1,
                //    [(int)GameTag.SILENCED] = 0,
                //    [(int)GameTag.WINDFURY] = 0,
                //    [(int)GameTag.TAUNT] = 0,
                //    [(int)GameTag.STEALTH] = 0,
                //    [(int)GameTag.DIVINE_SHIELD] = 0,
                //    [(int)GameTag.CHARGE] = 0,
                //    [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                //    [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                //    [(int)GameTag.RARITY] = (int)Rarity.COMMON,
                //    //[(int)GameTag.BATTLECRY] = 1,
                //    [(int)GameTag.FROZEN] = 0,
                //    [(int)GameTag.ZONE_POSITION] = 0,
                //    [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                //    [(int)GameTag.FORCED_PLAY] = 0,
                //    [(int)GameTag.TO_BE_DESTROYED] = 0,
                //    //[(int)GameTag.AURA] = 1,
                //    [(int)GameTag.START_WITH_1_HEALTH] = 0,
                //    [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                //    //[(int)GameTag.RITUAL] = 1,
                //    [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                //    [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 4,
                //    //[479] = 4,
                //}));
                //Adapter.SendMessage(TagChangeTest(62, (int)GameTag.ZONE_POSITION, 4));
                //Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 1));
                //Adapter.SendMessage(TagChangeTest(2, 467, 0));
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.MAIN_ACTION));
                //Adapter.SendMessage(BlockEndTest());

                /* MAIN ACTION !!! */
                //Adapter.SendMessage(TagChangeTest(1, (int)GameTag.STEP, (int)Step.MAIN_ACTION));

                //Adapter.SendMessage(BlockStartTest("", 2, 2, 0, (int)BlockType.TRIGGER));
                //    Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.MAIN_END));
                //Adapter.SendMessage(BlockEndTest());
            }

            if (chooseEntities.ID == 2)
            {

                Adapter.SendMessage(TagChangeTest(2, (int) GameTag.MULLIGAN_STATE, (int) Mulligan.DEALING));

                Adapter.SendMessage(BlockStartTest("", 6, 2, 0, (int) BlockType.TRIGGER));
                Adapter.SendMessage(ShowEntityTest(43, "CS2_029", new Dictionary<int, int>
                {
                    [(int) GameTag.PREMIUM] = 0,
                    [(int) GameTag.DAMAGE] = 0,
                    //[(int)GameTag.HEALTH] = 5,
                    //[(int)GameTag.ATK] = 5,
                    [(int) GameTag.COST] = 4,
                    [(int) GameTag.ZONE] = (int) Zone.HAND,
                    [(int) GameTag.CONTROLLER] = 2,
                    [(int) GameTag.ENTITY_ID] = 43,
                    //[(int)GameTag.ELITE] = 1,
                    [(int) GameTag.SILENCED] = 0,
                    [(int) GameTag.WINDFURY] = 0,
                    [(int) GameTag.TAUNT] = 0,
                    [(int) GameTag.STEALTH] = 0,
                    [(int) GameTag.DIVINE_SHIELD] = 0,
                    [(int) GameTag.CHARGE] = 0,
                    [(int) GameTag.FACTION] = (int) Faction.NEUTRAL,
                    [(int) GameTag.CARDTYPE] = (int) CardType.SPELL,
                    [(int) GameTag.RARITY] = (int) Rarity.FREE,
                    [(int) GameTag.FROZEN] = 0,
                    [(int) GameTag.ZONE_POSITION] = 0,
                    [(int) GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.FORCED_PLAY] = 0,
                    [(int) GameTag.TO_BE_DESTROYED] = 0,
                    [(int) GameTag.START_WITH_1_HEALTH] = 0,
                    [(int) GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                    [(int) GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 4,
                    [479] = 0,
                }));
                Adapter.SendMessage(TagChangeTest(43, (int) GameTag.ZONE_POSITION, 1));
                Adapter.SendMessage(new KettleHistoryHideEntity() {EntityID = 57, Zone = (int) Zone.DECK});
                Adapter.SendMessage(TagChangeTest(57, (int) GameTag.ZONE, (int) Zone.DECK));
                Adapter.SendMessage(TagChangeTest(57, (int) GameTag.ZONE_POSITION, 0));

                Adapter.SendMessage(ShowEntityTest(59, "OG_141", new Dictionary<int, int>
                {
                    [(int) GameTag.PREMIUM] = 0,
                    [(int) GameTag.DAMAGE] = 0,
                    [(int) GameTag.HEALTH] = 10,
                    [(int) GameTag.ATK] = 10,
                    [(int) GameTag.COST] = 10,
                    [(int) GameTag.ZONE] = (int) Zone.HAND,
                    [(int) GameTag.CONTROLLER] = 2,
                    [(int) GameTag.ENTITY_ID] = 59,
                    //[(int)GameTag.ELITE] = 1,
                    [(int) GameTag.SILENCED] = 0,
                    [(int) GameTag.WINDFURY] = 0,
                    [(int) GameTag.TAUNT] = 0,
                    [(int) GameTag.STEALTH] = 0,
                    [(int) GameTag.DIVINE_SHIELD] = 0,
                    [(int) GameTag.CHARGE] = 0,
                    //[(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                    [(int) GameTag.CARDTYPE] = (int) CardType.MINION,
                    [(int) GameTag.RARITY] = (int) Rarity.COMMON,
                    [(int) GameTag.FROZEN] = 0,
                    [(int) GameTag.ZONE_POSITION] = 0,
                    [(int) GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.FORCED_PLAY] = 0,
                    [(int) GameTag.TO_BE_DESTROYED] = 0,
                    [(int) GameTag.START_WITH_1_HEALTH] = 0,
                    [(int) GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                    [(int) GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 10,
                    [479] = 10,
                }));
                Adapter.SendMessage(TagChangeTest(59, (int) GameTag.ZONE_POSITION, 2));
                Adapter.SendMessage(new KettleHistoryHideEntity() {EntityID = 34, Zone = (int) Zone.DECK});
                Adapter.SendMessage(TagChangeTest(34, (int) GameTag.ZONE, (int) Zone.DECK));
                Adapter.SendMessage(TagChangeTest(34, (int) GameTag.ZONE_POSITION, 0));

                Adapter.SendMessage(ShowEntityTest(51, "EX1_399", new Dictionary<int, int>
                {
                    [(int) GameTag.PREMIUM] = 0,
                    [(int) GameTag.TRIGGER_VISUAL] = 1,
                    [(int) GameTag.DAMAGE] = 0,
                    [(int) GameTag.HEALTH] = 7,
                    [(int) GameTag.ATK] = 2,
                    [(int) GameTag.COST] = 5,
                    [(int) GameTag.ZONE] = (int) Zone.HAND,
                    [(int) GameTag.CONTROLLER] = 2,
                    [(int) GameTag.ENTITY_ID] = 51,
                    //[(int)GameTag.ELITE] = 1,
                    [(int) GameTag.SILENCED] = 0,
                    [(int) GameTag.WINDFURY] = 0,
                    [(int) GameTag.TAUNT] = 0,
                    [(int) GameTag.STEALTH] = 0,
                    [(int) GameTag.DIVINE_SHIELD] = 0,
                    [(int) GameTag.CHARGE] = 0,
                    [(int) GameTag.FACTION] = (int) Faction.NEUTRAL,
                    [(int) GameTag.CARDTYPE] = (int) CardType.MINION,
                    [(int) GameTag.RARITY] = (int) Rarity.COMMON,
                    [(int) GameTag.FROZEN] = 0,
                    [(int) GameTag.ZONE_POSITION] = 0,
                    [(int) GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.FORCED_PLAY] = 0,
                    [(int) GameTag.TO_BE_DESTROYED] = 0,
                    [(int) GameTag.START_WITH_1_HEALTH] = 0,
                    [(int) GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                    [(int) GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                    [(int) GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 5,
                    [479] = 5,
                }));
                Adapter.SendMessage(TagChangeTest(51, (int) GameTag.ZONE_POSITION, 3));
                Adapter.SendMessage(new KettleHistoryHideEntity() {EntityID = 46, Zone = (int) Zone.DECK});
                Adapter.SendMessage(TagChangeTest(46, (int) GameTag.ZONE, (int) Zone.DECK));
                Adapter.SendMessage(TagChangeTest(46, (int) GameTag.ZONE_POSITION, 0));
                Adapter.SendMessage(TagChangeTest(2, (int) GameTag.MULLIGAN_STATE, (int) Mulligan.WAITING));

                Adapter.SendMessage(BlockEndTest());

                Adapter.SendMessage(BlockStartTest("", 7, 2, 0, (int) BlockType.TRIGGER));
                    Adapter.SendMessage(TagChangeTest(2, (int) GameTag.MULLIGAN_STATE, (int) Mulligan.DONE));
                Adapter.SendMessage(BlockEndTest());
            }
        }

        public void OnCreateGame(KettleCreateGame createGame)
        {
            Console.WriteLine("OnCreateGame");

            Adapter.SendMessage(CreateGameTest());
            Adapter.SendMessage(CreateFullEntities().Select(p => p as KettlePayload).ToList());
            Adapter.SendMessage(TagChangeTest(1, (int) GameTag.STATE, (int) State.RUNNING));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.PLAYSTATE, (int)PlayState.PLAYING));
            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.PLAYSTATE, (int)PlayState.PLAYING));

            Adapter.SendMessage(BlockStartTest("", -1, 1, 0, (int)BlockType.TRIGGER));

            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.CURRENT_PLAYER, 1));
            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.FIRST_PLAYER, 1));
            Adapter.SendMessage(TagChangeTest(1, (int)GameTag.TURN, 1));
            //Adapter.SendMessage(TagChangeTest(2, 467, 4));
            Adapter.SendMessage(ShowEntityTest(20, "EX1_016", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                [(int)GameTag.HEALTH] = 5,
                [(int)GameTag.ATK] = 5,
                [(int)GameTag.COST] = 6,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 20,
                [(int)GameTag.ELITE] = 1,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 0,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                //[(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                [(int)GameTag.RARITY] = (int)Rarity.LEGENDARY,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 6,
                [479] = 5,
            }));

            Adapter.SendMessage(TagChangeTest(20, (int)GameTag.ZONE_POSITION, 1));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 1));
            //Adapter.SendMessage(TagChangeTest(2, 467, 3));
            Adapter.SendMessage(ShowEntityTest(15, "EX1_606", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                //[(int)GameTag.HEALTH] = 3,
                //[(int)GameTag.ATK] = 4,
                [(int)GameTag.COST] = 3,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 15,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 0,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.SPELL,
                [(int)GameTag.RARITY] = (int)Rarity.COMMON,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 3,
                [479] = 0,
            }));

            Adapter.SendMessage(TagChangeTest(15, (int)GameTag.ZONE_POSITION, 2));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 2));
            //TagChangeTest(2, 467, 2);
            Adapter.SendMessage(ShowEntityTest(18, "EX1_410", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                //[(int)GameTag.HEALTH] = 3,
                //[(int)GameTag.ATK] = 4,
                [(int)GameTag.COST] = 1,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 18,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 0,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.SPELL,
                [(int)GameTag.RARITY] = (int)Rarity.EPIC,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.AFFECTED_BY_SPELL_POWER] = 1,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 1,
                [479] = 0,
            }));
            Adapter.SendMessage(TagChangeTest(18, (int)GameTag.ZONE_POSITION, 3));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 3));
            //Adapter.SendMessage(TagChangeTest(2, 467, 0));

            Adapter.SendMessage(ShowEntityTest(32, "BRM_015", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                //[(int)GameTag.HEALTH] = 3,
                //[(int)GameTag.ATK] = 4,
                [(int)GameTag.COST] = 2,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 32,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 0,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                //[(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.SPELL,
                [(int)GameTag.RARITY] = (int)Rarity.RARE,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 2,
                [479] = 0,
            }));
            Adapter.SendMessage(TagChangeTest(32, (int)GameTag.ZONE_POSITION, 4));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 4));
            //Adapter.SendMessage(TagChangeTest(2, 467, 0));
            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.NUM_TURNS_LEFT, 1));

            Adapter.SendMessage(FullEntityCreate(68, "GAME_005", new Dictionary<int, int>
            {
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 68,
                [(int)GameTag.CARDTYPE] = (int)CardType.SPELL,
                [(int)GameTag.ZONE_POSITION] = 5,
                [(int)GameTag.CREATOR] = 1,
            }));

            //Adapter.SendMessage(TagChangeTest(3, 467, 3));

            Adapter.SendMessage(ShowEntityTest(57, "EX1_097", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                [(int)GameTag.HEALTH] = 4,
                [(int)GameTag.ATK] = 4,
                [(int)GameTag.COST] = 5,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 2,
                [(int)GameTag.ENTITY_ID] = 57,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 1,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                [(int)GameTag.RARITY] = (int)Rarity.RARE,
                [(int)GameTag.DEATHRATTLE] = 1,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 5,
                [479] = 4,
            }));
            Adapter.SendMessage(TagChangeTest(57, (int)GameTag.ZONE_POSITION, 1));
            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 1));
            //Adapter.SendMessage(TagChangeTest(3, 467, 2));

            Adapter.SendMessage(ShowEntityTest(34, "CS2_187", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                [(int)GameTag.HEALTH] = 4,
                [(int)GameTag.ATK] = 5,
                [(int)GameTag.COST] = 5,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 2,
                [(int)GameTag.ENTITY_ID] = 34,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 1,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                [(int)GameTag.FACTION] = (int)Faction.HORDE,
                [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                [(int)GameTag.RARITY] = (int)Rarity.COMMON,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 5,
                [479] = 5,
            }));
            Adapter.SendMessage(TagChangeTest(34, (int)GameTag.ZONE_POSITION, 2));
            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 2));
            //Adapter.SendMessage(TagChangeTest(3, 467, 1));

            Adapter.SendMessage(ShowEntityTest(46, "CS2_162", new Dictionary<int, int>
            {
                [(int)GameTag.PREMIUM] = 0,
                [(int)GameTag.DAMAGE] = 0,
                [(int)GameTag.HEALTH] = 5,
                [(int)GameTag.ATK] = 6,
                [(int)GameTag.COST] = 6,
                [(int)GameTag.ZONE] = (int)Zone.HAND,
                [(int)GameTag.CONTROLLER] = 2,
                [(int)GameTag.ENTITY_ID] = 46,
                [(int)GameTag.SILENCED] = 0,
                [(int)GameTag.WINDFURY] = 0,
                [(int)GameTag.TAUNT] = 1,
                [(int)GameTag.STEALTH] = 0,
                [(int)GameTag.DIVINE_SHIELD] = 0,
                [(int)GameTag.CHARGE] = 0,
                [(int)GameTag.FACTION] = (int)Faction.ALLIANCE,
                [(int)GameTag.CARDTYPE] = (int)CardType.MINION,
                [(int)GameTag.RARITY] = (int)Rarity.COMMON,
                [(int)GameTag.FROZEN] = 0,
                [(int)GameTag.ZONE_POSITION] = 0,
                [(int)GameTag.NUM_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.FORCED_PLAY] = 0,
                [(int)GameTag.TO_BE_DESTROYED] = 0,
                [(int)GameTag.START_WITH_1_HEALTH] = 0,
                [(int)GameTag.CUSTOM_KEYWORD_EFFECT] = 0,
                [(int)GameTag.EXTRA_ATTACKS_THIS_TURN] = 0,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 6,
                [479] = 6,
            }));
            Adapter.SendMessage(TagChangeTest(46, (int)GameTag.ZONE_POSITION, 3));
            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.NUM_CARDS_DRAWN_THIS_TURN, 3));
            //Adapter.SendMessage(TagChangeTest(3, 467, 0));

            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.NUM_TURNS_LEFT, 1));

            //Adapter.SendMessage(TagChangeTest(2, (int)GameTag.TIMEOUT, 75));
            //Adapter.SendMessage(TagChangeTest(3, (int)GameTag.TIMEOUT, 75));
            //Adapter.SendMessage(TagChangeTest(1, 10, 85));
            Adapter.SendMessage(TagChangeTest(1, (int)GameTag.NEXT_STEP, (int)Step.BEGIN_MULLIGAN));

            Adapter.SendMessage(BlockEndTest());

            Adapter.SendMessage(TagChangeTest(1, (int)GameTag.STEP, (int)Step.BEGIN_MULLIGAN));

            Adapter.SendMessage(BlockStartTest("", -1, 1, 0, (int)BlockType.TRIGGER));

            Adapter.SendMessage(TagChangeTest(2, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.INPUT));
            Adapter.SendMessage(EntityChoicesTest((int)ChoiceType.MULLIGAN, 5, 0, 1, new List<int> { 20, 15, 18, 32, 68 }, 1, 1));

            Adapter.SendMessage(TagChangeTest(3, (int)GameTag.MULLIGAN_STATE, (int)Mulligan.INPUT));
            Adapter.SendMessage(EntityChoicesTest((int)ChoiceType.MULLIGAN, 3, 0, 1, new List<int> { 57, 34, 46 }, 2, 2));

            Adapter.SendMessage(BlockEndTest());
        }

        private void SendChoicesOrOptions()
        {
            // getting choices mulligan choices for players ...
            var entityChoicesPlayer1 = PowerChoicesBuilder.EntityChoices(_game, _game.Player1.Choice);
            var entityChoicesPlayer2 = PowerChoicesBuilder.EntityChoices(_game, _game.Player2.Choice);

            // getting options for currentPlayer ...
            var options = PowerOptionsBuilder.AllOptions(_game, _game.CurrentPlayer.Options());

            if (entityChoicesPlayer1 != null)
                SendEntityChoices(entityChoicesPlayer1);

            if (entityChoicesPlayer2 != null)
                SendEntityChoices(entityChoicesPlayer2);

            if (options != null)
                SendOptions(options);
        }

        private void SendEntityChoices(PowerEntityChoices choices)
        {
            Adapter.SendMessage(new KettleEntityChoices(choices));
        }

        private void SendOptions(PowerAllOptions options)
        {
            Adapter.SendMessage(new KettleOptionsBlock(options, _game.CurrentPlayer.PlayerId));
        }

        private void SendPowerHistory(List<IPowerHistoryEntry> history)
        {
            List<KettlePayload> message = new List<KettlePayload>();
            foreach (var entry in history)
            {
                message.Add(KettleHistoryEntry.From(entry));
            }
            Adapter.SendMessage(message);
        }

        private static KettleHistoryCreateGame CreateGameTest()
        {
            var k = new KettleHistoryCreateGame
            {
                Game = new KettleEntity
                {
                    EntityID = 1,
                    Tags = new Dictionary<int, int>
                    {
                        [(int)GameTag.ENTITY_ID] = 1,
                        [(int)GameTag.ZONE] = (int)Zone.PLAY,
                        [(int)GameTag.CARDTYPE] = (int)CardType.GAME,
                    }
                },
                Players = new List<KettlePlayer>(),
            };

            k.Players.Add(new KettlePlayer
            {
                Entity = new KettleEntity()
                {
                    EntityID = 2,
                    Tags = new Dictionary<int, int>
                    {
                        [(int)GameTag.ENTITY_ID] = 2,
                        [(int)GameTag.PLAYER_ID] = 1,
                        [(int)GameTag.HERO_ENTITY] = 4,
                        [(int)GameTag.MAXHANDSIZE] = 10,
                        [(int)GameTag.STARTHANDSIZE] = 4,
                        [(int)GameTag.TEAM_ID] = 1,
                        [(int)GameTag.ZONE] = (int)Zone.PLAY,
                        [(int)GameTag.CONTROLLER] = 1,
                        [(int)GameTag.MAXRESOURCES] = 10,
                        [(int)GameTag.CARDTYPE] = (int)CardType.PLAYER,
                    }
                },
                PlayerID = 1,
                CardBack = 0
            });


            k.Players.Add(new KettlePlayer
            {
                Entity = new KettleEntity()
                {
                    EntityID = 3,
                    Tags = new Dictionary<int, int>
                    {
                        [(int)GameTag.ENTITY_ID] = 3,
                        [(int)GameTag.PLAYER_ID] = 2,
                        [(int)GameTag.HERO_ENTITY] = 6,
                        [(int)GameTag.MAXHANDSIZE] = 10,
                        [(int)GameTag.STARTHANDSIZE] = 4,
                        [(int)GameTag.TEAM_ID] = 2,
                        [(int)GameTag.ZONE] = (int)Zone.PLAY,
                        [(int)GameTag.CONTROLLER] = 2,
                        [(int)GameTag.MAXRESOURCES] = 10,
                        [(int)GameTag.CARDTYPE] = (int)CardType.PLAYER,
                    }
                },
                PlayerID = 1,
                CardBack = 0
            });
            return k;
        }

        private static List<KettleHistoryFullEntity> CreateFullEntities()
        {
            var list = new List<KettleHistoryFullEntity>();
            for (var i = 0; i < 60; i++)
            {
                list.Add(FullEntityCreate(i + 8, "", new Dictionary<int, int>
                {
                    [(int)GameTag.ZONE] = (int)Zone.DECK,
                    [(int)GameTag.CONTROLLER] = i < 30 ? 1 : 2,
                    [(int)GameTag.ENTITY_ID] = i + 4,

                }));
            }

            list.Add(FullEntityCreate(4, "HERO_01", new Dictionary<int, int>
            {
                [(int)GameTag.HEALTH] = 30,
                [(int)GameTag.ZONE] = (int)Zone.PLAY,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 4,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.HERO,
                [(int)GameTag.RARITY] = (int)Rarity.FREE,
                [(int)GameTag.HERO_POWER] = 725,
            }));

            list.Add(FullEntityCreate(5, "CS2_102", new Dictionary<int, int>
            {
                [(int)GameTag.COST] = 2,
                [(int)GameTag.ZONE] = (int)Zone.PLAY,
                [(int)GameTag.CONTROLLER] = 1,
                [(int)GameTag.ENTITY_ID] = 5,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.HERO_POWER,
                [(int)GameTag.RARITY] = (int)Rarity.FREE,
                [(int)GameTag.CREATOR] = 4,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 2,
            }));

            list.Add(FullEntityCreate(6, "HERO_08", new Dictionary<int, int>
            {
                [(int)GameTag.HEALTH] = 30,
                [(int)GameTag.ZONE] = (int)Zone.PLAY,
                [(int)GameTag.CONTROLLER] = 2,
                [(int)GameTag.ENTITY_ID] = 6,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.HERO,
                [(int)GameTag.RARITY] = (int)Rarity.FREE,
                [(int)GameTag.HERO_POWER] = 807,
            }));

            list.Add(FullEntityCreate(7, "CS2_034", new Dictionary<int, int>
            {
                [(int)GameTag.COST] = 2,
                [(int)GameTag.ZONE] = (int)Zone.PLAY,
                [(int)GameTag.CONTROLLER] = 2,
                [(int)GameTag.ENTITY_ID] = 7,
                [(int)GameTag.FACTION] = (int)Faction.NEUTRAL,
                [(int)GameTag.CARDTYPE] = (int)CardType.HERO_POWER,
                [(int)GameTag.RARITY] = (int)Rarity.FREE,
                [(int)GameTag.CREATOR] = 6,
                [(int)GameTag.TAG_LAST_KNOWN_COST_IN_HAND] = 2,
            }));

            return list;
        }

        public static KettleHistoryFullEntity FullEntityCreate(int entityId, string cardId, Dictionary<int, int> tags)
        {
            var k = new KettleHistoryFullEntity
            {
                Name = cardId,
                Entity = new KettleEntity()
                {
                    EntityID = entityId,
                    Tags = tags
                }
            };
            return k;
        }

        public static KettleHistoryShowEntity ShowEntityTest(int entityId, string cardId, Dictionary<int, int> tags)
        {
            var k = new KettleHistoryShowEntity
            {
                Name = cardId,
                Entity = new KettleEntity()
                {
                    EntityID = entityId,
                    Tags = tags
                }
            };
            return k;
        }

        public static KettleHistoryTagChange TagChangeTest(int entityId, int tag, int value)
        {
            var k = new KettleHistoryTagChange
            {
                EntityID = entityId,
                Tag = tag,
                Value = value,
            };

            return k;
        }

        public static KettleEntityChoices EntityChoicesTest(int choiceType, int countMax, int countMin, int source, List<int> entities, int playerId, int index)
        {
            var k = new KettleEntityChoices
            {
                ChoiceType = choiceType,
                CountMax = countMax,
                CountMin = countMin,
                Source = source,
                Entities = entities,
                PlayerID = playerId,
                ID = index,
            };
            return k;
        }

        public static KettleHistoryBlockBegin BlockStartTest(string effectCardId, int index, int source, int target, int blockType)
        {
            var k = new KettleHistoryBlockBegin
            {
                EffectCardId = effectCardId,
                Index = index,
                Source = source,
                Target = target,
                Type = blockType,
            };
            return k;
        }

        public static KettleHistoryBlockEnd BlockEndTest()
        {
            var k = new KettleHistoryBlockEnd();
            return k;
        }

    }
}