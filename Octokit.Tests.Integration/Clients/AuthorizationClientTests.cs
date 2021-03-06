﻿using System;
using System.Threading.Tasks;
using Xunit;

namespace Octokit.Tests.Integration.Clients
{
    public class AuthorizationClientTests
    {
        [ApplicationTest]
        public async Task CanCreateAndGetAuthorizationWithoutFingerPrint()
        {
            var client = Helper.GetAuthenticatedClient();
            var note = Helper.MakeNameWithTimestamp("Testing authentication");
            var newAuthorization = new NewAuthorization(
                note,
                new[] { "user" });

            // the first call will create the authorization
            var created = await client.Authorization.GetOrCreateApplicationAuthentication(
                Helper.ClientId,
                Helper.ClientSecret,
                newAuthorization);

            Assert.NotNull(created);
            Assert.False(String.IsNullOrWhiteSpace(created.Token));
            Assert.True(String.IsNullOrWhiteSpace(created.TokenLastEight));
            Assert.True(String.IsNullOrWhiteSpace(created.HashedToken));

            // we can then query it through the regular API
            var get = await client.Authorization.Get(created.Id);

            Assert.Equal(created.Id, get.Id);
            Assert.Equal(created.Note, get.Note);

            // but the second time we call this API we get
            // a different set of data
            var getExisting = await client.Authorization.GetOrCreateApplicationAuthentication(
                Helper.ClientId,
                Helper.ClientSecret,
                newAuthorization);

            Assert.Equal(created.Id, getExisting.Id);

            // NOTE: the old API will continue to return the full
            //       token if no Fingerprint is included
            Assert.False(String.IsNullOrWhiteSpace(getExisting.Token));

            // NOTE: and these new values are not included
            Assert.True(String.IsNullOrWhiteSpace(getExisting.TokenLastEight));
            Assert.True(String.IsNullOrWhiteSpace(getExisting.HashedToken));

            await client.Authorization.Delete(created.Id);
        }

        [ApplicationTest]
        public async Task CanCreateAndGetAuthorizationByFingerprint()
        {
            var client = Helper.GetAuthenticatedClient();
            var fingerprint = Helper.MakeNameWithTimestamp("authorization-testing");
            var note = Helper.MakeNameWithTimestamp("Testing authentication");
            var newAuthorization = new NewAuthorization(
                note,
                new[] { "user" },
                fingerprint);

            var created = await client.Authorization.GetOrCreateApplicationAuthentication(
                Helper.ClientId,
                Helper.ClientSecret,
                newAuthorization);

            Assert.NotNull(created);
            Assert.False(String.IsNullOrWhiteSpace(created.Token));

            // we can then query it through the regular API
            var get = await client.Authorization.Get(created.Id);

            Assert.Equal(created.Id, get.Id);
            Assert.Equal(created.Note, get.Note);

            // but the second time we call this API we get
            // a different set of data
            var getExisting = await client.Authorization.GetOrCreateApplicationAuthentication(
                Helper.ClientId,
                Helper.ClientSecret,
                newAuthorization);

            Assert.Equal(created.Id, getExisting.Id);

            // NOTE: the new API will no longer return the full
            //       token as soon as you specify a Fingerprint
            Assert.True(String.IsNullOrWhiteSpace(getExisting.Token));

            // NOTE: however you will get these two new properties
            //       to help identify the authorization at hand
            Assert.False(String.IsNullOrWhiteSpace(getExisting.TokenLastEight));
            Assert.False(String.IsNullOrWhiteSpace(getExisting.HashedToken));

            await client.Authorization.Delete(created.Id);
        }
    }
}
