using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using AncibleCoreCommon;
using AncibleCoreServer.Data;
using AncibleCoreServer.Services.Ability;
using AncibleCoreServer.Services.CharacterClass;
using AncibleCoreServer.Services.Database;
using AncibleCoreServer.Services.ObjectManager;
using MessageBusLib;
using ECDiffieHellman = MLAPI.Cryptography.KeyExchanges.ECDiffieHellman;

namespace AncibleCoreServer.Services.ClientManager
{
    public class ClientManagerService : WorldService
    {
        public static List<PlayerClient> Clients { get; private set; }
        
        public static int CheckInTicks { get; private set; }
        public static int DisconnectTicks { get; private set; }
        
        public override string Name => "Client Manager Service";

        private const int ITERATIONS = 100000;

        private int _checkinTicks = 12;
        private int _disconnectTecks = 30;

        public ClientManagerService()
        {
            Clients = new List<PlayerClient>();
            CheckInTicks = _checkinTicks;
            DisconnectTicks = _disconnectTecks;
        }

        public override void Start()
        {
            SubscribeToMessages();
            base.Start();
        }

        public static int GetClientConnectionId(string clientId)
        {
            var client = Clients.Find(c => c.WorldId == clientId);
            if (client != null)
            {
                return client.NetworkId;
            }

            return -1;
        }

        private string HashPassword(string password)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS);
            var hash = pbkdf2.GetBytes(20);

