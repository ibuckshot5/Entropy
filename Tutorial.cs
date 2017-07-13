using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using POGOLib.Official.Net;
using POGOLib.Official.Util;
using POGOProtos.Data.Player;
using POGOProtos.Enums;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using Troschuetz.Random;

namespace Entropy
{
    public class Tutorial
    {
        public static async Task<RepeatedField<TutorialState>> GetTutorialState(Session session)
        {
            // Form1.Logger.Debug($"Checking tutorial state for {session.Player.Data.Username}.");
            // Get player locale based on current location
            var request = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetPlayer,
                RequestMessage = new GetPlayerMessage
                {
                    PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
                    {
                        Country = "US",
                        Language = "en",
                        Timezone = "America/New_York"
                    }
                }.ToByteString()
            });
            var response = GetPlayerResponse.Parser.ParseFrom(request);
            return response.PlayerData.TutorialState;
        }

        public static async Task CompleteTutorial(Session session, RepeatedField<TutorialState> tutorialState)
        {
            var random = new TRandom();
            if (!tutorialState.Contains(0))
            {
                await Task.Delay(random.Next(1000, 5000));
                var mtcReq = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.MarkTutorialComplete,
                    RequestMessage = new MarkTutorialCompleteMessage
                    {
                        SendMarketingEmails = false,
                        SendPushNotifications = false
                    }.ToByteString()
                });
                var mtcResp = MarkTutorialCompleteResponse.Parser.ParseFrom(mtcReq);
                // Form1.Logger.Debug($"Sending 0 tutorials_completed for {session.Player.Data.Username}.");
            }
            if (!tutorialState.Contains((TutorialState)1))
            {
                await Task.Delay(random.Next(5000, 12000));
                var caReq = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.SetAvatar,
                    RequestMessage = new SetAvatarMessage
                    {
                        PlayerAvatar = new PlayerAvatar
                        {
                            Hair = random.Next(1, 5),
                            Shirt = random.Next(1, 3),
                            Pants = random.Next(1, 2),
                            Shoes = random.Next(1, 6),
                            Avatar = random.Next(0, 1),
                            Eyes = random.Next(1, 4),
                            Backpack = random.Next(1, 5)
                        }
                    }.ToByteString()
                });
                var caResp = SetAvatarResponse.Parser.ParseFrom(caReq);
                // Form1.Logger.Debug($"Sending set random player character request for ${session.Player.Data.Username}.");
            }
            await Task.Delay(random.Next(500, 600));
            var gppr = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
            {
                RequestType = RequestType.GetPlayerProfile,
                RequestMessage = new GetPlayerProfileMessage
                {
                    PlayerName = session.Player.Data.Username
                }.ToByteString()
            });
            var gppre = GetPlayerProfileResponse.Parser.ParseFrom(gppr);
            // Form1.Logger.Debug($"Fetching player profile for {session.Player.Data.Username}...");
            if (!tutorialState.Contains((TutorialState)3))
            {
                await Task.Delay(random.Next(1000, 1500));
                var starters = new[] { PokemonId.Bulbasaur, PokemonId.Charmander, PokemonId.Squirtle };
                var starter = starters[random.Next(0, starters.Length)];
                var streq = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.EncounterTutorialComplete,
                    RequestMessage = new EncounterTutorialCompleteMessage
                    {
                        PokemonId = starter
                    }.ToByteString()
                });
                // Form1.Logger.Debug($"Catching the starter for {session.Player.Data.Username}. (Chose {starter.ToString()})");
                var gpr = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = new GetPlayerMessage
                    {
                        PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
                        {
                            Country = "US",
                            Language = "en",
                            Timezone = "America/New_York"
                        }
                    }.ToByteString()
                });
                var gpre = GetPlayerResponse.Parser.ParseFrom(gpr);
            }
            if (!tutorialState.Contains((TutorialState)4))
            {
                await Task.Delay(random.Next(5000, 12000));
                var ccreq = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.ClaimCodename,
                    RequestMessage = new ClaimCodenameMessage
                    {
                        Codename = session.Player.Data.Username
                    }.ToByteString()
                });
                var ccres = ClaimCodenameResponse.Parser.ParseFrom(ccreq);
                // Form1.Logger.Debug($"Claimed codename for {session.Player.Data.Username}.");
                var gpr = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = new GetPlayerMessage
                    {
                        PlayerLocale = new GetPlayerMessage.Types.PlayerLocale
                        {
                            Country = "US",
                            Language = "en",
                            Timezone = "America/New_York"
                        }
                    }.ToByteString()
                });
                var gpre = GetPlayerResponse.Parser.ParseFrom(gpr);
            }
            var firstOrDefault = session.Player.Inventory.InventoryItems.Select(i => i.InventoryItemData?.PokemonData).FirstOrDefault(p => p != null && p.PokemonId > 0);
            if (firstOrDefault != null)
            {
                var starterId = firstOrDefault.Id;
                if (starterId != null)
                {
                    var budreq = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                    {
                        RequestType = RequestType.SetBuddyPokemon,
                        RequestMessage = new SetBuddyPokemonMessage
                        {
                            PokemonId = starterId
                        }.ToByteString()
                    });
                    var budres = SetBuddyPokemonResponse.Parser.ParseFrom(budreq);
                    // Form1.Logger.Debug($"Setting buddy pokemon for {session.Player.Data.Username}.");
                }
            }
            // Form1.Logger.Info($"And {session.Player.Data.Username} is done. Waiting a sec to avoid throttle.");
            await Task.Delay(random.Next(2000, 4000));
        }

        public static async Task TutorialPokestopSpin(Session session)
        {
            var playerLevel = session.Player.Stats.Level;
            if (playerLevel > 1)
            {
                // Form1.Logger.Debug($"No need to spin a PokeStop. {session.Player.Data.Username} is already level {playerLevel}.");
            }
            else
            {
                // Form1.Logger.Debug($"Spinning PokeStop for account {session.Player.Data.Username}.");
                foreach (var fortData in session.Map.GetFortsSortedByDistance(f => f.Type == FortType.Checkpoint && f.LureInfo != null))
                {
                    session.Player.SetCoordinates(fortData.Latitude, fortData.Longitude);
                    if (fortData.CooldownCompleteTimestampMs <= TimeUtil.GetCurrentTimestampInMilliseconds())
                    {
                        var playerDistance = session.Player.DistanceTo(fortData.Latitude, fortData.Longitude);
                        if (playerDistance <= session.GlobalSettings.FortSettings.InteractionRangeMeters)
                        {
                            var fortSearchResponseBytestring = await session.RpcClient.SendRemoteProcedureCallAsync(new Request
                            {
                                RequestType = RequestType.FortSearch,
                                RequestMessage = new FortSearchMessage
                                {
                                    FortId = fortData.Id,
                                    FortLatitude = fortData.Latitude,
                                    FortLongitude = fortData.Longitude,
                                    PlayerLatitude = session.Player.Latitude,
                                    PlayerLongitude = session.Player.Longitude
                                }.ToByteString()
                            });

                            var fortSearchResponse = FortSearchResponse.Parser.ParseFrom(fortSearchResponseBytestring);
                            switch (fortSearchResponse.Result)
                            {
                                case FortSearchResponse.Types.Result.NoResultSet:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: Unknown error.");
                                    break;
                                case FortSearchResponse.Types.Result.Success:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: Spun pokestop successfully. {session.Player.Data.Username} is now level {session.Player.Stats.Level}.");
                                    break;
                                case FortSearchResponse.Types.Result.OutOfRange:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: Out of range.");
                                    break;
                                case FortSearchResponse.Types.Result.InCooldownPeriod:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: In cooldown.");
                                    break;
                                case FortSearchResponse.Types.Result.InventoryFull:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: Inventory full.");
                                    break;
                                case FortSearchResponse.Types.Result.ExceededDailyLimit:
                                    // Form1.Logger.Debug($"{session.Player.Data.Username}: Exceeded daily pokestop limit.");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(fortSearchResponse));
                            }
                        }
                    }
                }
            }
        }
    }
}