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
            var bla = new User(first: "Ada1", last: "Lovelace", born: 1815, address: new Address(street: "HereStreet"));
            await docRef.SetAsync(bla);
            await context.Response.WriteAsync($"Created Cloud Firestore client with project ID: {project}");
        }
    }

    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string First { get; }

        [FirestoreProperty]
        public string Last { get; }

        [FirestoreProperty]
        public int Born { get; }

        [FirestoreProperty]
        public Address Address { get; }

        public User() // For serialization
        {
        }

        public User(string first, string last, int born, Address address)
        {
            First = first;
            Last = last;
            Born = born;
            Address = address;
        }
    }

    [FirestoreData]
    public class Address
    {
        [FirestoreProperty]
        public string Street { get; set; }

        public Address() // For serialization
        {
        }

        public Address(string street)
        {
            Street = street;
        }
    }
}