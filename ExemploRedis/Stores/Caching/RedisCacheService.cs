﻿using System;
using System.Linq;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExemploRedis.Stores.Caching
{
    public class RedisCacheService
    {
        private readonly IConfiguration _configuration;

        public RedisCacheService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private Lazy<ConnectionMultiplexer> lazyConnection { get { return CreateConnection(); } }

        public ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        private Lazy<ConnectionMultiplexer> CreateConnection()
        {
            return new Lazy<ConnectionMultiplexer>(() =>
            {
                string cacheConnection = _configuration.GetConnectionString("RedisServer");
                return ConnectionMultiplexer.Connect(cacheConnection);
            });
        }

        public IDatabase GetDatabase(int db = -1)
        {
            return BasicRetry(() => Connection.GetDatabase(db));
        }

        public System.Net.EndPoint[] GetEndPoints()
        {
            return BasicRetry(() => Connection.GetEndPoints());
        }

        public IServer GetServer(string host, int port)
        {
            return BasicRetry(() => Connection.GetServer(host, port));
        }
        #region Resiliencia

        private long lastReconnectTicks = DateTimeOffset.MinValue.UtcTicks;
        private DateTimeOffset firstErrorTime = DateTimeOffset.MinValue;
        private DateTimeOffset previousErrorTime = DateTimeOffset.MinValue;

        private readonly object reconnectLock = new object();

        // In general, let StackExchange.Redis handle most reconnects,
        // so limit the frequency of how often ForceReconnect() will
        // actually reconnect.
        public TimeSpan ReconnectMinFrequency => TimeSpan.FromSeconds(60);

        // If errors continue for longer than the below threshold, then the
        // multiplexer seems to not be reconnecting, so ForceReconnect() will
        // re-create the multiplexer.
        public TimeSpan ReconnectErrorThreshold => TimeSpan.FromSeconds(30);

        public int RetryMaxAttempts => 5;

        private void CloseConnection(Lazy<ConnectionMultiplexer> oldConnection)
        {
            if (oldConnection == null)
                return;

            try
            {
                oldConnection.Value.Close();
            }
            catch (Exception)
            {
                // Example error condition: if accessing oldConnection.Value causes a connection attempt and that fails.
            }
        }

        /// <summary>
        /// Force a new ConnectionMultiplexer to be created.
        /// NOTES:
        ///     1. Users of the ConnectionMultiplexer MUST handle ObjectDisposedExceptions, which can now happen as a result of calling ForceReconnect().
        ///     2. Don't call ForceReconnect for Timeouts, just for RedisConnectionExceptions or SocketExceptions.
        ///     3. Call this method every time you see a connection exception. The code will:
        ///         a. wait to reconnect for at least the "ReconnectErrorThreshold" time of repeated errors before actually reconnecting
        ///         b. not reconnect more frequently than configured in "ReconnectMinFrequency"
        /// </summary>
        public void ForceReconnect()
        {
            var utcNow = DateTimeOffset.UtcNow;
            long previousTicks = Interlocked.Read(ref lastReconnectTicks);
            var previousReconnectTime = new DateTimeOffset(previousTicks, TimeSpan.Zero);
            TimeSpan elapsedSinceLastReconnect = utcNow - previousReconnectTime;

            // If multiple threads call ForceReconnect at the same time, we only want to honor one of them.
            if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                return;

            lock (reconnectLock)
            {
                utcNow = DateTimeOffset.UtcNow;
                elapsedSinceLastReconnect = utcNow - previousReconnectTime;

                if (firstErrorTime == DateTimeOffset.MinValue)
                {
                    // We haven't seen an error since last reconnect, so set initial values.
                    firstErrorTime = utcNow;
                    previousErrorTime = utcNow;
                    return;
                }

                if (elapsedSinceLastReconnect < ReconnectMinFrequency)
                    return; // Some other thread made it through the check and the lock, so nothing to do.

                TimeSpan elapsedSinceFirstError = utcNow - firstErrorTime;
                TimeSpan elapsedSinceMostRecentError = utcNow - previousErrorTime;

                bool shouldReconnect =
                    elapsedSinceFirstError >= ReconnectErrorThreshold // Make sure we gave the multiplexer enough time to reconnect on its own if it could.
                    && elapsedSinceMostRecentError <= ReconnectErrorThreshold; // Make sure we aren't working on stale data (e.g. if there was a gap in errors, don't reconnect yet).

                // Update the previousErrorTime timestamp to be now (e.g. this reconnect request).
                previousErrorTime = utcNow;

                if (!shouldReconnect)
                    return;

                firstErrorTime = DateTimeOffset.MinValue;
                previousErrorTime = DateTimeOffset.MinValue;

                Lazy<ConnectionMultiplexer> oldConnection = lazyConnection;
                CloseConnection(oldConnection);
                //lazyConnection = CreateConnection();
                Interlocked.Exchange(ref lastReconnectTicks, utcNow.UtcTicks);
            }
        }

        // In real applications, consider using a framework such as
        // Polly to make it easier to customize the retry approach.
        private T BasicRetry<T>(Func<T> func)
        {
            int reconnectRetry = 0;
            int disposedRetry = 0;

            while (true)
            {
                try
                {
                    return func();
                }
                catch (Exception ex) when (ex is RedisConnectionException || ex is SocketException)
                {
                    reconnectRetry++;
                    if (reconnectRetry > RetryMaxAttempts)
                        throw;
                    ForceReconnect();
                }
                catch (ObjectDisposedException)
                {
                    disposedRetry++;
                    if (disposedRetry > RetryMaxAttempts)
                        throw;
                }
            }
        }

        #endregion
    }

    public static class RedisDataBaseExtensions
    {
        public static async Task<List<T>> Listar<T>(this IDatabase cache, string key)
        {
            var items = await cache.ListRangeAsync(key);

            var lista = items.Select(x => JsonConvert.DeserializeObject<T>(x));

            return lista.ToList();
        }

    }
}
