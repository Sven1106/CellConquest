
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace HelloFunctions
{
    public class Function : IHttpFunction
    {
        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            const string project = "cellconquest";
            var db = await FirestoreDb.CreateAsync(project);
            var docRef = db.Collection("users")
                .Document("123asd");
            var bla = new User { First = "Ada1", Last = "Lovelace", Born = 1815, Address = new Address { Street = "HereStreet" } };
            await docRef.SetAsync(bla);
            await context.Response.WriteAsync($"Created Cloud Firestore client with project ID: {project}");
        }
    }

    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string First { get; set; }

        [FirestoreProperty]
        public string Last { get; set; }

        [FirestoreProperty]
        public int Born { get; set; }

        [FirestoreProperty]
        public Address Address { get; set; }
    }

    [FirestoreData]
    public class Address
    {
        [FirestoreProperty]
        public string Street { get; set; }
    }
}