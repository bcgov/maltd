using BcGov.Malt.Web.Models.Authorization;
using System;
using Xunit;
using NodaTime;
using NodaTime.Testing;

namespace BcGov.Malt.Web.Tests.Models.Authorization
{
    public class TokenTests
    {
        private readonly DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

        [Fact]
        public void when_creating_a_token_the_created_date_is_set_to_the_current_date_using_fake_clock()
        {
            FakeClock clock = CreateClock();

            Token sut = new Token(() => clock.GetCurrentInstant().ToDateTimeOffset());

            Assert.Equal(_utcNow, sut.CreatedAtUtc);
        }

        [Fact]
        public void access_token_should_be_expired_when_the_current_date_equals_expiration_date()
        {
            // create a clock that advances a minute every time GetCurrentInstant() is called
            FakeClock clock = CreateClock(autoAdvance: TimeSpan.FromMinutes(1));

            Token sut = new Token(() => clock.GetCurrentInstant().ToDateTimeOffset());
            sut.ExpiresIn = 60 * 2; // expires in two mimutes

            Assert.Equal(_utcNow, sut.CreatedAtUtc);    // +0:00
            Assert.Equal(_utcNow.AddMinutes(2), sut.AccessTokenExpiresAtUtc);
        }


        [Fact]
        public void refresh_token_should_be_expired_when_the_current_date_equals_expiration_date()
        {
            // create a clock that advances a minute every time GetCurrentInstant() is called
            FakeClock clock = CreateClock(autoAdvance: TimeSpan.FromMinutes(1));

            Token sut = new Token(() => clock.GetCurrentInstant().ToDateTimeOffset());
            sut.RefreshTokenExpiresIn = 60 * 2; // expires in two mimutes

            Assert.Equal(_utcNow, sut.CreatedAtUtc);    // +0:00
            Assert.Equal(_utcNow.AddMinutes(2), sut.RefreshTokenExpiresAtUtc); 
        }

        [Fact]
        public void AccessTokenExpiresAtUtc_should_be_the_token_created_date_plus_ExpiresIn_seconds()
        {
            // create a clock that advances a minute every time GetCurrentInstant() is called
            FakeClock clock = CreateClock(autoAdvance: TimeSpan.FromMinutes(1));

            Token sut = new Token(() => clock.GetCurrentInstant().ToDateTimeOffset());
            sut.ExpiresIn = (int) TimeSpan.FromMinutes(2).TotalSeconds;

            Assert.Equal(_utcNow.AddMinutes(2), sut.AccessTokenExpiresAtUtc);
        }

        [Fact]
        public void RefreshTokenExpiresAtUtc_should_be_the_created_date_plus_RefreshTokenExpiresIn_seconds()
        {
            // create a clock that advances a minute every time GetCurrentInstant() is called
            FakeClock clock = CreateClock(TimeSpan.FromMinutes(1));
            int expiresInSeconds = 60 * 2;

            Token sut = new Token(() => clock.GetCurrentInstant().ToDateTimeOffset());
            sut.RefreshTokenExpiresIn = expiresInSeconds;

            Assert.Equal(_utcNow.AddSeconds(expiresInSeconds), sut.RefreshTokenExpiresAtUtc);
        }

        private FakeClock CreateClock() 
            => new FakeClock(Instant.FromDateTimeOffset(_utcNow));

        private FakeClock CreateClock(TimeSpan autoAdvance) 
            => new FakeClock(Instant.FromDateTimeOffset(_utcNow), Duration.FromTimeSpan(autoAdvance));
    }
}
