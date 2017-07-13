using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Entropy.Responses;
using GeoCoordinatePortable;
using Google.Protobuf;
using Newtonsoft.Json;
using POGOLib.Official.LoginProviders;
using POGOLib.Official.Net;
using POGOLib.Official.Net.Authentication;
using POGOLib.Official.Net.Authentication.Data;
using POGOLib.Official.Util.Hash;
using POGOProtos.Inventory.Item;
using POGOProtos.Map.Fort;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;
using RestSharp;
using Troschuetz.Random;

namespace Entropy
{
    public class Creator
    {
        public static async Task<AccountCreationResult> Create(AccountCreationOptions options)
        {
            var random = new TRandom();
            var client = new RestClient { Proxy = options.Proxy };
            
            var page = client.Execute(new RestRequest("https://club.pokemon.com/us/pokemon-trainer-club/sign-up/", 
                Method.GET));
            
            var csrf = string.Empty;
            
            // Get CSRF
            var match =
                new Regex("<input type='hidden' name='csrfmiddlewaretoken' value='(\\w+)' />").Match(page.Content);
            if (match.Success)
                csrf = match.Groups[1].Value;

            client.Execute(new RestRequest("https://club.pokemon.com/us/pokemon-trainer-club/sign-up/", Method.POST)
                .AddParameter("csrfmiddlewaretoken", csrf)
                .AddParameter("dob", options.Dob)
                .AddParameter("country", "US")
                .AddParameter("country", "US")
                .AddParameter("picker__year", options.Dob.Split('-')[0])
                .AddParameter("picker__month", options.Dob.Split('-')[1]));

            await Task.Delay(random.Next(2000, 3000));

            var user = client.Execute<VerifyUsernameResponse>(new RestRequest("https://club.pokemon.com/api/signup/" +
                                                                              "verify-username", Method.POST)
                .AddJsonBody(new {name = options.Username})
                .AddHeader("X-CSRFToken", csrf));
            
            // If username is in use, switch to a random suggestion from PTC
            if (user.Data.InUse)
                options.Username = random.Choice(user.Data.Suggestions);

            var captcha = options.CaptchaService.Solve();

            await Task.Delay(random.Next(1500, 2500));

            var res = client.Execute(new RestRequest("https://club.pokemon.com/us/pokemon-trainer-club/sign-up/", Method
                    .POST)
                .AddParameter("csrfmiddlewaretoken", csrf)
                .AddParameter("username", options.Username));
            
            return new AccountCreationResult
            {
                Successful = res.StatusCode == HttpStatusCode.Found,
                Username = options.Username,
                Password = options.Password
            };
        }