            var hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        private int PasswordMatches(string username, string password)
        {
            var user = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE).FindOne(u => u.Username == username);
            if (user != null)
            {
                var hashBytes = Convert.FromBase64String(user.Password);
                var salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS);
                var hash = pbkdf2.GetBytes(20);
                for (var i = 0; i < 20; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                    {
                        return -1;
                    }
                }
                return user._id;
            }
            return -1;
        }

        private bool IsUserLoggedIn(string username)
        {
            var user = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE).FindOne(u => u.Username == username);
            if (user != null)
            {
                return user.Active;
            }
            return false;
        }

        private bool DoesUserNameExist(string username)
        {
            return DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE).Count(u => u.Username == username.ToLower()) > 0;
        }

        private bool IsUsernameValid(string username)
        {
            return username.All(c => !char.IsWhiteSpace(c) && (char.IsLetter(c) || char.IsNumber(c) || c == '_' || c == '-') );
        }

        private void SubscribeToMessages()
        {
            this.Subscribe<RegisterClientMessage>(RegisterClient);
            this.Subscribe<ClientLoginRequestMessage>(ClientLoginRequest);
            this.Subscribe<CreateUserMessage>(CreateUser);
            this.Subscribe<UnregisterClientMessage>(UnregisterClient);
            this.Subscribe<UserListMessage>(UsersList);
            this.Subscribe<ResetPasswordForUserMessage>(ResetPasswordForUser);
        }

        private void RegisterClient(RegisterClientMessage msg)
        {
            var existingClient = Clients.Find(c => c.NetworkId == msg.NetworkId);
            if (existingClient != null)
            {
                var clientRegisterResultMsg = new ClientRegisterResultMessage { Success = false, Message = "Cannot connect to server. Please try again later."};
                WorldServer.SendMessageToClient(clientRegisterResultMsg, msg.NetworkId);
            }
            else
            {
                Log($"Client {msg.NetworkId} has connected to the server");
                
                var dh = new ECDiffieHellman();
                var client = new PlayerClient (msg.NetworkId, new AuthenticationSession { PublicKey = dh.GetPublicKey(), AuthKey = dh.GetPrivateKey() });
                Clients.Add(client);
                var clientRegisterResultMsg = new ClientRegisterResultMessage {ClientId = client.WorldId, Key = client.Session.PublicKey, Success = true};
                WorldServer.SendMessageToClient(clientRegisterResultMsg, client.NetworkId);
            }
        }

        private void UnregisterClient(UnregisterClientMessage msg)
        {
            var existingClient = Clients.Find(c => c.NetworkId == msg.NetworkId);
            if (existingClient != null)
            {
                if (!string.IsNullOrEmpty(existingClient.User))
                {
                    var userCollection = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE);
                    var user = userCollection.FindOne(u => u.Username == existingClient.User);
                    if (user != null)
                    {
                        user.Active = false;
                        userCollection.Update(user);
                    }
                }
                this.SendMessageTo(DisconnectClientMessage.INSTANCE, existingClient);
                Clients.Remove(existingClient);
                
            }
        }

        private void ClientLoginRequest(ClientLoginRequestMessage msg)
        {
            var existingClient = Clients.Find(c => c.WorldId == msg.ClientId);
            if (existingClient != null)
            {
                var thread = new Thread(() =>
                {
                    var userName = string.Empty;
                    var start = DateTime.Now;
                    var returnMessage = string.Empty;
                    var key = msg.Key;
                    var dh = new ECDiffieHellman(existingClient.Session.AuthKey);
                    try
                    {
                        var secureLogin = AncibleUtils.ConvertToSecureLogin(AncibleCrypto.Decrypt(msg.Login, dh.GetSharedSecretRaw(key), msg.Iv));
                        if (IsUserLoggedIn(secureLogin.Username))
                        {
                            returnMessage = "User is already logged in at another locaion. Please wait a few minutes and try again";
                        }
                        else
                        {
                            userName = secureLogin.Username;
                            var id = PasswordMatches(secureLogin.Username, secureLogin.Password);
                            if (id > -1)
                            {
                                existingClient.Authenticate();
                            }

                            if (existingClient.Session.Authenticated)
                            {

                                Log($"User {secureLogin.Username} has succesfully been authenticated");
                                existingClient.User = secureLogin.Username;
                                existingClient.UserId = id;
                                var userCollection = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE);
                                var user = userCollection.FindOne(u => u.Username == existingClient.User);
                                if (user != null)
                                {
                                    user.Active = true;
                                    userCollection.Update(user);
                                    existingClient.UserDatabase = DatabaseService.OpenDatabase(user.DataPath);
                                }
                            }
                            else
                            {
                                returnMessage = "Username does not exist or password is incorrect. Please try again";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Exception during Login Attempt for Client {msg.ClientId} - {ex}");
                        returnMessage = "Username does not exist or password is incorrect. Please try again";
                    }
                    var clientLoginResultMsg = new ClientLoginResultMessage { Success = existingClient.Session.Authenticated, Message = returnMessage, StartingClasses = CharacterClassService.GetStartingClasses()};
                    WorldServer.SendMessageToClient(clientLoginResultMsg, existingClient.NetworkId);

                    var clientSetTickRateMsg = new ClientSetTickRateMessage {TickRate = TickService.TickRate, GlobalCooldown = AbilityService.GlobalCooldown};
                    WorldServer.SendMessageToClient(clientSetTickRateMsg, existingClient.NetworkId);
                    Log($"Client {existingClient.NetworkId} Login took {(DateTime.Now - start).Milliseconds} ms to login - {existingClient.Session.Authenticated}");
                });
                thread.Start();
            }
        }

        private void CreateUser(CreateUserMessage msg)
        {
            var worldUserCollection = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE);
            var existingUser = worldUserCollection.FindOne(u => u.Username == msg.Username);
            if (existingUser == null)
            {
                existingUser = new WorldUser
                {
                    Username = msg.Username,
                    Password = HashPassword(msg.Password),
                    DataPath = DatabaseService.CreateUserDatabase(msg.Username)
                };

                worldUserCollection.Insert(existingUser);
                Log($"User {msg.Username} created");
            }
            else
            {
                Log($"User {msg.Username} already exists");
            }
        }

        private void UsersList(UserListMessage msg)
        {
            var userStats = Clients.Select(c => c.GetUser()).ToArray();
            var logMessage = $"Users:";
            for (var i = 0; i < userStats.Length; i++)
            {
                logMessage = $"{logMessage}{Environment.NewLine}{userStats[i]}";
            }

            logMessage = $"{logMessage}{Environment.NewLine}Total Users Online: {userStats.Length}";
            Log(logMessage);
        }

        private void ResetPasswordForUser(ResetPasswordForUserMessage msg)
        {
            var user = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE).FindOne(u => u.Username.ToLower() == msg.User);
            if (user != null)
            {
                user.ResetKey = Guid.NewGuid().ToString();
                Log($"User {user.Username}'s password has been reset - {user.ResetKey}");
            }
            else
            {
                Log($"User {msg.User} does not exist");
            }
        }

        //private void ClientPasswordReset(ClientResetPasswordMessage msg)
        //{
        //    var client = Clients.Find(c => c.WorldId == msg.ClientId);
        //    if (client != null)
        //    {
        //        if (!string.IsNullOrEmpty(msg.ResetKey))
        //        {
        //            var users = DatabaseService.Main.GetCollection<WorldUser>(WorldUser.TABLE);
        //            var login = AncibleUtils.ConvertToSecureLogin(AncibleCrypto.Decrypt(msg.Login, msg.Key, msg.Iv));
        //            var user = users.FindOne(u => u.ResetKey == msg.ResetKey && u.Username == login.Username);
        //            if (user != null)
        //            {
        //                user.Password = login.Password;
        //                user.ResetKey = string.Empty;
        //                users.Update(user);
        //                Log($"Password Reset Succesfully for {user.Username}");
        //                WorldServer.SendMessageToClient(new ClientResetPasswordResultMessage { Success = true }, client.NetworkId);
        //            }
        //            else
        //            {
        //                WorldServer.SendMessageToClient(new ClientResetPasswordResultMessage { Success = false, Message = "Unable to changed password. Please check your username and reset key then try again." }, client.NetworkId);
        //            }
        //        }
        //        else
        //        {
        //            WorldServer.SendMessageToClient(new ClientResetPasswordResultMessage { Success = false, Message = "Unable to changed password. Please check your username and reset key then try again." }, client.NetworkId);
        //        }
        //    }


        //}

        public override void Stop()
        {
            base.Stop();
            Clients.Clear();
        }
    }
}