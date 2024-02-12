using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace TourneyBot {
    public class TestUser : IUser {
        public TestUser() {
            Id = 123456789;
        }
        public ulong Id { get; }
        public DateTimeOffset CreatedAt { get; }
        public string Mention { get; }
        public UserStatus Status { get; }
        public IReadOnlyCollection<ClientType> ActiveClients { get; }
        public IReadOnlyCollection<IActivity> Activities { get; }
        public string GetAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128) {
            return "";
        }

        public string GetDefaultAvatarUrl() {
            return "";
        }

        public Task<IDMChannel> CreateDMChannelAsync(RequestOptions options = null) {
            throw new NotImplementedException();
        }

        public string AvatarId { get; }
        public string Discriminator { get; }
        public ushort DiscriminatorValue { get; }
        public bool IsBot { get; }
        public bool IsWebhook { get; }
        public string Username { get; set; }
        public UserProperties? PublicFlags { get; }
    }
}