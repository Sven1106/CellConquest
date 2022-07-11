using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Mime;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Api = CellConquest.API;

namespace CreateGameFunction
{
    public class Function : IHttpFunction
    {
        private readonly ILogger _logger;
        public Function(ILogger logger) => _logger = logger;

        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var contentType = new ContentType(request.ContentType);
            if (contentType.MediaType != "application/json")
            {
                _logger.LogError("MediaType isn't application/json");
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            using TextReader reader = new StreamReader(context.Request.Body);
            var json = await reader.ReadToEndAsync();
            var gameConfig = JsonConvert.DeserializeObject<GameConfig>(json);
            if (gameConfig is null)
            {
                _logger.LogError("GameConfig was null");
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var db = await FirestoreDb.CreateAsync(Api.Shared.StaticFirestoreValues.ProjectId);
            var gameRef = db.Collection(Api.Shared.StaticFirestoreValues.GameCollection).Document(gameConfig.GameId);
            if ((await gameRef.GetSnapshotAsync()).Exists)
            {
                _logger.LogError("Game with id {GameConfigGameId} already exists", gameConfig.GameId);
                response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            
            
            
        }
    }

    public class GameConfig
    {
        public string GameId { get; }
        public string Owner { get; }
        public PointF[] Outline { get; }

        public GameConfig(string gameId, string owner, PointF[] Outline)
        {
            GameId = gameId;
            Owner = owner;
            this.Outline = Outline;
        }
    }
}