        public static async Task HandleAccountAfterCreation(Configuration config, AccountCreationResult account)
        {
            var random = new TRandom();
            POGOLib.Official.Configuration.Hasher = new PokeHashHasher(config.TutorialSettings.HashKey);
            var client = await GetSession(new PtcLoginProvider(account.Username, account.Password),
                config.TutorialSettings.Latitude, config.TutorialSettings.Longitude);

            if (!await client.StartupAsync())
            {
                throw new SessionFailedError();
            }

            if (config.TutorialSettings.Level2 || config.TutorialSettings.Level5)
            {
                var ts = await Tutorial.GetTutorialState(client);
                await Tutorial.CompleteTutorial(client, ts);
                // Level 2: Stop here.
                // Level 5: Keep going.
                if (config.TutorialSettings.Level5)
                {
                    while (client.Player.Stats.Level < 5)
                    {
                        var lat = client.Player.Latitude + random.NextDouble(0.005, -0.005);
                        var lng = client.Player.Longitude + random.NextDouble(0.005, -0.005);
                        client.Player.SetCoordinates(lat, lng);

                        foreach (var cell in client.Map.Cells)
                        {
                            foreach (var pokemon in cell.CatchablePokemons)
                            {
                                if (DistanceBetweenPlaces(client.Player.Longitude, client.Player.Latitude,
                                        pokemon.Latitude, pokemon.Longitude) <= client.GlobalSettings.MapSettings
                                        .EncounterRangeMeters / 1000)
                                {
                                    var encRes = EncounterResponse.Parser.ParseFrom(await client.RpcClient.
                                        SendRemoteProcedureCallAsync(new Request
                                    {
                                        RequestType = RequestType.Encounter,
                                        RequestMessage = new EncounterMessage
                                        {
                                            EncounterId = pokemon.EncounterId,
                                            SpawnPointId = pokemon.SpawnPointId,
                                            PlayerLatitude = client.Player.Latitude,
                                            PlayerLongitude = client.Player.Longitude
                                        }.ToByteString()
                                    }));

                                    await Task.Delay(random.Next(825, 1250));

                                    if (encRes.Status == EncounterResponse.Types.Status.EncounterSuccess)
                                    {
                                        while (true)
                                        {
                                            var r = CatchPokemonResponse.Parser.ParseFrom(
                                                await client.RpcClient.SendRemoteProcedureCallAsync(new Request
                                            {
                                                RequestType = RequestType.CatchPokemon,
                                                RequestMessage = new CatchPokemonMessage
                                                {
                                                    EncounterId = pokemon.EncounterId,
                                                    HitPokemon = true,
                                                    SpawnPointId = pokemon.SpawnPointId,
                                                    NormalizedHitPosition = 1,
                                                    NormalizedReticleSize = random.NextDouble(1.7, 1.95),
                                                    Pokeball = ItemId.ItemPokeBall,
                                                    SpinModifier = 1
                                                }.ToByteString()
                                            }));

                                            if (r.Status == CatchPokemonResponse.Types.CatchStatus.CatchSuccess || 
                                                r.Status == CatchPokemonResponse.Types.CatchStatus.CatchEscape)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            foreach (var fort in cell.Forts)
                            {
                                if (DistanceBetweenPlaces(client.Player.Longitude, client.Player.Latitude,
                                        fort.Longitude, fort.Latitude) <=
                                    client.GlobalSettings.FortSettings.InteractionRangeMeters / 1000)
                                {
                                    if (fort.Type == FortType.Checkpoint)
                                    {
                                        await client.RpcClient.SendRemoteProcedureCallAsync(new Request
                                        {
                                            RequestType = RequestType.FortDetails,
                                            RequestMessage = new FortDetailsMessage
                                            {
                                                FortId = fort.Id,
                                                Latitude = fort.Latitude,
                                                Longitude = fort.Longitude
                                            }.ToByteString()
                                        });

                                        await Task.Delay(random.Next(600, 750));

                                        await client.RpcClient.SendRemoteProcedureCallAsync(new Request
                                        {
                                            RequestType = RequestType.FortSearch,
                                            RequestMessage = new FortSearchMessage
                                            {
                                                FortId = fort.Id,
                                                FortLatitude = fort.Latitude,
                                                FortLongitude = fort.Longitude,
                                                PlayerLatitude = client.Player.Latitude,
                                                PlayerLongitude = client.Player.Longitude
                                            }.ToByteString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task<Session> GetSession(ILoginProvider loginProvider, double initLat, double initLong)
        {
            return await Login.GetSession(loginProvider, initLat, initLong);
        }
        
        // cos(d) = sin(φА)·sin(φB) + cos(φА)·cos(φB)·cos(λА − λB),
        //  where φА, φB are latitudes and λА, λB are longitudes
        // Distance = d * R
        private static double DistanceBetweenPlaces(double lon1, double lat1, double lon2, double lat2)
        {
            const double R = 6371; // km

            var sLat1 = Math.Sin(Radians(lat1));
            var sLat2 = Math.Sin(Radians(lat2));
            var cLat1 = Math.Cos(Radians(lat1));
            var cLat2 = Math.Cos(Radians(lat2));
            var cLon = Math.Cos(Radians(lon1) - Radians(lon2));

            var cosD = sLat1*sLat2 + cLat1*cLat2*cLon;

            var d = Math.Acos(cosD);

            var dist = R * d;

            return dist;
        }
        
        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }
    }

    public class SessionFailedError : Exception
    {
    }
}